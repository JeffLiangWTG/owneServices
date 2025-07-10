using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[SystemDefinedValues]
	public class OrgSupBuyLinkTrnMode : AutoOrgSupBuyLinkTrnMode, ICanDelete, ICusCodeDataTypeSupporter
	{
		public OrgSupBuyLinkTrnMode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoOrgSupBuyLinkTrnMode.Schema
		{
			public const string PF_USPortOfLading = "PF_USPortOfLading";
			public const string PF_USPortOfUnLading = "PF_USPortOfUnLading";
		}

		#region New Properties
		#region PF_USPortOfLading
		[List("Lookups.USPortsOfLading")]
		public virtual ZString PF_USPortOfLading
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.PF_USPortOfLading); }
			set
			{
				ZString oldValue = PF_USPortOfLading;
				CheckMaximumLength(PF_USPortOfLadingInfo, value);
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.PF_USPortOfLading, AddOnColumnDataType.Codes.String, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePF_USPortOfLading();
					}
				}
				PF_USPortOfLadingInfo.RefreshBinding(oldValue);
			}
		}

		public virtual ZPropertyInfo PF_USPortOfLadingInfo
		{
			get { return GetZPropertyInfo(Schema.PF_USPortOfLading); }
		}

		protected int PF_USPortOfLading_MaxLength
		{
			get { return IsUSImporterCountry ? USCForeignPortSchema.UH_Code.MaxLength : RegionDistrictPortMaxLength; }
		}

		const int RegionDistrictPortMaxLength = 4;

		#endregion

		#region PF_USPortOfUnLading
		[List("Lookups.USPortsOfUnLading")]
		public virtual ZString PF_USPortOfUnLading
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.PF_USPortOfUnLading);
			}
			set
			{
				ZString oldValue = PF_USPortOfUnLading;
				CheckMaximumLength(PF_USPortOfUnLadingInfo, value);
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.PF_USPortOfUnLading, AddOnColumnDataType.Codes.String, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePF_USPortOfUnLading();
					}
				}
				PF_USPortOfUnLadingInfo.RefreshBinding(oldValue);
			}
		}

		public virtual ZPropertyInfo PF_USPortOfUnLadingInfo
		{
			get { return GetZPropertyInfo(Schema.PF_USPortOfUnLading); }
		}

		protected int PF_USPortOfUnLading_MaxLength
		{
			get { return IsUSImporterCountry ? RegionDistrictPortMaxLength : USCForeignPortSchema.UH_Code.MaxLength; }
		}

		#endregion
		#endregion

		#region AddInfo

		public XmlAddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					var type = ObjectFactory.GetType<IOrgSupBuyLinkTrnModeAddInfo>();
					if (type != null)
					{
						addInfo = (XmlAddInfo)Activator.CreateInstance(type, PF_AddInfoInfo);
						RegisterEditableChildObject(addInfo);
					}
				}
				return addInfo;
			}
		}
		XmlAddInfo addInfo;

		#endregion

		#region Overrides

		[List("Lookups.TransportModeList")]
		public override ZString PF_TransportMode
		{
			get { return base.PF_TransportMode; }
			set
			{
				if (base.PF_TransportMode != value)
				{
					base.PF_TransportMode = value;
					MarkSiblingsAsNeedingValidation();
					if (value == Core.Constants.TransportModes.All)
					{
						PF_ContainerMode = ZString.Empty;
					}
				}
			}
		}
		[List("Lookups.ContainerModeList")]
		public override ZString PF_ContainerMode
		{
			get { return base.PF_ContainerMode; }
			set
			{
				if (base.PF_ContainerMode != value)
				{
					base.PF_ContainerMode = value;
					MarkSiblingsAsNeedingValidation();
				}
			}
		}

		protected bool PF_ContainerMode_ReadOnly
		{
			get
			{
				return PF_TransportMode == Core.Constants.TransportModes.All;
			}
		}

		protected bool PF_EstDeliveryDays_ReadOnly
		{
			get { return !PF_OverrideDeliveryDays; }
		}

		[List("Lookups.DefaultServiceLevels")]
		public override ZString PF_RS_NKDefaultServiceLevel
		{
			get { return base.PF_RS_NKDefaultServiceLevel; }
			set { base.PF_RS_NKDefaultServiceLevel = value; }
		}

		[List("Lookups.IncoTermList")]
		public override ZString PF_IncoTerm
		{
			get { return base.PF_IncoTerm; }
			set { base.PF_IncoTerm = value; }
		}

		[List("Lookups.IncoTermModeList")]
		public override ZString PF_IncoTermMode
		{
			get { return base.PF_IncoTermMode; }
			set { base.PF_IncoTermMode = value; }
		}

		[List("Lookups.ImportCustomsAgents")]
		public override ZGuid PF_OH_ImportCustomsAgent
		{
			get { return base.PF_OH_ImportCustomsAgent; }
			set { base.PF_OH_ImportCustomsAgent = value; }
		}

		[List("Lookups.ControllingCustomers")]
		public override ZGuid PF_OH_ControllingCustomer
		{
			get { return base.PF_OH_ControllingCustomer; }
			set { base.PF_OH_ControllingCustomer = value; }
		}

		[List("Lookups.Forwarders")]
		public override ZGuid PF_OH_ReceivingAgent
		{
			get { return base.PF_OH_ReceivingAgent; }
			set { base.PF_OH_ReceivingAgent = value; }
		}

		[List("Lookups.Forwarders")]
		public override ZGuid PF_OH_SendingAgent
		{
			get { return base.PF_OH_SendingAgent; }
			set { base.PF_OH_SendingAgent = value; }
		}

		[List("Lookups.CarrierLines")]
		public override ZGuid PF_OH_CarrierLine
		{
			get { return base.PF_OH_CarrierLine; }
			set { base.PF_OH_CarrierLine = value; }
		}

		[List("PF_OA_OverridePickupAddress_ZAddress.OrgAddress_List")]
		public override ZGuid PF_OA_OverridePickupAddress
		{
			get { return base.PF_OA_OverridePickupAddress; }
			set
			{
				if (base.PF_OA_OverridePickupAddress != value)
				{
					base.PF_OA_OverridePickupAddress = value;

					if (!PF_OC_OverrideSupplierContact.IsEmpty && !Lookups.OverrideSupplierContacts.Contains(PF_OC_OverrideSupplierContact))
					{
						PF_OC_OverrideSupplierContact = ZGuid.Empty;
					}
				}
			}
		}

		[List("PF_OA_OverrideDeliveryAddress_ZAddress.OrgAddress_List")]
		public override ZGuid PF_OA_OverrideDeliveryAddress
		{
			get { return base.PF_OA_OverrideDeliveryAddress; }
			set
			{
				if (base.PF_OA_OverrideDeliveryAddress != value)
				{
					base.PF_OA_OverrideDeliveryAddress = value;

					if (!PF_OC_OverrideConsigneeContact.IsEmpty && !Lookups.OverrideConsigneeContacts.Contains(PF_OC_OverrideConsigneeContact))
					{
						PF_OC_OverrideConsigneeContact = ZGuid.Empty;
					}
				}
			}
		}

		[List("PF_OA_OverrideNotifyPartyAddress_ZAddress.OrgAddress_List")]
		public override ZGuid PF_OA_OverrideNotifyPartyAddress
		{
			get { return base.PF_OA_OverrideNotifyPartyAddress; }
			set
			{
				if (base.PF_OA_OverrideNotifyPartyAddress != value)
				{
					base.PF_OA_OverrideNotifyPartyAddress = value;

					if (!Lookups.OverrideNotifyParties.Contains(PF_OC_OverrideNotifyParty))
					{
						// When notify party address is not specified, do not fallback to buyer's contact for backwards compatibility
						PF_OC_OverrideNotifyParty = DefaultContactFinder.GetDefaultNotifyPartyContact(OverrideNotifyPartyAddress?.Header);
					}
				}
			}
		}

		protected override ZAddress GetNewPF_OA_OverrideNotifyPartyAddress_ZAddress()
		{
			var result = base.GetNewPF_OA_OverrideNotifyPartyAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		public RefCountry ImporterCountry
		{
			get { return SupplierBuyerLink != null ? SupplierBuyerLink.ImporterCountry ?? RefCountry.OrganisationCountry(SupplierBuyerLink.Buyer) : null; }
		}

		public bool IsUSImporterCountry
		{
			get { return ImporterCountry != null && ImporterCountry.PK == Core.Constants.CountryGuids.UnitedStates; }
		}

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return SupplierBuyerLink == null || SupplierBuyerLink.OrgSupBuyLinkTrnModes.Count > 1; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("7749281b-82aa-4f3f-940a-ee837d3ae19f", "This Supplier/Buyer link must have at least 1 Mode."); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result =
				SupplierBuyerLink != null && SupplierBuyerLink.Buyer != null && !SupplierBuyerLink.Buyer.SecurityProvider.HasModifyConsigneeRelationshipsSecurity
				&& SupplierBuyerLink.Supplier != null && !SupplierBuyerLink.Supplier.SecurityProvider.HasModifyConsignorRelationshipsSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Implementation

		void MarkSiblingsAsNeedingValidation()
		{
			if (SupplierBuyerLink != null)
			{
				SupplierBuyerLink.OrgSupBuyLinkTrnModes.MarkAsNeedingValidation();
			}
		}

		#endregion

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			if (AddInfo != null)
			{
				AddInfo.Deserialise();
			}
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusCodeDataTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.CustomsOffice, ObjectFactory.GetType<CN.ICustomsOffice>() }
			};
		}

		static class CusCodeDataTypeList
		{
			public static class Codes
			{
				public const string CustomsOffice = "COF";
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
			}
			base.Delete();
		}
	}
}
