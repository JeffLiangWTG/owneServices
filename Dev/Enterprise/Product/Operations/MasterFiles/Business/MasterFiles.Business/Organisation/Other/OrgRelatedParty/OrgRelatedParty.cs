using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgRelatedParty : AutoOrgRelatedParty
	{
		public OrgRelatedParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		new class Schema : AutoOrgRelatedParty.Schema
		{
			public const string PartyTypeDescription = "PartyTypeDescription";
			public const string RelatedPartyName = "RelatedPartyName";
			public const string CalculatedDirection = "CalculatedDirection";
			public const string CompanyLevel = "CompanyLevel";
			public const string ParentName = "ParentName";
		}

		#endregion

		#region Load

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetCalculatedDirection();
		}

		#endregion

		#region Save

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && ParentOrg != null)
			{
				ParentOrg.AllRelatedParties.Load();
				ParentOrg.AllParentParties.Load();
			}

			if (!saveSucceeded && !IsInDatabase)
			{
				AddParentRelatedRelationshipLog(LoggingAction.Detach);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsDeleted && !IsInDatabase)
			{
				AddParentRelatedRelationshipLog(LoggingAction.Attach);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (IsInDatabase)
				{
					AddParentRelatedRelationshipLog(LoggingAction.Detach);
				}
				MarkInvoiceTermsAsNeedValidation();
			}
			base.Delete();
		}

		#endregion

		#region Logging

		internal void AddParentRelatedRelationshipLog(LoggingAction action)
		{
			var parent = Parent;
			var related = RelatedParty;

			if (parent != null && !parent.IsDeleted && related != null && !related.IsDeleted)
			{
				related.AddRelationshipLog(action, parent.OH_Code, parent.OH_FullName);

				parent.AddRelationshipLog(action, related.OH_Code, related.OH_FullName);

				parent.OH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				parent.OH_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;

				related.OH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				related.OH_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
			}
		}

		#endregion

		#region Properties

		public bool IsCompanySpecific
		{
			get { return PR_PartyType == RelatedPartyTypeList.Codes.APSettlementGroup || PR_PartyType == RelatedPartyTypeList.Codes.ARSettlementGroup || PR_PartyType == RelatedPartyTypeList.Codes.AccountingVATGSTGroup; }
		}

		public bool IsEnterpriseLevelOnly
		{
			get { return PR_PartyType == RelatedPartyTypeList.Codes.ShipperBroker || PR_PartyType == RelatedPartyTypeList.Codes.ExportConsolidationDepot || PR_PartyType == RelatedPartyTypeList.Codes.ControllingAgent; }
		}

		public bool ShouldHaveMode
		{
			get
			{
				switch (PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.ForwarderCFS:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.LocalTransportBillTo:
					case RelatedPartyTypeList.Codes.ClientCFS:
					case RelatedPartyTypeList.Codes.ForwarderLocalTransport:
					case RelatedPartyTypeList.Codes.ReceivingAgent:
					case RelatedPartyTypeList.Codes.SendingAgent:
					case RelatedPartyTypeList.Codes.DeliveryAgent:
					case RelatedPartyTypeList.Codes.DeliveryTo:
					case RelatedPartyTypeList.Codes.PickupAgent:
					case RelatedPartyTypeList.Codes.PickupFrom:
					case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.NotifyParty:
					case RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo:
					case RelatedPartyTypeList.Codes.InvoiceFreightJobsTo:
					case RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo:
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						return true;
					default:
						return false;
				}
			}
		}

		public bool ShouldHaveDirection
		{
			get { return OrgRelatedPartyTypeHelper.ShouldHaveDirection(PR_PartyType); }
		}

		public bool ShouldCalculateDirection
		{
			get { return OrgRelatedPartyTypeHelper.ShouldCalculateDirection(PR_PartyType); }
		}

		public bool DirectionReadonly
		{
			get { return ShouldCalculateDirection || !ShouldHaveDirection; }
		}

		public bool CanHaveLocation
		{
			get
			{
				switch (PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.ClientCFS:
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.ForwarderCFS:
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.DeliveryAgent:
					case RelatedPartyTypeList.Codes.DeliveryTo:
					case RelatedPartyTypeList.Codes.PickupAgent:
					case RelatedPartyTypeList.Codes.PickupFrom:
					case RelatedPartyTypeList.Codes.ReceivingAgent:
					case RelatedPartyTypeList.Codes.SendingAgent:
					case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						return true;
					default:
						return false;
				}
			}
		}

		public OrgHeader ParentOrg
		{
			get { return Parent; }
		}

		public override ZGuid PR_OH_Parent
		{
			get { return base.PR_OH_Parent; }
			set
			{
				OrgHeader oldParent = Parent;
				base.PR_OH_Parent = value;

				MarkInvoiceTermsAsNeedValidation(oldParent);
			}
		}

		#region PR_PartyType

		[List("Lookups.AllPartyTypeList")]
		public override ZString PR_PartyType
		{
			get { return base.PR_PartyType; }
			set
			{
				string oldPartyType = base.PR_PartyType;
				base.PR_PartyType = value;

				DefaultOrgAddressSetter();
				SetDefaultRelatedParty();

				if (PR_OA_ReadOnly)
				{
					PR_OA = ZGuid.Empty;
				}

				if (!PR_FreightTransportMode.IsEmpty && !ShouldHaveMode)
				{
					PR_FreightTransportMode = ZString.Empty;
					PR_FreightContainerMode = ZString.Empty;
				}

				if (!PR_Location.IsDefault && !CanHaveLocation)
				{
					PR_Location = ZString.Empty;
				}

				if (PR_PartyType != RelatedPartyTypeList.Codes.ForwarderCoLoadWith)
				{
					PR_RN_NKImporterCountry = ZString.Empty;
				}

				if (!PR_FreightDirection.IsEmpty && !ShouldHaveDirection)
				{
					PR_FreightDirection = ZString.Empty;
				}
				else
				{
					SetDefaultDirection();
				}
				SetCalculatedDirection();
				SetCompanyLevel();
				PartyTypeDescriptionInfo.RefreshBinding();
				if (ParentOrg != null)
				{
					ParentOrg.RefreshRelatedPartyProxyFields();
				}

				MarkAsNeedingValidation();
				MarkInvoiceTermsAsNeedValidation(oldPartyType: oldPartyType);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePR_PartyType();
					Validation.ValidatePR_Location();
					Validation.ValidatePR_OH_RelatedParty();
					if (ParentOrg != null)
					{
						ParentOrg.Validation.ValidateOH_IsControllingCustomer();
					}
				}

				UpdateCSAStatusAsAddPendingIfApplicable();
			}
		}

		void DefaultOrgAddressSetter()
		{
			switch (PR_PartyType)
			{
				case RelatedPartyTypeList.Codes.Warehouse:
				case RelatedPartyTypeList.Codes.ProductRelationship:
					var address = Address;
					if (address == null || address.OA_OH != PR_OH_RelatedParty)
					{
						var relatedParty = RelatedParty;
						if (relatedParty != null)
						{
							PR_OA = relatedParty.MainAddress.PK;
						}
					}
					break;
				case RelatedPartyTypeList.Codes.LocalTransport:
				case RelatedPartyTypeList.Codes.NationalDistributionCentre:
					if (PR_OA.IsEmpty)
					{
						var parentOrg = Parent;
						if (parentOrg != null)
						{
							PR_OA = parentOrg.MainAddress.PK;
						}
					}
					break;
				default:
					break;
			}
		}

		[List("Lookups.AllParentTypeList")]
		public virtual ZString ParentType
		{
			get { return this.PR_PartyType; }
		}

		void MarkInvoiceTermsAsNeedValidation(OrgHeader oldParent = null, string oldPartyType = "")
		{
			Action<OrgHeader> markTermsAsNeedValidation = (parent) =>
			{
				if (parent != null && !parent.IsDeleted)
				{
					if (oldPartyType == RelatedPartyTypeList.Codes.APSettlementGroup ||
						PR_PartyType == RelatedPartyTypeList.Codes.APSettlementGroup)
					{
						parent.CompanyData.MarkAsNeedingValidation();
					}
					else if (oldPartyType == RelatedPartyTypeList.Codes.ARSettlementGroup ||
							 PR_PartyType == RelatedPartyTypeList.Codes.ARSettlementGroup)
					{
						parent.CompanyData.ARTerms.MarkAsNeedingValidationIncludingChildren();
					}
				}
			};

			markTermsAsNeedValidation(oldParent);
			markTermsAsNeedValidation(Parent);
		}

		void SetDefaultDirection()
		{
			var defaultDirection = OrgRelatedPartyTypeHelper.GetDefaultDirection(PR_PartyType);
			if (!defaultDirection.IsEmpty)
			{
				PR_FreightDirection = defaultDirection;
			}
		}

		void SetCalculatedDirection()
		{
			calculatedDirection = ShouldCalculateDirection ? ZString.Empty : PR_FreightDirection;
		}

		void SetCompanyLevel()
		{
			PR_GC = IsCompanySpecific ? GlbCompany.CurrentCompany.PK : ZGuid.Empty;
		}

		void SetDefaultRelatedParty()
		{
			if (PR_PartyType == RelatedPartyTypeList.Codes.AccountingVATGSTGroup)
			{
				PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			}
			else if (PR_PartyType == RelatedPartyTypeList.Codes.ControllingAgent
				&& ParentOrg != null
				&& !ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity
				&& ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity)
			{
				if (GlbBranch.CurrentBranch.OrgProxy != null)
				{
					PR_OH_RelatedParty = GlbBranch.CurrentBranch.OrgProxy.PK;
				}
				else
				{
					PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				}
			}
		}

		#endregion

		#region PR_OH_RelatedParty

		[List("Lookups.RelatedParties")]
		public override ZGuid PR_OH_RelatedParty
		{
			get { return base.PR_OH_RelatedParty; }
			set
			{
				DefaultOtherRelatedPartiesIfNecessary(value);
				base.PR_OH_RelatedParty = value;
				DefaultAddressFromRelatedParty();

				if (ParentOrg != null)
				{
					ParentOrg.RefreshRelatedPartyProxyFields();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePR_PartyType();
				}

				MarkInvoiceTermsAsNeedValidation();
			}
		}

		void DefaultAddressFromRelatedParty()
		{
			OrgAddress relatedPartyAddress = null;
			switch (PR_PartyType)
			{
				case RelatedPartyTypeList.Codes.Warehouse:
				case RelatedPartyTypeList.Codes.ProductRelationship:
					relatedPartyAddress = RelatedParty?.MainAddress;
					break;
				case RelatedPartyTypeList.Codes.NotifyParty:
					relatedPartyAddress = RelatedParty?.GetAddressWithFallback(AddressType.OFC);
					break;
			}

			if (relatedPartyAddress == null)
			{
				return;
			}

			var address = Address;
			if (address == null || address.OA_OH != PR_OH_RelatedParty)
			{
				PR_OA = relatedPartyAddress.PK;
			}
		}

		void DefaultOtherRelatedPartiesIfNecessary(ZGuid newValue)
		{
			if (ParentOrg != null &&
					(PR_PartyType == RelatedPartyTypeList.Codes.InvoiceFreightJobsTo
					|| PR_PartyType == RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo))
			{
				OrgHeader org = ParentOrg.GetRelatedParty(RelatedPartyTypeList.Codes.ReportRevenueTo, PR_FreightDirection);
				if (org != null && org.PK == (ZGuid)PR_OH_RelatedPartyInfo.PersistentValue)
				{
					ParentOrg.SetRelatedParty(newValue, RelatedPartyTypeList.Codes.ReportRevenueTo, PR_FreightDirection);
				}
			}
		}

		#endregion

		#region PR_FreightTransportMode

		[List("Lookups.TransportModeList")]
		public override ZString PR_FreightTransportMode
		{
			get
			{
				return base.PR_FreightTransportMode;
			}
			set
			{
				base.PR_FreightTransportMode = value;
				if (value == Core.Constants.TransportModes.All)
				{
					PR_FreightContainerMode = ZString.Empty;
				}
			}
		}

		protected bool PR_FreightTransportMode_ReadOnly
		{
			get { return !ShouldHaveMode; }
		}

		#endregion

		#region PR_FreightContainerMode

		[List("Lookups.ContainerModeList")]
		public override ZString PR_FreightContainerMode
		{
			get
			{
				return base.PR_FreightContainerMode;
			}
			set
			{
				base.PR_FreightContainerMode = value;
			}
		}

		protected bool PR_FreightContainerMode_ReadOnly
		{
			get
			{
				return PR_FreightTransportMode_ReadOnly || PR_FreightTransportMode == Core.Constants.TransportModes.All || !CanHaveContainerMode;
			}
		}

		public bool CanHaveContainerMode
		{
			get
			{
				return PR_PartyType != RelatedPartyTypeList.Codes.SelfFilerForICS2;
			}
		}

		#endregion

		#region PR_FreightDirection

		[List("Lookups.FreightDirectionList")]
		public override ZString PR_FreightDirection
		{
			get { return base.PR_FreightDirection; }
			set
			{
				base.PR_FreightDirection = value;
				if (!IsSettingCalculatedDirection)
				{
					calculatedDirection = ShouldCalculateDirection ? ZString.Empty : value;
				}
			}
		}

		#endregion

		#region PR_OA

		[List("Lookups.Addresses")]
		public override ZGuid PR_OA
		{
			get { return base.PR_OA; }
			set { base.PR_OA = value; }
		}

		protected bool PR_OA_ReadOnly
		{
			get
			{
				switch (PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.Warehouse:
					case RelatedPartyTypeList.Codes.NationalDistributionCentre:
					case RelatedPartyTypeList.Codes.PickupFrom:
					case RelatedPartyTypeList.Codes.DeliveryTo:
					case RelatedPartyTypeList.Codes.NotifyParty:
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
					case RelatedPartyTypeList.Codes.ProductRelationship:
						return false;
					default:
						return true;
				}
			}
		}

		#endregion

		#region PR_Location

		[List("Lookups.Locations")]
		public override ZString PR_Location
		{
			get { return base.PR_Location; }
			set
			{
				base.PR_Location = value;

				if (PR_PartyType == RelatedPartyTypeList.Codes.SelfFilerForICS2)
				{
					PR_RN_NKImporterCountry = value.SubstringSafe(0, 2);
				}
			}
		}

		protected bool PR_Location_ReadOnly
		{
			get { return !CanHaveLocation; }
		}

		#endregion

		#region CompanyLevel

		public override ZGuid PR_GC
		{
			get { return base.PR_GC; }
			set
			{
				base.PR_GC = value;

				MarkAsNeedingValidation();
				CompanyLevelInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CompanyLevelList")]
		[MaxLength(3)]
		public ZString CompanyLevel
		{
			get { return PR_GC == ZGuid.Empty ? CompanyLevelList.Codes.ENT : CompanyLevelList.Codes.COM; }
			set { PR_GC = (value == CompanyLevelList.Codes.COM) ? GlbCompany.CurrentCompany.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo CompanyLevelInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyLevel); }
		}

		#endregion

		#region CalculatedDirection

		[List("Lookups.FreightDirectionList")]
		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(DirectionReadonly))]
		public ZString CalculatedDirection
		{
			get { return calculatedDirection; }
			set
			{
				calculatedDirection = ShouldCalculateDirection ? ZString.Empty : value;
				IsSettingCalculatedDirection = true;
				PR_FreightDirection = value;
				IsSettingCalculatedDirection = false;
				CalculatedDirectionInfo.RefreshBinding();
			}
		}
		ZString calculatedDirection;

		public ZWrappedPropertyInfo CalculatedDirectionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CalculatedDirection, x => PR_FreightDirectionInfo); }
		}

		bool IsSettingCalculatedDirection;

		#endregion

		#region PartyTypeDescription

		ZString GetPartyTypeDescription(CodeDescriptionPairList list, ZString code)
		{
			ZString result = list.GetDescriptionFromCode(code);
			if (result.IsEmpty)
			{
				result = incorrectPartyTypeDescription;
			}
			return result;
		}

		ZString incorrectPartyTypeDescription;
		[BusinessObjectTestExclude] // will not store data if not in list
		[MaxLength(35)]
		public ZString PartyTypeDescription
		{
			get
			{
				var list = (CodeDescriptionPairList)MetaData.GetListDataSource(this, PR_PartyTypeInfo.PropertyDescriptor);
				return GetPartyTypeDescription(list, PR_PartyType);
			}
			set
			{
				PR_PartyType = Lookups.PartyTypeList.GetCodeFromDescription(value);
				if (PR_PartyType.IsEmpty)
				{
					incorrectPartyTypeDescription = value;
				}
			}
		}

		public ZPropertyInfo PartyTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PartyTypeDescription); }
		}

		[List("Lookups.ParentPartyTypeList")]
		[MaxLength(35)]
		public ZString ParentPartyTypeDescription
		{
			get { return GetPartyTypeDescription(Lookups.ParentPartyTypeList, ParentType); }
		}

		public ZPropertyInfo ParentPartyTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ParentPartyTypeDescription)); }
		}

		#endregion

		#region RelatedPartyName

		public ZString RelatedPartyName
		{
			get { return RelatedParty != null ? RelatedParty.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo RelatedPartyNameInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedPartyName); }
		}

		#endregion

		#region ParentName

		public ZString ParentName
		{
			get { return Parent != null ? Parent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo ParentNameInfo
		{
			get { return GetZPropertyInfo(Schema.ParentName); }
		}

		#endregion

		#region IsAlrightToAddOrDelete

		public ZBool IsAlrightToAddOrDelete
		{
			get
			{
				var lists = new OrgCodeLists();
				if (ParentOrg != null)
				{
					var isOK = true;

					switch (PR_PartyType)
					{
						case RelatedPartyTypeList.Codes.ControllingAgent:
							isOK = ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity
								|| ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity;
							break;

						default:
							isOK = (ParentOrg.SecurityProvider.HasModifyDetailsFinancialRelatedPartiesSecurity && lists.RelatedPartyFinancialDescriptions_List().ContainsCode(PartyTypeDescription))
								|| (ParentOrg.SecurityProvider.HasModifyDetailsNonFinancialRelatedPartiesSecurity && !lists.RelatedPartyFinancialDescriptions_List().ContainsCode(PartyTypeDescription));
							break;
					}
					return isOK;
				}

				return true;
			}
		}

		#endregion

		#region PR_RN_NKImporterCountry

		public override ZString PR_RN_NKImporterCountry
		{
			get => base.PR_RN_NKImporterCountry;
			set
			{
				base.PR_RN_NKImporterCountry = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidatePR_OH_RelatedParty();
				}
			}
		}

		protected bool PR_RN_NKImporterCountry_ReadOnly => !CanHaveImporterCountry;

		public bool CanHaveImporterCountry
		{
			get
			{
				switch (PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			var shouldBeReadOnly = false;
			if (ParentOrg != null)
			{
				switch (PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.ControllingAgent:
						shouldBeReadOnly = IsInDatabase
											&& (ZString)PR_PartyTypeInfo.OriginalValue == RelatedPartyTypeList.Codes.ControllingAgent
											&& (!ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity && !ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity
												|| !IsOrgProxy((ZGuid)PR_OH_RelatedPartyInfo.OriginalValue) && !ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity
											);
						break;

					default:
						var lists = new OrgCodeLists();
						shouldBeReadOnly = !ParentOrg.SecurityProvider.HasModifyDetailsRelatedPartiesSecurity
										|| (!ParentOrg.SecurityProvider.HasModifyDetailsFinancialRelatedPartiesSecurity && lists.RelatedPartyFinancialDescriptions_List().ContainsCode(PartyTypeDescription) && IsInDatabase)
										|| (!ParentOrg.SecurityProvider.HasModifyDetailsNonFinancialRelatedPartiesSecurity && !lists.RelatedPartyFinancialDescriptions_List().ContainsCode(PartyTypeDescription) && IsInDatabase);
						break;
				}
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				var canDelete = true;
				switch (PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.ControllingAgent:
						canDelete = !IsInDatabase
								|| (ZString)PR_PartyTypeInfo.OriginalValue != RelatedPartyTypeList.Codes.ControllingAgent
								|| (ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity)
								|| (ParentOrg.SecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity && IsOrgProxy((ZGuid)PR_OH_RelatedPartyInfo.OriginalValue));
						break;

					default:
						canDelete = PR_CustomsStatus == CSARelatedPartyStatusList.Codes.Added ||
							PR_CustomsStatus == CSARelatedPartyStatusList.Codes.AddPendingSeeCustomsMessagingMenu && !IsInDatabase ||
							PR_CustomsStatus == CSARelatedPartyStatusList.Codes.DeleteRejected ||
							PR_CustomsStatus == CSARelatedPartyStatusList.Codes.AddRejected ||
							PR_CustomsStatus.IsEmpty;
						break;
				}

				return canDelete;
			}
		}

		internal static bool IsOrgProxy(ZGuid orgPK)
		{
			return orgPK == GlbCompany.CurrentCompany.GC_OH_OrgProxy || GlbCompany.CurrentCompany.Branches.Any(branch => branch.GB_OH_OrgProxy == orgPK);
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("3a6bbc85-a3fc-4f5e-b046-43d1d2e16cb6", "Delete not allowed, action pending or you do not have the required security rights."); }
		}

		#endregion

		#region Party CSA Status

		public ZBool IsCSARelatedPartyType
		{
			get { return PR_PartyType == RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee || PR_PartyType == RelatedPartyTypeList.Codes.CSAApprovedVendor; }
		}

		public void UpdateCSAStatusAsAddedIfApplicable()
		{
			if (IsCSARelatedPartyType)
			{
				PR_CustomsStatus = CSARelatedPartyStatusList.Codes.Added;
			}
		}

		public void UpdateCSAStatusAsRefreshPendingIfApplicable()
		{
			if (IsCSARelatedPartyType)
			{
				PR_CustomsStatus = CSARelatedPartyStatusList.Codes.RefreshPendingSeeCustomsMessagingMenu;
			}
		}

		public void UpdateCSAStatusAsAddPendingIfApplicable()
		{
			if (IsCSARelatedPartyType)
			{
				PR_CustomsStatus = CSARelatedPartyStatusList.Codes.AddPendingSeeCustomsMessagingMenu;
			}
			else
			{
				PR_CustomsStatus = ZString.Empty;
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgRelatedPartyFetchStrategy(this);
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public OrgRelatedParty[] LoadPartiesWithRelatedOrgAndType(ZGuid relatedOrgPK, string type)
			{
				var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, relatedOrgPK);
				query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, type);
				return Factory.Load<OrgRelatedParty>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(OrgRelatedParty);
		}

		#endregion
	}
}
