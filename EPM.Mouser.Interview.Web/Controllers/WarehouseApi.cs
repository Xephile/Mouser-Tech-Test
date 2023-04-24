using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("api/warehouse")]
    public class WarehouseApi : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseApi(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        /*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
        [HttpGet("id")]
        public async Task<Product?> GetProduct(long id)
        {
            return await _warehouseRepository.Get(id);
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet]
        public async Task<List<Product>> GetPublicInStockProducts()
        {
            return await _warehouseRepository.List();
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPost("order")]
        public async Task<UpdateResponse> OrderItem(UpdateQuantityRequest request)
        {
            if (request.Quantity < 0)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.QuantityInvalid,
                    Success = false
                };

            var product = await _warehouseRepository.Get(request.Id);

            if (product is null)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.InvalidRequest,
                    Success = false
                };

            if (product.ReservedQuantity + request.Quantity > product.InStockQuantity)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.NotEnoughQuantity,
                    Success = false
                };

            try
            {
                await _warehouseRepository.UpdateQuantities(new Product
                {
                    Id = request.Id,
                    InStockQuantity = product.InStockQuantity,
                    Name = product.Name,
                    ReservedQuantity = product.ReservedQuantity + request.Quantity
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // I would log this error using whatever error logging standard the company uses.
                return new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.InvalidRequest
                };
            }

            return new UpdateResponse
            {
                Success = true
            };
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPost("ship")]
        public async Task<UpdateResponse> ShipItem(UpdateQuantityRequest request)
        {
            if (request.Quantity < 0)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.QuantityInvalid,
                    Success = false
                };

            var product = await _warehouseRepository.Get(request.Id);

            if (product is null)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.InvalidRequest,
                    Success = false
                };

            if (product.ReservedQuantity + request.Quantity > product.InStockQuantity)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.NotEnoughQuantity,
                    Success = false
                };

            var finalAmount = product.ReservedQuantity - request.Quantity;

            await _warehouseRepository.UpdateQuantities(new Product
            {
                Id = request.Id,
                InStockQuantity = product.InStockQuantity,
                Name = product.Name,
                ReservedQuantity = finalAmount > 0 ? finalAmount : 0
            });

            return new UpdateResponse
            {
                Success = true
            };
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPost("restock")]
        public async Task<UpdateResponse> RestockItem(UpdateQuantityRequest request)
        {
            if (request.Quantity < 0)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.QuantityInvalid,
                    Success = false
                };

            var product = await _warehouseRepository.Get(request.Id);

            if (product is null)
                return new UpdateResponse
                {
                    ErrorReason = ErrorReason.InvalidRequest,
                    Success = false
                };

            await _warehouseRepository.UpdateQuantities(new Product
            {
                Id = request.Id,
                InStockQuantity = product.InStockQuantity + request.Quantity,
                Name = product.Name,
                ReservedQuantity = product.ReservedQuantity
            });

            return new UpdateResponse
            {
                Success = true
            };
        }

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        [HttpPost("add")]
        public async Task<CreateResponse<Product>> AddNewProduct(Product addRequest)
        {
            if (string.IsNullOrEmpty(addRequest.Name))
                return new CreateResponse<Product>
                {
                    ErrorReason = ErrorReason.InvalidRequest,
                    Success = false,
                    Model = new Product()
                };

            if (addRequest.InStockQuantity < 0)
                return new CreateResponse<Product>
                {
                    ErrorReason = ErrorReason.QuantityInvalid,
                    Success = false,
                    Model = new Product()
                };

            var allProducts = await _warehouseRepository.List();

            if (allProducts.Any(x => x.Name.Equals(addRequest.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var count = 1;
                for (var i = 0; i < count; i++)
                {
                    addRequest.Name += count.ToString();
                    if (allProducts.Any(x => x.Name.Equals(addRequest.Name, StringComparison.OrdinalIgnoreCase))) count++;
                }
            }

            var newProduct = await _warehouseRepository.Insert(new Product
            {
                Id = addRequest.Id,
                Name = addRequest.Name,
                InStockQuantity = addRequest.InStockQuantity,
                ReservedQuantity = 0
            });

            return new CreateResponse<Product>
            {
                Success = true,
                Model = newProduct
            };

            /*
            I noticed the request object takes a reserved quantity, but the reserved quantity is always hard coded to 0.
            If this was a bigger object with more redundant properties, I would maybe have made a separate object called 'AddProductRequest' with just these properties:
            {
               *           "id": 1,
               *           "inStockQuantity": 1,
               *           "name": "product name"
            }
            And then I would make the 'Product' object inherit 'AddProductRequest' and then add the reserved quantity property so the other methods still have access to it.

            I also noticed when inserting, the 'Insert' method given doesn't use the Id passed in as the Id, instead it sets it as the count of all the products.
            If this was an actual DLL and not compiled I would fix it to use the Id given.
            */
        }
    }
}