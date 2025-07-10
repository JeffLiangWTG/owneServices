using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	public class CsvActionResult : IHttpActionResult
	{
		HttpStatusCode statusCode { get; set; }
		DataTable data { get; set; }
		string csvData;
		const string singleQuotationMark = "\"";
		const string doubleQuotationMarks = "\"\"";
		const string valueSeparator = ",";

		string CsvData
		{
			get
			{
				if (string.IsNullOrEmpty(csvData))
				{
					csvData = Csv(data);
				}
				return csvData;
			}
		}

		public CsvActionResult(HttpStatusCode statusCode, DataTable data)
		{
			this.statusCode = statusCode;
			this.data = data;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			HttpResponseMessage response = new HttpResponseMessage(statusCode);
			response.Content = new StringContent(CsvData);
			return Task.FromResult(response);
		}

		string Csv(DataTable data)
		{
			StringBuilder sb = new StringBuilder();
			IEnumerable<string> columnNames = data.Columns.Cast<DataColumn>().
											  Select(column => column.ColumnName);
			sb.AppendLine(string.Join(valueSeparator, columnNames));

			foreach (DataRow row in data.Rows)
			{
				var fields = row.ItemArray.Select(field =>
				{
					if (field is null)
					{
						return QuoteValue(string.Empty);
					}

					string result;
					if (field is byte[] bytes)
					{
						result = (NoResString)"0x" + BitConverter.ToString(bytes).Replace((NoResString)"-", (NoResString)"");
					}
					else
					{
						result = field.ToString();
					}
					return QuoteValue(result);
				});

				sb.AppendLine(string.Join(valueSeparator, fields));
			}

			return sb.ToString();
		}

		string QuoteValue(string value)
		{
			return string.Concat(singleQuotationMark, value.Replace(singleQuotationMark, doubleQuotationMarks), singleQuotationMark);
		}
	}
}
