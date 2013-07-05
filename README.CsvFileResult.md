# MvcExtras.CsvFileResult

Simple CSV action result for ASP.NET MVC. 

## Using CsvFileResult action result

Sample usage:

		class Product {
			public string Name { get; set; }
			public string Category { get; set; }
			public decimal Price { get; set; }
			public int Qty { get; set; }
		}

		class ProductDAL {
			...
			public static IEnumerable<Product> GetAllProducts() {
				IEnumerable<Product> products;

				while ((products = GetNextPageOfProducts()) != null) 
					for (var product in products)
						yield return product;
			}
			...
		}

		public class CsvController : Controller { 
			...
			public ActionResult GetCsvFile() {

				Func<Product,IEnumerable<string>> productToStringArray = p => new [] { p.Name, p.Category, p.Price, p.Qty };

				var csvFileResult = new CsvFileResult<Product>(ProductDAL.GetAllProducts(), productToStringArray, "text/csv");
				csvFileResult.FileDownloadName = "test.csv";
				csvFileResult.HeaderItems = new[] { "Name", "Category", "Price", "Qty" };

				return csvFileResult;
			}
			...
		}

See unit tests for more examples, and `CsvFileResult`'s properties for more options.

