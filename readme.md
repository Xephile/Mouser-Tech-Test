I noticed the request object for the takes a reserved quantity, but the reserved quantity is always hard coded to 0.
If this was a bigger object with more redundant properties, I would maybe have made a separate object called 'AddProductRequest' with just these properties:
            ```{
                "id": 1,
                "inStockQuantity": 1,
                "name": "product name"
            }```
And then I would make the 'Product' object inherit 'AddProductRequest' and then add the reserved quantity property so the other methods still have access to it.

I also noticed when inserting, the 'Insert' method given doesn't use the Id passed in as the Id, instead it sets it as the count of all the products.
If this was an actual DLL and not compiled I would fix it to use the Id given.
