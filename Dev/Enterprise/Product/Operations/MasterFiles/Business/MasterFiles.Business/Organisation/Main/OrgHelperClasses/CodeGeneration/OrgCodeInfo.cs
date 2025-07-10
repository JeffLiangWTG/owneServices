using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgCodeInfo : IOrgCodeInfo
	{
		OrgCodeInfo(DataRow row)
		{
			OH_IsBroker = new ZBool(row[OrgHeaderSchema.Constants.OH_IsBroker]);
			OH_Code = new ZString(row[OrgHeaderSchema.Constants.OH_Code]).Trim();
			OH_IsCompetitor = new ZBool(row[OrgHeaderSchema.Constants.OH_IsCompetitor]);
			OH_IsConsignee = new ZBool(row[OrgHeaderSchema.Constants.OH_IsConsignee]);
			OH_IsConsignor = new ZBool(row[OrgHeaderSchema.Constants.OH_IsConsignor]);
			CountryCode = new ZString(row[RefCountrySchema.Constants.RN_Code]).Trim();
			OH_FullName = new ZString(row[OrgHeaderSchema.Constants.OH_FullName]).Trim();
			OH_IsGlobalAccount = new ZBool(row[OrgHeaderSchema.Constants.OH_IsGlobalAccount]);
			OH_IsForwarder = new ZBool(row[OrgHeaderSchema.Constants.OH_IsForwarder]);
			IataCode = new ZString(row[RefUNLOCOSchema.Constants.RL_IATA]).Trim();
			IsCreditorForAnyCompany = new ZBool(row[OrgCompanyDataSchema.Constants.OB_IsCreditor]);
			IsDebtorForAnyCompany = new ZBool(row[OrgCompanyDataSchema.Constants.OB_IsDebtor]);
			OH_IsMiscFreightServices = new ZBool(row[OrgHeaderSchema.Constants.OH_IsMiscFreightServices]);
			OH_IsNationalAccount = new ZBool(row[OrgHeaderSchema.Constants.OH_IsNationalAccount]);
			PK = new ZGuid(row[OrgHeaderSchema.Constants.PK]).ToGuid();
			OH_IsSalesLead = new ZBool(row[OrgHeaderSchema.Constants.OH_IsSalesLead]);
			OH_IsShippingProvider = new ZBool(row[OrgHeaderSchema.Constants.OH_IsShippingProvider]);
			OH_IsTransportClient = new ZBool(row[OrgHeaderSchema.Constants.OH_IsTransportClient]);
			UnlocoCode = new ZString(row[OrgHeaderSchema.Constants.OH_RL_NKClosestPort]).Trim();
			OH_IsWarehouseClient = new ZBool(row[OrgHeaderSchema.Constants.OH_IsWarehouseClient]);
			OH_Language = new ZString(row[OrgHeaderSchema.Constants.OH_Language]);
			PortName = new ZString(row[RefUNLOCOSchema.Constants.RL_PortName]);
			CountryName = new ZString(row[RefCountrySchema.Constants.RN_Desc]);
		}

		public string CountryCode { get; private set; }
		public string IataCode { get; private set; }
		public bool IsCreditorForAnyCompany { get; private set; }
		public bool IsDebtorForAnyCompany { get; private set; }
		public string OH_Code { get; private set; }
		public bool OH_IsBroker { get; private set; }
		public bool OH_IsCompetitor { get; private set; }
		public bool OH_IsConsignee { get; private set; }
		public bool OH_IsConsignor { get; private set; }
		public bool OH_IsForwarder { get; private set; }
		public bool OH_IsGlobalAccount { get; private set; }
		public bool OH_IsMiscFreightServices { get; private set; }
		public bool OH_IsNationalAccount { get; private set; }
		public bool OH_IsSalesLead { get; private set; }
		public bool OH_IsShippingProvider { get; private set; }
		public bool OH_IsTransportClient { get; private set; }
		public bool OH_IsWarehouseClient { get; private set; }
		public string OH_Language { get; private set; }
		public string OH_FullName { get; private set; }
		public Guid PK { get; private set; }
		public string UnlocoCode { get; private set; }
		public string PortName { get; private set; }
		public string CountryName { get; private set; }
		public string InvalidUnlocoCode { get; private set; }

		public static OrgCodeInfo[] LoadAllAndUseHeapsOfResources(BusinessObjectFactory factory)
		{
			string sqlText =
@"SELECT ISNULL(OB_IsCreditor, 0) AS OB_IsCreditor, ISNULL(OB_IsDebtor, 0) AS OB_IsDebtor, OH_Code, OH_IsBroker, OH_IsCompetitor,
       OH_IsConsignee, OH_IsConsignor, OH_Language, OH_FullName, OH_IsGlobalAccount, OH_IsForwarder, OH_PK, OH_IsMiscFreightServices,
       OH_IsNationalAccount, OH_IsSalesLead, OH_IsShippingProvider, OH_IsTransportClient, OH_IsWarehouseClient, OH_RL_NKClosestPort,
       ISNULL(RL_IATA, '') AS RL_IATA, ISNULL(RN_Code, '') AS RN_Code, ISNULL(RL_PortName, '') AS RL_PortName, ISNULL(RN_Desc, '') AS RN_Desc
FROM dbo.OrgHeader
LEFT JOIN dbo.RefUnloco ON OH_RL_NKClosestPort = RL_Code
LEFT JOIN dbo.RefCountry ON RL_RN_NKCountryCode = RN_Code
LEFT JOIN
(
  SELECT OB_OH, 1 as OB_IsCreditor
  FROM dbo.OrgCompanyData
  WHERE OB_IsCreditor = 1
  GROUP BY OB_OH
) Creditors ON Creditors.OB_OH = OH_PK
LEFT JOIN
(
  SELECT OB_OH, 1 as OB_IsDebtor
  FROM dbo.OrgCompanyData
  WHERE OB_IsDebtor = 1
  GROUP BY OB_OH
) Debtors ON Debtors.OB_OH = OH_PK";

			using (DataTable table = ZArchitecture.Core.Utilities.GetDataTableFromQuery(sqlText))
			{
				OrgCodeInfo[] result = new OrgCodeInfo[table.Rows.Count];
				for (int i = 0; i < table.Rows.Count; i++)
				{
					result[i] = new OrgCodeInfo(table.Rows[i]);
				}
				return result;
			}
		}
	}
}
