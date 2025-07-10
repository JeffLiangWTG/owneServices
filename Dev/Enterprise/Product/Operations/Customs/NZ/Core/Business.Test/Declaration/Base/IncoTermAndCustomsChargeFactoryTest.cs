namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var allIncoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			CombineAssertions(() =>
			{
				AssertEquals("Length", 13, allIncoTerms.Length);
				AssertCollectionContains(IncoTermList.Codes.CarriagePaidTo, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.CostAndFreight, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.CostInsuranceAndFreight, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.DeliveredDutyPaid, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.ExWorks, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.FreeAlongsideShip, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.FreeOnBoard, allIncoTerms);
				AssertCollectionContains(IncoTermAndCustomsChargeFactory.ErrorIncoTermCode, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.CarriageAndInsurancePaidTo, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.FreeCarrier, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.DeliveredAtPlace, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.DeliveredAtTerminal, allIncoTerms);
				AssertCollectionContains(IncoTermList.Codes.DeliveredAtPlaceUnloaded, allIncoTerms);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\Declaration\Base\JobComInvoiceHeaderCharge\TestFile\IncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext()
		{
			return Core.Constants.CountryCodes.NewZealand;
		}
	}
}
