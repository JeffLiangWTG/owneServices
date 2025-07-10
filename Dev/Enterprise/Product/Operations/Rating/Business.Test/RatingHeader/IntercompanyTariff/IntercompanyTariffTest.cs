using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class IntercompanyTariffTest : RatingTestCase
	{
		public void TestNewIntercompanyTariffMustBeGlobal()
		{
			var newIntercompanyTariff = Factory.NewWithValidTestData<IntercompanyTariff>();

			AssertEquals(ZGuid.Empty, newIntercompanyTariff.TH_GC);
		}

		public void TestIntercompanyTariffIs()
		{
			var client = Helper.NewOrgHeader();
			var intercompanyTariff = Helper.NewIntercompanyTariff(client);

			AssertEquals(true, intercompanyTariff.IsIntercompanyTariff());

			AssertEquals(false, intercompanyTariff.IsClientRate());
			AssertEquals(false, intercompanyTariff.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(false, intercompanyTariff.IsTariff());
			AssertEquals(false, intercompanyTariff.IsLevelOneTariff());
			AssertEquals(false, intercompanyTariff.IsAdditionalTariff());

			AssertEquals(false, intercompanyTariff.IsQuote());

			AssertEquals(false, intercompanyTariff.IsCosting());
			AssertEquals(false, intercompanyTariff.IsWiseCostRate());
			AssertEquals(false, intercompanyTariff.IsStandardCostRate());
		}

		public void TestIntercompanyTariffWithoutClient()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(null);

			AssertEquals("Should still be considered an intercompany tariff even if there hasn't been a client set yet", true, intercompanyTariff.IsIntercompanyTariff());

			AssertEquals(true, intercompanyTariff.IsIntercompanyTariff());

			AssertEquals(false, intercompanyTariff.IsClientRate());
			AssertEquals(false, intercompanyTariff.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(false, intercompanyTariff.IsTariff());
			AssertEquals(false, intercompanyTariff.IsLevelOneTariff());
			AssertEquals(false, intercompanyTariff.IsAdditionalTariff());

			AssertEquals(false, intercompanyTariff.IsQuote());

			AssertEquals(false, intercompanyTariff.IsCosting());
			AssertEquals(false, intercompanyTariff.IsWiseCostRate());
			AssertEquals(false, intercompanyTariff.IsStandardCostRate());
		}
	}

	#region Business Object TestCase

	[TestedType(typeof(IntercompanyTariff))]
	public class IntercompanyTariffBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var rate = Factory.New<IntercompanyTariff>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			return rate;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rate = factory.New<IntercompanyTariff>();
			rate.TH_OH = factory.NewWithValidTestData<OrgHeader>().PK;

			return rate;
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory) =>
			factory.LoadTop1<IntercompanyTariff>(new ZQuery());
	}

	#endregion
}
