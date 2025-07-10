using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class FRNCustomsOfficesCodeListTest : TestCaseWithFactory
	{
		public void TestFRNCustomsOfficesCodeList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var list = new FRNCustomsOfficesCodeList();
				AssertEquals(1, list.Count);
				var item = list[0];
				AssertEquals("3234002346", item.Code);
				AssertEquals("FAN:3234002346 Office:JHB", item.Description);
			}
		}

		public void TestReadOnlyCodeDescriptionPairList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var list = new FRNCustomsOfficesCodeList();
				var iList = ((DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider)list).GetCodeDescriptionPairList();
				AssertEquals(1, iList.Count);
				var item = iList[0];
				AssertEquals("3234002346", item.Code);
				AssertEquals("FAN:3234002346 Office:JHB", item.Description);
			}
		}
	}
}
