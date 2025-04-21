USE EcomApp;
GO

-- Customers table
CREATE TABLE customers 
(
customer_id INT PRIMARY KEY IDENTITY(1,1),
name VARCHAR(100) NOT NULL,
email VARCHAR(100) NOT NULL UNIQUE,
password VARCHAR(100) NOT NULL
)

-- Products table
CREATE TABLE products 
(
product_id INT PRIMARY KEY IDENTITY(1,1),
name VARCHAR(100) NOT NULL,
price DECIMAL(10, 2) NOT NULL,
description VARCHAR(500),
stockQuantity INT NOT NULL
)

-- Cart table
CREATE TABLE cart 
(
cart_id INT PRIMARY KEY IDENTITY(1,1),
customer_id INT NOT NULL,
product_id INT NOT NULL,
quantity INT NOT NULL,
FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
FOREIGN KEY (product_id) REFERENCES products(product_id)
)

-- Orders table
CREATE TABLE orders 
(
order_id INT PRIMARY KEY IDENTITY(1,1),
customer_id INT NOT NULL,
order_date DATETIME NOT NULL DEFAULT GETDATE(),
total_price DECIMAL(10, 2) NOT NULL,
shipping_address VARCHAR(200) NOT NULL,
FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
)

-- Order items table
CREATE TABLE order_items 
(
order_item_id INT PRIMARY KEY IDENTITY(1,1),
order_id INT NOT NULL,
product_id INT NOT NULL,
quantity INT NOT NULL,
FOREIGN KEY (order_id) REFERENCES orders(order_id),
FOREIGN KEY (product_id) REFERENCES products(product_id)
)

SELECT * FROM customers
SELECT * FROM products
SELECT * FROM cart
SELECT * FROM orders
SELECT * FROM order_items 



