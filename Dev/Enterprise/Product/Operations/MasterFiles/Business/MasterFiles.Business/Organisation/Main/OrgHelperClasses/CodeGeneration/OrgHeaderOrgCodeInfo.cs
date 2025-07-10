using System;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgHeaderOrgCodeInfo : IOrgCodeInfo
	{
		readonly OrgHeader organisation;

		public OrgHeaderOrgCodeInfo(OrgHeader organisation)
		{
			this.organisation = organisation;
		}

		public string CountryCode
		{
			get { return organisation.CountryCode; }
		}

		public string CountryName
		{
			get { return organisation.CountryName.GetUnresolvedString(); }
		}

		public string IataCode
		{
			get
			{
				var unloco = organisation.UNLOCO;
				return unloco == null ? string.Empty : unloco.RL_IATA.ToString();
			}
		}

		public bool IsCreditorForAnyCompany
		{
			get
			{
				var filter = new ZQuery(OrgCompanyDataSchema.OB_OH, PK);
				filter.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
				return organisation.Factory.LoadTop1<OrgCompanyData>(filter) != null;
			}
		}

		public bool IsDebtorForAnyCompany
		{
			get
			{
				var filter = new ZQuery(OrgCompanyDataSchema.OB_OH, PK);
				filter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
				return organisation.Factory.LoadTop1<OrgCompanyData>(filter) != null;
			}
		}

		public string OH_Code
		{
			get { return organisation.OH_Code; }
		}

		public string OH_FullName
		{
			get { return organisation.OH_FullNameTruncated; }
		}

		public bool OH_IsBroker
		{
			get { return organisation.OH_IsBroker; }
		}

		public bool OH_IsCompetitor
		{
			get { return organisation.OH_IsCompetitor; }
		}

		public bool OH_IsConsignee
		{
			get { return organisation.OH_IsConsignee; }
		}

		public bool OH_IsConsignor
		{
			get { return organisation.OH_IsConsignor; }
		}

		public bool OH_IsForwarder
		{
			get { return organisation.OH_IsForwarder; }
		}

		public bool OH_IsGlobalAccount
		{
			get { return organisation.OH_IsGlobalAccount; }
		}

		public bool OH_IsMiscFreightServices
		{
			get { return organisation.OH_IsMiscFreightServices; }
		}

		public bool OH_IsNationalAccount
		{
			get { return organisation.OH_IsNationalAccount; }
		}

		public bool OH_IsSalesLead
		{
			get { return organisation.OH_IsSalesLead; }
		}

		public bool OH_IsShippingProvider
		{
			get { return organisation.OH_IsShippingProvider; }
		}

		public bool OH_IsTransportClient
		{
			get { return organisation.OH_IsTransportClient; }
		}

		public bool OH_IsWarehouseClient
		{
			get { return organisation.OH_IsWarehouseClient; }
		}

		public string OH_Language
		{
			get { return organisation.OH_Language; }
		}

		public Guid PK
		{
			get { return organisation.PK.ToGuid(); }
		}

		public string PortName
		{
			get { return organisation.PortName; }
		}

		public string UnlocoCode
		{
			get { return organisation.UNLOCO != null ? organisation.OH_RL_NKClosestPort.ToString() : string.Empty; } // Only return a value if it is valid.
		}

		public string InvalidUnlocoCode
		{
			get { return organisation.UNLOCO == null ? organisation.OH_RL_NKClosestPort.ToString() : string.Empty; }
		}
	}
}
