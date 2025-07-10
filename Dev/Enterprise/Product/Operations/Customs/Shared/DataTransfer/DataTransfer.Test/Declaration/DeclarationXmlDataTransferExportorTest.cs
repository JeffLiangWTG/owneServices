using System.Collections;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class DeclarationXmlDataTransferExportorTest : TestCaseWithFactory
	{
		public void TestExport()
		{
			using (var file = TempFile.New())
			{
				var jobDec = SetupDeclarationForExport();
				Factory.Save();

				var notify = new NotificationBuffer();

				using (Stream writer = new FileStream(file.Filename, FileMode.Append))
				{
					var jobDecs = new[] { jobDec };
					exporter.DoExport(writer, jobDecs, notify);
				}
				AssertEquals("No errors should occur on export", false, notify.HasErrors);

				jobDec.Delete();
				Factory.Save();
			}
		}

		TestDeclarationXmlDataTransferExporter exporter;

		protected override void SetUp()
		{
			base.SetUp();
			exporter = new TestDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter.New(), false);
		}

		BaseJobDeclaration SetupDeclarationForExport()
		{
			var jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.FillWithValidTestData();
			jobDec.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			jobDec.Importer.OH_FullName = "Importer Name";
			jobDec.JE_RL_NKOrigin = "AUMEL";
			jobDec.JE_RL_NKFinalDestination = "SGSIN";

			return jobDec;
		}

		sealed class TestDeclarationXmlDataTransferExporter : DeclarationXmlDataTransferExporter
		{
			public TestDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public new void DoExport(Stream file, IList selectedBusinessObjects, INotifications notify)
			{
				base.DoExport(file, selectedBusinessObjects, notify);
			}
		}
	}
}
