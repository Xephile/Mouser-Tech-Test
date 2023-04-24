using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using EPM.Mouser.Interview.Web.Controllers;
using NUnit.Framework;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WarehouseApiTests
{
    public class WarehouseApiTests
    {
        private WarehouseApi _warehouseApi { get; set; } = null!;
        private const long ValidId = 1;
        private const long InvalidId = -1;
        private const int ValidQuantity = 10;
        private const int InvalidQuantity = -10;

        [SetUp]
        public void Setup()
        {
            IWarehouseRepository warehouseRepository = new WarehouseRepository();
            _warehouseApi = new WarehouseApi(warehouseRepository);
        }

        #region GetProducts
        [Test]
        public async Task GettingSingleProduct()
        {
            //Arrange

            //Act
            var product = await _warehouseApi.GetProduct(ValidId);

            //Assert
            Assert.IsNotNull(product);
            Assert.AreEqual(ValidId, product!.Id);
        }

        [Test]
        public async Task FailGettingSingleProduct()
        {
            //Arrange
            //Act
            var product = await _warehouseApi.GetProduct(InvalidId);

            //Assert
            Assert.IsNull(product);
        }

        [Test]
        public async Task GetAllStockedProducts()
        {
            //Arrange

            //Act
            var products = await _warehouseApi.GetPublicInStockProducts();

            //Assert
            Assert.NotZero(products.Count);
        }
        #endregion

        #region Order
        [Test]
        public async Task OrderItem()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.OrderItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(true, response.Success);
        }

        [Test]
        public async Task OrderNegativeQuantity()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.OrderItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = InvalidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, response.ErrorReason);
        }

        [Test]
        public async Task OrderInvalidProduct()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.OrderItem(new UpdateQuantityRequest
            {
                Id = InvalidId,
                Quantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, response.ErrorReason);
        }

        [Test]
        public async Task OrderOverQuantity()
        {
            //Arrange
            var product = await _warehouseApi.GetProduct(ValidId);
            var quantity = product!.InStockQuantity + product.ReservedQuantity + 1;

            //Act
            var response = await _warehouseApi.OrderItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = quantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.NotEnoughQuantity, response.ErrorReason);
        }
        #endregion

        #region Shipping
        [Test]
        public async Task ShipItem()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.ShipItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(true, response.Success);
        }

        [Test]
        public async Task ShipNegativeQuantity()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.ShipItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = InvalidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, response.ErrorReason);
        }

        [Test]
        public async Task ShipInvalidProduct()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.ShipItem(new UpdateQuantityRequest
            {
                Id = InvalidId,
                Quantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, response.ErrorReason);
        }

        [Test]
        public async Task ShipMoreThanInStockQuantity()
        {
            //Arrange
            var product = await _warehouseApi.GetProduct(ValidId);
            var quantity = product!.InStockQuantity + 1;
            //Act
            var response = await _warehouseApi.ShipItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = quantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.NotEnoughQuantity, response.ErrorReason);
        }
        #endregion

        #region Restock
        [Test]
        public async Task RestockItem()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.RestockItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(true, response.Success);
        }

        [Test]
        public async Task RestockInvalidProduct()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.RestockItem(new UpdateQuantityRequest
            {
                Id = InvalidId,
                Quantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.InvalidRequest, response.ErrorReason);
        }

        [Test]
        public async Task RestockWithNegativeQuantity()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.RestockItem(new UpdateQuantityRequest
            {
                Id = ValidId,
                Quantity = InvalidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(ErrorReason.QuantityInvalid, response.ErrorReason);
        }
        #endregion

        #region Add
        [Test]
        public async Task AddItem()
        {
            //Arrange
            var allProducts = await _warehouseApi.GetPublicInStockProducts();

            //Act
            var response = await _warehouseApi.AddNewProduct(new Product
            {
                Id = ValidId,
                Name = "ThisIsAUniqueName(Hopefully)",
                InStockQuantity = ValidQuantity + ValidQuantity,
                ReservedQuantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(true, response.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(new Product
            {
                Id = allProducts.Count,
                ReservedQuantity = 0,
                InStockQuantity = ValidQuantity + ValidQuantity,
                Name = "ThisIsAUniqueName(Hopefully)"
            }), JsonConvert.SerializeObject(response.Model));
        }

        [Test]
        public async Task AddWithEmptyName()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.AddNewProduct(new Product
            {
                Id = ValidId,
                Name = "",
                InStockQuantity = ValidQuantity + ValidQuantity,
                ReservedQuantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(new Product()), JsonConvert.SerializeObject(response.Model));
        }

        [Test]
        public async Task AddWithNegativeQuantity()
        {
            //Arrange

            //Act
            var response = await _warehouseApi.AddNewProduct(new Product
            {
                Id = ValidId,
                Name = "ThisIsAUniqueName(Hopefully)",
                InStockQuantity = InvalidQuantity,
                ReservedQuantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(false, response.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(new Product()), JsonConvert.SerializeObject(response.Model));
        }

        [Test]
        public async Task AddWithDupeName()
        {
            //Arrange
            var product = await _warehouseApi.GetProduct(ValidId);

            //Act
            var response = await _warehouseApi.AddNewProduct(new Product
            {
                Id = ValidId,
                Name = product!.Name,
                InStockQuantity = ValidQuantity,
                ReservedQuantity = ValidQuantity
            });

            //Assert
            Assert.AreEqual(true, response.Success);
            Assert.AreEqual($"{product.Name}1", response.Model.Name);
        }

        #endregion
    }
}