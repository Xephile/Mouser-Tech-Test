I noticed the request object for the 'Product' object takes a reserved quantity, but the reserved quantity is always hard coded to 0.
If this was a bigger object with more redundant properties, I would maybe have made a separate object called 'AddProductRequest' with just these properties:

            {
                "id": 1,
                "inStockQuantity": 1,
                "name": "product name"
            }
            
And then I would make the 'Product' object inherit 'AddProductRequest' and then add the reserved quantity property so the other methods still have access to it. E.G.
```
namespace EPM.Mouser.Interview.Web.Models
{
    public class AddProductRequest
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int InStockQuantity { get; set; }
    }

    public class Product : AddProductRequest
    {
        public int ReservedQuantity { get; set; }
    }
}
```


I also noticed when inserting, the 'Insert' method given doesn't use the Id passed in as the Id, instead it sets it as the count of all the products.
If this was an actual DLL and not compiled I would fix it to use the Id given.


I wanted to add some more functionality to the product page. I was going to link the buttons I had placed to their respective API methods but unfortunately I did not have time.
