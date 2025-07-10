using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class IcelandForwardingConsolSupportTest : TestCaseWithFactory
	{
		public void TestSendingarnumer()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			OrgHeader carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgCusCode carrierCode = Factory.New<OrgCusCode>();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			carrierCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			carrierCode.OK_CustomsRegNo = "F";
			carrierCode.OK_OH = carrier.PK;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals("should be blank", ZString.Empty, consol.JK_CRN);
			AssertNull(consol.ShippingLine);

			Factory.Save();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("should be F", "F", consol.JK_CRN);
		}

		ZString GetCustomsHouseCode(ForwardingConsol consol)
		{
			CusEntryNumber result = GetCustomsHouseEntryNum(consol);
			return result == null ? ZString.Empty : result.CE_EntryNum;
		}

		CusEntryNumber GetCustomsHouseEntryNum(ForwardingConsol consol)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Iceland);
			CusEntryNumber[] results = (CusEntryNumber[])consol.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		CusEntryNumber GetCustomsHouseEntryNumDirectly(ForwardingConsol consol)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Iceland);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, consol.PK);
			CusEntryNumber[] results = Factory.Load<CusEntryNumber>(query);
			return results.Length > 0 ? results[0] : null;
		}

		public void TestIcelandicCustomsHouseCode()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ISAKU";
			AssertEquals("Empty number", "", GetCustomsHouseCode(consol));

			RefUNLOCO iSZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
			iSZZZ.RL_Code = "ISZZZ";
			RefLocoMap map = iSZZZ.RefLocoMaps.AddNew();
			map.RY_RN = Core.Constants.CountryGuids.Iceland;
			map.RY_SystemUsage = ISLocoMapSystemUsageList.Codes.CustomsOfficeCode;
			map.RY_LocalPortCode = "Z123";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.Iceland);

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ISAKU";
			AssertEquals("Empty number", "", GetCustomsHouseCode(consol));
			Factory.Save();
			CusEntryNumber customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			AssertEquals("COC", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			OrgHeader newAgent = Factory.NewWithValidTestData<OrgHeader>();
			newAgent.OH_RL_NKClosestPort = "ISAKU";
			consol.JK_OA_ReceivingForwarderAddress = newAgent.MainAddress.PK;
			AssertEquals("Empty number", "", GetCustomsHouseCode(consol));

			newAgent.OH_RL_NKClosestPort = "ISZZZ";
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = newAgent.MainAddress.PK;
			customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			AssertEquals("COC of consol receiving forwarder", "Z123", GetCustomsHouseCode(consol));
			Factory.Save();
			AssertEquals("COC: From <> to <Z123> due to change of Receiving Agent by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			consol.JK_RL_NKDischargePort = "HKHKG";
			AssertEquals("Empty number", "", GetCustomsHouseCode(consol));
			Factory.Save();
			AssertEquals("COC: From <Z123> to <> due to change of Receiving Agent by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			consol.JK_RL_NKLoadPort = "ISAKU";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("Empty number", "", GetCustomsHouseCode(consol));

			Env.Security.MaintainConsolCOCOverride.IsAllowed = false;
			consol.JK_RL_NKLoadPort = "ISZZZ";
			AssertEquals("COC of consol load port", "Z123", GetCustomsHouseCode(consol));
			Factory.Save();
			customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			AssertEquals("COC: From <> to <Z123> due to change of Port of Loading by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Can't change customs house code from GUI", true, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Factory.Save();
			ForwardingConsol reloadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedConsol);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Now in database", true, customsHouseEntryNum.IsInDatabase);
			AssertEquals("COC code", IcelandForwardingShipmentSupport.CustomsOfficeCode, customsHouseEntryNum.CE_EntryType);
			AssertEquals("COC of consol load port", "Z123", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("Can't change customs house code from GUI", true, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Env.Security.MaintainConsolCOCOverride.IsAllowed = true;
			consol.JK_RL_NKLoadPort = "ISAKU";
			consol.JK_RL_NKLoadPort = "ISZZZ";
			customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			AssertEquals("Can change customs house code from GUI", false, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Factory.Save();
			reloadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedConsol);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Now in database", true, customsHouseEntryNum.IsInDatabase);
			AssertEquals("COC code", IcelandForwardingShipmentSupport.CustomsOfficeCode, customsHouseEntryNum.CE_EntryType);
			AssertEquals("COC of consol load port", "Z123", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("Can change customs house code from GUI", false, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			customsHouseEntryNum.CE_EntryNum = "AAA";
			AssertEquals("Overriden COC", "AAA", customsHouseEntryNum.CE_EntryNum);
			Factory.Save();
			AssertEquals("COC: From <Z123> to <AAA> due to manual override by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);
			reloadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedConsol);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Now in database", true, customsHouseEntryNum.IsInDatabase);
			AssertEquals("COC code", IcelandForwardingShipmentSupport.CustomsOfficeCode, customsHouseEntryNum.CE_EntryType);
			AssertEquals("COC of consol load port", "AAA", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("Can change customs house code from GUI", false, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Env.Security.MaintainConsolCOCOverride.IsAllowed = false;

			Factory.Save();
			reloadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedConsol);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Now in database", true, customsHouseEntryNum.IsInDatabase);
			AssertEquals("COC code", IcelandForwardingShipmentSupport.CustomsOfficeCode, customsHouseEntryNum.CE_EntryType);
			AssertEquals("COC of consol load port", "AAA", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("Can't change customs house code from GUI", true, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			consol.JK_RL_NKLoadPort = "ISAKU";
			consol.JK_RL_NKLoadPort = "ISZZZ";
			customsHouseEntryNum = GetCustomsHouseEntryNum(consol);
			AssertEquals("Default COC", "Z123", customsHouseEntryNum.CE_EntryNum);

			consol = Factory.New<ForwardingConsol>();
			customsHouseEntryNum = GetCustomsHouseEntryNumDirectly(consol);
			AssertNull("Iceland customs house entry num", customsHouseEntryNum);

			consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			customsHouseEntryNum = GetCustomsHouseEntryNumDirectly(consol);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);

			consol = Factory.New<ForwardingConsol>();
			consol.Numbers.Load();
			customsHouseEntryNum = GetCustomsHouseEntryNumDirectly(consol);
			AssertNotNull("Iceland customs house entry num created on load", customsHouseEntryNum);
		}
	}
}
