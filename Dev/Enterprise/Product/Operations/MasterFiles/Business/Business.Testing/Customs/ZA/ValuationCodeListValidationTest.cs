using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Customs.ZA.Testing
{
	sealed class ValuationCodeListValidationTest : TestCaseWithFactory
	{
		public void TestValidateIndicator()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_IsConsignee = true;
			var buyerSupplier = Factory.New<OrgHeader>();
			buyerSupplier.OH_RL_NKClosestPort = "AUSYD";

			var link = organisation.SupplierLinks.AddNew();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;

			link.OL_RelatedParty = RelatedIndicatorList.Codes.Yes;
			link.OL_ValuationBasis = ZString.Empty;
			AssertHasMessageErrorContaining(link.OL_ValuationBasisInfo, ValuationCodeListValidation.ValuationCodeRequired);
			link.OL_ValuationBasis = ValuationCodeList.Codes.Section1;
			AssertNoNotifications(link.OL_ValuationBasisInfo);

			link.OL_RelatedParty = RelatedIndicatorList.Codes.No;
			link.OL_ValuationBasis = ValuationCodeList.Codes.Section1;
			AssertNoNotifications(link.OL_ValuationBasisInfo);
			link.OL_ValuationBasis = ZString.Empty;
			AssertHasMessageErrorContaining(link.OL_ValuationBasisInfo, ValuationCodeListValidation.ValuationCodeRequired);

			link.OL_RelatedParty = RelatedIndicatorList.Codes.Exempt;
			link.OL_ValuationBasis = ValuationCodeList.Codes.Section1;
			AssertHasMessageErrorContaining(link.OL_ValuationBasisInfo, ValuationCodeListValidation.ValuationCodeIsNowAllowed);
		}
	}
}
