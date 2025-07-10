using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCustomsNumberViewStmNumsWrapper))]
	sealed class USCustomsNumberViewStmNumsWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestWhenOwnerIsBranch()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "TSC";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "TSB";
			var entryFiler = new EntryFiler()
			{ EntryFilerCode = "XJ8" };
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = company.CustomsNumberProvider;
			var wrapper = new USCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.SN_Owner = branch.PK;
			wrapper.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			AssertEquals("Entry Filer Code", ZString.Empty, wrapper.AppliesTo);
			wrapper.SN_Type = ZString.Empty;
			using (USCustomsDataRegistry.Instance.EntryFiler.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler))
			{
				wrapper.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
				AssertEquals("Entry Filer Code", entryFiler.EntryFilerCode, wrapper.AppliesTo);
			}
		}

		[UseSnapshotProtection]
		public void TestProperties()
		{
			var stmNum = GlbCompany.CurrentCompany.CustomsNumberProvider.CustomsNumbers.AddNew();
			stmNum.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			stmNum.SN_FountainName = USCustomsNumberViewStmNumsWrapper.GenerateFountainName("XJ5", 1);
			stmNum.HasChanges = false;
			var wrapper = new USCustomsNumberViewStmNumsWrapper(stmNum);
			CombineAssertions(() =>
			{
				AssertEquals("HasChanges", false, wrapper.HasChanges);
				AssertEquals("CheckDigitAddition", (ZByte)1, wrapper.CheckDigitAddition);
				AssertEquals("CheckDigitAdditionInfo.ReadOnly", false, wrapper.CheckDigitAdditionInfo.ReadOnly);
				AssertEquals("AppliesTo", "XJ5", wrapper.AppliesTo);
				AssertEquals("AppliesTo.ReadOnly", true, wrapper.AppliesToInfo.ReadOnly);
				AssertEquals("ReadOnly", false, wrapper.ReadOnly);
				AssertEquals($"Owner: Company - EDI - Eagle Datamation International, Range Type: ENS, Check Digit Addition: 1, Applies To: XJ5", wrapper.Detail);
			});
			CombineAssertions(() =>
			{
				stmNum.SN_FountainName = USCustomsNumberViewStmNumsWrapper.GenerateFountainName("X6", 2);
				AssertEquals("HasChanges", true, wrapper.HasChanges);
				AssertEquals("CheckDigitAddition", (ZByte)2, wrapper.CheckDigitAddition);
				AssertEquals("CheckDigitAdditionInfo.ReadOnly", false, wrapper.CheckDigitAdditionInfo.ReadOnly);
				AssertEquals("AppliesTo", "X6", wrapper.AppliesTo);
				AssertEquals("AppliesTo.ReadOnly", true, wrapper.AppliesToInfo.ReadOnly);
				AssertEquals("ReadOnly", false, wrapper.ReadOnly);
				AssertEquals($"Owner: Company - EDI - Eagle Datamation International, Range Type: ENS, Check Digit Addition: 2, Applies To: X6", wrapper.Detail);
			});
			stmNum.Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNum.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNum.SN_Name);
			stmNum = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			wrapper = new USCustomsNumberViewStmNumsWrapper(stmNum);
			CombineAssertions(() =>
			{
				AssertEquals("HasChanges", false, wrapper.HasChanges);
				AssertEquals("CheckDigitAddition", (ZByte)2, wrapper.CheckDigitAddition);
				AssertEquals("CheckDigitAdditionInfo.ReadOnly", true, wrapper.CheckDigitAdditionInfo.ReadOnly);
				AssertEquals("AppliesTo", "X6", wrapper.AppliesTo);
				AssertEquals("AppliesTo.ReadOnly", true, wrapper.AppliesToInfo.ReadOnly);
				AssertEquals("ReadOnly", false, wrapper.ReadOnly);
			});
			CombineAssertions(() =>
			{
				wrapper.CheckDigitAddition = 5;
				AssertEquals("HasChanges", true, wrapper.HasChanges);
				AssertEquals("CheckDigitAddition", (ZByte)5, wrapper.CheckDigitAddition);
				AssertEquals("CheckDigitAdditionInfo.ReadOnly", true, wrapper.CheckDigitAdditionInfo.ReadOnly);
				AssertEquals("AppliesTo", "X6", wrapper.AppliesTo);
				AssertEquals("AppliesTo.ReadOnly", true, wrapper.AppliesToInfo.ReadOnly);
				AssertEquals("ReadOnly", false, wrapper.ReadOnly);
			});
			CombineAssertions(() =>
			{
				wrapper.AppliesTo = "R7";
				AssertEquals("HasChanges", true, wrapper.HasChanges);
				AssertEquals("CheckDigitAddition", (ZByte)5, wrapper.CheckDigitAddition);
				AssertEquals("AppliesTo", "R7", wrapper.AppliesTo);
				AssertEquals("SN_FountainName", "R7 :5", stmNum.SN_FountainName);
				AssertEquals("ReadOnly", false, wrapper.ReadOnly);
			});
		}

		public void TestExtraDataFromFountainName()
		{
			ZString appliesTo;
			ZByte checkDigit;
			var fountainName = "";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals(ZString.Empty, appliesTo);
			AssertEquals(ZByte.Zero, checkDigit);
			fountainName = "asdb";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals("ASD", appliesTo);
			AssertEquals(ZByte.Zero, checkDigit);
			fountainName = "XJ:A";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals("XJ", appliesTo);
			AssertEquals(ZByte.Zero, checkDigit);
			fountainName = "XJ :A";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals("XJ", appliesTo);
			AssertEquals(ZByte.Zero, checkDigit);
			fountainName = "JDK:";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals("JDK", appliesTo);
			AssertEquals(ZByte.Zero, checkDigit);
			fountainName = "JDK: ";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals("JDK", appliesTo);
			AssertEquals(ZByte.Zero, checkDigit);
			fountainName = "JKD:53";
			USCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out appliesTo, out checkDigit);
			AssertEquals("JKD", appliesTo);
			AssertEquals((ZByte)5, checkDigit);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			return new USCustomsNumberViewStmNumsWrapper(stmNum);
		}
	}
}
