using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.TariffBulkChange;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(NZTariffBulkChange))]
	public class NZTariffBulkChangeTest : BaseTariffBulkChangeTest
	{
		public void TestShortConcessionSourcesGetIgnored()
		{
			CusClassification classification1 = Factory.NewWithValidTestData<CusClassification>();
			classification1.CC_ConcessionCode = "111111A";

			CusClassification classification2 = Factory.NewWithValidTestData<CusClassification>();
			classification2.CC_ConcessionCode = "123456A";

			CusClassification classification3 = Factory.NewWithValidTestData<CusClassification>();
			classification3.CC_ConcessionCode = "";
			Factory.Save();

			NZTariffBulkChange bulkUpdater = new NZTariffBulkChange(Factory);

			string csvFileContent = @"
,,888888Z
123,,999999Z
123456A,,999A
111111A,,111111B
,,";

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(csvFileContent)))
			{
				bulkUpdater.ChangeTCO(stream);
			}

			CombineAssertions(delegate
			{
				AssertEquals("classification1.CC_ConcessionCode", "111111B", classification1.CC_ConcessionCode);
				AssertEquals("classification2.CC_ConcessionCode", "123456A", classification2.CC_ConcessionCode);
				AssertEquals("classification3.CC_ConcessionCode", "", classification3.CC_ConcessionCode);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZTariffBulkChange(Factory);
		}

		public void TestLoadConcordanceWithNoAuto()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);

			NZTariffBulkChange topLevelObject = new NZTariffBulkChange(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.NZ.Business.Test.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, automaticConvert: false);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2OneOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			string tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
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
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots)
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
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots[0].Part.OP_PartNum);

			TariffBulkChangeOldTariff selectedOldTariff = topLevelObject.TariffBulkChangeOldTariffs[1];
			ZString lookupCode1TariffNum = selectedOldTariff.OriginalClassifications[0].CC_TariffNum;
			TariffBulkChangeNewTariff selectedNewTariff = selectedOldTariff.TariffBulkChangeNewTariffs[1];
			ZString newTariffNum = selectedNewTariff.NewTariffNum;
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
			foreach (BaseCusClassification classification in selectedOldTariff.NewClassifications)
			{
				if (classification.CC_LookupCode.IsEmpty)
				{
					classification.CC_LookupCode = "NEW" + uniqueCode;
					uniqueCode = "B";
					tempstring += classification.CC_TariffNum + "*";
					AssertEquals(BaseCusClassification.ClassificationType.Both, classification.CC_ClassificationType);
					AssertEquals(classification.CC_Description, classification.CC_TariffNum);
				}
			}
			AssertEquals(true, tempstring.Contains(selectedNewTariffs[0].NewTariffNum));
			AssertEquals(true, tempstring.Contains(selectedNewTariffs[1].NewTariffNum));

			BaseCusClassPartPivot savePivot1 = selectedOldTariff.TariffItemPivots[0];
			BaseCusClassPartPivot savePivot2 = selectedOldTariff.TariffItemPivots[1];

			var selectedNewClassification = selectedOldTariff.NewClassifications[2];
			selectedNewClassification.NewLookupCode = NewLookupCode;
			BaseCusClassPartPivot[] selectedPivots;
			selectedPivots = new BaseCusClassPartPivot[2];
			selectedPivots[0] = savePivot2;
			selectedPivots[1] = savePivot1;
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
			LogsForNominatedEvent fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass1.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass2.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, NZTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
		}

		public void TestLoadConcordanceWithAutoOne2One()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.Both, ZString.Empty);

			NZTariffBulkChange topLevelObject = new NZTariffBulkChange(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.NZ.Business.Test.TariffBulkChange.TestTariffConcordance.csv");
			topLevelObject.LoadConcordance(stream, automaticConvert: true);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			string tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots)
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
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.OP_PartNum);

			Factory.Save();

			var one2OnePart = Factory.LoadTop1<MasterFiles.OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, one2OnePart);
			AssertEquals(One2OneNewTariffNum, one2OnePart.PivotsForBinding[0].NewTariffNum);

			AssertNotEquals("Pre-condition: Parts traiffnum should not have changed yet", One2OneNewTariffNum, one2OnePart.PivotsForBinding[0].Classification.CC_TariffNum);
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Parts traiffnum should now have changed", One2OneNewTariffNum, one2OnePart.PivotsForBinding[0].Classification.CC_TariffNum);
		}

		public void TestCountrySpecificOverrides()
		{
			var topLevelObject = new NZTariffBulkChange(Factory);
			AssertEquals("ReferenceKey", "HS2017 TARIFF BTH", topLevelObject.ReferenceKey);
			AssertEquals("CountryPK", Core.Constants.CountryGuids.NewZealand, topLevelObject.CountryPK);
			AssertEquals("CountryCode", Enterprise.Core.Constants.CountryCodes.NewZealand, topLevelObject.CountryCode);
		}

		[TestedType(typeof(TariffBulkChangeOldTariff))]
		class TariffBulkChangeOldTariffTest : NonPersistentBusinessObjectTestCase
		{
			public void TestOldTariffNum()
			{
				NZTariffBulkChange tBC = new NZTariffBulkChange(Factory);
				TariffBulkChangeOldTariff tBCOldTariff = tBC.TariffBulkChangeOldTariffs.AddNew();
				tBCOldTariff.OldTariffNum = "12345678";
				AssertEquals("1234.56.78", tBCOldTariff.OldTariffNum);
			}

			protected override void SetUp()
			{
				base.SetUp();
				var dummyClassification = Factory.New<CusClassification>();
				dummyClassification.CC_LookupCode = "TestLookup";
				dummyClassification.CC_ClassificationType = "BTH";
				dummyClassification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
				Factory.Save();
			}
		}

		[TestedType(typeof(TariffBulkChangeNewTariff))]
		class TariffBulkChangeNewTariffTest : NonPersistentBusinessObjectTestCase
		{
			public void TestNewTariffNum()
			{
				NZTariffBulkChange tBC = new NZTariffBulkChange(Factory);
				TariffBulkChangeOldTariff tBCOldTariff = tBC.TariffBulkChangeOldTariffs.AddNew();
				TariffBulkChangeNewTariff tBCNewTariff = tBCOldTariff.TariffBulkChangeNewTariffs.AddNew();
				tBCNewTariff.NewTariffNum = "12345678";
				AssertEquals("1234.56.78", tBCNewTariff.NewTariffNum);
			}

			protected override void SetUp()
			{
				base.SetUp();
				var dummyClassification = Factory.New<CusClassification>();
				dummyClassification.CC_LookupCode = "TestLookup";
				dummyClassification.CC_ClassificationType = "BTH";
				dummyClassification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
				Factory.Save();
			}
		}

		[TestedType(typeof(TariffBulkChangeOldTariffCollection))]
		class TariffBulkChangeOldTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffBulkChangeOldTariffCollection>
		{
			protected override TariffBulkChangeOldTariffCollection GetCollectionToTest()
			{
				return new TariffBulkChangeOldTariffCollection(TBC);
			}

			protected override BusinessObject GetNewElementToAddToTheCollection()
			{
				return new TariffBulkChangeOldTariff(TBC);
			}

			NZTariffBulkChange TBC
			{
				get
				{
					if (tbc == null)
					{
						tbc = new NZTariffBulkChange(Factory);
					}
					return tbc;
				}
			}
			NZTariffBulkChange tbc;
		}

		[TestedType(typeof(TariffBulkChangeNewTariffCollection))]
		class TariffBulkChangeNewTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffBulkChangeNewTariffCollection>
		{
			protected override TariffBulkChangeNewTariffCollection GetCollectionToTest()
			{
				return new TariffBulkChangeNewTariffCollection(TBC, OldTariffNum);
			}

			protected override BusinessObject GetNewElementToAddToTheCollection()
			{
				return new TariffBulkChangeNewTariff(TBC, OldTariffNum);
			}

			NZTariffBulkChange TBC
			{
				get
				{
					if (tbc == null)
					{
						tbc = new NZTariffBulkChange(Factory);
					}
					return tbc;
				}
			}
			NZTariffBulkChange tbc;

			TariffBulkChangeOldTariff OldTariffNum
			{
				get
				{
					if (oldtariffnum == null)
					{
						oldtariffnum = new TariffBulkChangeOldTariff();
					}
					return oldtariffnum;
				}
			}
			TariffBulkChangeOldTariff oldtariffnum;
		}

		[TestedType(typeof(NZTariffBulkChange.TBCClassificationCollection))]
		class TBCClassificationCollectionTest : BusinessObjectCollectionTestCase
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new NZTariffBulkChange.TBCClassificationCollection(Factory);
			}
		}

		[TestedType(typeof(TariffBulkChangerPivotCollection))]
		class TariffBulkChangerPivotCollectionTest : BusinessObjectCollectionTestCase
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TariffBulkChangerPivotCollection(Factory);
			}
		}

		[TestedType(typeof(NZTariffBulkChange.TBCClassification))]
		public class TBCClassificationBusinessObjectTest : EnterpriseBusinessObjectTestCase
		{
		}

		class TBCClassificationTest : TestCaseWithFactory
		{
			public void TestTBCClassificationNewProperties()
			{
				NZTariffBulkChange.TBCClassification @class = Factory.New<NZTariffBulkChange.TBCClassification>();
				@class.NewLookupCode = "ABC";
				AssertEquals("ABC", @class.NewLookupCode);
				@class.NewTariffNum = "1234567812Q";
				AssertEquals("1234.56.78.12Q", @class.NewTariffNum);
			}
		}
	}
}
