using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	sealed class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			return new ActiveCusEntryHeaderCollection((JobDeclaration)Declaration);
		}

		[ExpectNoExceptions]
		public override void TestAreAllEntriesCleared()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "Ref1";
			newFactory.Save();
			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			entry1 = Factory.Load<CusEntryHeader>(entry1.PK);
			NUnit.Framework.Assert.That(declaration.ActiveEntryHeaders.AreAllEntriesCleared, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = entry1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "EXP";
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			Factory.Save();
			NUnit.Framework.Assert.That(declaration.ActiveEntryHeaders.AreAllEntriesCleared, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "Ref2";
			Factory.Save();
			NUnit.Framework.Assert.That(declaration.ActiveEntryHeaders.AreAllEntriesCleared, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = entry2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "EXP";
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryStatus = ClearanceStatusCodeList.Codes.C2;
			Factory.Save();
			NUnit.Framework.Assert.That(declaration.ActiveEntryHeaders.AreAllEntriesCleared, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}
	}
}
