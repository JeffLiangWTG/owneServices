using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCustomsNumberViewStmNumsWrapper))]
	sealed class TWCustomsNumberViewStmNumsWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestMessageTypeList()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			stmNum.SN_Owner = GlbCompany.CurrentCompany.PK;
			stmNum.SN_FountainName = "TWEntryNum_IMP_C";
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			AssertEquals(3, wrapper.MessageTypeList.Count);
			AssertEquals("IMP, EXP, TRS", wrapper.MessageTypeList.CodesAsString);
		}

		public void TestRangeTypeCodeList()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			stmNum.SN_Owner = GlbCompany.CurrentCompany.PK;
			stmNum.SN_FountainName = "TWEntryNum_IMP_C";
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.MessageType = "IMP";
			AssertEquals(4, wrapper.RangeTypeCodeList.Count);
			AssertEquals("A, B, C, D", wrapper.RangeTypeCodeList.CodesAsString);
			wrapper.MessageType = "EXP";
			AssertEquals(4, wrapper.RangeTypeCodeList.Count);
			AssertEquals("A, B, C, D", wrapper.RangeTypeCodeList.CodesAsString);
			wrapper.MessageType = "TRS";
			AssertEquals(1, wrapper.RangeTypeCodeList.Count);
			AssertEquals("T", wrapper.RangeTypeCodeList.CodesAsString);
		}

		[UseSnapshotProtection]
		public void TestProperties()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			stmNum.SN_Owner = GlbCompany.CurrentCompany.PK;
			stmNum.SN_FountainName = "TWEntryNum_IMP_C";
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			CombineAssertions(() =>
			{
				AssertEquals("SN_Type", "CUS", wrapper.SN_Type);
				AssertEquals("Range Type", "C", wrapper.RangeType);
				AssertEquals("Range Type Description", "雜項報單", wrapper.RangeTypeDescription);
				AssertEquals("RangeTypeInfo.ReadOnly", false, wrapper.RangeTypeInfo.ReadOnly);
				AssertEquals("CountInfo.ReadOnly", true, wrapper.CountInfo.ReadOnly);
				AssertEquals("MessageType", "IMP", wrapper.MessageType);
				AssertEquals("MessageType.ReadOnly", false, wrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("ReadOnly", false, wrapper.ReadOnly);
				AssertEquals($"Owner: Company - EDI - Eagle Datamation International, Range Type: C, MessageType: IMP", wrapper.Detail);
				wrapper.MessageType = CustomsNumberMessageTypeList.Codes.Transhipment;
				AssertEquals("Range Type", "T", wrapper.RangeType);
				AssertEquals("RangeTypeInfo.ReadOnly", true, wrapper.RangeTypeInfo.ReadOnly);
				AssertEquals("StartNumber", "00001", wrapper.StartNumber);
				AssertEquals("EndNumber", "ZZZZ9", wrapper.EndNumber);
				AssertEquals("CurrentValue", "00001", wrapper.CurrentValue);
				AssertEquals("Count", 6888105L, wrapper.Count);
				wrapper.StartNumber = "00002";
				AssertEquals("StartNumber", "00002", wrapper.StartNumber);
				AssertEquals("EndNumber", "ZZZZ9", wrapper.EndNumber);
				AssertEquals("CurrentValue", "00002", wrapper.CurrentValue);
				AssertEquals("Count", 6888104L, wrapper.Count);
				wrapper.CurrentValue = "00004";
				wrapper.EndNumber = "00003";
				AssertEquals("StartNumber", "00002", wrapper.StartNumber);
				AssertEquals("EndNumber", "00003", wrapper.EndNumber);
				AssertEquals("CurrentValue", "00004", wrapper.CurrentValue);
				AssertEquals("Count", 2L, wrapper.Count);
			}

			);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("MessageType.ReadOnly", true, wrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("SN_TypeInfo.ReadOnly", true, wrapper.RangeTypeInfo.ReadOnly);
				AssertEquals("CountInfo.ReadOnly", true, wrapper.CountInfo.ReadOnly);
			}

			);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			return new TWCustomsNumberViewStmNumsWrapper(stmNum)
			{ MessageType = "IMP", SN_Type = "C" };
		}
	}
}
