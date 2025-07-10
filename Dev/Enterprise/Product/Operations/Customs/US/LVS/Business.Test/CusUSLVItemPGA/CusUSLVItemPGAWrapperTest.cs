using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemPGAWrapper))]
	public class CusUSLVItemPGAWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDisclaimReason()
		{
			AssertHasCustomAttribute<ListAttribute>(Wrapper.GetType(), nameof(Wrapper.DisclaimReason), false, a => a.ListDataSourceMember == nameof(Wrapper.PGADisclaimReasonList));
			AssertHasCustomAttribute<MaxLengthAttribute>(Wrapper.GetType(), nameof(Wrapper.DisclaimReason), false, a => a.MaxLength == 1);

			Wrapper.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoExceptionThrown(() => Wrapper.DisclaimReason = "T");

			Wrapper.Indicator = OGAIndicatorList.Codes.Declared;
			AssertEquals("", Wrapper.DisclaimReason);
		}

		public void TestIndicator()
		{
			AssertHasCustomAttribute<ListAttribute>(Wrapper.GetType(), nameof(Wrapper.Indicator), false, a => a.ListDataSourceMember == nameof(OGAIndicatorList));
			AssertHasCustomAttribute<MaxLengthAttribute>(Wrapper.GetType(), nameof(Wrapper.Indicator), false, a => a.MaxLength == 1);
			AssertEquals("", Wrapper.Indicator);

			AssertEquals(0, Item.CusUSLVItemPGAs.Count);
			Wrapper.Indicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(1, Item.CusUSLVItemPGAs.Count);

			AssertNoExceptionThrown(() => Wrapper.Indicator = "");
			AssertEquals(0, Item.CusUSLVItemPGAs.Count);
		}

		public void TestValidateDisclaimReason()
		{
			Wrapper.Indicator = OGAIndicatorList.Codes.Disclaimed;
			Wrapper.DisclaimReason = "T";
			AssertEquals(1, Item.CusUSLVItemPGAs.Count);
			var pga = Item.CusUSLVItemPGAs.Single() as CusUSLVItemPGA;

			pga.ULP_DisclaimReasonInfo.AddError("Coffee is cold!");
			Wrapper.ValidateDisclaimReason();

			AssertHasError(Wrapper.DisclaimReasonInfo, "Coffee is cold!");

			Wrapper.DisclaimReason = ZString.Empty;
			AssertHasMessageError(Wrapper.DisclaimReasonInfo, "Agency Program declarations are not supported in this module and will need to be completed using a stand alone declaration.");
			ErrorReporter.Clear();
		}

		public void TestValidateDisclaimReasonAPHIS()
		{
			var tariff = CreateTariffWithPGACodes("12345678", "AQX");
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance.ULH_MasterBill = "MASTERBILL";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "HOUSEBILL";
			var itemLine = consignment.CusUSLVItems.AddNew();
			itemLine.ULI_Tariff = tariff.UE_Tariff;
			var aphis = itemLine.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().FirstOrDefault(x => x.AgencyCode == "APHIS");
			AssertHasWarning(aphis.DisclaimReasonInfo, CusUSLVItemPGAWrapper.APHISDataMayBeRequired);

			aphis.DisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoWarning(aphis.DisclaimReasonInfo, CusUSLVItemPGAWrapper.APHISDataMayBeRequired);
		}

		public void TestListValidationOnDisclaimReason()
		{
			var tariff = CreateTariffWithPGACodes("12345678", "FD4NM2");
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportTypeList.Codes.Sea;
			clearance.ULH_MasterBill = "MASTERBILL";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "HOUSEBILL";
			var itemLine = consignment.CusUSLVItems.AddNew();
			itemLine.ULI_Tariff = tariff.UE_Tariff;
			var fda = itemLine.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().FirstOrDefault(x => x.Agency == "FDA");
			fda.DisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(fda.DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);

			fda.DisclaimReason = "E";
			fda.Indicator = "C";
			AssertHasMessageError(fda.DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedItemLine = newFactory.Load<CusUSLVItem>(itemLine.PK);
			var loadFDA = loadedItemLine.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().FirstOrDefault(x => x.Agency == "FDA");
			loadFDA.DisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(loadFDA.DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
		}

		USCTariff CreateTariffWithPGACodes(ZString tariff, ZString pgaCodes)
		{
			var result = Factory.New<USCTariff>();
			result.UE_Tariff = tariff;
			result.UE_PGACodes = pgaCodes;
			result.UE_DateFrom = new ZDateTime(2008, 1, 1);
			result.UE_DateTo = ZDateTime.Today.AddDays(1);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Wrapper;
		}

		CusUSLVItemPGAWrapper Wrapper => _wrapper ?? (_wrapper = new CusUSLVItemPGAWrapper(Factory, new CusUSLVItemPGAAgencyRequirementsProvider(Item), "TST"));
		CusUSLVItemPGAWrapper _wrapper;

		CusUSLVItem Item => _item ?? (_item = Factory.New<CusUSLVItem>());
		CusUSLVItem _item;
	}
}
