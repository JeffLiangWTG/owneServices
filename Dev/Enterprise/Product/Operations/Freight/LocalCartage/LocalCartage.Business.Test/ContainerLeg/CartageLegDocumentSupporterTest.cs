using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageLegDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestSupportedDataContext()
		{
			AssertEquals(true, iCartageLeg.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CommonCartageLeg)));
			AssertEquals(true, iCartageLeg.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ContainerLeg)));
			AssertEquals(true, iCartageLeg.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CartageAdvice)));
			AssertEquals(true, iCartageLeg.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.DocumentDailyWorkSheet)));
			AssertEquals(true, iCartageLeg.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.TransportCustomiseDocuments, iCartageLeg.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.ContainerLeg, iCartageLeg.DocumentSupporter.BusinessContext);
		}

		public void TestGetDocumentWrappersInternal()
		{
			var wrappers = cartageLeg.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			CombineAssertions(() =>
			{
				AssertEquals("Should generate 1 wrapper", 1, wrappers.Length);
				AssertNotEquals("Wrapper should not be for cartage leg", cartageLeg.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
				AssertEquals("Wrapper should be for cartage itself", cartageLeg.Cartage.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
			});
		}

		public void TestGetDocumentWrappersInternal_DataContextGenericFreightJob_NoCartage_DoesNotWrapObject()
		{
			var cartageLeg = Factory.New<CommonCartageLeg>();
			AssertNull("Precondition", cartageLeg.Cartage);
			var wrappers = cartageLeg.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertNull("Should not generate wrappers", wrappers);
		}

		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			transportProvider.OH_FullName = "ABC Transport";
			Factory.Save();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = transportProvider.PK;
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			iCartageLeg = cartageLeg;
		}

		CommonCartageLeg cartageLeg;
		IDocumentSupportable iCartageLeg;
	}
}
