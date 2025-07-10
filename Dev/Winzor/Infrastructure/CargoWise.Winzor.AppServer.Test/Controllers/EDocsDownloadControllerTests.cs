using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.Winzor.Architecture.Test;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test.Controllers
{
	[TestFixture]
	public class EDocsDownloadControllerTests
	{
		[Test]
		public async Task TestEDocsDownloadAsync()
		{
			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);

			var data = "Test txt";
			var fileName = "test.txt";
			var dataArray = Encoding.UTF32.GetBytes(data);

			var bizoFactory = new BusinessObjectFactory();
			var workItem = bizoFactory.New<WorkItem>();
			var bizPK = workItem.PK.ToString();
			var eDoc = workItem.DocManagerInfo.AddFileOrDocument(dataArray, $"{fileName}", "UAT");
			var docPK = eDoc.UniqueKey.ToString();

			workItem.DocManagerInfo.Save();
			bizoFactory.Save();

			using var client = new HttpClient();
			var response = await client.GetAsync($"{ctx.ServerBaseUrl}/edoc/download/{fileName}?docPK={docPK}&bizPK={bizPK}");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			var contentHeader = response.Content.Headers;
			Assert.That(contentHeader.ContentType.MediaType, Is.EqualTo("application/octet-stream"));
			Assert.That(contentHeader.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
			Assert.That(contentHeader.ContentDisposition.FileName, Is.EqualTo(fileName));

			var bytes = await response.Content.ReadAsByteArrayAsync();
			Assert.That(Encoding.UTF32.GetString(bytes), Is.EqualTo(data));

			workItem.Delete();
			bizoFactory.Save();
		}
	}
}
