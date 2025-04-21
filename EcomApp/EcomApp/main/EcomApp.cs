using EcomApp.dao;
using EcomApp.entity;
using EcomApp.exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomApp.main
{
    class EcomApp
    {
        private static IOrderProcessorRepository orderProcessor = new OrderProcessorRepositoryImpl();

        static void Main(string[] args)
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nE-Commerce Application Menu:");
                Console.WriteLine("1. Register Customer");
                Console.WriteLine("2. Create Product");
                Console.WriteLine("3. Delete Product");
                Console.WriteLine("4. Add to Cart");
                Console.WriteLine("5. View Cart");
                Console.WriteLine("6. Place Order");
                Console.WriteLine("7. View Customer Orders");
                Console.WriteLine("8. Exit");
                Console.Write("Enter your choice: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    try
                    {
                        switch (choice)
                        {
                            case 1:
                                RegisterCustomer();
                                break;
                            case 2:
                                CreateProduct();
                                break;
                            case 3:
                                DeleteProduct();
                                break;
                            case 4:
                                AddToCart();
                                break;
                            case 5:
                                ViewCart();
                                break;
                            case 6:
                                PlaceOrder();
                                break;
                            case 7:
                                ViewCustomerOrders();
                                break;
                            case 8:
                                exit = true;
                                Console.WriteLine("Exiting application...");
                                break;
                            default:
                                Console.WriteLine("Invalid choice. Please try again.");
                                break;
                        }
                    }
                    catch (CustomerNotFoundException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    catch (ProductNotFoundException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    catch (OrderNotFoundException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }
        }

        static void RegisterCustomer()
        {
            Console.Write("Enter customer name: ");
            string name = Console.ReadLine();
            Console.Write("Enter email: ");
            string email = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            Customer customer = new Customer(name, email, password);
            bool success = orderProcessor.CreateCustomer(customer);
            Console.WriteLine(success ? "Customer registered successfully!" : "Failed to register customer.");
        }

        static void CreateProduct()
        {
            Console.Write("Enter product name: ");
            string name = Console.ReadLine();
            Console.Write("Enter price: ");
            decimal price = decimal.Parse(Console.ReadLine());
            Console.Write("Enter description: ");
            string description = Console.ReadLine();
            Console.Write("Enter stock quantity: ");
            int stockQuantity = int.Parse(Console.ReadLine());

            Product product = new Product(name, price, description, stockQuantity);
            bool success = orderProcessor.CreateProduct(product);
            Console.WriteLine(success ? "Product created successfully!" : "Failed to create product.");
        }

        static void DeleteProduct()
        {
            Console.Write("Enter product ID to delete: ");
            int productId = int.Parse(Console.ReadLine());
            bool success = orderProcessor.DeleteProduct(productId);
            Console.WriteLine(success ? "Product deleted successfully!" : "Failed to delete product.");
        }

        static void AddToCart()
        {
            Console.Write("Enter customer ID: ");
            int customerId = int.Parse(Console.ReadLine());
            Console.Write("Enter product ID: ");
            int productId = int.Parse(Console.ReadLine());
            Console.Write("Enter quantity: ");
            int quantity = int.Parse(Console.ReadLine());

            Customer customer = new Customer { CustomerId = customerId };
            Product product = new Product { ProductId = productId };
            bool success = orderProcessor.AddToCart(customer, product, quantity);
            Console.WriteLine(success ? "Product added to cart successfully!" : "Failed to add product to cart.");
        }

        static void ViewCart()
        {
            Console.Write("Enter customer ID: ");
            int customerId = int.Parse(Console.ReadLine());
            Customer customer = new Customer { CustomerId = customerId };

            List<Product> cartItems = orderProcessor.GetAllFromCart(customer);
            if (cartItems.Count == 0)
            {
                Console.WriteLine("Cart is empty.");
                return;
            }

            Console.WriteLine("\nCart Items:");
            foreach (var product in cartItems)
            {
                Console.WriteLine($"ID: {product.ProductId}, Name: {product.Name}, Price: {product.Price:C}, Description: {product.Description}");
            }
        }

        static void PlaceOrder()
        {
            Console.Write("Enter customer ID: ");
            int customerId = int.Parse(Console.ReadLine());
            Customer customer = new Customer { CustomerId = customerId };

            // Get cart items
            List<Product> cartItems = orderProcessor.GetAllFromCart(customer);
            if (cartItems.Count == 0)
            {
                Console.WriteLine("Cannot place order - cart is empty.");
                return;
            }

            // Convert to dictionary for PlaceOrder method
            Dictionary<Product, int> productsWithQuantities = new Dictionary<Product, int>();
            foreach (var product in cartItems)
            {
                // In a real app, we would get the actual quantity from the cart
                productsWithQuantities.Add(product, 1); // Assuming quantity 1 for simplicity
            }

            Console.Write("Enter shipping address: ");
            string shippingAddress = Console.ReadLine();

            bool success = orderProcessor.PlaceOrder(customer, productsWithQuantities, shippingAddress);
            Console.WriteLine(success ? "Order placed successfully!" : "Failed to place order.");
        }

        static void ViewCustomerOrders()
        {
            Console.Write("Enter customer ID: ");
            int customerId = int.Parse(Console.ReadLine());

            List<Dictionary<Product, int>> orders = orderProcessor.GetOrdersByCustomer(customerId);
            if (orders.Count == 0)
            {
                Console.WriteLine("No orders found for this customer.");
                return;
            }

            Console.WriteLine("\nCustomer Orders:");
            for (int i = 0; i < orders.Count; i++)
            {
                Console.WriteLine($"\nOrder #{i + 1}:");
                foreach (var item in orders[i])
                {
                    Console.WriteLine($"- Product: {item.Key.Name}, Quantity: {item.Value}, Price: {item.Key.Price:C}, Total: {item.Key.Price * item.Value:C}");
                }
            }
        }
    }
}
