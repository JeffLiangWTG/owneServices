using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemTaxValidation))]
	sealed class AsycudaPackedItemTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAET_ChargeType()
		{
			var targetInfo = tax.AET_ChargeTypeInfo;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XX", ChargeTypeOtherList.Codes.CT);

			packedItem.AsycudaTaxes.AddNew().AET_ChargeType = ChargeTypeOtherList.Codes.AT;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			AssertNoMessageErrorContaining(targetInfo, "There is already another record for");

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.AT;
			AssertHasMessageErrorContaining(targetInfo, "There is already another record for");
		}

		public void TestCheckAET_Tariff()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			packedItem.API_Tariff = "87031000002";
			foreach (var chargeType in new[] { ChargeTypeOtherList.Codes.AT, ChargeTypeOtherList.Codes.CT, ChargeTypeOtherList.Codes.SS, ChargeTypeOtherList.Codes.TT })
			{
				AssertCheckAET_Tariff(chargeType, tax.AET_TariffInfo);
			}
		}

		void AssertCheckAET_Tariff(ZString chargeType, ZPropertyInfo targetInfo)
		{
			tax.AET_ChargeType = chargeType;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			tax.AET_Tariff = $"{chargeType}Tariff";
			AssertNoMessageErrors(tax.AET_TariffInfo);
			tax.AET_Tariff = $"{chargeType}DoesNotBelongToMainTariff";
			AssertHasMessageErrorContaining(tax.AET_TariffInfo, ValidationConstants.InvoiceLineTax.ChildTariffDoesNotBelongToMainTariff(tax.AET_Tariff, packedItem.API_Tariff));
			tax.AET_Tariff = "TESTSEDAN";
			AssertHasMessageErrorContaining(tax.AET_TariffInfo, ValidationConstants.InvoiceLineTax.TariffDoesNotBelongToType(tax.AET_Tariff, tax.AET_ChargeType));
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
			tax.Validation.ValidateAET_Tariff();
			AssertNoMessageErrors(tax.AET_TariffInfo);
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTS;
			tax.Validation.ValidateAET_Tariff();
			AssertNoMessageErrors(tax.AET_TariffInfo);
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			tax.Validation.ValidateAET_Tariff();
			AssertNoMessageErrors(tax.AET_TariffInfo);
		}

		public void TestCheckAET_BaseValue()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(tax.AET_BaseValueInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_CustomsValue = 2000m;
			packedItem = bill.PackedItems.AddNew();
			tax = packedItem.AsycudaTaxes.AddNew();
			tax.AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
		}

		AsycudaPackedItem packedItem;
		AsycudaPackedItemTax tax;
	}
}
