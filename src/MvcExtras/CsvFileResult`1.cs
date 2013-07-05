using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MvcExtras
{
	/// <summary>
	/// CsvFileResult
	/// </summary>
	public class CsvFileResult<T> : FileResult where T : class
	{
		private const string DefaultContentType = "text/csv";
		private const char DefaultFieldDelimiter = ',';
		private const int BufferSize = 0x1000;
		private static readonly Encoding DefaultEncoding = Encoding.UTF8;
		private readonly IEnumerable<T> _dataItems;
		private readonly Func<T, IEnumerable<string>> _transformation;
		private bool _noMoreData;
		private IEnumerator<IEnumerable<string>> _lineEnumerator;
		private StringBuilder _buffer, _dataChunk;
		private string _fieldDelimiter;

		/// <summary>
		/// CsvFileResult
		/// </summary>
		/// <param name="dataItems">IEnumerable containing values that are to be transformed into lines of the CSV file.</param>
		/// <param name="transformation">Transformation function that takes in a single data item and transforms it into an IEnumerable of strings that match cells of a single line.</param>
		/// <param name="contentType">Content type (e.g. "text/csv").</param>
		public CsvFileResult(IEnumerable<T> dataItems, Func<T, IEnumerable<string>> transformation, string contentType)
			: base(contentType)
		{
			if (dataItems == null)
				throw new ArgumentNullException("dataItems");
			_dataItems = dataItems;

			if (transformation == null)
				throw new ArgumentNullException("transformation");
			_transformation = transformation;

			FieldDelimiter = DefaultFieldDelimiter;
			ContentEncoding = DefaultEncoding;
			NewLine = Environment.NewLine;
			BufferOutput = true;
		}

		/// <summary>
		/// CsvFileResult
		/// </summary>
		/// <param name="dataItems">IEnumerable containing values that are to be transformed into lines of the CSV file.</param>
		/// <param name="transformation">Transformation function that takes in a single data item and transforms it into an IEnumerable of strings that match cells of a single line.</param>
		public CsvFileResult(IEnumerable<T> dataItems, Func<T, IEnumerable<string>> transformation)
			: this(dataItems, transformation, DefaultContentType)
		{
		}

		/// <summary>
		/// Content Encoding (default is UTF8).
		/// </summary>
		public Encoding ContentEncoding { get; set; }

		/// <summary>
		/// CSV field delimiter (default is comma).
		/// </summary>
		public char FieldDelimiter { get; set; }

		/// <summary>
		/// Newline delimiter (default is Environment.NewLine)
		/// </summary>
		public string NewLine { get; set; }

		/// <summary>
		/// IEnumerable containing CSV header, i.e. the first line of the CSV file. If set to null, header will not be included in the output.
		/// </summary>
		public IEnumerable<string> HeaderItems { get; set; }

		/// <summary>
		/// Indicates whether to buffer output (default is true)
		/// </summary>
		public bool BufferOutput { get; set; }

		/// <summary>
		/// If set, preamble/byte order mark (BOM) for the encoding used will be output as the first bytes of the output stream (default is false).
		/// </summary>
		public bool IncludePreamble { get; set; }

		protected override void WriteFile(System.Web.HttpResponseBase response)
		{
			response.ContentEncoding = ContentEncoding;
			response.BufferOutput = BufferOutput;
			
			var outputStream = response.OutputStream;			

			var preambleSet = !IncludePreamble ? true : false;

			StringBuilder data;

			InitCsvDataReader();

			while ((data = ReadCsvData()).Length != 0) {
				if (!preambleSet) {
					var preamble = ContentEncoding.GetPreamble();

					if (preamble.Length > 0)
						outputStream.Write(preamble, 0, preamble.Length);

					preambleSet = true;
				}

				var buf = ContentEncoding.GetBytes(data.ToString());
				outputStream.Write(buf, 0, buf.Length);
			}
		}

		private void InitCsvDataReader()
		{
			_buffer = new StringBuilder();
			_dataChunk = new StringBuilder();
			_fieldDelimiter = FieldDelimiter.ToString();
		}

		private StringBuilder ReadCsvData()
		{
			_dataChunk.Clear();

			IEnumerable<string> cells = null;

			while (true) {
				var overflow = _buffer.Length - BufferSize;

				if (overflow >= 0 || _noMoreData) {
					var len = overflow > 0 ? _buffer.Length - overflow : _buffer.Length;
					_dataChunk.Append(_buffer.ToString(0, len));
					_buffer.Remove(0, len);

					break;
				}

				if (_lineEnumerator == null)
					_lineEnumerator = GetLines().GetEnumerator();

				if (!_lineEnumerator.MoveNext())
					_noMoreData = true;
				else
					cells = _lineEnumerator.Current;

				if (_noMoreData)
					continue;

				var cellArray = cells.Select(ToCsvString).ToArray();
				var line = String.Join(_fieldDelimiter, cellArray) + NewLine;

				_buffer.Append(line);
			}

			return _dataChunk;
		}

		private IEnumerable<IEnumerable<string>> GetLines()
		{
			if (HeaderItems != null)
				yield return HeaderItems;

			foreach (var dataItem in _dataItems)
				yield return _transformation(dataItem);
		}

		private string ToCsvString(string arg)
		{
			const string enclosedInDoubleQuotesFormatString = "\"{0}\"";

			if (String.IsNullOrEmpty(arg))
				return String.Empty;

			var len = arg.Length;

			bool encloseInDoubleQuotes, escapeQuotes;
			escapeQuotes = encloseInDoubleQuotes = false;

			for (var i = 0; i < len; ++i) {
				if (!encloseInDoubleQuotes &&
					(arg[i] == '\n' || arg[i] == FieldDelimiter || ((i == 0 || i == (len - 1)) && Char.IsWhiteSpace(arg[i]))))
					encloseInDoubleQuotes = true;
				if (arg[i] == '"') {
					escapeQuotes = encloseInDoubleQuotes = true;
					break;
				}
			}

			if (escapeQuotes)
				arg = arg.Replace("\"", "\"\"");

			return encloseInDoubleQuotes ? String.Format(enclosedInDoubleQuotesFormatString, arg) : arg;
		}

	}
}
