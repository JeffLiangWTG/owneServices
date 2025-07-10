using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class IcelandForwardingShipmentSupportTest : TestCaseWithFactory
	{
		#region Test Icelandic specific

		ZString GetCustomsHouseCode(ForwardingShipment shipment)
		{
			CusEntryNumber result = GetCustomsHouseEntryNum(shipment);
			return result == null ? ZString.Empty : result.CE_EntryNum;
		}

		CusEntryNumber GetCustomsHouseEntryNum(ForwardingShipment shipment)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Iceland);
			CusEntryNumber[] results = (CusEntryNumber[])shipment.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		public void TestIcelandicCustomsHouseCode()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ISAKU";
			AssertNull("Iceland customs house entry num", GetCustomsHouseEntryNum(shipment));

			RefUNLOCO iSZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
			iSZZZ.RL_Code = "ISZZZ";
			RefLocoMap map = iSZZZ.RefLocoMaps.AddNew();
			map.RY_RN = Core.Constants.CountryGuids.Iceland;
			map.RY_SystemUsage = ISLocoMapSystemUsageList.Codes.CustomsOfficeCode;
			map.RY_LocalPortCode = "Z123";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.Iceland);

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ISAKU";
			CusEntryNumber customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Empty number", "", customsHouseEntryNum.CE_EntryNum);
			Factory.Save();
			AssertEquals("COC", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			OrgHeader newAgent = Factory.NewWithValidTestData<OrgHeader>();
			newAgent.OH_RL_NKClosestPort = "ISAKU";
			shipment.ConsigneePK = newAgent.PK;
			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Empty number", "", customsHouseEntryNum.CE_EntryNum);

			Env.Security.MaintainShipmentCOCOverride.IsAllowed = false;
			newAgent.OH_RL_NKClosestPort = "ISZZZ";
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsigneePK = newAgent.PK;
			AssertEquals("COC of shipment receiving forwarder", "Z123", GetCustomsHouseCode(shipment));
			Factory.Save();
			AssertEquals("COC: From <> to <Z123> due to change of Consignee by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Can't change customs house code from GUI", true, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Env.Security.MaintainShipmentCOCOverride.IsAllowed = true;
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsigneePK = newAgent.PK;
			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			AssertEquals("Can change customs house code from GUI", false, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Factory.Save();
			ForwardingShipment reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedShipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Now in database", true, customsHouseEntryNum.IsInDatabase);
			AssertEquals("COC of shipment receiving forwarder", "Z123", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("Can change customs house code from GUI", false, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			customsHouseEntryNum.CE_EntryNum = "AAA";
			AssertEquals("Overriden COC", "AAA", customsHouseEntryNum.CE_EntryNum);
			Factory.Save();
			AssertEquals("COC: From <Z123> to <AAA> due to manual override by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsigneePK = newAgent.PK;
			AssertEquals("Default COC", "Z123", customsHouseEntryNum.CE_EntryNum);

			Factory.Save();
			reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedShipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Now in database", true, customsHouseEntryNum.IsInDatabase);
			AssertEquals("Can change customs house code from GUI", false, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			Env.Security.MaintainShipmentCOCOverride.IsAllowed = false;
			Factory.Save();
			reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			customsHouseEntryNum = GetCustomsHouseEntryNum(reloadedShipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Can't change customs house code from GUI", true, customsHouseEntryNum.CE_EntryNumInfo.ReadOnly);
			AssertEquals("Can't change customs house code type from GUI", true, customsHouseEntryNum.CE_EntryTypeInfo.ReadOnly);

			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			customsHouseEntryNum.CE_EntryNum = "Z123";

			shipment.JS_RL_NKDestination = "HKHKG";
			AssertEquals("Empty number", "", GetCustomsHouseCode(shipment));
			Factory.Save();
			AssertEquals("COC: From <Z123> to <> due to change of Origin/Destination by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			shipment.JS_RL_NKOrigin = "ISAKU";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("Empty number", "", GetCustomsHouseCode(shipment));
			Factory.Save();
			AssertEquals("COC: From <Z123> to <> due to change of Origin/Destination by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			shipment.JS_RL_NKOrigin = "ISZZZ";
			AssertEquals("Empty number", "", GetCustomsHouseCode(shipment));
			Factory.Save();
			AssertEquals("COC: From <Z123> to <> due to change of Origin/Destination by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = ZString.Empty;
			consol.JK_RL_NKDischargePort = "AUSYD";
			shipment.JS_GoodsDescription = "22222";
			Factory.Save();
			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Empty number with consol wo load port", "", customsHouseEntryNum.CE_EntryNum);

			consol.JK_RL_NKLoadPort = "ISAKU";
			shipment.JS_GoodsDescription = "1111";
			Factory.Save();
			customsHouseEntryNum = GetCustomsHouseEntryNum(shipment);
			AssertNotNull("Iceland customs house entry num", customsHouseEntryNum);
			AssertEquals("Empty number with consol and load port wo COC", "", customsHouseEntryNum.CE_EntryNum);

			consol.JK_RL_NKLoadPort = "ISZZZ";
			shipment.JS_GoodsDescription = "22222";
			Factory.Save();
			AssertEquals("COC of consol load port", "Z123", GetCustomsHouseCode(shipment));
			AssertEquals("COC: From <> to <Z123> due to change of Port of Loading by user", customsHouseEntryNum.Logs.MostRecentLog.SL_Reference);
		}

		#endregion

		#region TestPreviousConsignorConsignee

		public void TestPreviousConsignor()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "~ZZZ111";
			Factory.Save();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
			Factory.Save();
			AssertNull("Previous consignor", shipment.PreviousConsignor);
			AssertEquals("Previous consignor name must be empty", ZString.Empty, shipment.PreviousConsignorName);

			RefCountry lastCountry = GlbBranch.CurrentBranch.Country;

			try
			{
				GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.Iceland);
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);

				shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				AssertNull("Previous consignor", shipment.PreviousConsignor);
				AssertEquals("Previous consignor name must be empty", ZString.Empty, shipment.PreviousConsignorName);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertNull("Previous consignor", shipment.PreviousConsignor);
				AssertEquals("Previous consignor name must be empty", ZString.Empty, shipment.PreviousConsignorName);

				OrgHeader header2 = Factory.NewWithValidTestData<OrgHeader>();
				header2.OH_FullName = "~ZZZ222";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = header2.PK;
				Factory.Save();
				AssertEquals("PreviousConsignor is a previous consignor", header, shipment.PreviousConsignor);
				AssertEquals("PreviousConsignor name is a previous consignor name", header.OH_FullName, shipment.PreviousConsignorName);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertEquals("PreviousConsignor is a previous consignor", header2, shipment.PreviousConsignor);
				AssertEquals("PreviousConsignor name is a previous consignor name", header2.OH_FullName, shipment.PreviousConsignorName);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				Factory.Save();
				AssertEquals("PreviousConsignor is a previous consignor", header, shipment.PreviousConsignor);
				AssertEquals("PreviousConsignor name is a previous consignor name", header.OH_FullName, shipment.PreviousConsignorName);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertNull("PreviousConsignor", shipment.PreviousConsignor);
				AssertEquals("PreviousConsignor name must be empty", ZString.Empty, shipment.PreviousConsignorName);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				Factory.Save();
				shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
				shipment.ConsignorDocumentaryAddress.E2_CompanyName = "~ZZZZ~~";
				Factory.Save();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertNull("PreviousConsignor", shipment.PreviousConsignor);
				AssertEquals("PreviousConsignor name is a previous consignor name", "~ZZZZ~~", shipment.PreviousConsignorName);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(lastCountry.RN_Code);
				GlbCompany.CurrentCompany.SetCountry(lastCountry.RN_Code);
			}
		}

		public void TestPreviousConsignee()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "~ZZZ111";
			Factory.Save();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;
			Factory.Save();
			AssertNull("PreviousConsignee", shipment.PreviousConsignee);
			AssertEquals("PreviousConsignee name must be empty", ZString.Empty, shipment.PreviousConsigneeName);

			RefCountry lastCountry = GlbBranch.CurrentBranch.Country;

			try
			{
				GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.Iceland);
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);

				shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				AssertNull("PreviousConsignee", shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name must be empty", ZString.Empty, shipment.PreviousConsigneeName);

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertNull("PreviousConsignee", shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name must be empty", ZString.Empty, shipment.PreviousConsigneeName);

				OrgHeader header2 = Factory.NewWithValidTestData<OrgHeader>();
				header2.OH_FullName = "~ZZZ222";
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = header2.PK;
				Factory.Save();
				AssertEquals("PreviousConsignee is a previous consignee", header, shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name is a previous consignee name", header.OH_FullName, shipment.PreviousConsigneeName);

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertEquals("PreviousConsignee is a previous consignee", header2, shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name is a previous consignee name", header2.OH_FullName, shipment.PreviousConsigneeName);

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				Factory.Save();
				AssertEquals("PreviousConsignee is a previous consignee", header, shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name is a previous consignee name", header.OH_FullName, shipment.PreviousConsigneeName);

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertNull("PreviousConsignee", shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name must be empty", ZString.Empty, shipment.PreviousConsigneeName);

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				Factory.Save();
				shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
				shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "~ZZZZ~~";
				Factory.Save();
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;
				Factory.Save();
				AssertNull("PreviousConsignee", shipment.PreviousConsignee);
				AssertEquals("PreviousConsignee name is a previous consignee name", "~ZZZZ~~", shipment.PreviousConsigneeName);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(lastCountry.RN_Code);
				GlbCompany.CurrentCompany.SetCountry(lastCountry.RN_Code);
			}
		}

		#endregion

		#region Previous COC

		public void TestPreviousCOC()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			AssertEquals("Previous COC must be empty", ZString.Empty, shipment.PreviousCOC);

			var lastCountry = GlbBranch.CurrentBranch.Country;
			var now = DateTime.Now;

			try
			{
				GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.Iceland);
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);

				shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.Numbers.AddNew();
				Factory.Save();
				var cusEntryNum = GetCustomsHouseEntryNum(shipment);
				SetEventTimeAndSave(cusEntryNum, now.AddDays(-3));
				AssertEquals("Previous COC must be empty", ZString.Empty, shipment.PreviousCOC);

				cusEntryNum.CE_EntryNum = "1111";
				Factory.Save();
				SetEventTimeAndSave(cusEntryNum, now.AddDays(-2));
				AssertEquals("Previous COC must be empty", ZString.Empty, shipment.PreviousCOC);

				cusEntryNum = GetCustomsHouseEntryNum(shipment);
				cusEntryNum.CE_EntryNum = "ABCDE";
				Factory.Save();
				SetEventTimeAndSave(cusEntryNum, now.AddDays(-1));
				AssertEquals("Previous COC", "1111", shipment.PreviousCOC);

				cusEntryNum = GetCustomsHouseEntryNum(shipment);
				cusEntryNum.CE_EntryNum = "3333";
				Factory.Save();
				SetEventTimeAndSave(cusEntryNum, now);
				AssertEquals("Previous COC", "ABCDE", shipment.PreviousCOC);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(lastCountry.RN_Code);
				GlbCompany.CurrentCompany.SetCountry(lastCountry.RN_Code);
			}
		}

		void SetEventTimeAndSave(CusEntryNumber cusEntryNum, DateTime eventTime)
		{
			var log = cusEntryNum.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			if (log != null)
			{
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_EventTime = eventTime;
				}

				Factory.Save();
			}
		}

		#endregion

		#region Inspection Status

		public void TestSetApprovedShipperStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IS"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				consignor.MainAddress.OA_RN_NKCountryCode = "IS";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ISEFJ";
				shipment.JS_RL_NKDestination = "SGSIN";

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' for non-approved consignor", "UNK", shipment.JS_InspectionTypeCode);

				var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Regulated Agent", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Account Consignor", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(2);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Known Consignor", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' for expired approval", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Boundary condition - Status is set to 'APP' for Known Consignor with approval expiring today", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		#endregion
	}
}
