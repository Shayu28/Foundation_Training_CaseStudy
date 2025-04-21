using EcomApp.entity;
using EcomApp.exception;
using EcomApp.util;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomApp.dao
{
    public class OrderProcessorRepositoryImpl : IOrderProcessorRepository
    {
        private readonly string connectionString;

        public OrderProcessorRepositoryImpl()
        {
            connectionString = DBPropertyUtil.GetConnectionString("app.config");
        }

        public bool CreateProduct(Product product)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("INSERT INTO products (name, price, description, stockQuantity) VALUES (@Name, @Price, @Description, @StockQuantity)", connection);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool CreateCustomer(Customer customer)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("INSERT INTO customers (name, email, password) VALUES (@Name, @Email, @Password)", connection);
                command.Parameters.AddWithValue("@Name", customer.Name);
                command.Parameters.AddWithValue("@Email", customer.Email);
                command.Parameters.AddWithValue("@Password", customer.Password);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool DeleteProduct(int productId)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("DELETE FROM products WHERE product_id = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", productId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new ProductNotFoundException($"Product with ID {productId} not found.");
                return rowsAffected > 0;
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("DELETE FROM customers WHERE customer_id = @CustomerId", connection);
                command.Parameters.AddWithValue("@CustomerId", customerId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new CustomerNotFoundException($"Customer with ID {customerId} not found.");
                return rowsAffected > 0;
            }
        }

        public bool AddToCart(Customer customer, Product product, int quantity)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                // Check if product exists
                SqlCommand checkProductCmd = new SqlCommand("SELECT COUNT(*) FROM products WHERE product_id = @ProductId", connection);
                checkProductCmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                connection.Open();
                int productCount = (int)checkProductCmd.ExecuteScalar();
                if (productCount == 0)
                    throw new ProductNotFoundException($"Product with ID {product.ProductId} not found.");

                // Check if customer exists
                SqlCommand checkCustomerCmd = new SqlCommand("SELECT COUNT(*) FROM customers WHERE customer_id = @CustomerId", connection);
                checkCustomerCmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                int customerCount = (int)checkCustomerCmd.ExecuteScalar();
                if (customerCount == 0)
                    throw new CustomerNotFoundException($"Customer with ID {customer.CustomerId} not found.");

                // Add to cart
                SqlCommand command = new SqlCommand("INSERT INTO cart (customer_id, product_id, quantity) VALUES (@CustomerId, @ProductId, @Quantity)", connection);
                command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                command.Parameters.AddWithValue("@ProductId", product.ProductId);
                command.Parameters.AddWithValue("@Quantity", quantity);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool RemoveFromCart(Customer customer, Product product)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("DELETE FROM cart WHERE customer_id = @CustomerId AND product_id = @ProductId", connection);
                command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                command.Parameters.AddWithValue("@ProductId", product.ProductId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new ProductNotFoundException($"Product with ID {product.ProductId} not found in customer's cart.");
                return rowsAffected > 0;
            }
        }

        public List<Product> GetAllFromCart(Customer customer)
        {
            List<Product> cartProducts = new List<Product>();
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(
                    "SELECT p.product_id, p.name, p.price, p.description, p.stockQuantity, c.quantity " +
                    "FROM cart c " +
                    "JOIN products p ON c.product_id = p.product_id " +
                    "WHERE c.customer_id = @CustomerId", connection);
                command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Product product = new Product
                    {
                        ProductId = Convert.ToInt32(reader["product_id"]),
                        Name = reader["name"].ToString(),
                        Price = Convert.ToDecimal(reader["price"]),
                        Description = reader["description"].ToString(),
                        StockQuantity = Convert.ToInt32(reader["stockQuantity"])
                    };
                    cartProducts.Add(product);
                }
            }
            return cartProducts;
        }

        public bool PlaceOrder(Customer customer, Dictionary<Product, int> productsWithQuantities, string shippingAddress)
        {
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Calculate total price
                    decimal totalPrice = 0;
                    foreach (var item in productsWithQuantities)
                    {
                        totalPrice += item.Key.Price * item.Value;
                    }

                    // Create order
                    SqlCommand orderCommand = new SqlCommand(
                        "INSERT INTO orders (customer_id, total_price, shipping_address) OUTPUT INSERTED.order_id VALUES (@CustomerId, @TotalPrice, @ShippingAddress)",
                        connection, transaction);
                    orderCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    orderCommand.Parameters.AddWithValue("@TotalPrice", totalPrice);
                    orderCommand.Parameters.AddWithValue("@ShippingAddress", shippingAddress);

                    int orderId = (int)orderCommand.ExecuteScalar();

                    // Add order items
                    foreach (var item in productsWithQuantities)
                    {
                        SqlCommand itemCommand = new SqlCommand(
                            "INSERT INTO order_items (order_id, product_id, quantity) VALUES (@OrderId, @ProductId, @Quantity)",
                            connection, transaction);
                        itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                        itemCommand.Parameters.AddWithValue("@ProductId", item.Key.ProductId);
                        itemCommand.Parameters.AddWithValue("@Quantity", item.Value);
                        itemCommand.ExecuteNonQuery();

                        // Update product stock
                        SqlCommand updateStockCommand = new SqlCommand(
                            "UPDATE products SET stockQuantity = stockQuantity - @Quantity WHERE product_id = @ProductId",
                            connection, transaction);
                        updateStockCommand.Parameters.AddWithValue("@Quantity", item.Value);
                        updateStockCommand.Parameters.AddWithValue("@ProductId", item.Key.ProductId);
                        updateStockCommand.ExecuteNonQuery();
                    }

                    // Clear cart
                    SqlCommand clearCartCommand = new SqlCommand(
                        "DELETE FROM cart WHERE customer_id = @CustomerId",
                        connection, transaction);
                    clearCartCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    clearCartCommand.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public List<Dictionary<Product, int>> GetOrdersByCustomer(int customerId)
        {
            List<Dictionary<Product, int>> orders = new List<Dictionary<Product, int>>();
            using (SqlConnection connection = DBConnUtil.GetConnection(connectionString))
            {
                // Get all orders for the customer
                SqlCommand orderCommand = new SqlCommand(
                    "SELECT o.order_id, o.order_date, o.total_price, o.shipping_address " +
                    "FROM orders o " +
                    "WHERE o.customer_id = @CustomerId", connection);
                orderCommand.Parameters.AddWithValue("@CustomerId", customerId);

                connection.Open();
                SqlDataReader orderReader = orderCommand.ExecuteReader();
                while (orderReader.Read())
                {
                    Dictionary<Product, int> orderItems = new Dictionary<Product, int>();

                    // Get items for each order
                    using (SqlConnection itemConnection = DBConnUtil.GetConnection(connectionString))
                    {
                        SqlCommand itemCommand = new SqlCommand(
                            "SELECT p.product_id, p.name, p.price, p.description, p.stockQuantity, oi.quantity " +
                            "FROM order_items oi " +
                            "JOIN products p ON oi.product_id = p.product_id " +
                            "WHERE oi.order_id = @OrderId", itemConnection);
                        itemCommand.Parameters.AddWithValue("@OrderId", Convert.ToInt32(orderReader["order_id"]));

                        itemConnection.Open();
                        SqlDataReader itemReader = itemCommand.ExecuteReader();
                        while (itemReader.Read())
                        {
                            Product product = new Product
                            {
                                ProductId = Convert.ToInt32(itemReader["product_id"]),
                                Name = itemReader["name"].ToString(),
                                Price = Convert.ToDecimal(itemReader["price"]),
                                Description = itemReader["description"].ToString(),
                                StockQuantity = Convert.ToInt32(itemReader["stockQuantity"])
                            };
                            orderItems.Add(product, Convert.ToInt32(itemReader["quantity"]));
                        }
                    }

                    orders.Add(orderItems);
                }
            }
            return orders;
        }
    }
}
