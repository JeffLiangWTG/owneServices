using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrganisationSecurityContainerControl : OrganisationContainerControl
	{
		public OrganisationSecurityContainerControl()
		{
			InitializeComponent();
		}

		#region Overrides

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				SecurityPanel.Visible = IsSecurityDenied;
			}
		}

		#endregion

		#region Parent Header

		OrganisationSecurityProvider SecurityProvider
		{
			get { return ((OrgHeader)CurrentDataItem).SecurityProvider; }
		}

		#endregion

		protected virtual Type TypeOfSecurityContainerControl
		{
			get { return typeof(OrganisationSecurityContainerControl); }
		}

		#region IsSecurityDenied

		protected bool IsSecurityDenied
		{
			get
			{
				bool result = false;

				foreach (PropertyInfo info in ActiveSecurityItemsList)
				{
					result |= !SecurityProvider.HasSecurityByName(info.Name);
				}

				return result;
			}
		}

		List<PropertyInfo> ActiveSecurityItemsList
		{
			get
			{
				if (fActiveSecurityItemsList == null)
				{
					fActiveSecurityItemsList = new List<PropertyInfo>();
					foreach (PropertyInfo controlProperty in TypeOfSecurityContainerControl.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
					{
						if (controlProperty.Name.StartsWith((NoResString)"Is") && controlProperty.PropertyType == typeof(bool) && (bool)controlProperty.GetValue(this, null))
						{
							fActiveSecurityItemsList.Add(controlProperty);
						}
					}
				}

				return fActiveSecurityItemsList;
			}
		}
		List<PropertyInfo> fActiveSecurityItemsList;

		#endregion

		#region IsModifyDetails

		[System.ComponentModel.Description("If this control is dependent on Details --> Security, then set this to true.")]
		public bool IsModifyDetails
		{
			get { return fIsModifyDetails; }
			set { fIsModifyDetails = value; }
		}
		bool fIsModifyDetails;

		#endregion

		#region IsModifyDetailsNameAndAddress

		[System.ComponentModel.Description("If this control is dependent on Details --> Name And Address Security, then set this to true.")]
		public bool IsModifyDetailsNameAndAddress
		{
			get { return fIsModifyDetailsNameAndAddress; }
			set { fIsModifyDetailsNameAndAddress = value; }
		}
		bool fIsModifyDetailsNameAndAddress;

		#endregion

		#region IsModifyDetailsPhFaxWebDetails

		[System.ComponentModel.Description("If this control is dependent on Details --> Ph Fax Web Details Security, then set this to true.")]
		public bool IsModifyDetailsPhFaxWebDetails
		{
			get { return fIsModifyDetailsPhFaxWebDetails; }
			set { fIsModifyDetailsPhFaxWebDetails = value; }
		}
		bool fIsModifyDetailsPhFaxWebDetails;

		#endregion

		#region IsModifyDetailsStaffAssignments

		[System.ComponentModel.Description("If this control is dependent on Details --> Staff Assignments Security, then set this to true.")]
		public bool IsModifyDetailsStaffAssignments
		{
			get { return fIsModifyDetailsStaffAssignments; }
			set { fIsModifyDetailsStaffAssignments = value; }
		}
		bool fIsModifyDetailsStaffAssignments;

		#endregion

		#region IsModifyDetailsWebSecurity

		[System.ComponentModel.Description("If this control is dependent on Details --> Web Security Security, then set this to true.")]
		public bool IsModifyDetailsWebSecurity
		{
			get { return fIsModifyDetailsWebSecurity; }
			set { fIsModifyDetailsWebSecurity = value; }
		}
		bool fIsModifyDetailsWebSecurity;

		#endregion

		#region IsModifyDetailsWebSecurity

		[System.ComponentModel.Description("If this control is dependent on Details --> Web Security Security, then set this to true.")]
		public bool IsNewDetailsWebSecurity
		{
			get { return fIsNewDetailsWebSecurity; }
			set { fIsNewDetailsWebSecurity = value; }
		}
		bool fIsNewDetailsWebSecurity;

		#endregion

		#region IsModifyDetailsRatingAndTariffs

		[System.ComponentModel.Description("If this control is dependent on Details --> Rating And Tariffs Security, then set this to true.")]
		public bool IsModifyDetailsRatingAndTariffs
		{
			get { return fIsModifyDetailsRatingAndTariffs; }
			set { fIsModifyDetailsRatingAndTariffs = value; }
		}
		bool fIsModifyDetailsRatingAndTariffs;

		#endregion

		#region IsModifyDetailsOrganisationType

		[System.ComponentModel.Description("If this control is dependent on Details --> Organisation Type Security, then set this to true.")]
		public bool IsModifyDetailsOrganisationType
		{
			get { return fIsModifyDetailsOrganisationType; }
			set { fIsModifyDetailsOrganisationType = value; }
		}
		bool fIsModifyDetailsOrganisationType;

		#endregion

		#region IsModifyDetailsCustomFields

		[System.ComponentModel.Description("If this control is dependent on Custom Defined Fields --> Security, then set this to true.")]
		public bool IsModifyDetailsCustomFields
		{
			get { return fIsModifyDetailsCustomFields; }
			set { fIsModifyDetailsCustomFields = value; }
		}
		bool fIsModifyDetailsCustomFields;

		#endregion

		#region IsModifyConfigFinancialRegistrationNumbersSecurity

		[System.ComponentModel.Description("If this control is dependent on Details --> Primary Registration Number Security, then set this to true.")]
		public bool IsModifyConfigFinancialRegistrationNumbersSecurity { get; set; }

		#endregion

		#region IsNewConfigModifyFinancialRegistrationNosSecurity

		[System.ComponentModel.Description("If this control is dependent on Details --> Primary Registration Number Security (for new records), then set this to true.")]
		public bool IsNewConfigModifyFinancialRegistrationNosSecurity { get; set; }

		#endregion

		#region IsModifyAddress

		[System.ComponentModel.Description("If this control is dependent on Address --> Security, then set this to true.")]
		public bool IsModifyAddress
		{
			get { return fIsModifyAddress; }
			set { fIsModifyAddress = value; }
		}
		bool fIsModifyAddress;

		#endregion

		#region IsModifyAddressCapabilities

		[System.ComponentModel.Description("If this control is dependent on Address --> Address Capabilities, then set this to true.")]
		public bool IsModifyAddressCapabilities
		{
			get { return fIsModifyAddressCapabilities; }
			set { fIsModifyAddressCapabilities = value; }
		}
		bool fIsModifyAddressCapabilities;

		#endregion

		#region IsModifyAddressCapabilitiesARAP

		[System.ComponentModel.Description("If this control is dependent on Address --> Address Capabilities -> AR/AP Security, then set this to true.")]
		public bool IsModifyAddressCapabilitiesARAP
		{
			get { return fIsModifyAddressCapabilitiesARAP; }
			set { fIsModifyAddressCapabilitiesARAP = value; }
		}
		bool fIsModifyAddressCapabilitiesARAP;

		#endregion

		#region IsModifyAddressCapabilitiesNonARAP

		[System.ComponentModel.Description("If this control is dependent on Address --> Address Capabilities -> Non AR/AP Security, then set this to true.")]
		public bool IsModifyAddressCapabilitiesNonARAP
		{
			get { return fIsModifyAddressCapabilitiesNonARAP; }
			set { fIsModifyAddressCapabilitiesNonARAP = value; }
		}
		bool fIsModifyAddressCapabilitiesNonARAP;

		#endregion

		#region IsModifyContact

		[System.ComponentModel.Description("If this control is dependent on Contact --> Security, then set this to true.")]
		public bool IsModifyContact
		{
			get { return fIsModifyContact; }
			set { fIsModifyContact = value; }
		}
		bool fIsModifyContact;

		#endregion

		#region IsModifyContactContactDetails

		[System.ComponentModel.Description("If this control is dependent on Contact --> Contact Details Security, then set this to true.")]
		public bool IsModifyContactContactDetails
		{
			get { return fIsModifyContactContactDetails; }
			set { fIsModifyContactContactDetails = value; }
		}
		bool fIsModifyContactContactDetails;

		#endregion

		#region IsModifyContactPersonalInformation

		[System.ComponentModel.Description("If this control is dependent on Contact --> Personal Information Security, then set this to true.")]
		public bool IsModifyContactPersonalInformation
		{
			get { return fIsModifyContactPersonalInformation; }
			set { fIsModifyContactPersonalInformation = value; }
		}
		bool fIsModifyContactPersonalInformation;

		#endregion

		#region IsModifyContactDocDeliveryDetails

		[System.ComponentModel.Description("If this control is dependent on Contact --> Doc Delivery Details Security, then set this to true.")]
		public bool IsModifyContactDocDeliveryDetails
		{
			get { return fIsModifyContactDocDeliveryDetails; }
			set { fIsModifyContactDocDeliveryDetails = value; }
		}
		bool fIsModifyContactDocDeliveryDetails;

		#endregion

		#region IsModifyReceivables

		[System.ComponentModel.Description("If this control is dependent on Receivables --> Security, then set this to true.")]
		public bool IsModifyReceivables
		{
			get { return fIsModifyReceivables; }
			set { fIsModifyReceivables = value; }
		}
		bool fIsModifyReceivables;

		#endregion

		#region IsModifyReceivablesConfig

		[System.ComponentModel.Description("If this control is dependent on Receivables --> Config Security, then set this to true.")]
		public bool IsModifyReceivablesConfig
		{
			get { return fIsModifyReceivablesConfig; }
			set { fIsModifyReceivablesConfig = value; }
		}
		bool fIsModifyReceivablesConfig;

		#endregion

		#region IsModifyReceivablesInvoicing

		[System.ComponentModel.Description("If this control is dependent on Receivables --> Invoicing Security, then set this to true.")]
		public bool IsModifyReceivablesInvoicing
		{
			get { return fIsModifyReceivablesInvoicing; }
			set { fIsModifyReceivablesInvoicing = value; }
		}
		bool fIsModifyReceivablesInvoicing;

		#endregion

		#region IsModifyPayables

		[System.ComponentModel.Description("If this control is dependent on Payables --> Security, then set this to true.")]
		public bool IsModifyPayables
		{
			get { return fIsModifyPayables; }
			set { fIsModifyPayables = value; }
		}
		bool fIsModifyPayables;

		#endregion

		#region IsModifyConsignor

		[System.ComponentModel.Description("If this control is dependent on Consignor --> Security, then set this to true.")]
		public bool IsModifyConsignor
		{
			get { return fIsModifyConsignor; }
			set { fIsModifyConsignor = value; }
		}
		bool fIsModifyConsignor;

		#endregion

		#region IsModifyConsignorDetails

		[System.ComponentModel.Description("If this control is dependent on Consignor --> Details Security, then set this to true.")]
		public bool IsModifyConsignorDetails
		{
			get { return fIsModifyConsignorDetails; }
			set { fIsModifyConsignorDetails = value; }
		}
		bool fIsModifyConsignorDetails;

		#endregion

		#region IsModifyConsignorRelationships

		[System.ComponentModel.Description("If this control is dependent on Consignor --> Relationships Security, then set this to true.")]
		public bool IsModifyConsignorRelationships
		{
			get { return fIsModifyConsignorRelationships; }
			set { fIsModifyConsignorRelationships = value; }
		}
		bool fIsModifyConsignorRelationships;

		#endregion

		#region IsModifyConsignorExporterScheme

		[System.ComponentModel.Description("If this control is dependent on Consignor --> Exporter Scheme Security, then set this to true.")]
		public bool IsModifyConsignorExporterScheme
		{
			get { return fIsModifyConsignorExporterScheme; }
			set { fIsModifyConsignorExporterScheme = value; }
		}
		bool fIsModifyConsignorExporterScheme;

		#endregion

		#region IsModifyConsignee

		[System.ComponentModel.Description("If this control is dependent on Consignee --> Security, then set this to true.")]
		public bool IsModifyConsignee
		{
			get { return fIsModifyConsignee; }
			set { fIsModifyConsignee = value; }
		}
		bool fIsModifyConsignee;

		#endregion

		#region IsModifyConsigneeDetails

		[System.ComponentModel.Description("If this control is dependent on Consignee --> Details Security, then set this to true.")]
		public bool IsModifyConsigneeDetails
		{
			get { return fIsModifyConsigneeDetails; }
			set { fIsModifyConsigneeDetails = value; }
		}
		bool fIsModifyConsigneeDetails;

		#endregion

		#region IsModifyConsigneeRelationships

		[System.ComponentModel.Description("If this control is dependent on Consignee --> Relationships Security, then set this to true.")]
		public bool IsModifyConsigneeRelationships
		{
			get { return fIsModifyConsigneeRelationships; }
			set { fIsModifyConsigneeRelationships = value; }
		}
		bool fIsModifyConsigneeRelationships;

		#endregion

		#region IsModifyConsigneeLandedCosting

		[System.ComponentModel.Description("If this control is dependent on Consignee --> Landed Costing Security, then set this to true.")]
		public bool IsModifyConsigneeLandedCosting
		{
			get { return fIsModifyConsigneeLandedCosting; }
			set { fIsModifyConsigneeLandedCosting = value; }
		}
		bool fIsModifyConsigneeLandedCosting;

		#endregion

		#region IsModifyWarehouse

		[System.ComponentModel.Description("If this control is dependent on Warehouse --> Security, then set this to true.")]
		public bool IsModifyWarehouse
		{
			get { return fIsModifyWarehouse; }
			set { fIsModifyWarehouse = value; }
		}
		bool fIsModifyWarehouse;

		#endregion

		#region IsModifyForwarder

		[System.ComponentModel.Description("If this control is dependent on Forwarder --> Security, then set this to true.")]
		public bool IsModifyForwarder
		{
			get { return fIsModifyForwarder; }
			set { fIsModifyForwarder = value; }
		}
		bool fIsModifyForwarder;

		#endregion

		#region IsModifyForwarderDetails

		[System.ComponentModel.Description("If this control is dependent on Forwarder --> Details Security, then set this to true.")]
		public bool IsModifyForwarderDetails
		{
			get { return fIsModifyForwarderDetails; }
			set { fIsModifyForwarderDetails = value; }
		}
		bool fIsModifyForwarderDetails;

		#endregion

		#region IsModifyForwarderProfitShare

		[System.ComponentModel.Description("If this control is dependent on Forwarder --> Profit Share Security, then set this to true.")]
		public bool IsModifyForwarderProfitShare
		{
			get { return fIsModifyForwarderProfitShare; }
			set { fIsModifyForwarderProfitShare = value; }
		}
		bool fIsModifyForwarderProfitShare;

		#endregion

		#region IsModifyCarrier

		[System.ComponentModel.Description("If this control is dependent on Carrier --> Security, then set this to true.")]
		public bool IsModifyCarrier
		{
			get { return fIsModifyCarrier; }
			set { fIsModifyCarrier = value; }
		}
		bool fIsModifyCarrier;

		#endregion

		#region IsModifyServices

		[System.ComponentModel.Description("If this control is dependent on Services --> Security, then set this to true.")]
		public bool IsModifyServices
		{
			get { return fIsModifyServices; }
			set { fIsModifyServices = value; }
		}
		bool fIsModifyServices;

		#endregion

		#region IsModifySales

		[System.ComponentModel.Description("If this control is dependent on Sales --> Security, then set this to true.")]
		public bool IsModifySales
		{
			get { return fIsModifySales; }
			set { fIsModifySales = value; }
		}
		bool fIsModifySales;

		#endregion

		#region IsModifySalesClientSummary

		[System.ComponentModel.Description("If this control is dependent on Sales --> Client Summary Security, then set this to true.")]
		public bool IsModifySalesClientSummary
		{
			get { return fIsModifySalesClientSummary; }
			set { fIsModifySalesClientSummary = value; }
		}
		bool fIsModifySalesClientSummary;

		#endregion

		#region IsModifySalesOpportunityManagement

		[System.ComponentModel.Description("If this control is dependent on Sales --> Opportunity Management Security, then set this to true.")]
		public bool IsModifySalesOpportunityManagement
		{
			get { return fIsModifySalesOpportunityManagement; }
			set { fIsModifySalesOpportunityManagement = value; }
		}
		bool fIsModifySalesOpportunityManagement;

		#endregion

		#region IsModifySalesTradeProfile

		[System.ComponentModel.Description("If this control is dependent on Sales --> Value Analysis Security, then set this to true.")]
		public bool IsModifySalesTradeProfile
		{
			get { return fIsModifySalesTradeProfile; }
			set { fIsModifySalesTradeProfile = value; }
		}
		bool fIsModifySalesTradeProfile;

		#endregion

		#region IsModifySalesClientRelationship

		[System.ComponentModel.Description("If this control is dependent on Sales --> Client Relationship Security, then set this to true.")]
		public bool IsModifySalesClientRelationship
		{
			get { return fIsModifySalesClientRelationship; }
			set { fIsModifySalesClientRelationship = value; }
		}
		bool fIsModifySalesClientRelationship;

		#endregion

		#region IsModifyCompetitor

		[System.ComponentModel.Description("If this control is dependent on Competitor --> Security, then set this to true.")]
		public bool IsModifyCompetitor
		{
			get { return fIsModifyCompetitor; }
			set { fIsModifyCompetitor = value; }
		}
		bool fIsModifyCompetitor;

		#endregion

		#region IsModifyCustom

		[System.ComponentModel.Description("If this control is dependent on Custom --> Security, then set this to true.")]
		public bool IsModifyCustom
		{
			get { return fIsModifyCustom; }
			set { fIsModifyCustom = value; }
		}
		bool fIsModifyCustom;

		#endregion

		#region IsModifyConfig

		[System.ComponentModel.Description("If this control is dependent on Config --> Security, then set this to true.")]
		public bool IsModifyConfig
		{
			get { return fIsModifyConfig; }
			set { fIsModifyConfig = value; }
		}
		bool fIsModifyConfig;

		#endregion

		#region IsModifyConfigRegistrationNumbers

		[System.ComponentModel.Description("If this control is dependent on Config --> Registration Numbers Security, then set this to true.")]
		public bool IsModifyConfigRegistrationNumbers
		{
			get { return fIsModifyConfigRegistrationNumbers; }
			set { fIsModifyConfigRegistrationNumbers = value; }
		}
		bool fIsModifyConfigRegistrationNumbers;

		#endregion

		#region IsModifyConfigEDICodeMapping

		[System.ComponentModel.Description("If this control is dependent on Config --> EDI Code Mapping Security, then set this to true.")]
		public bool IsModifyConfigEDICodeMapping
		{
			get { return fIsModifyConfigEDICodeMapping; }
			set { fIsModifyConfigEDICodeMapping = value; }
		}
		bool fIsModifyConfigEDICodeMapping;

		#endregion

		#region IsModifyConfigBrandsAndCompanyNames

		[System.ComponentModel.Description("If this control is dependent on Config --> Brands And Company Names Security, then set this to true.")]
		public bool IsModifyConfigBrandsAndCompanyNames
		{
			get { return fIsModifyConfigBrandsAndCompanyNames; }
			set { fIsModifyConfigBrandsAndCompanyNames = value; }
		}
		bool fIsModifyConfigBrandsAndCompanyNames;

		#endregion

		#region IsModifyConfigGeneral

		[System.ComponentModel.Description("If this control is dependent on Config --> General Security, then set this to true.")]
		public bool IsModifyConfigGeneral
		{
			get { return fIsModifyConfigGeneral; }
			set { fIsModifyConfigGeneral = value; }
		}
		bool fIsModifyConfigGeneral;

		#endregion

		#region IsModifyBranchProxies

		[System.ComponentModel.Description("If this control is dependent on Organisation Proxies --> Edit Branch Proxies, then set this to true.")]
		public bool IsModifyBranchProxies => true;

		#endregion

		#region IsModifyCompanyProxies

		[System.ComponentModel.Description("If this control is dependent on Organisation Proxies --> Edit Company Proxies, then set this to true.")]
		public bool IsModifyCompanyProxies => true;

		#endregion
	}
}
