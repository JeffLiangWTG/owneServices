using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGPrefixHelperTest : TestCaseWithFactory
	{
		public void TestGetUnnoPrefix()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "8000";

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			AssertEquals("When unno is 8000, prefix should be ID.", "ID", undgDataItem.GetUnnoPrefix());

			undgSubstance.DG_UNNO = "8001";
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			AssertEquals("When unno is 8001, prefix should be ID.", "ID", undgDataItem.GetUnnoPrefix());

			undgSubstance.DG_UNNO = "6969";
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			AssertEquals("When unno is not 8000 or 8001, prefix should be UN.", "UN", undgDataItem.GetUnnoPrefix());
		}

		public void TestGetUnnoPrefix_ReturnsCFRPrefixIfApplicable()
		{
			var undgSubstance = Factory.New<UNDGSubstanceCFR>();
			undgSubstance.CFR_UNNO = "8000";
			undgSubstance.CFR_Variant = "00";
			undgSubstance.CFR_Prefix = "ZZ";

			Factory.Save();

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.LinkDefault(undgSubstance);

			AssertEquals("Prefix should use CFR_Prefix if CFR substance", "ZZ", undgDataItem.GetUnnoPrefix());
		}

		public void TestGetUnnoPrefix_DefaultsUNForCFRPrefix()
		{
			var undgSubstance = Factory.New<UNDGSubstanceCFR>();
			undgSubstance.CFR_UNNO = "8000";
			undgSubstance.CFR_Variant = "00";
			undgSubstance.CFR_Prefix = ZString.Empty;

			Factory.Save();

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.LinkDefault(undgSubstance);

			AssertEquals("Prefix should use default UN if no prefix on CFR Substance", "UN", undgDataItem.GetUnnoPrefix());
		}

		public void TestGetUnnoPrefix_DefaultsUNWhenSubstanceIsNull()
		{
			var undgDataItem = Factory.New<UNDGDataItem>();
			AssertEquals("Prefix should use default UN if UNDGSubtance is null", "UN", undgDataItem.GetUnnoPrefix());
		}
	}
}
