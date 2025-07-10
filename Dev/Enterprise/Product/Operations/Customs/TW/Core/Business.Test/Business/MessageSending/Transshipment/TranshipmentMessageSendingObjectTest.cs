using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TranshipmentMessageSendingObject))]
	sealed class TranshipmentMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TranshipmentMessageSendingObject(Factory.NewWithValidTestData<CusInBondHeader>(), "");
		}

		public void TestDefaultValues()
		{
			var entryHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			entryHeader.ReceiptOffice = "AA";
			entryHeader.UnladingOffice = "BB";
			entryHeader.BH_ReleaseStatus = "A";
			entryHeader.BH_MessageStatus = "B";
			entryHeader.TW_BoxNumber = "123";
			var sendingObject = new TranshipmentMessageSendingObject(entryHeader, "");
			NUnit.Framework.Assert.That(sendingObject.EntryStatus, NUnit.Framework.Is.EqualTo(entryHeader.BH_ReleaseStatus));
			NUnit.Framework.Assert.That(sendingObject.MessageStatus, NUnit.Framework.Is.EqualTo(entryHeader.BH_MessageStatus));
			entryHeader.BH_ReleaseStatus = "A1";
			entryHeader.BH_MessageStatus = "B1";
			NUnit.Framework.Assert.That(sendingObject.EntryStatus, NUnit.Framework.Is.EqualTo(entryHeader.BH_ReleaseStatus));
			NUnit.Framework.Assert.That(sendingObject.MessageStatus, NUnit.Framework.Is.EqualTo(entryHeader.BH_MessageStatus));
			NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			entryHeader.BH_MessageStatus = ZString.Empty;
			sendingObject = new TranshipmentMessageSendingObject(entryHeader, "");
			NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			var cusHead1 = Factory.NewWithValidTestData<CusInBondHeader>();
			cusHead1.ReceiptOffice = "AA";
			cusHead1.UnladingOffice = "BB";
			cusHead1.TW_BoxNumber = "123";
			var cusHead2 = Factory.NewWithValidTestData<CusInBondHeader>();
			cusHead2.ReceiptOffice = "AA";
			cusHead2.UnladingOffice = "BB";
			cusHead2.TW_BoxNumber = "123";
			var cusHead3 = Factory.NewWithValidTestData<CusInBondHeader>();
			cusHead3.ReceiptOffice = "AA";
			cusHead3.UnladingOffice = "BB";
			cusHead3.TW_BoxNumber = "123";
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "TRS";
			cusNum1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = cusHead2.PK;
			cusNum2.CE_Category = "XXX";
			cusNum2.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "NO2";
			cusHead2.AllocateEntryNumber();
			cusHead3.AllocateEntryNumber();
			Factory.Save();
			var sendingObject1 = new TranshipmentMessageSendingObject(cusHead1, "");
			var sendingObject2 = new TranshipmentMessageSendingObject(cusHead2, "");
			var sendingObject3 = new TranshipmentMessageSendingObject(cusHead3, "");
			NUnit.Framework.Assert.That(sendingObject1.EntryNumber, NUnit.Framework.Is.EqualTo(cusNum1.CE_EntryNum));
			AssertNotNullOrEmpty(sendingObject2.EntryNumber);
			AssertNotNullOrEmpty(sendingObject3.EntryNumber);
			NUnit.Framework.Assert.That(sendingObject2.EntryNumber.Substring(0, 4), NUnit.Framework.Is.EqualTo("AABB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sendingObject3.EntryNumber.Substring(0, 4), NUnit.Framework.Is.EqualTo("AABB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestActionList()
		{
			var cusHead1 = Factory.NewWithValidTestData<CusInBondHeader>();
			cusHead1.TW_BoxNumber = "123";
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "TRS";
			cusNum1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			Factory.Save();
			cusHead1.BH_ReleaseStatus = "A";
			var action = new TranshipmentMessageSendingObject(cusHead1, "");
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestGetMessageOwner()
		{
			var sendingObject = GetNewBusinessObject() as TranshipmentMessageSendingObject;
			NUnit.Framework.Assert.That(sendingObject.GetMessageOwner(), NUnit.Framework.Is.EqualTo(ZString.Empty));
		}
	}
}
