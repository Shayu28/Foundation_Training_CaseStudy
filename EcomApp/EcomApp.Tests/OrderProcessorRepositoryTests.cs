using EcomApp.dao;
using EcomApp.entity;
using EcomApp.exception;
using EcomApp.util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace EcomApp.Tests
{
    [TestFixture]
    public class OrderProcessorRepositoryTests
    {
        private IOrderProcessorRepository repository;
        private Customer testCustomer;
        private Product testProduct;

        [SetUp]
        public void Setup()
        {
            repository = new OrderProcessorRepositoryImpl();

            // Create a test customer
            testCustomer = new Customer("Test User", "test@example.com", "password");
            repository.CreateCustomer(testCustomer);

            // Create a test product
            testProduct = new Product("Test Product", 9.99m, "Test Description", 100);
            repository.CreateProduct(testProduct);
        }

        [TearDown]
        public void Cleanup()
        {
            try { repository.DeleteCustomer(testCustomer.CustomerId); } catch { }
            try { repository.DeleteProduct(testProduct.ProductId); } catch { }
        }


        [Test]
        public void CreateProduct_ValidProduct_ReturnsTrue()
        {
            // Arrange
            Product product = new Product("New Product", 19.99m, "New Description", 50);

            // Act
            bool result = repository.CreateProduct(product);

            // Assert
            Assert.That(result, Is.Empty);

            // Cleanup
            repository.DeleteProduct(product.ProductId);
        }

        [Test]
        public void CreateCustomer_ValidCustomer_ReturnsTrue()
        {
            // Arrange
            Customer customer = new Customer("New Customer", "new@example.com", "password");

            // Act
            bool result = repository.CreateCustomer(customer);

            // Assert
            Assert.That(result, Is.Empty);

            // Cleanup
            repository.DeleteCustomer(customer.CustomerId);
        }

        [Test]
        public void AddToCart_ValidItems_ReturnsTrue()
        {
            // Act
            bool result = repository.AddToCart(testCustomer, testProduct, 2);

            // Assert
            Assert.That(result, Is.Empty);

            // Cleanup
            repository.RemoveFromCart(testCustomer, testProduct);
        }

        [Test]
        public void PlaceOrder_ValidOrder_ReturnsTrue()
        {
            // Arrange
            repository.AddToCart(testCustomer, testProduct, 1);
            var cartItems = new Dictionary<Product, int> { { testProduct, 1 } };

            // Act
            bool result = repository.PlaceOrder(testCustomer, cartItems, "123 Test St");

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DeleteProduct_InvalidProductId_ThrowsProductNotFoundException()
        {
            // Arrange
            int invalidProductId = -1;

            // Act & Assert
            Assert.Throws<ProductNotFoundException>(() => repository.DeleteProduct(invalidProductId));
        }

        [Test]
        public void DeleteCustomer_InvalidCustomerId_ThrowsCustomerNotFoundException()
        {
            // Arrange
            int invalidCustomerId = -1;

            // Act & Assert
            Assert.Throws<CustomerNotFoundException>(() => repository.DeleteCustomer(invalidCustomerId));
        }
    }
}