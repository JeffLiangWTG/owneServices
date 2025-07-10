using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DeclarationMessageSendingObject))]
	sealed class DeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new DeclarationMessageSendingObject(entryHeader);
		}

		[TestDate(2020, 8, 5)]
		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var warehouseOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = warehouseOrgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = warehouseOrgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = warehouseOrgHeader.MainAddress.PK;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new DeclarationMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.EntryStatus, NUnit.Framework.Is.EqualTo(entryHeader.CH_EntryStatus));
			NUnit.Framework.Assert.That(sendingObject.MessageStatus, NUnit.Framework.Is.EqualTo(entryHeader.CH_Status));
			entryHeader.CH_EntryStatus = "A1";
			entryHeader.CH_Status = "B1";
			NUnit.Framework.Assert.That(sendingObject.EntryStatus, NUnit.Framework.Is.EqualTo(entryHeader.CH_EntryStatus));
			NUnit.Framework.Assert.That(sendingObject.MessageStatus, NUnit.Framework.Is.EqualTo(entryHeader.CH_Status));
			NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			entryHeader.CH_Status = ZString.Empty;
			sendingObject = new DeclarationMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_JE = declaration.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "B1";
			entryInstruction.CEI_CustomsOffice = "BA";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = warehouseOrgHeader.MainAddress.PK;
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var cusHead2 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead2.CH_JE = declaration2.PK;
			cusHead2.CH_CEI_Instruction = entryInstruction.PK;
			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var cusHead3 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead3.CH_JE = declaration3.PK;
			cusHead3.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			cusHead1.AllocateEntryNumber();
			var sendingObject1 = new DeclarationMessageSendingObject(cusHead1);
			var sendingObject2 = new DeclarationMessageSendingObject(cusHead2);
			var sendingObject3 = new DeclarationMessageSendingObject(cusHead3);
			NUnit.Framework.Assert.That(sendingObject1.EntryNumber, NUnit.Framework.Is.EqualTo("BAB10923480001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sendingObject2.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(sendingObject3.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(sendingObject1.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));
			cusHead1.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			Factory.Save();
			sendingObject1 = new DeclarationMessageSendingObject(cusHead1);
			NUnit.Framework.Assert.That(sendingObject1.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Update).Using(CustomComparers.TypeComparison));
			declaration.EntryNumber = "A";
			cusHead1.EntryNumber = ZString.Empty;
			sendingObject1 = new DeclarationMessageSendingObject(cusHead1);
			NUnit.Framework.Assert.That(sendingObject1.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestActionList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_JE = declaration.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			Factory.Save();
			cusHead1.CH_EntryStatus = "A";
			cusHead1.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var action = new DeclarationMessageSendingObject(cusHead1);
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Update));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
			cusHead1.CH_EntryStatus = "";
			list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Update));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
			declaration.EntryNumber = "A";
			cusHead1.EntryNumber = ZString.Empty;
			action = new DeclarationMessageSendingObject(cusHead1);
			list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list[0].Code, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create));
		}

		[ExpectNoExceptions]
		public void TestFriendlyNameForMessageManager()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new DeclarationMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.FriendlyNameForMessageManager, NUnit.Framework.Is.EqualTo("Send Customs Declaration").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGovernmentProcedureDescriptions()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.TW_OverrideTradersRemarks = true;
			entryInstruction.TW_TradersRemarks = "1.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,\r\n6003573046,6003573048,6003573601,\r\n6003573602,6003574047,6003574048\r\n三方交易案件，買方為國內廠商\r\n應加費用: (內陸費&amp;報關手續費)USD25\r\n常年(長期)委任報關核准文號：\r\n111業二18838\r\n起：111年11月01日\r\n迄：116年10月31日";
			var sendingObject = new DeclarationMessageSendingObjectForTesting(entryHeader);
			var governmentProcedureDescriptions = sendingObject.GovernmentProcedureDescriptionsExploded;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentProcedureDescriptions.First(), NUnit.Framework.Is.EqualTo("1.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(governmentProcedureDescriptions.Last(), NUnit.Framework.Is.EqualTo("6003573046,6003573048,6003573601,\r\n6003573602,6003574047,6003574048\r\n三方交易案件，買方為國內廠商\r\n應加費用: (內陸費&amp;報關手續費)USD25\r\n常年(長期)委任報關核准文號：\r\n111業二18838\r\n起：111年11月01日\r\n迄：116年10月31日").Using(CustomComparers.TypeComparison));

				entryInstruction.TW_TradersRemarks = "1.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,\r\n";
				NUnit.Framework.Assert.That(governmentProcedureDescriptions.Single(), NUnit.Framework.Is.EqualTo("1.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,").Using(CustomComparers.TypeComparison));

				entryInstruction.TW_TradersRemarks = "1.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,.2.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,\r\n";
				NUnit.Framework.Assert.That(governmentProcedureDescriptions.First(), NUnit.Framework.Is.EqualTo("1.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,.").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(governmentProcedureDescriptions.Last(), NUnit.Framework.Is.EqualTo("2.Invoice No.: I23060415\r\n2.此批貨物屬台灣恩智浦半導體股份有限公司(統一編號: 89002500)\r\n，頎邦代為加工，頎邦加工費總金額 : USD 99,210.08\r\n3. 本批貨物以頎邦名義將貨物報關出口逕交台灣恩智浦半導體股份有限公司指定之境外地區\r\n4. PACKING LIST NO: 6003571899,6003572344,\r\n6003572345,6003572349,6003572727,\r\n6003573037,6003573040,6003573045,").Using(CustomComparers.TypeComparison));
			});
		}

		class DeclarationMessageSendingObjectForTesting : DeclarationMessageSendingObject
		{
			public DeclarationMessageSendingObjectForTesting(CusEntryHeader header) : base(header)
			{
			}

			public IEnumerable<ZString> GovernmentProcedureDescriptionsExploded => GetGovernmentProcedureDescriptions();
		}
	}
}
