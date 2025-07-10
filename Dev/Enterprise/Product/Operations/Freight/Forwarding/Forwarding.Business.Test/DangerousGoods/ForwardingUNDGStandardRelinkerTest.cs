using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGStandardRelinkerTest : TestCaseWithFactory
	{
		public void TestRelinkDataItemsToTargetStandard()
		{
			var substanceRID = Factory.New<UNDGSubstanceRID>();
			substanceRID.RID_UNNO = "9999";

			var substanceADR = Factory.New<UNDGSubstanceADR>();
			substanceADR.ADR_UNNO = "9999";

			var substanceIMO = Factory.New<UNDGSubstance>();
			substanceIMO.DG_UNNO = "9999";
			substanceIMO.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var substanceIATA = Factory.New<UNDGSubstance>();
			substanceIATA.DG_UNNO = "9999";
			substanceIATA.DG_Standard = "IAT";

			var dataItemRID = Factory.New<UNDGDataItem>();
			dataItemRID.LinkDefault(substanceRID);

			var dataItemADR = Factory.New<UNDGDataItem>();
			dataItemADR.LinkDefault(substanceADR);

			var dataItemIMO = Factory.New<UNDGDataItem>();
			dataItemIMO.LinkDefault(substanceIMO);

			var dataItems = new List<UNDGDataItem>()
			{
				dataItemRID,
				dataItemADR,
				dataItemIMO
			};

			Factory.Save();

			ForwardingUNDGStandardRelinker.TryRelinkAllDataItemsToTargetStandard(Factory, dataItems, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);

			foreach (var dataItem in dataItems)
			{
				AssertEquals(substanceIATA, dataItem.Substance);
			}
		}

		public void TestRelinkingDoesntOccurWhenVariantsExist()
		{
			var substanceIATA = Factory.New<UNDGSubstance>();
			substanceIATA.DG_UNNO = "9999";
			substanceIATA.DG_Standard = "IAT";

			var substanceIMO = Factory.New<UNDGSubstance>();
			substanceIMO.DG_UNNO = "9999";
			substanceIMO.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var substanceADR = Factory.New<UNDGSubstanceADR>();
			substanceADR.ADR_UNNO = "9999";

			var substanceRID1 = Factory.New<UNDGSubstanceRID>();
			substanceRID1.RID_UNNO = "9999";
			substanceRID1.RID_Variant = "A";

			var substanceRID2 = Factory.New<UNDGSubstanceRID>();
			substanceRID2.RID_UNNO = "9999";
			substanceRID2.RID_Variant = "B";

			var dataItemIATA = Factory.New<UNDGDataItem>();
			dataItemIATA.LinkDefault(substanceIATA);

			var dataItemADR = Factory.New<UNDGDataItem>();
			dataItemADR.LinkDefault(substanceADR);

			var dataItemIMO = Factory.New<UNDGDataItem>();
			dataItemIMO.LinkDefault(substanceIMO);

			var dataItems = new List<UNDGDataItem>()
			{
				dataItemIATA,
				dataItemADR,
				dataItemIMO
			};

			Factory.Save();

			ForwardingUNDGStandardRelinker.TryRelinkAllDataItemsToTargetStandard(Factory, dataItems, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID);

			foreach (var dataItem in dataItems)
			{
				AssertNotEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID, dataItem.Substance.DG_Standard);
			}
		}

		public void TestRelinkingAgainstMultipleSubstances()
		{
			var substanceIAT = Factory.New<UNDGSubstance>();
			substanceIAT.DG_UNNO = "9999";
			substanceIAT.DG_Standard = "IAT";

			var substanceIMO = Factory.New<UNDGSubstance>();
			substanceIMO.DG_UNNO = "7777";
			substanceIMO.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var substanceADR = Factory.New<UNDGSubstanceADR>();
			substanceADR.ADR_UNNO = "8888";

			var substanceRID9999 = Factory.New<UNDGSubstanceRID>();
			substanceRID9999.RID_UNNO = "9999";

			var substanceRID8888 = Factory.New<UNDGSubstanceRID>();
			substanceRID8888.RID_UNNO = "8888";

			var substanceRID7777 = Factory.New<UNDGSubstanceRID>();
			substanceRID7777.RID_UNNO = "7777";

			var dataItemIATA = Factory.New<UNDGDataItem>();
			dataItemIATA.LinkDefault(substanceIAT);

			var dataItemADR = Factory.New<UNDGDataItem>();
			dataItemADR.LinkDefault(substanceADR);

			var dataItemIMO = Factory.New<UNDGDataItem>();
			dataItemIMO.LinkDefault(substanceIMO);

			var dataItems = new List<UNDGDataItem>()
			{
				dataItemIATA,
				dataItemADR,
				dataItemIMO
			};

			Factory.Save();

			ForwardingUNDGStandardRelinker.TryRelinkAllDataItemsToTargetStandard(Factory, dataItems, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID);

			AssertEquals(substanceRID9999.PK, dataItemIATA.Substance.PK);
			AssertEquals(substanceRID8888.PK, dataItemADR.Substance.PK);
			AssertEquals(substanceRID7777.PK, dataItemIMO.Substance.PK);
		}
	}
}
