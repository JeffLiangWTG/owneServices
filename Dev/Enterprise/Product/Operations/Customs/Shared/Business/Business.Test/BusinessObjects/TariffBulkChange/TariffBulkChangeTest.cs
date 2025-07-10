using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.TariffBulkChange;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(TariffBulkChange))]
	sealed class TariffBulkChangeTest : BaseTariffBulkChangeTest
	{
		protected override BusinessObject GetNewBusinessObject() => new TariffBulkChangeTestHelper(Factory);

		public void TestLoadConcordanceWithNoAuto()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);
			var pivotWithTariffNum = one2ManyValidPart.PivotsForBinding.AddNew();
			pivotWithTariffNum.CI_TariffNum = One2ManyOldValidTariffNum;
			pivotWithTariffNum.CI_ChildType = "XXX";

			var topLevelObject = new TariffBulkChangeTestHelper(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, false);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2OneOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots[0].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			string tempstring = "";
			foreach (BaseCusClassification classification1 in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification1.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2OneLookupCode));
			AssertEquals(true, tempstring.Contains(One2OneFreeStandingLookupCode));
			AssertEquals(One2OnePartNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots.Count);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[1].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			tempstring = "";
			foreach (BaseCusClassification classification1 in topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications)
			{
				tempstring += classification1.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (BaseCusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));

			AssertEquals(One2ManyOldValidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots.Count);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots[0].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots[0].Part.OP_PartNum);

			var selectedOldTariff = topLevelObject.TariffBulkChangeOldTariffs[1];
			var lookupCode1TariffNum = selectedOldTariff.OriginalClassifications[0].CC_TariffNum;
			var selectedNewTariff = selectedOldTariff.TariffBulkChangeNewTariffs[1];
			var newTariffNum = selectedNewTariff.NewTariffNum;
			BaseCusClassification[] selectedOriginalClassifications;
			selectedOriginalClassifications = new BaseCusClassification[2];
			selectedOriginalClassifications[0] = selectedOldTariff.OriginalClassifications[1];
			selectedOriginalClassifications[1] = selectedOldTariff.OriginalClassifications[2];
			topLevelObject.UpdateOriginalLookups(selectedOldTariff, selectedNewTariff, selectedOriginalClassifications);
			AssertEquals("Class 1 does not change", lookupCode1TariffNum, selectedOldTariff.OriginalClassifications[0].CC_TariffNum);
			AssertEquals("Class 1 newtariff is empy", "", selectedOldTariff.OriginalClassifications[0].NewTariffNum);
			AssertEquals("Now only 1 original class left", 1, selectedOldTariff.OriginalClassifications.Count);
			AssertEquals("Now 2 New Classifications", 2, selectedOldTariff.NewClassifications.Count);
			AssertEquals("Existing Tariff Num does not change", lookupCode1TariffNum, selectedOldTariff.NewClassifications[0].CC_TariffNum);
			AssertEquals("Existing Tariff Num does not change", lookupCode1TariffNum, selectedOldTariff.NewClassifications[1].CC_TariffNum);
			AssertEquals("New Classes now have new code", newTariffNum, selectedOldTariff.NewClassifications[0].NewTariffNum);
			AssertEquals("New Classes now have new code", newTariffNum, selectedOldTariff.NewClassifications[1].NewTariffNum);

			var saveClass1 = selectedOldTariff.NewClassifications[0];
			var saveClass2 = selectedOldTariff.NewClassifications[1];

			TariffBulkChangeNewTariff[] selectedNewTariffs;
			selectedNewTariffs = new TariffBulkChangeNewTariff[2];
			selectedNewTariffs[0] = selectedOldTariff.TariffBulkChangeNewTariffs[0];
			selectedNewTariffs[1] = selectedOldTariff.TariffBulkChangeNewTariffs[1];
			topLevelObject.MakeNewClassifications(selectedOldTariff, selectedNewTariffs);
			AssertEquals("Now another 2 New Classifications should exist", 4, selectedOldTariff.NewClassifications.Count);
			tempstring = "";
			ZString uniqueCode = "A";
			foreach (BaseCusClassification classification1 in selectedOldTariff.NewClassifications)
			{
				if (classification1.CC_LookupCode.IsEmpty)
				{
					classification1.CC_LookupCode = "NEW" + uniqueCode;
					uniqueCode = "B";
					tempstring += classification1.CC_TariffNum + "*";
					AssertEquals(BaseCusClassification.ClassificationType.Both, classification1.CC_ClassificationType);
					AssertEquals(classification1.CC_Description, classification1.CC_TariffNum);
				}
			}
			AssertEquals(true, tempstring.Contains(selectedNewTariffs[0].NewTariffNum));
			AssertEquals(true, tempstring.Contains(selectedNewTariffs[1].NewTariffNum));

			var savePivot1 = selectedOldTariff.TariffItemPivots[0];
			var savePivot2 = selectedOldTariff.TariffItemPivots[1];

			var selectedNewClassification = selectedOldTariff.NewClassifications[2];
			selectedNewClassification.NewLookupCode = NewLookupCode;
			var selectedPivots = new BaseCusClassPartPivot[2];
			selectedPivots[0] = savePivot1;
			selectedPivots[1] = savePivot2;
			topLevelObject.UpdateProducts(selectedOldTariff, selectedNewClassification, null, selectedPivots);
			AssertEquals("Pivots now have new lookup code", NewLookupCode, savePivot1.NewLookUpCode);
			AssertEquals("Pivots now have new lookup code", NewLookupCode, savePivot2.NewLookUpCode);

			selectedNewClassification = selectedOldTariff.NewClassifications[0];
			selectedNewClassification.NewLookupCode = AnotherNewLookupCode;
			selectedPivots = new BaseCusClassPartPivot[1];
			selectedPivots[0] = savePivot2;
			topLevelObject.UpdateProducts(selectedOldTariff, selectedNewClassification, null, selectedPivots);
			AssertEquals("This part should not change", NewLookupCode, savePivot1.NewLookUpCode);
			AssertEquals("This part should have another new code", AnotherNewLookupCode, savePivot2.NewLookUpCode);

			Factory.Save();

			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass1.CC_TariffChangePending);
			var fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass1.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass2.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, TariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
		}

		public void TestLoadConcordanceWithAutoOne2One()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);
			var pivotWithTariffNum = one2ManyValidPart.PivotsForBinding.AddNew();
			pivotWithTariffNum.CI_TariffNum = One2ManyOldValidTariffNum;
			pivotWithTariffNum.CI_ChildType = "XXX";

			TariffBulkChange topLevelObject = new TariffBulkChangeTestHelper(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, true);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots[0].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots[1].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			string tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (BaseCusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));

			AssertEquals(One2ManyOldValidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots.Count);
			Assert(topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.RemoveNonEssentialValidationForBulkTariffUpdate);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.OP_PartNum);

			Factory.Save();

			var one2OnePart = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, one2OnePart);
			AssertEquals("Just one pivot", 1, one2OnePart.PivotsForBinding.Count);
			var onlyPivot = one2OnePart.PivotsForBinding[0];

			AssertEquals(One2OneNewTariffNum, onlyPivot.NewTariffNum);

			AssertNotEquals("Pre-condition: Parts traiffnum should not have changed yet", One2OneNewTariffNum, onlyPivot.Classification.CC_TariffNum);
			AssertNotEquals("Pre-condition: Parts traiffnum should not have changed yet", One2OneNewTariffNum, onlyPivot.OldTariffCode);
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Parts traiffnum should now have changed", One2OneNewTariffNum, onlyPivot.Classification.CC_TariffNum);
			AssertEquals("Parts traiffnum should now have changed", One2OneNewTariffNum, onlyPivot.OldTariffCode);
		}

		public void TestLoadConcordanceWithTariffOnPivot()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);
			var pivotWithTariffNum = one2ManyValidPart.PivotsForBinding.AddNew();
			pivotWithTariffNum.CI_TariffNum = One2ManyOldValidTariffNum;
			pivotWithTariffNum.CI_ChildType = "XXX";

			var topLevelObject = new TariffBulkChangeTestHelper(Factory);
			topLevelObject.IsPivotTariffNumSupportedExposed = true;
			topLevelObject.ValidPivotTypeExposed = "XXX";

			Assert("pre-condition", One2ManyOldValidTariffNum != One2ManyOldValidNew2TariffNum);

			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, false);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs.Count);

			var interestedOldTariff = topLevelObject.TariffBulkChangeOldTariffs[2];

			AssertEquals(One2ManyOldValidTariffNum, interestedOldTariff.OldTariffNum);
			AssertEquals(2, interestedOldTariff.TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew2TariffNum, interestedOldTariff.TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, interestedOldTariff.OriginalClassifications.Count);
			AssertEquals(2, interestedOldTariff.NewClassifications.Count);
			AssertEquals(2, interestedOldTariff.TariffItemPivots.Count);
			var interestedPivot = interestedOldTariff.TariffItemPivots[0].CI_TariffNum == One2ManyOldValidTariffNum ?
				interestedOldTariff.TariffItemPivots[0] : interestedOldTariff.TariffItemPivots[1];
			AssertEquals(One2ManyOldValidTariffNum, interestedPivot.CI_TariffNum);
			var interestedPart = interestedPivot.Part;
			AssertEquals(One2ManyOldValidPartNum, interestedPart.OP_PartNum);
			Assert(interestedPart.RemoveNonEssentialValidationForBulkTariffUpdate);
			var selectedNewTariffNum = interestedOldTariff.TariffBulkChangeNewTariffs[1];
			var selectedPivots = new BaseCusClassPartPivot[1];
			selectedPivots[0] = interestedPivot;
			topLevelObject.UpdateProducts(interestedOldTariff, null, selectedNewTariffNum, selectedPivots);
			AssertEquals("Existing pivots tariff num", One2ManyOldValidTariffNum, interestedPivot.CI_TariffNum);
			AssertEquals("Pivots now have new tariff num", One2ManyOldValidNew2TariffNum, interestedPivot.NewTariffNum);

			Factory.Save();
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Pivots tariff num changed to new value", One2ManyOldValidNew2TariffNum, interestedPivot.CI_TariffNum);
			AssertEquals(ZString.Empty, interestedPivot.NewTariffNum);
		}

		public void TestLoadConcordanceWithTariffOnPivot_One2Many()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);
			var pivotWithTariffNum = one2ManyValidPart.PivotsForBinding.AddNew();
			pivotWithTariffNum.CI_TariffNum = One2MannyClass2OldTariffNum;
			pivotWithTariffNum.CI_ChildType = "XXX";

			var topLevelObject = new TariffBulkChangeTestHelper(Factory);
			topLevelObject.IsPivotTariffNumSupportedExposed = true;
			topLevelObject.ValidPivotTypeExposed = "XXX";

			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, false);
			AssertEquals(5, topLevelObject.TariffBulkChangeOldTariffs.Count);

			var interestedOldTariff = topLevelObject.TariffBulkChangeOldTariffs[3];

			AssertEquals(One2MannyClass2OldTariffNum, interestedOldTariff.OldTariffNum);
			AssertEquals(4, interestedOldTariff.TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2MannyClass2New1TariffNum, interestedOldTariff.TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(0, interestedOldTariff.OriginalClassifications.Count);
			AssertEquals(0, interestedOldTariff.NewClassifications.Count);
			AssertEquals(1, interestedOldTariff.TariffItemPivots.Count);
			var interestedPivot = interestedOldTariff.TariffItemPivots[0];
			AssertEquals(One2MannyClass2OldTariffNum, interestedPivot.CI_TariffNum);
			var interestedPart = interestedPivot.Part;
			AssertEquals(One2ManyOldValidPartNum, interestedPart.OP_PartNum);
			Assert(interestedPart.RemoveNonEssentialValidationForBulkTariffUpdate);
			var selectedNewTariffNum = interestedOldTariff.TariffBulkChangeNewTariffs[0];
			var selectedPivots = new BaseCusClassPartPivot[1];
			selectedPivots[0] = interestedPivot;
			topLevelObject.UpdateProducts(interestedOldTariff, null, selectedNewTariffNum, selectedPivots);
			AssertEquals("Existing pivots tariff num", One2MannyClass2OldTariffNum, interestedPivot.CI_TariffNum);
			AssertEquals("Pivots now have new tariff num", One2MannyClass2New1TariffNum, interestedPivot.NewTariffNum);

			Factory.Save();
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Pivots tariff num changed to new value", One2MannyClass2New1TariffNum, interestedPivot.CI_TariffNum);
			AssertEquals(ZString.Empty, interestedPivot.NewTariffNum);
		}

		public void TestLoadConcordanceWithTariffOnPivotAndAutoOne2One()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);
			var pivotWithTariffNum = one2ManyValidPart.PivotsForBinding.AddNew();
			pivotWithTariffNum.CI_TariffNum = One2OneOldTariffNum;
			pivotWithTariffNum.CI_ChildType = "XXX";

			var topLevelObject = new TariffBulkChangeTestHelper(Factory);
			topLevelObject.IsPivotTariffNumSupportedExposed = true;
			topLevelObject.ValidPivotTypeExposed = "XXX";

			Assert("pre-condition", One2OneOldTariffNum != One2OneNewTariffNum);

			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, true);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals("New Tariff set on pivot", One2OneNewTariffNum, pivotWithTariffNum.NewTariffNum);

			Factory.Save();
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Pivots tariff num changed to new value", One2OneNewTariffNum, pivotWithTariffNum.CI_TariffNum);
			AssertEquals(ZString.Empty, pivotWithTariffNum.NewTariffNum);
		}

		public void TestLoadConcordanceWithStatCode()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);
			var pivotWithTariffNum = one2ManyValidPart.PivotsForBinding.AddNew();
			pivotWithTariffNum.CI_TariffNum = One2OneOldTariffNum.Replace(".", "") + "12";
			pivotWithTariffNum.CI_ChildType = "XXX";
			one2OneClassification.CC_TariffNum = One2OneOldTariffNum.Replace(".", "") + "23";
			Factory.Save();

			var topLevelObject = new TariffBulkChangeTestHelper(Factory);
			topLevelObject.IsPivotTariffNumSupportedExposed = true;
			topLevelObject.ValidPivotTypeExposed = "XXX";

			Assert("pre-condition", One2OneOldTariffNum != One2OneNewTariffNum);

			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordanceWithStatCode.csv");
			topLevelObject.LoadConcordance(stream, true);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs.Count);

			Factory.Save();
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Lookup tariff num changed to new value", One2OneNewTariffNum + " 23", one2OneClassification.CC_TariffNum);
			AssertEquals("Pivots tariff num changed to new value", One2OneNewTariffNum + " 12", pivotWithTariffNum.CI_TariffNum);
		}

		public void TestAdditionalContinueWithSave()
		{
			var allowDate = new ZDateTime(2006, 12, 30);
			var priorToAllowDate = new ZDateTime(2006, 12, 29);
			const string DemoCompanyCode = "DEM";
			const string NonDemoCompanyCode = "EDI";
			var topLevelObject = new TariffBulkChangeTestHelper(Factory);

			RunOneAdditionalContinueWithSaveTest(topLevelObject, false, DemoCompanyCode, allowDate, false, ZDialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, false, DemoCompanyCode, allowDate, true, ZDialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, false, NonDemoCompanyCode, allowDate, false, ZDialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, false, NonDemoCompanyCode, allowDate, true, ZDialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");

			RunOneAdditionalContinueWithSaveTest(topLevelObject, true, DemoCompanyCode, allowDate, false, ZDialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, true, DemoCompanyCode, allowDate, true, ZDialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, true, DemoCompanyCode, priorToAllowDate, false, ZDialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, true, DemoCompanyCode, priorToAllowDate, true, ZDialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, true, NonDemoCompanyCode, allowDate, false, ZDialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
			RunOneAdditionalContinueWithSaveTest(topLevelObject, true, NonDemoCompanyCode, allowDate, true, ZDialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
		}

		void RunOneAdditionalContinueWithSaveTest(TariffBulkChangeTestHelper topLevelObject, bool setProductionDataBase, ZString companyCode, ZDateTime currentDate,
bool automaticConvert, ZDialogResult questionAnswer, string expectedLastWarnTextMessage, ContinueWithSave expectedResult,
string expectedLastErrorTextMessage, string expectedFinalConfirmationContains1, string expectedFinalConfirmationContains2

			)
		{
			topLevelObject.SetIsProductionDataBase = setProductionDataBase;
			GlbCompany.CurrentCompany.GC_Code = companyCode;
			topLevelObject.SetDateTimeNow = currentDate;

			topLevelObject.WarnIfDemoOrDateInvalid();
			if (string.IsNullOrEmpty(expectedLastWarnTextMessage))
			{
				Assert("There should not be any warning", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
			else
			{
				Assert("Sould be a warning", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(expectedLastWarnTextMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			UnitTestUserNotification.Instance.AddAnswer(questionAnswer);
			AssertEquals("Expected Result", expectedResult, topLevelObject.AdditionalContinueWithSave(automaticConvert));
			if (!string.IsNullOrEmpty(expectedLastErrorTextMessage))
			{
				Assert("Sould be an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(expectedLastErrorTextMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			if (!string.IsNullOrEmpty(expectedFinalConfirmationContains1))
			{
				Assert("There should be a question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("Question should contain '" + expectedFinalConfirmationContains1 + "'", UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedFinalConfirmationContains1));
			}
			if (!string.IsNullOrEmpty(expectedFinalConfirmationContains2))
			{
				Assert("There should be a question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("Question should contain '" + expectedFinalConfirmationContains2 + "'", UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedFinalConfirmationContains2));
			}
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestIsSaveAllowed()
		{
			var topLevelObject = new TariffBulkChangeTestHelper(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, true, isSaveAllowed: false);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Save not allowed", ContinueWithSave.No, topLevelObject.ApplyAdditionalContinueWithSave());
			AssertEquals("Was correct message", topLevelObject.SaveNotAllowedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
"Enterprise.Customs.Business.Testing.BusinessObjects.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, true, isSaveAllowed: true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			AssertEquals("Save is allowed", ContinueWithSave.Yes, topLevelObject.ApplyAdditionalContinueWithSave());
		}

		[TestedType(typeof(TariffBulkChangeOldTariff))]
		class TariffBulkChangeOldTariffTest : NonPersistentBusinessObjectTestCase
		{
			public void TestOldTariffNum()
			{
				TariffBulkChange tBC = new TariffBulkChangeTestHelper(Factory);
				TariffBulkChangeOldTariff tBCOldTariff = tBC.TariffBulkChangeOldTariffs.AddNew();
				tBCOldTariff.OldTariffNum = "12345678";
				AssertEquals("1234.56.78", tBCOldTariff.OldTariffNum);
			}
		}

		[TestedType(typeof(TariffBulkChangeNewTariff))]
		class TariffBulkChangeNewTariffTest : NonPersistentBusinessObjectTestCase
		{
			public void TestNewTariffNum()
			{
				TariffBulkChange tBC = new TariffBulkChangeTestHelper(Factory);
				TariffBulkChangeOldTariff tBCOldTariff = tBC.TariffBulkChangeOldTariffs.AddNew();
				TariffBulkChangeNewTariff tBCNewTariff = tBCOldTariff.TariffBulkChangeNewTariffs.AddNew();
				tBCNewTariff.NewTariffNum = "12345678";
				AssertEquals("1234.56.78", tBCNewTariff.NewTariffNum);
			}
		}

		[TestedType(typeof(TariffBulkChangeOldTariffCollection))]
		class TariffBulkChangeOldTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffBulkChangeOldTariffCollection>
		{
			protected override TariffBulkChangeOldTariffCollection GetCollectionToTest() => new TariffBulkChangeOldTariffCollection(TBC);

			protected override BusinessObject GetNewElementToAddToTheCollection() => new TariffBulkChangeOldTariff(TBC);

			TariffBulkChange TBC => tbc ?? (tbc = new TariffBulkChangeTestHelper(Factory));
			TariffBulkChange tbc;
		}

		[TestedType(typeof(TariffBulkChangeNewTariffCollection))]
		class TariffBulkChangeNewTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffBulkChangeNewTariffCollection>
		{
			protected override TariffBulkChangeNewTariffCollection GetCollectionToTest() => new TariffBulkChangeNewTariffCollection(TBC, OldTariffNum);

			protected override BusinessObject GetNewElementToAddToTheCollection() => new TariffBulkChangeNewTariff(TBC, OldTariffNum);

			TariffBulkChange TBC => tbc ?? (tbc = new TariffBulkChangeTestHelper(Factory));
			TariffBulkChange tbc;

			TariffBulkChangeOldTariff OldTariffNum => oldtariffnum ?? (oldtariffnum = new TariffBulkChangeOldTariff());
			TariffBulkChangeOldTariff oldtariffnum;
		}

		[TestedType(typeof(TBCClassificationCollection<TBCClassification>))]
		class TBCClassificationCollectionTest : BusinessObjectCollectionTestCase
		{
			protected override BusinessObjectCollection GetCollectionToTest() => new TBCClassificationCollection<TBCClassification>(Factory);
		}

		[TestedType(typeof(TariffBulkChangerPivotCollection))]
		class TariffBulkChangerPivotCollectionTest : BusinessObjectCollectionTestCase
		{
			protected override BusinessObjectCollection GetCollectionToTest() => new TariffBulkChangerPivotCollection(Factory);
		}

		class OrgSupplierPartAndPivotTBCTest : TestCaseWithFactory
		{
			public void TestOrgSupplierPartNewProperties()
			{
				OrgSupplierPart part = Factory.New<OrgSupplierPart>();
				TBCClassification oldClass = Factory.New<TBCClassification>();
				oldClass.CC_TariffNum = "11111111";
				oldClass.CC_LookupCode = "OLD";
				TBCClassification newClass = Factory.New<TBCClassification>();
				newClass.CC_TariffNum = "22222222";
				newClass.CC_LookupCode = "NEW";
				BaseCusClassPartPivot pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_OP = part.PK;
				pivot.CI_CC = oldClass.PK;

				pivot.NewLookUpPK = newClass.PK;
				AssertEquals("NEW", pivot.NewLookUpCode);
				AssertEquals("2222.22.22", pivot.NewTariffNum);
				newClass.NewTariffNum = "33333333";
				AssertEquals("3333.33.33", pivot.NewTariffNum);
			}
		}

		[TestedType(typeof(TBCClassification))]
		public class TBCClassificationBusinessObjectTest : EnterpriseBusinessObjectTestCase
		{
		}

		class TBCClassificationTest : TestCaseWithFactory
		{
			public void TestTBCClassificationNewProperties()
			{
				var @class = Factory.New<TBCClassification>();
				@class.NewLookupCode = "ABC";
				AssertEquals("ABC", @class.NewLookupCode);
				@class.NewTariffNum = "12345678";
				AssertEquals("1234.56.78", @class.NewTariffNum);
			}
		}
	}
}
