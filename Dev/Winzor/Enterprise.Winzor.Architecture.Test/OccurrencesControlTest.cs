using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.IssueManager.Business.Test;
using Enterprise.Client.EDI.IssueManager.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class OccurrencesControlTest
{
	EmbeddedResourceRetriever resourceRetriever;
	string resourcesFolder;

	[SetUp]
	protected void SetUp()
	{
		resourceRetriever = new EmbeddedResourceRetriever(typeof(TestCaseWithXmlDoc).Assembly);
		resourcesFolder = resourceRetriever.SaveAllResourcesToFiles();
	}

	[TearDown]
	protected void TearDown()
	{
		resourceRetriever.Dispose();
	}

	string GetTestFilePath(string fileName) => Path.Combine(resourcesFolder, "ZClientEDI.Business.Test.IssueManager.HelpErrorLog.Testing." + fileName);

	string SampleXmlFile => GetTestFilePath("SampleSimpleExceptionReport.xml");

	string SampleHtmlResult => GetTestFilePath("SampleSimpleExceptionReport.html");

	[Test, WithPlaywrightPage]
	public async Task TestShowCurrentOccurrence()
	{
		try
		{
			await using var ctx = new InMemoryAppServerTestContext();
			var page = await ctx.LoadFormAsync(() =>
			{
				ctx.Using(ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI));
				var factory = new BusinessObjectFactory();
				var xmlData = File.ReadAllText(SampleXmlFile);
				var log = factory.New<EdiHelpErrorLog>();
				var occurrence1 = log.Occurrences.AddNew();
				occurrence1.HO_XMLData = xmlData;

				var form = new Form() { Width = 1000, Height = 1000 };

				var oc = new OccurrencesControl();
				oc.Dock = DockStyle.Fill;
				form.Controls.Add(oc);
				oc.SetDataBinding(log, "");
				return form;
			});

			var sampleHtml = File.ReadAllText(SampleHtmlResult);
			Assert.That(async () => await page.Locator(".richtextbox__data").InnerHTMLAsync() , Is.EqualTo(sampleHtml).After(5000, 100));
		}
		finally
		{
			var tempPathInfo = new DirectoryInfo(Env.TempPath);
			foreach (var file in tempPathInfo.GetFiles())
			{
				file.Delete();
			}

			foreach (var dir in tempPathInfo.GetDirectories())
			{
				dir.Delete(true);
			}
		}
	}
}
