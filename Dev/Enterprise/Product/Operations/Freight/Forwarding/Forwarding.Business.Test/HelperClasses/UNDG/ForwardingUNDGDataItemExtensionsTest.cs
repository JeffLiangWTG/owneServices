using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGDataItemExtensionsTest : TestCaseWithFactory
	{
		public void TestGetPSNWithAdditionalTextIfSupportForNOS()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew() as ForwardingUNDGDataItem;

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "2395";
			substance.DG_Code = "2395";
			substance.DG_PSN = "Isobutyryl chloride";
			dataItem.DI_DG = substance.PK;
			AssertEquals("Isobutyryl chloride", dataItem.GetPSNWithAdditionalTextIfSupportForNOS());

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "0190";
			substance.DG_Code = "0190";
			substance.DG_PSN = "Samples, explosive";
			var attribute = substance.QualifyingDescriptiveTexts.AddNew();
			attribute.DA_Descriptor = "other than initiating explosives";
			attribute.DA_Language = "EN";
			dataItem.DI_DG = substance.PK;
			AssertEquals("Samples, explosive", dataItem.GetPSNWithAdditionalTextIfSupportForNOS());

			dataItem.DI_IsNotOtherwiseSpecified = true;
			AssertEquals("Samples, explosive, n.o.s., other than initiating explosives", dataItem.GetPSNWithAdditionalTextIfSupportForNOS());
			substance.QualifyingDescriptiveTexts.DeleteAll();
			AssertEquals("Samples, explosive, n.o.s.", dataItem.GetPSNWithAdditionalTextIfSupportForNOS());

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "3535";
			substance.DG_Code = "3535a";
			substance.DG_PSN = "Toxic solid, flammable, inorganic";
			attribute = substance.QualifyingDescriptiveTexts.AddNew();
			attribute.DA_Descriptor = "A5";
			attribute.DA_Language = "EN";
			dataItem.DI_DG = substance.PK;
			AssertEquals("Toxic solid, flammable, inorganic", dataItem.GetPSNWithAdditionalTextIfSupportForNOS());

			dataItem.DI_IsNotOtherwiseSpecified = true;
			AssertEquals("Toxic solid, flammable, inorganic, n.o.s.", dataItem.GetPSNWithAdditionalTextIfSupportForNOS());
		}

		public void TestIsRadioactiveInExceptedQuantities()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance.DG_Class = "7";
			substance.DG_Variant = "a";
			substance.DG_UNNO = "2911";
			AssertEquals(true, substance.IsRadioactiveInExceptedQuantities());

			substance.DG_Standard = UNDGSubstanceStandardTypes.IMO;
			substance.DG_Class = "7";
			substance.DG_Variant = "a";
			substance.DG_UNNO = "2911";
			AssertEquals("Standard not matched", false, substance.IsRadioactiveInExceptedQuantities());

			substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance.DG_Class = "1";
			substance.DG_Variant = "a";
			substance.DG_UNNO = "2911";
			AssertEquals("Class not matched", false, substance.IsRadioactiveInExceptedQuantities());

			substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance.DG_Class = "7";
			substance.DG_Variant = "c";
			substance.DG_UNNO = "2911";
			AssertEquals("Variant not matched", false, substance.IsRadioactiveInExceptedQuantities());

			substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance.DG_Class = "7";
			substance.DG_Variant = "a";
			substance.DG_UNNO = "123";
			AssertEquals("UNNO not matched", false, substance.IsRadioactiveInExceptedQuantities());

			substance = null;
			AssertEquals("Substance not specified", false, substance.IsRadioactiveInExceptedQuantities());
		}
	}
}
