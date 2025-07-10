using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Testing
{
	class JsonActionResultTest : TestCase
	{
		public void TestDataTableGetsConvertedToJson()
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

			var expectedJSon = Newtonsoft.Json.JsonConvert.SerializeObject(d);

			Task.Run(() =>
			{
				var cancellationToken = new CancellationToken(false);
				var jsonActionResult = new JsonActionResult(HttpStatusCode.OK, d);
				var responseTask = jsonActionResult.ExecuteAsync(cancellationToken);
				var response = responseTask.Result;
				AssertEquals(true, response.Content.Headers.ContentEncoding.Contains("gzip"));
				AssertEquals(true, response.Content.Headers.ContentType.MediaType == "application/json");
				var byteStream = response.Content.ReadAsStreamAsync();
				using (var m = new MemoryStream())
				{
					var deflated = new GZipStream(byteStream.Result, CompressionMode.Decompress, leaveOpen: true);
					AssertEquals(expectedJSon, new StreamReader(deflated).ReadToEnd());
				}
			}).Wait();
		}
	}
}
