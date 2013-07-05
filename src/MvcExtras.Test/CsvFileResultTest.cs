using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using Xunit;

namespace MvcExtras.Test
{
	public class CsvFileResultTest
	{
	    private List<TestProduct> _products;
	    private string _defaultOutput;
	    private MemoryStream _outputBuffer;
	    private HttpResponseBase _response;
	    private ControllerContext _controllerCtx;

		public CsvFileResultTest()
		{
            CreateProducts();
            SetupControllerContextMocks();
            SetDefaultOutput();
		}

		[Fact]
		public void Default()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray);
			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();

			Assert.Equal("text/csv", result.ContentType);
			Assert.Equal(_defaultOutput, output);
			Assert.True(result.BufferOutput);
		}

		[Fact]
		public void Delimiter_Semicolon()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray);
			result.FieldDelimiter = ';';
			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();

			_defaultOutput = _defaultOutput.Replace(',', ';');

			Assert.Equal(_defaultOutput, output);
		}

		[Fact]
		public void NewLine_LineFeed()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray);
			result.NewLine = "\n";
			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();

			_defaultOutput = _defaultOutput.Replace(Environment.NewLine, "\n");

			Assert.Equal(_defaultOutput, output);
		}

		[Fact]
		public void Include_UTF8_Preamble()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray);
			result.IncludePreamble = true;
			result.ExecuteResult(_controllerCtx);

			var preamble = Encoding.UTF8.GetPreamble();

			_outputBuffer.Position = 0;
			var buf = new byte[preamble.Length];
			_outputBuffer.Read(buf, 0, preamble.Length);

			Assert.Equal(preamble, buf);

			var output = GetOutputBufferAsString(true);

			Assert.Equal(_defaultOutput, output);
		}

		[Fact]
		public void Encoding_UTF16()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray);
			result.ContentEncoding = Encoding.Unicode;
			result.ExecuteResult(_controllerCtx);

			string output;
			_outputBuffer.Position = 0;
			using (var reader = new StreamReader(_outputBuffer, Encoding.Unicode))
				output = reader.ReadToEnd();

			Assert.Equal(_defaultOutput, output);
		}

		[Fact]
		public void With_Header()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray);
			result.HeaderItems = new[] {"Name", "Category", "Price", "Qty"};
			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();

			_defaultOutput = "Name,Category,Price,Qty" + Environment.NewLine + _defaultOutput;

			Assert.Equal(_defaultOutput, output);
		}

		[Fact]
		public void With_ContentType()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray, "application/csv");

			Assert.Equal("application/csv", result.ContentType);
		}

		[Fact]
		public void With_NonBuffered_Output()
		{
			var result = new CsvFileResult<TestProduct>(_products, ProductToStringArray, "application/csv");
			result.BufferOutput = false;
			result.ExecuteResult(_controllerCtx);

			Assert.False(_response.BufferOutput);
		}

		[Fact]
		public void Field_With_Leading_And_Trailing_Whitespace()
		{
			var products = new[] {new TestProduct(" tomato", "fruit & vegetable ", 10.50m, 1)};
			var result = new CsvFileResult<TestProduct>(products, ProductToStringArray);
			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();

			var expected = "\" tomato\",\"fruit & vegetable \",10.50,1" + Environment.NewLine;

			Assert.Equal(expected, output);
		}

		[Fact]
		public void Field_With_DoubleQuote()
		{
			var products = new[] { new TestProduct("tomato", "\"fruit\" & vegetable", 10.50m, 1) };
			var result = new CsvFileResult<TestProduct>(products, ProductToStringArray);
			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();
			
			var expected = "tomato,\"\"\"fruit\"\" & vegetable\",10.50,1" + Environment.NewLine;
			
			Assert.Equal(expected, output);
		}

		[Fact]
		public void Field_With_Delimiter()
		{
			var products = new[] { new TestProduct("tomato", "fruit, vegetable", 10.50m, 1) };
			var result = new CsvFileResult<TestProduct>(products, ProductToStringArray);

			result.ExecuteResult(_controllerCtx);

			var output = GetOutputBufferAsString();

			var expected = "tomato,\"fruit, vegetable\",10.50,1" + Environment.NewLine;

			Assert.Equal(expected, output);			
		}


        // Helper methods
        
        private void CreateProducts()
        {
            var products = new List<TestProduct>();

            products.Add(new TestProduct("tomato", "fruit & vegetable", 10.0m, 1));
            products.Add(new TestProduct("watermelon", "fruit", 5.50m, 2));
            products.Add(new TestProduct("yoghurt", "dairy", 20.15m, 2));
            products.Add(new TestProduct("banana", "fruit", 7.50m, 15));
            products.Add(new TestProduct("kiwi", "fruit", 9.25m, 10));
            products.Add(new TestProduct("cheese", "dairy", 30.15m, 12));
            products.Add(new TestProduct("lettuce", "vegetable", 11.0m, 16));
            products.Add(new TestProduct("pepper", "spice", 4.45m, 30));
            products.Add(new TestProduct("pineapple", "fruit", 11.75m, 21));
            products.Add(new TestProduct("cucumber", "vegetable", 20.85m, 40));
            products.Add(new TestProduct("paprika", "vegetable", 21.25m, 28));
            products.Add(new TestProduct("plum", "fruit", 14.0m, 15));
            products.Add(new TestProduct("potato", "vegetable", 7.50m, 100));
            products.Add(new TestProduct("zucchini", "vegetable", 15.20m, 28));
            products.Add(new TestProduct("spinach", "vegetable", 11.10m, 7));

            var k = products.Count;
            for (int i = 0; i < 1000; ++i)
            {
                var x = i % k;
                products.Add(products[x]);
            }

            _products = products;
        }

        private void SetupControllerContextMocks()
        {
            _outputBuffer = new MemoryStream();

            Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(x => x.OutputStream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback
                <byte[], int, int>((buf, offset, count) => _outputBuffer.Write(buf, offset, count));

            var controllerCtxMock = new Mock<ControllerContext>();
            controllerCtxMock.Setup(x => x.HttpContext.Response).Returns(responseMock.Object);

            _response = responseMock.Object;
            _controllerCtx = controllerCtxMock.Object;
        }

        private string GetOutputBufferAsString(bool noResetPosition = false)
        {
            string output;

            if (!noResetPosition)
                _outputBuffer.Position = 0;
            using (var reader = new StreamReader(_outputBuffer))
                output = reader.ReadToEnd();

            return output;
        }

        private void SetDefaultOutput()
        {
            var sb = new StringBuilder();
            foreach (var product in _products)
                sb.Append(String.Format("{0},{1},{2},{3}", product.Name, product.Category, product.Price, product.Qty) +
                          Environment.NewLine);

            _defaultOutput = sb.ToString();
        }

        private static string[] ProductToStringArray(TestProduct p)
        {
            return new[] { p.Name, p.Category, p.Price.ToString(), p.Qty.ToString() };
        }

	}

    /// <summary>
    /// Sample product (for test purposes)
    /// </summary>
    internal class TestProduct
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public int Qty { get; set; }

        public TestProduct(string name, string category, decimal price, int qty)
        {
            Name = name;
            Category = category;
            Price = price;
            Qty = qty;
        }
    }

}
