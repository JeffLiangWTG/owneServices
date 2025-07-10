using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostingDocumentDeclarationSupporter))]
	sealed class LandedCostingDocumentDeclarationSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals("business context of customs", BusinessContext.LandedCostHeader, Supporter.BusinessContext);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("DataContext.LandedCostHeader is Supported", true, Supporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.LandedCostHeader)));
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrappers = Supporter.GetDocumentWrappers(DataContext.LandedCostHeader, null);
			AssertEquals("1 wrapper", 1, wrappers.Length);
			DocumentWrapper wrapper = wrappers[0];
			AssertEquals("DocLandedCostHeader", "Enterprise.DocumentWrappers.DocLandedCostHeader", wrapper.GetType().FullName);
			AssertEquals("Resulting Wrapper.WrappedBusinessObject", landedCostHeader, wrapper.WrappedObject);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("CustomisationSecurityCheckpoint", Env.Security.CustomsDeclarationCustomiseDocument, Supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetDataStateBeforeRun()
		{
			DocumentZQuery filter = new DocumentZQuery("Customs", "Landed Costing");
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			DocumentSupporterDataState result = Supporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Is invalid as there are no LC lines", false, result.IsValid);

			landedCostHeader.Histories.AddNew();
			result = Supporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Is valid", true, result.IsValid);

			landedCostHeader.Histories.DeleteAll();
			filter = new DocumentZQuery("Customs", "Landed Costing - Old");
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			result = Supporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Is invalid as there are no LC lines", true, result.IsValid);
		}

		LandedCostHeader landedCostHeader;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			BusinessObject testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			landedCostHeader = Factory.New<LandedCostHeader>();
			landedCostHeader.LT_ParentID = testDec.PK;
			landedCostHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return landedCostHeader;
		}

		LandedCostingDocumentDeclarationSupporter supporter;
		LandedCostingDocumentDeclarationSupporter Supporter => supporter ?? (supporter = new LandedCostingDocumentDeclarationSupporter((LandedCostHeader)GetDocumentSupportableBusinessObject()));
	}
}
