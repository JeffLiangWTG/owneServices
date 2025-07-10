using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5301MessageSendingObject))]
	sealed class TransferApplicationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new N5301MessageSendingObject(Factory.NewWithValidTestData<CusInBondHeader>());
		}

		public void TestDefaultValues()
		{
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHeader.ReceiptOffice = "AA";
			cusInBondHeader.UnladingOffice = "BB";
			cusInBondHeader.TW_BoxNumber = "123";
			cusInBondHeader.BH_ReleaseStatus = "A";
			cusInBondHeader.BH_MessageStatus = "B";
			var sendingObject = new N5301MessageSendingObject(cusInBondHeader);
			NUnit.Framework.Assert.That(sendingObject.EntryStatus, NUnit.Framework.Is.EqualTo(cusInBondHeader.BH_ReleaseStatus));
			NUnit.Framework.Assert.That(sendingObject.MessageStatus, NUnit.Framework.Is.EqualTo(cusInBondHeader.BH_MessageStatus));
			cusInBondHeader.BH_ReleaseStatus = "A1";
			cusInBondHeader.BH_MessageStatus = "B1";
			NUnit.Framework.Assert.That(sendingObject.EntryStatus, NUnit.Framework.Is.EqualTo(cusInBondHeader.BH_ReleaseStatus));
			NUnit.Framework.Assert.That(sendingObject.MessageStatus, NUnit.Framework.Is.EqualTo(cusInBondHeader.BH_MessageStatus));
			NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			cusInBondHeader.BH_MessageStatus = ZString.Empty;
			sendingObject = new N5301MessageSendingObject(cusInBondHeader);
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
			var sendingObject1 = new N5301MessageSendingObject(cusHead1);
			var sendingObject2 = new N5301MessageSendingObject(cusHead2);
			var sendingObject3 = new N5301MessageSendingObject(cusHead3);
			NUnit.Framework.Assert.That(sendingObject1.EntryNumber, NUnit.Framework.Is.EqualTo(cusNum1.CE_EntryNum));
			AssertNotNullOrEmpty(sendingObject2.EntryNumber);
			AssertNotNullOrEmpty(sendingObject3.EntryNumber);
			NUnit.Framework.Assert.That(sendingObject2.EntryNumber.Substring(0, 4), NUnit.Framework.Is.EqualTo("AABB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sendingObject3.EntryNumber.Substring(0, 4), NUnit.Framework.Is.EqualTo("AABB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestActionList()
		{
			var action = GetNewBusinessObject() as N5301MessageSendingObject;
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.GetCodeFromDescription(ActionCodeList.Codes.Update), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(list.GetCodeFromDescription(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestGetMessageOwner()
		{
			var sendingObject = GetNewBusinessObject() as N5301MessageSendingObject;
			NUnit.Framework.Assert.That(sendingObject.GetMessageOwner(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID02125200", Core.Constants.CountryCodes.Taiwan);
			sendingObject.Header.BH_OA_Importer = importerOrg.MainAddress.PK;
			NUnit.Framework.Assert.That(sendingObject.GetMessageOwner(), NUnit.Framework.Is.EqualTo(sendingObject.Applicant.ID));
		}
	}
}
