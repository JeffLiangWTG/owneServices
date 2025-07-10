using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackageDocumentSupporter))]
	class HVLVOuterPackageDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.HVLVOuterPackage, supporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.HVLVBookingHeaderCustomiseDocuments, supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(1, supporter.ListOfSupportedDataContexts.Count);
		}

		public void TestGetDocumentWrappers()
		{
			var outerPackage = Factory.New<HVLVOuterPackage>();
			outerPackage.Items.AddNew();
			outerPackage.Items.AddNew();
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Blahblah";

			var supporter = outerPackage.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals("FreightWrapperFromHVLVOuterPackage", wrappers[0].GetType().Name);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<HVLVOuterPackage>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			supporter = GetDocumentSupportableBusinessObject().DocumentSupporter;
		}

		DocumentSupporter supporter;

		#endregion
	}
}
