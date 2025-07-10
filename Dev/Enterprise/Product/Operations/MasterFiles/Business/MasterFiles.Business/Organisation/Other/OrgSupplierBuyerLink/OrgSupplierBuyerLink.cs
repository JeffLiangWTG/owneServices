using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgSupplierBuyerLink : AutoOrgSupplierBuyerLink, IHaveRequiredDocuments
	{
		#region Schema

		public new class Schema : AutoOrgSupplierBuyerLink.Schema
		{
			public const string ExpectedShipmentMonthsAddition = "ExpectedShipmentMonthsAddition";
			public const string UpdateShipmentDate = "UpdateShipmentDate";
		}

		#endregion

		public OrgSupplierBuyerLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Logging

		protected override ZString CustomLogReferenceSuffix
		{
			get { return (!IsDeleted && Supplier != null && Buyer != null) ? (NoResString)"Supplier " + Supplier.OH_Code + (NoResString)"/Buyer " + Buyer.OH_Code : ""; }
		}

		#endregion

		#region ReadOnlySecurity

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result =
				Buyer != null && !Buyer.SecurityProvider.HasModifyConsigneeRelationshipsSecurity
				&& Supplier != null && !Supplier.SecurityProvider.HasModifyConsignorRelationshipsSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Replacement Agent

		/// <summary>
		/// This is used when printing Routing Order Replacements - tentatively holds the new agent
		/// </summary>
		public OrgHeader ReplacementAgent { get; set; }

		#endregion

		#region Loading

		public override void OnLoaded()
		{
			base.OnLoaded();
			OriginalOL_InitialShipmentExpected = OL_InitialShipmentExpected;
		}
		ZDateTime OriginalOL_InitialShipmentExpected;

		#endregion

		#region GetExistingOrgSupplierBuyerLink (Static)

		public static ZString GetDefaultINCO(OrgHeader supplier, OrgHeader buyer, ZString importCountryCode, ZString transportMode, ZString containerMode)
		{
			return GetDefaultINCO(GetExistingOrgSupplierBuyerLink(supplier, buyer, importCountryCode), supplier, buyer, transportMode, containerMode);
		}

		public static ZString GetDefaultINCO(OrgSupplierBuyerLink supplierBuyerLink, OrgHeader supplier, OrgHeader buyer, ZString transportMode, ZString containerMode)
		{
			var result = ZString.Empty;
			OrgSupBuyLinkTrnMode linkMode = supplierBuyerLink != null ? supplierBuyerLink.OrgSupBuyLinkTrnModes.Find(transportMode, containerMode) : null;

			if (linkMode != null && !linkMode.PF_IncoTerm.IsEmpty)
			{
				result = linkMode.PF_IncoTerm;
			}
			else if (buyer != null)
			{
				result = buyer.MiscServ?.OM_IMDefaultINCOTerm ?? ZString.Empty;
			}
			else if (supplier != null)
			{
				result = supplier.MiscServ?.OM_EXDefaultIncoTerm ?? ZString.Empty;
			}

			return result;
		}

		public static (ZString incoterm, ZString incotermPlace, ZString incotermMode) GetDefaultINCOTermWithPlaceAndMode(OrgHeader supplier, OrgHeader buyer, ZString importCountryCode, ZString transportMode, ZString containerMode)
		{
			return GetDefaultINCOTermWithPlaceAndMode(GetExistingOrgSupplierBuyerLink(supplier, buyer, importCountryCode), supplier, buyer, transportMode, containerMode);
		}

		public static (ZString incoterm, ZString incotermPlace, ZString incotermMode) GetDefaultINCOTermWithPlaceAndMode(OrgSupplierBuyerLink supplierBuyerLink, OrgHeader supplier, OrgHeader buyer, ZString transportMode, ZString containerMode)
		{
			(ZString incoterm, ZString incotermPlace, ZString incotermMode) result = ("", "", "");
			OrgSupBuyLinkTrnMode linkMode = supplierBuyerLink != null ? supplierBuyerLink.OrgSupBuyLinkTrnModes.Find(transportMode, containerMode) : null;

			if (linkMode != null && !linkMode.PF_IncoTerm.IsEmpty)
			{
				result = (linkMode.PF_IncoTerm, linkMode.PF_IncoTermPlace, linkMode.PF_IncoTermMode);
			}
			else if (buyer != null)
			{
				result = (buyer.MiscServ.OM_IMDefaultINCOTerm, "", "");
			}
			else if (supplier != null)
			{
				result = (supplier.MiscServ.OM_EXDefaultIncoTerm, "", "");
			}
			return result;
		}

		public static OrgSupplierBuyerLink GetExistingOrgSupplierBuyerLink(OrgHeader supplier, OrgHeader buyer, ZString importCountryCode, bool fallbackToBuyerCountry = true)
		{
			OrgSupplierBuyerLink result = null;
			if (supplier != null && buyer != null)
			{
				//should use current country?
				//SQLFilter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_RN_NKImporterCountry, ImporterCountry != null ? ImporterCountry.RN_Code : GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				result = GetExistingOrgSupplierBuyerLinkCore(supplier, buyer, importCountryCode);
				if (fallbackToBuyerCountry && result == null)
				{
					RefCountry buyerCountry = RefCountry.OrganisationCountry(buyer);
					ZString buyerCountryCode = buyerCountry != null ? buyerCountry.RN_Code : ZString.Empty;
					result = GetExistingOrgSupplierBuyerLinkCore(supplier, buyer, buyerCountryCode);
				}
			}
			return result;
		}

		static OrgSupplierBuyerLink GetExistingOrgSupplierBuyerLinkCore(OrgHeader supplier, OrgHeader buyer, ZString importCountryCode)
		{
			OrgSupplierBuyerLink result = null;
			var query = GetQueryForExistingOrgSupplierBuyerLink(supplier, buyer, importCountryCode);
			if (query != ZQuery.NoResultQuery)
			{
				result = supplier.Factory.LoadTop1<OrgSupplierBuyerLink>(query);
			}
			return result;
		}

		public static ZQuery GetQueryForExistingOrgSupplierBuyerLink(OrgHeader supplier, OrgHeader buyer, ZString importCountryCode)
		{
			ZQuery result = ZQuery.NoResultQuery;
			if (!importCountryCode.IsEmpty && supplier != null && buyer != null)
			{
				result = new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, supplier.PK);
				result.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, buyer.PK);
				result.AddToFilter(OrgSupplierBuyerLinkSchema.OL_RN_NKImporterCountry, importCountryCode);
			}
			return result;
		}

		#endregion

		#region New Properties

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region ImporterCountryHasValuationBasisList

		public ZBool ImporterCountryHasValuationBasisList
		{
			get { return fImporterCountryHasValuationBasisList; }
		}

		public ZPropertyInfo ImporterCountryHasValuationBasisListInfo
		{
			get { return GetZPropertyInfo(nameof(ImporterCountryHasValuationBasisList)); }
		}

		#endregion

		#region ImporterCountryDoesNotHaveValuationBasisList

		public ZBool ImporterCountryDoesNotHaveValuationBasisList
		{
			get { return !fImporterCountryHasValuationBasisList; }
		}

		public ZPropertyInfo ImporterCountryDoesNotHaveValuationBasisListInfo
		{
			get { return GetZPropertyInfo(nameof(ImporterCountryDoesNotHaveValuationBasisList)); }
		}

		#endregion

		#region SelectedForPrinting

		/// <summary>
		/// Used for Document Generation when choosing which supplier/buyer to generate
		/// documents for. Default is true.
		/// </summary>
		public ZBool SelectedForPrinting
		{
			get { return fSelectedForPrinting; }
			set
			{
				fSelectedForPrinting = value;
				SelectedForPrintingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SelectedForPrintingInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedForPrinting)); }
		}

		ZBool fSelectedForPrinting;

		#endregion

		#region Expected Shipment Months Addition

		ZString fExpectedShipmentMonthsAddition;
		[List("MonthList")]
		[MaxLength(2)]
		public ZString ExpectedShipmentMonthsAddition
		{
			get { return fExpectedShipmentMonthsAddition; }
			set
			{
				CheckMaximumLength(ExpectedShipmentMonthsAdditionInfo, value);
				SetNonPersistentPropertyValue(ExpectedShipmentMonthsAdditionInfo, ref fExpectedShipmentMonthsAddition, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExpectedShipmentMonthsAddition();
				}
			}
		}

		public ZPropertyInfo ExpectedShipmentMonthsAdditionInfo
		{
			get { return GetZPropertyInfo(Schema.ExpectedShipmentMonthsAddition); }
		}

		protected bool ExpectedShipmentMonthsAddition_ReadOnly
		{
			get { return !UpdateShipmentDate; }
		}

		#endregion

		#region Update Expected Shipment Date

		public ZBool UpdateShipmentDate
		{
			get { return fUpdateShipmentDate; }
			set
			{
				if (SetNonPersistentPropertyValue(UpdateShipmentDateInfo, ref fUpdateShipmentDate, value))
				{
					ExpectedShipmentMonthsAdditionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UpdateShipmentDateInfo
		{
			get { return GetZPropertyInfo(Schema.UpdateShipmentDate); }
		}

		public bool UpdateShipmentDate_ReadOnly
		{
			get { return updateShipmentDate_ReadOnly; }
			set { updateShipmentDate_ReadOnly = value; }
		}

		bool updateShipmentDate_ReadOnly;
		ZBool fUpdateShipmentDate;

		#endregion

		#region AddInfo

		public XmlAddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					var type = ObjectFactory.GetType<Enterprise.Integration.Customs.IOrgSupplierBuyerLinkAddInfo>();
					if (type != null)
					{
						addInfo = (XmlAddInfo)Activator.CreateInstance(type, OL_AddInfoInfo);
						RegisterEditableChildObject(addInfo);
					}
				}
				return addInfo;
			}
		}
		XmlAddInfo addInfo;

		[List("Lookups.RelationTypeList")]
		public override ZString OL_ProductRelation
		{
			get { return base.OL_ProductRelation; }
			set { base.OL_ProductRelation = value; }
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			if (AddInfo != null)
			{
				AddInfo.Deserialise();
			}
		}

		#endregion

		#region OL_AuthorityToLeave

		[List("Lookups.AuthorityToLeaveOptions")]
		public override ZString OL_AuthorityToLeave
		{
			get { return base.OL_AuthorityToLeave; }
			set { base.OL_AuthorityToLeave = value; }
		}

		#endregion

		#endregion

		#region Property Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = base.HumanReadableNameCore;
				if (!IsDeleted && !OL_OH_Supplier.IsEmpty && !OL_OH_Buyer.IsEmpty)
				{
					var buyerCode = Buyer != null ? Buyer.OH_Code : ZString.Empty;
					var suppilerCode = Supplier != null ? Supplier.OH_Code : ZString.Empty;

					result += " (" + suppilerCode + " - " + buyerCode + ")";
				}

				return result;
			}
		}

		[List("Currencies")]
		public override ZString OL_RX_NKDefaultCurrency
		{
			get
			{
				return base.OL_RX_NKDefaultCurrency;
			}
			set
			{
				base.OL_RX_NKDefaultCurrency = value;
			}
		}

		[List("BuyerNotifyParties")]
		public override ZGuid OL_OC_NotifyPartyContact
		{
			get
			{
				return base.OL_OC_NotifyPartyContact;
			}
			set
			{
				base.OL_OC_NotifyPartyContact = value;
			}
		}

		[List("Controllers")]
		public override ZGuid OL_OH_ControllingCustomer
		{
			get
			{
				return base.OL_OH_ControllingCustomer;
			}
			set
			{
				base.OL_OH_ControllingCustomer = value;
			}
		}

		[List("Brokers")]
		public override ZGuid OL_OH_ImportBroker
		{
			get
			{
				return base.OL_OH_ImportBroker;
			}
			set
			{
				base.OL_OH_ImportBroker = value;
			}
		}

		[List("Suppliers")]
		public override ZGuid OL_OH_Supplier
		{
			get { return base.OL_OH_Supplier; }
			set
			{
				if (base.OL_OH_Supplier != value)
				{
					base.OL_OH_Supplier = value;
					MarkSiblingsAsNeedingValidation();
					OrgSupBuyLinkTrnModes.MarkAsNeedingValidation();
				}
			}
		}

		[List("OL_SendImportDocsTo_List")]
		public override ZString OL_SendImportDocsTo
		{
			get
			{
				return base.OL_SendImportDocsTo;
			}
			set
			{
				base.OL_SendImportDocsTo = value;
			}
		}

		[List("Buyers")]
		public override ZGuid OL_OH_Buyer
		{
			get { return base.OL_OH_Buyer; }
			set
			{
				if (base.OL_OH_Buyer != value)
				{
					base.OL_OH_Buyer = value;
					SetDefaultImporterCountry();
					MarkSiblingsAsNeedingValidation();
					OrgSupBuyLinkTrnModes.MarkAsNeedingValidation();
				}
			}
		}

		[List("Countries")]
		public override ZString OL_RN_NKImporterCountry
		{
			get { return base.OL_RN_NKImporterCountry; }
			set
			{
				if (base.OL_RN_NKImporterCountry != value)
				{
					base.OL_RN_NKImporterCountry = value;
					MarkSiblingsAsNeedingValidation();
					OrgSupBuyLinkTrnModes.MarkAsNeedingValidation();
				}
				SetImporterCountryHasValuationBasisList(CountryHasValuationBasisList);
			}
		}

		[List("OL_ValuationBasis_List")]
		public override ZString OL_ValuationBasis
		{
			get { return base.OL_ValuationBasis.ToUpper(); }
			set
			{
				var oldValue = OL_ValuationBasis;
				base.OL_ValuationBasis = value.ToUpper();
				var newVlaue = OL_ValuationBasis;

				if (OL_RN_NKImporterCountry == Core.Constants.CountryCodes.Canada && oldValue != newVlaue)
				{
					var relatedParty = ZString.Empty;
					switch (newVlaue.SubstringSafe(0, 1))
					{
						case "1":
							relatedParty = Customs.RelatedPartyList.Codes.Unrelated;
							break;
						case "2":
							relatedParty = Customs.RelatedPartyList.Codes.Related;
							break;
					}
					OL_RelatedParty = relatedParty;
				}
			}
		}

		[List("OL_RelatedParty_List")]
		[ReadOnlyMember(nameof(OL_RelatedParty_ReadOnly))]
		public override ZString OL_RelatedParty
		{
			get { return base.OL_RelatedParty; }
			set
			{
				var hasChanged = OL_RelatedParty != value;
				base.OL_RelatedParty = value;

				if (OL_RN_NKImporterCountry == Core.Constants.CountryCodes.SouthAfrica && hasChanged)
				{
					if (Customs.RelatedIndicatorList.Codes.Exempt.Equals(value))
					{
						OL_ValuationBasis = ZString.Empty;
					}
				}
			}
		}

		ZBool OL_RelatedParty_ReadOnly
		{
			get { return OL_RN_NKImporterCountry == Core.Constants.CountryCodes.Canada; }
		}

		[List("EFreightStatus_List")]
		public override ZString OL_EFreightStatus
		{
			get { return base.OL_EFreightStatus; }
			set { base.OL_EFreightStatus = value; }
		}

		[ResourceStringData("FED7F8F9-C8E4-4E66-96FA-C5933867777C", Caption = "Buying Commission Percentage", ShortCaption = "BCM")]
		public override ZDecimal OL_BuyingCommissionPercentage
		{
			get => base.OL_BuyingCommissionPercentage;
			set => base.OL_BuyingCommissionPercentage = value;
		}

		public new bool IsCopying
		{
			get { return base.IsCopying; }
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new OrgSupplierBuyerLinkUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class OrgSupplierBuyerLinkUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrgSupplierBuyerLinkUniqueIndexFailureHandler(OrgSupplierBuyerLink link)
			{
				parent = link;
			}

			readonly OrgSupplierBuyerLink parent;

			IEnumerable<string> IUniqueIndexFailureHandler.HandledUniqueIndexNames
			{
				get { yield return OrgSupplierBuyerLinkSchema.Constants.Indexes.FK_UX__OL_OH_Supplier_OL_OH_Buyer_OL_RN_NKImporterCountry; }
			}

			void IUniqueIndexFailureHandler.NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var errorMsg = Res.GetString("fa1ba7ab-cca8-4411-8d4a-02086c99cc28",
					"The value of Supplier Name + Buyer Organization + Importer/Buyer Country must be unique on Supplier/Buyer Relationship. The duplicate values are: (Buyer: {0}, Supplier: {1}, Country: {2}).",
					parent.Buyer.OH_Code, parent.Supplier.OH_Code, parent.OL_RN_NKImporterCountry);

				notifier.ReportError(errorMsg, Res.GetString("6c3fd9f6-534e-4eeb-90f3-d46bdfcdbfd1", "Error"));
			}
		}

		#endregion

		#endregion

		#region OrgSupBuyLinkTrnModes

		[ChildEditable(true)]
		public OrgSupBuyLinkTrnModeDependentCollection OrgSupBuyLinkTrnModes
		{
			get { return orgSupBuyLinkTrnModes ?? (orgSupBuyLinkTrnModes = GetNewOrgSupBuyLinkTrnModeDependentCollection()); }
		}
		OrgSupBuyLinkTrnModeDependentCollection orgSupBuyLinkTrnModes;

		OrgSupBuyLinkTrnModeDependentCollection GetNewOrgSupBuyLinkTrnModeDependentCollection()
		{
			OrgSupBuyLinkTrnModeDependentCollection result = new OrgSupBuyLinkTrnModeDependentCollection(this, Factory);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region OrgBuyerSupplierLinkPackPivot

		[ChildEditable(true)]
		public OrgBuyerSupplierLinkPackPivotCollection PackPivots
		{
			get { return orgBuyerSupplierLinkPackPivot ?? (orgBuyerSupplierLinkPackPivot = GetNewOrgBuyerSupplierLinkPackPivotCollection()); }
		}
		OrgBuyerSupplierLinkPackPivotCollection orgBuyerSupplierLinkPackPivot;

		OrgBuyerSupplierLinkPackPivotCollection GetNewOrgBuyerSupplierLinkPackPivotCollection()
		{
			OrgBuyerSupplierLinkPackPivotCollection result = new OrgBuyerSupplierLinkPackPivotCollection(this);
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region OrgSupplierBuyerLinkTolerance

		[ChildEditable(true)]
		public OrgSupplierBuyerLinkToleranceCollection Tolerances
		{
			get { return tolerances ?? (tolerances = GetNewOrgSupplierBuyerLinkToleranceCollection()); }
		}
		OrgSupplierBuyerLinkToleranceCollection tolerances;

		OrgSupplierBuyerLinkToleranceCollection GetNewOrgSupplierBuyerLinkToleranceCollection()
		{
			var result = new OrgSupplierBuyerLinkToleranceCollection(this);
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region Collections for FindBox and DropEdit binding

		#region AirShippingProviders

		protected AirShippingProviderCollection fAirShippingProviders;
		public AirShippingProviderCollection AirShippingProviders
		{
			get
			{
				if (fAirShippingProviders == null)
				{
					fAirShippingProviders = new AirShippingProviderCollection(Factory);
				}
				return fAirShippingProviders;
			}
		}

		#endregion

		#region Brokers

		protected BrokerCollection fBrokers;
		public BrokerCollection Brokers
		{
			get
			{
				if (fBrokers == null)
				{
					fBrokers = new BrokerCollection(Factory);
				}
				return fBrokers;
			}
		}

		#endregion

		#region Buyers

		protected ConsigneeCollection fBuyers;
		public ConsigneeCollection Buyers
		{
			get
			{
				if (fBuyers == null)
				{
					fBuyers = new ConsigneeCollection(Factory);
				}
				return fBuyers;
			}
		}

		#endregion

		#region Controllers

		protected OrganisationsFindBoxCollection fControllers;
		public OrganisationsFindBoxCollection Controllers
		{
			get
			{
				if (fControllers == null)
				{
					fControllers = new OrganisationsFindBoxCollection(Factory);
				}
				return fControllers;
			}
		}

		#endregion

		#region Countries

		protected RefCountryCollection fCountries;
		public RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}
				return fCountries;
			}
		}

		#endregion

		#region Currencies

		protected RefCurrencyCollection fCurrencies;
		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}

		#endregion

		#region NotifyParties

		public OrgContactDependentCollection BuyerNotifyParties
		{
			get { return Buyer != null ? Buyer.GetActiveContacts() : new OrgContactDependentCollection(Factory); }
		}

		public OrgContactDependentCollection SupplierNotifyParties
		{
			get { return Supplier != null ? Supplier.Contacts : new OrgContactDependentCollection(Factory); }
		}
		#endregion

		#region SeaShippingProviders

		protected SeaShippingProviderCollection fSeaShippingProviders;
		public SeaShippingProviderCollection SeaShippingProviders
		{
			get
			{
				if (fSeaShippingProviders == null)
				{
					fSeaShippingProviders = new SeaShippingProviderCollection(Factory);
				}
				return fSeaShippingProviders;
			}
		}

		#endregion

		#region Suppliers

		protected ConsignorCollection fSuppliers;
		public ConsignorCollection Suppliers
		{
			get
			{
				if (fSuppliers == null)
				{
					fSuppliers = new ConsignorCollection(Factory);
				}
				return fSuppliers;
			}
		}

		#endregion

		#region Forwarders

		protected ForwarderCollection fForwarders;
		public ForwarderCollection Forwarders
		{
			get
			{
				if (fForwarders == null)
				{
					fForwarders = new ForwarderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, true));
				}
				return fForwarders;
			}
		}

		#endregion

		#region Month Listing

		CodeDescriptionPairList fMonthList;
		public CodeDescriptionPairList MonthList
		{
			get
			{
				if (fMonthList == null)
				{
					fMonthList = new CodeDescriptionPairList();
					for (int i = 1; i <= 12; i++)
					{ fMonthList.Add(new CodeDescriptionPair(i.ToString(), i.ToString())); }
				}
				return fMonthList;
			}
		}

		#endregion

		#endregion

		#region CodeLists

		public CodeDescriptionPairList OL_ValuationBasis_List
		{
			get
			{
				CodeDescriptionPairList result;
				switch (OL_RN_NKImporterCountry)
				{
					case Enterprise.Core.Constants.CountryCodes.Australia:
						result = new Customs.AU.ValuationBasisList();
						break;
					case Enterprise.Core.Constants.CountryCodes.SouthAfrica:
						result = new Customs.ZA.ValuationCodeList();
						break;
					case Enterprise.Core.Constants.CountryCodes.Canada:
						result = ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICAValuationBasisListProvider>().GetCodeDescriptionPairList() as CodeDescriptionPairList;
						break;
					case Enterprise.Core.Constants.CountryCodes.Netherlands:
						result = new Customs.EU.ValuationMethodList();
						break;
					default:
						result = null;
						break;
				}
				return result ?? new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList OL_RelatedParty_List
		{
			get
			{
				CodeDescriptionPairList result;
				switch (OL_RN_NKImporterCountry)
				{
					case Enterprise.Core.Constants.CountryCodes.SouthAfrica:
						result = new Customs.RelatedIndicatorList();
						break;
					case Enterprise.Core.Constants.CountryCodes.NewZealand:
						result = new Customs.NZ.RelatedPartyList();
						break;
					case Enterprise.Core.Constants.CountryCodes.Brazil:
						result = new Customs.BR.RelatedIndicatorList();
						break;
					default:
						result = null;
						break;
				}
				return result ?? Factory.GetCachedValue<Customs.RelatedPartyList>();
			}
		}

		public CodeDescriptionPairList OL_SendImportDocsTo_List
		{
			get { return OrgCodeLists.SendImportDocsTo_List; }
		}

		public CodeDescriptionPairList EFreightStatus_List
		{
			get
			{
				return Factory.GetCachedValue("EFreightStatus_List",
					() => new CodeDescriptionPairList(OLookUpEditType.EFreightStatus));
			}
		}

		#endregion

		#region Calendar Reminders

#if DEBUG
		internal
#endif
		Reminder InitialShipmentExpectedReminder
		{
			get
			{
				string subject = Res.GetString("ea39df2b-5c4b-4c2a-93b5-30000708fa88", "Initial shipment expected for Buyer {0} / Supplier {1}", Buyer.OH_FullNameTruncated, Supplier.OH_FullNameTruncated);
				string body = Res.GetString("ea39df2b-5c4b-4c2a-93b5-30000708fa88", "Initial shipment expected for Buyer {0} / Supplier {1}", Buyer.OH_FullNameTruncated, Supplier.OH_FullNameTruncated) + "\r\n\r\n";

				OrgContact buyerContact = new DefaultContactFinder(Buyer).DefaultContact(ContactType.Consignee);
				body += Res.GetString("4b5e1bbf-913c-440d-8ade-5ab41b013408", "Buyer Contact Details\r\n\r\nContact: {0}\r\nEmail: {1}\r\nFax: {2}\r\nPhone: {3}", buyerContact.OC_ContactName, buyerContact.OC_Email, buyerContact.OC_Fax, buyerContact.OC_Phone) + "\r\n\r\n";

				OrgContact supplierContact = new DefaultContactFinder(Supplier).DefaultContact(ContactType.Consignor);
				body += Res.GetString("40210b15-eafb-465f-939f-c989f9ac6c0e", "Supplier Contact Details\r\n\r\nContact: {0}\r\nEmail: {1}\r\nFax: {2}\r\nPhone: {3}", supplierContact.OC_ContactName, supplierContact.OC_Email, supplierContact.OC_Fax, supplierContact.OC_Phone) + "\r\n\r\n";

				return new Reminder(string.Format("InitialShipment-{0}-{1}", Buyer.OH_FullNameTruncated, Supplier.OH_FullNameTruncated), PK, new ZString(TableName), DateTimeKind.Local, OL_InitialShipmentExpected, OL_InitialShipmentExpected, subject, body);// vCal Reminder Key
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && ShouldCreateReminder)
			{
				InitialShipmentExpectedReminder.CreateAppointment();
			}
		}

		bool ShouldCreateReminder
		{
			get { return (OL_InitialShipmentExpected != OriginalOL_InitialShipmentExpected) && (Buyer != null && Supplier != null); }
		}

		#endregion

		#region Set Defaults

		ZBool fSettingDefaults;
		public ZBool SettingDefaults
		{
			get { return fSettingDefaults; }
		}

		protected override void SetDefaultValues()
		{
			fSettingDefaults = true;
			try
			{
				base.SetDefaultValues();

				OrgSupBuyLinkTrnModes.AddNew();

				OL_OH_Supplier = ZGuid.Empty;
				OL_OH_Buyer = ZGuid.Empty;
				OL_RX_NKDefaultCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				OL_SendImportDocsTo = "";
				UpdateShipmentDate = false;
				OL_RN_NKImporterCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				SetImporterCountryHasValuationBasisList(false);
			}
			finally
			{
				fSettingDefaults = false;
			}
		}

		protected void SetDefaultImporterCountry()
		{
			if (Buyer?.UNLOCO?.Country != null)
			{
				OL_RN_NKImporterCountry = Buyer.UNLOCO.RL_RN_NKCountryCode;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				OrgSupBuyLinkTrnModes.RemoveAndDeleteAll();
				PackPivots.DeleteAll();
			}
			base.Delete();
		}
		#endregion

		#region Valuation Basis

		protected ZBool fImporterCountryHasValuationBasisList;
		protected void SetImporterCountryHasValuationBasisList(ZBool value)
		{
			if (SettingDefaults || fImporterCountryHasValuationBasisList != value)
			{
				fImporterCountryHasValuationBasisList = value;
				ImporterCountryHasValuationBasisListInfo.RefreshBinding();
				ImporterCountryHasValuationBasisListInfo.RefreshBinding();
			}
		}

		protected bool CountryHasValuationBasisList
		{
			get { return OL_ValuationBasis_List.Count > 0; }
		}

		#endregion

		#region Expected Shipment Months

		public void ResetExpectedShipmentMonthsAddition()
		{
			ExpectedShipmentMonthsAddition = OL_InitialShipmentExpected.IsEmpty ? OrgSupplierBuyerLinkDependentCollection.DefaultExpectedShipmentMonths.ToString() : "";
		}

		#endregion

		#region IHaveRequiredDocuments Members

		public ZString UniqueConsignRef
		{
			get { return null; }
		}

		public ZString HouseBill
		{
			get { return null; }
		}

		public ZString MasterBill
		{
			get { return null; }
		}

		public OrgHeader ExportBroker
		{
			get { return null; }
		}

		public ZString TableCode
		{
			get { return OrgSupplierBuyerLinkSchema.Constants.Prefix; }
		}

		public IReadOnlyList<ZString> AdditionalRefTypes
		{
			get { return new ZString[] { Constants.ReferenceTypes.SupplyChainLogistics }; }
		}

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		public BusinessObject UltimateDocumentParent
		{
			get { return this; }
		}

		#endregion

		#region Implementation

		void MarkSiblingsAsNeedingValidation()
		{
			if (Buyer != null)
			{
				Buyer.SupplierLinks.MarkAsNeedingValidation();
			}

			if (Supplier != null)
			{
				Supplier.BuyerLinks.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			AuthorisedToLeaveLogger.Log(this, OL_AuthorityToLeaveInfo);
		}

		#endregion
	}
}
