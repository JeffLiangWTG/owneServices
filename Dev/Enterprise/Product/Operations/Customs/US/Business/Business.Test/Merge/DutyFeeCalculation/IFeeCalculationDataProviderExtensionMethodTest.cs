using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FeeApplicableHelperTest : TestCaseWithFactory
	{
		public void TestIsFeeApplicableForCoffeeFee()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Coffee Fee Test";
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "Test Name", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_SchDEntry = "1234";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 2000m;
			invoiceLine1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff.UE_Tariff;
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var coffeeFee = invoiceLine1.FeeCusCodes.Cast<FeeCusCodeData>().FirstOrDefault(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertNotNull(coffeeFee);
			AssertEquals("Only X line has a PR Coffee Fee", 1000m, ZDecimal.Parse(coffeeFee.CY_Data));

			coffeeFee = invoiceLine2.FeeCusCodes.Cast<FeeCusCodeData>().FirstOrDefault(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertNull("V line have not a PR Coffee Fee ", coffeeFee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
