using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCodeInfoTest : TestCaseWithFactory
	{
		void AssertLoadedInfo(IOrgCodeInfo[] loadedInfos, IOrgCodeInfo org)
		{
			IOrgCodeInfo loadedInfo = Array.Find(loadedInfos, delegate(IOrgCodeInfo info)
			{
				return (info.PK == org.PK);
			});

			AssertEquals("CountryCode", org.CountryCode, loadedInfo.CountryCode);
			AssertEquals("IataCode", org.IataCode, loadedInfo.IataCode);
			AssertEquals("IsCreditorForAnyCompany", org.IsCreditorForAnyCompany, loadedInfo.IsCreditorForAnyCompany);
			AssertEquals("IsDebtorForAnyCompany", org.IsDebtorForAnyCompany, loadedInfo.IsDebtorForAnyCompany);
			AssertEquals("OH_Code", org.OH_Code, loadedInfo.OH_Code);
			AssertEquals("OH_FullName", org.OH_FullName, loadedInfo.OH_FullName);
			AssertEquals("OH_IsBroker", org.OH_IsBroker, loadedInfo.OH_IsBroker);
			AssertEquals("OH_IsCompetitor", org.OH_IsCompetitor, loadedInfo.OH_IsCompetitor);
			AssertEquals("OH_IsConsignee", org.OH_IsConsignee, loadedInfo.OH_IsConsignee);
			AssertEquals("OH_IsConsignor", org.OH_IsConsignor, loadedInfo.OH_IsConsignor);
			AssertEquals("OH_IsForwarder", org.OH_IsForwarder, loadedInfo.OH_IsForwarder);
			AssertEquals("OH_IsGlobalAccount", org.OH_IsGlobalAccount, loadedInfo.OH_IsGlobalAccount);
			AssertEquals("OH_IsMiscFreightServices", org.OH_IsMiscFreightServices, loadedInfo.OH_IsMiscFreightServices);
			AssertEquals("OH_IsNationalAccount", org.OH_IsNationalAccount, loadedInfo.OH_IsNationalAccount);
			AssertEquals("OH_IsSalesLead", org.OH_IsSalesLead, loadedInfo.OH_IsSalesLead);
			AssertEquals("OH_IsShippingProvider", org.OH_IsShippingProvider, loadedInfo.OH_IsShippingProvider);
			AssertEquals("PK", org.PK, loadedInfo.PK);
			AssertEquals("UnlocoCode", org.UnlocoCode, loadedInfo.UnlocoCode);
			AssertEquals("UnlocoCode", org.PortName, loadedInfo.PortName);
			AssertEquals("UnlocoCode", org.CountryName, loadedInfo.CountryName);
		}

		IOrgCodeInfo CreateOrg(ZString flagPropertyName, ZString unlocoCode)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result[flagPropertyName] = ZBool.True;
			result.OH_Code = flagPropertyName.Substring(5, flagPropertyName.Length - 5).SubstringSafe(0, 10) + "_C";
			result.OH_FullName = flagPropertyName + "_N";
			result.OH_RL_NKClosestPort = unlocoCode;
			return new OrgHeaderOrgCodeInfo(result);
		}

		public void TestLoad()
		{
			var broker = CreateOrg(OrgHeaderSchema.Constants.OH_IsBroker, ZString.Empty);
			var creditor = CreateOrg(OrgHeader.OHConstants.OH_IsCreditor, ZString.Empty);
			var competitor = CreateOrg(OrgHeaderSchema.Constants.OH_IsCompetitor, ZString.Empty);
			var consignee = CreateOrg(OrgHeaderSchema.Constants.OH_IsConsignee, ZString.Empty);
			var consignor = CreateOrg(OrgHeaderSchema.Constants.OH_IsConsignor, ZString.Empty);
			var debtor = CreateOrg(OrgHeader.OHConstants.OH_IsDebtor, ZString.Empty);
			var globalAccount = CreateOrg(OrgHeaderSchema.Constants.OH_IsGlobalAccount, ZString.Empty);
			var forwarder = CreateOrg(OrgHeaderSchema.Constants.OH_IsForwarder, ZString.Empty);
			var miscFreightServices = CreateOrg(OrgHeaderSchema.Constants.OH_IsMiscFreightServices, ZString.Empty);
			var nationalAccount = CreateOrg(OrgHeaderSchema.Constants.OH_IsNationalAccount, ZString.Empty);
			var salesLead = CreateOrg(OrgHeaderSchema.Constants.OH_IsSalesLead, "USORD");
			var shippingProvider = CreateOrg(OrgHeaderSchema.Constants.OH_IsShippingProvider, "AUSYD");
			var transportClient = CreateOrg(OrgHeaderSchema.Constants.OH_IsTransportClient, ZString.Empty);
			var warehouseClient = CreateOrg(OrgHeaderSchema.Constants.OH_IsWarehouseClient, "AUBNE");

			Factory.Save();

			IOrgCodeInfo[] loadedInfos = OrgCodeInfo.LoadAllAndUseHeapsOfResources(Factory);

			AssertLoadedInfo(loadedInfos, broker);
			AssertLoadedInfo(loadedInfos, creditor);
			AssertLoadedInfo(loadedInfos, competitor);
			AssertLoadedInfo(loadedInfos, consignee);
			AssertLoadedInfo(loadedInfos, consignor);
			AssertLoadedInfo(loadedInfos, debtor);
			AssertLoadedInfo(loadedInfos, globalAccount);
			AssertLoadedInfo(loadedInfos, forwarder);
			AssertLoadedInfo(loadedInfos, miscFreightServices);
			AssertLoadedInfo(loadedInfos, nationalAccount);
			AssertLoadedInfo(loadedInfos, salesLead);
			AssertLoadedInfo(loadedInfos, shippingProvider);
			AssertLoadedInfo(loadedInfos, transportClient);
			AssertLoadedInfo(loadedInfos, warehouseClient);
		}
	}
}
