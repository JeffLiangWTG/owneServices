using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Testing
{
	class CsvActionResultTest : TestCase
	{
		public void TestDataTableGetsConvertedToCsv()
		{
			DataTable d = new DataTable("DummyTable");
			d.Columns.Add("ColA");
			d.Columns.Add("ColB");
			d.Columns.Add("ColC");

			for (int i = 0; i < 100; i++)
			{
				d.Rows.Add($"Row{i}Col1", $"Row{i}Col2", $"Row{i}Col3");
			}

			AssertEquals(d.Columns.Count, 3);
			AssertEquals(d.Rows.Count, 100);

			StringBuilder expectedCSV = new StringBuilder();
			expectedCSV.AppendLine("ColA,ColB,ColC");
			for (int i = 0; i < 100; i++)
			{
				expectedCSV.AppendLine($"\"Row{i}Col1\",\"Row{i}Col2\",\"Row{i}Col3\"");
			}

			Task.Run(() =>
			{
				var cancellationToken = new CancellationToken(false);
				var csvActionResult = new CsvActionResult(HttpStatusCode.OK, d);
				var responseTask = csvActionResult.ExecuteAsync(cancellationToken);
				var response = responseTask.Result;
				var byteStream = response.Content.ReadAsStreamAsync();
				AssertEquals(expectedCSV.ToString(), new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestDataTableWithQuotationsGetsConvertedToCsv()
		{
			DataTable d = new DataTable("DummyTable");
			d.Columns.Add("ColA");
			d.Columns.Add("ColB");
			d.Columns.Add("ColC");

			for (int i = 0; i < 100; i++)
			{
				d.Rows.Add($"\"Row{i}\"Col1\"", $"\"Row{i}\"Col2\"", $"\"Row{i}\"Col3\"");
			}

			AssertEquals(d.Columns.Count, 3);
			AssertEquals(d.Rows.Count, 100);

			StringBuilder expectedCSV = new StringBuilder();
			expectedCSV.AppendLine("ColA,ColB,ColC");
			for (int i = 0; i < 100; i++)
			{
				expectedCSV.AppendLine($"\"\"\"Row{i}\"\"Col1\"\"\",\"\"\"Row{i}\"\"Col2\"\"\",\"\"\"Row{i}\"\"Col3\"\"\"");
			}

			Task.Run(() =>
			{
				var cancellationToken = new CancellationToken(false);
				var csvActionResult = new CsvActionResult(HttpStatusCode.OK, d);
				var response = csvActionResult.ExecuteAsync(cancellationToken);
				var byteStream = response.Result.Content.ReadAsStreamAsync();
				AssertEquals(expectedCSV.ToString(), new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestDataTableWithNoRowDataGetsConvertedToCsv()
		{
			DataTable d = new DataTable("DummyTable");
			d.Columns.Add("ColA");
			d.Columns.Add("ColB");
			d.Columns.Add("ColC");

			AssertEquals(d.Columns.Count, 3);
			AssertEquals(d.Rows.Count, 0);

			StringBuilder expectedCSV = new StringBuilder();
			expectedCSV.AppendLine("ColA,ColB,ColC");

			Task.Run(() =>
			{
				var cancellationToken = new CancellationToken(false);
				var csvActionResult = new CsvActionResult(HttpStatusCode.OK, d);
				var response = csvActionResult.ExecuteAsync(cancellationToken);
				var byteStream = response.Result.Content.ReadAsStreamAsync();
				AssertEquals(expectedCSV.ToString(), new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}
	}
}
