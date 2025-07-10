using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestLines))]
	sealed class ExportCustomsManifestLinesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetTopBusinessObject()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			var line = Factory.New<ExportCustomsManifestLines>();
			line.EL_ED = header.PK;
			var controllerFactory = System.Reflection.Assembly.Load("Enterprise.ZArchitecture.GUI").GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory").GetField("Instance").GetValue(null);
			var controller = controllerFactory.GetType().GetMethod("GetControllerForBizo").Invoke(controllerFactory, new[] { line.GetTopBusinessObject() });
			AssertNull("No controller for base class", controller);
		}

		public void TestPopulatesSendersReferenceIfNeededOnSave()
		{
			ZString expectedReference = Env.NumberFountains.ManifestJobLineNo.PeekPreliminaryFormatted(Factory);
			AssertEquals(ZString.Empty, line.EL_UserReferenceNum);
			Factory.Save();
			AssertEquals(expectedReference, line.EL_UserReferenceNum);
		}

		public void TestSendersReferenceNotPopulatedIfSaveFails()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			var testLine = Factory.New<TestExportCustomsManifestLines>();
			testLine.EL_ED = header.PK;
			testLine.ShouldThrowExceptionOnSaving = true;
			try
			{
				Factory.Save();
			}
			catch
			{
			}
			AssertEquals("Reference should be empty", ZString.Empty, testLine.EL_UserReferenceNum);
			testLine.ShouldThrowExceptionOnSaving = false;
			Factory.Save();
			AssertNotEquals("Reference should NOT be empty", ZString.Empty, testLine.EL_UserReferenceNum);
		}

		public void TestISendersMessageReferenceProviderSendersReference()
		{
			line.EL_UserReferenceNum = "foo";
			AssertEquals("foo", ((ISendersMessageReferenceProvider)line).SendersReference);
		}

		#region Lines

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ExportCustomsManifestHeader header = factory.New<ExportCustomsManifestHeader>();
			return header.Lines.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			line = Factory.New<ExportCustomsManifestHeader>().Lines.AddNew();
		}

		ExportCustomsManifestLines line;

		#endregion
	}
}
