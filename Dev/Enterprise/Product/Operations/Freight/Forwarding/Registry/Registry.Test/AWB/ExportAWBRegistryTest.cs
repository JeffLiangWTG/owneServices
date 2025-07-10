using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.AWB.Testing
{
	[TestedType(typeof(ExportAWBRegistry))]
	class ExportAWBRegistryTest : RegistryItemSetTestCaseWithFactory<ExportAWBRegistry>
	{
		#region IATA

		public void TestGroupMAWBOtherChargesByIATACode()
		{
			TestRegistryItem(ItemSet.MAWBGroupOtherChargesByIATACode, "MAWBGroupOtherChargesByIATACode", "Freight/AWB/MAWB/Other Charges", "Group By IATA Code",
				"Specifies whether the 'Other Charges' will be grouped by IATA Code on MAWB documents.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestGroupHAWBOtherChargesByIATACode()
		{
			TestRegistryItem(ItemSet.HAWBGroupOtherChargesByIATACode, "HAWBGroupOtherChargesByIATACode", "Freight/AWB/HAWB/Other Charges", "Group By IATA Code",
				"Specifies whether the 'Other Charges' will be grouped by IATA Code on HAWB documents.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		#region Display Options

		public void TestHAWBPrepaidDisplayOption()
		{
			AssertEquals("HAWBPrepaidDisplayOption", ItemSet.HAWBPrepaidDisplayOption.Name);
			AssertEquals("Specify display options for prepaid HAWB.", ItemSet.HAWBPrepaidDisplayOption.Hint);
			AssertEquals("Freight/AWB/HAWB/Other Charges", ItemSet.HAWBPrepaidDisplayOption.Category);
			AssertEquals("Prepaid", ItemSet.HAWBPrepaidDisplayOption.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.HAWBPrepaidDisplayOption.Storage);
			AssertEquals(AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB).Count, ItemSet.HAWBPrepaidDisplayOption.DefaultValue.Count);
		}

		public void TestHAWBCollectDisplayOption()
		{
			AssertEquals("HAWBCollectDisplayOption", ItemSet.HAWBCollectDisplayOption.Name);
			AssertEquals("Specify display options for collect HAWB.", ItemSet.HAWBCollectDisplayOption.Hint);
			AssertEquals("Freight/AWB/HAWB/Other Charges", ItemSet.HAWBCollectDisplayOption.Category);
			AssertEquals("Collect", ItemSet.HAWBCollectDisplayOption.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.HAWBCollectDisplayOption.Storage);
			AssertEquals(AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB).Count, ItemSet.HAWBCollectDisplayOption.DefaultValue.Count);
		}

		public void TestMAWBPrepaidDisplayOption()
		{
			AssertEquals("MAWBPrepaidDisplayOption", ItemSet.MAWBPrepaidDisplayOption.Name);
			AssertEquals("Specify display options for prepaid MAWB.", ItemSet.MAWBPrepaidDisplayOption.Hint);
			AssertEquals("Freight/AWB/MAWB/Other Charges", ItemSet.MAWBPrepaidDisplayOption.Category);
			AssertEquals("Prepaid", ItemSet.MAWBPrepaidDisplayOption.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.MAWBPrepaidDisplayOption.Storage);
			AssertEquals(AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB).Count, ItemSet.MAWBPrepaidDisplayOption.DefaultValue.Count);
		}

		public void TestMAWBCollectDisplayOption()
		{
			AssertEquals("MAWBCollectDisplayOption", ItemSet.MAWBCollectDisplayOption.Name);
			AssertEquals("Specify display options for collect MAWB.", ItemSet.MAWBCollectDisplayOption.Hint);
			AssertEquals("Freight/AWB/MAWB/Other Charges", ItemSet.MAWBCollectDisplayOption.Category);
			AssertEquals("Collect", ItemSet.MAWBCollectDisplayOption.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.MAWBCollectDisplayOption.Storage);
			AssertEquals(AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB).Count, ItemSet.MAWBCollectDisplayOption.DefaultValue.Count);
		}

		#endregion

		#endregion
	}
}
