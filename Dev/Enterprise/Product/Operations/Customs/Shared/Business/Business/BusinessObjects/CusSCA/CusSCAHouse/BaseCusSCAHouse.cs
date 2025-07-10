using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[UniversalDataContext(DataContextType.SeaHouseBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class BaseCusSCAHouse :
		AutoCusSCAHouse,
		Integration.Customs.Shared.IBaseCusSCAHouse,
		ISynchroniserReadOnlyMembersProvider,
		IWorkflowProviderEvent,
		Integration.Customs.IHVLVCustomsStatusPublisher
	{
		protected BaseCusSCAHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static readonly TypeDecider TypeDecider = new BaseCusSCAHouseTypeDecider();

		[ReadOnly(true)]
		public override ZString CA_UnderbondStatus
		{
			get { return base.CA_UnderbondStatus; }
		}

		[ReadOnly(true)]
		public override ZString CA_ShipmentStatus
		{
			get { return base.CA_ShipmentStatus; }
		}

		[ReadOnly(true)]
		public override ZString CA_MessageStatus
		{
			get { return base.CA_MessageStatus; }
			set { base.CA_MessageStatus = value; }
		}

		public ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = new CachedProperty<ForwardingShipment>(Factory, delegate
					{ return Factory.Load<ForwardingShipment>(CA_JS); });
				}
				return shipment.Value;
			}
		}
		CachedProperty<ForwardingShipment> shipment;

		public BaseCusSCAOceanBill OceanBill
		{
			get
			{
				if (oceanBill == null)
				{
					oceanBill = new CachedProperty<BaseCusSCAOceanBill>(Factory, delegate
					{ return Factory.Load<BaseCusSCAOceanBill>(CA_CB); });
				}
				return oceanBill.Value;
			}
		}
		CachedProperty<BaseCusSCAOceanBill> oceanBill;

		public ForwardingConsol Consol
		{
			get { return OceanBill == null ? null : OceanBill.Consol; }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = CreateEDIMessageCollection();
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
					fMessages.CountChanged += fMessages_CountChanged;
				}
				return fMessages;
			}
		}
		protected EDIMessageCollection fMessages;

		void fMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnMessagesCountChanged();
		}

		protected virtual void OnMessagesCountChanged()
		{
		}

		protected virtual EDIMessageCollection CreateEDIMessageCollection()
		{
			return new EDIMessageCollection(this);
		}

		public void DeleteAnyNewMessages()
		{
			if (fMessages == null)
			{
				return;
			}

			foreach (EDIMessage message in Messages.ToArray())
			{
				if (!message.IsInDatabase && !message.IsDeleted)
				{
					message.Delete();
				}
			}
		}

		public IEnumerable<BaseCusSCAPivot> CusSCAPivotCollection => GetCusSCAPivotCollection();

		protected abstract IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection();

		protected virtual bool StateProvinceIsSupported
		{
			get { return false; }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("32558196-4677-49eb-b107-302994330fb7", "Sea Cargo House");
				var parameters = new ZStringBuilder();
				parameters.AppendIfNotEmpty(Res.GetString("ec18eef6-14a7-4507-b306-5eb3e36973dc", "HBL") + ": ", CA_HouseBill);
				parameters.AppendIfNotEmpty(Res.GetString("f44f4de0-f7f2-4c18-81c2-59c46c53aa75", "MHB") + ": ", CA_MasterHouseBill);
				if (!parameters.IsEmpty)
				{
					result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
				}
				return result;
			}
		}

		#region Override Freight Defaults

		public sealed override ZBool CA_OverrideFreightDefaults
		{
			get { return base.CA_OverrideFreightDefaults; }
			set
			{
				SetOverrideFreightDefaults(value);
			}
		}

		void SetOverrideFreightDefaults(ZBool value)
		{
			if (CA_OverrideFreightDefaults != value)
			{
				if (!value && Shipment != null && !Messages.IsMatchingMessages(ZString.Empty, Array.Empty<ZString>(), ZString.Empty, true))
				{
					CancelEventArgs args = new CancelEventArgs(false);
					if (OnOverrideFreightDefaultsChanging != null)
					{
						OnOverrideFreightDefaultsChanging(this, args);
					}
					if (!args.Cancel)
					{
						base.CA_OverrideFreightDefaults = value;
						OnOverrideFreightDefaultSet();
					}
					else
					{
						CA_OverrideFreightDefaultsInfo.RefreshBinding();
					}
				}
				else
				{
					base.CA_OverrideFreightDefaults = value;
				}
				OnOverrideFreightDefaultChanged();
				RefreshBindingIncludingChildren();
			}
		}

		protected virtual void OnOverrideFreightDefaultSet()
		{
		}

		protected virtual void OnOverrideFreightDefaultChanged()
		{
		}

		public event CancelEventHandler OnOverrideFreightDefaultsChanging;

		#endregion

		#region Shipment Status

		protected virtual bool MessageInProgress
		{
			get { return false; }
		}

		protected virtual bool MessageAcknowledged
		{
			get { return false; }
		}

		#endregion

		#region Consignee

		public override ZGuid CA_OH_Consignee
		{
			get { return base.CA_OH_Consignee; }
			set
			{
				bool hasChanged = CA_OH_Consignee != value;
				base.CA_OH_Consignee = value;
				if (hasChanged && !IsCopying)
				{
					CopyConsigneeDetails();
				}
			}
		}

		public override ZGuid CA_OA_ConsigneeAddress
		{
			get => base.CA_OA_ConsigneeAddress;
			set
			{
				bool hasChanged = CA_OA_ConsigneeAddress != value;
				base.CA_OA_ConsigneeAddress = value;
				if (hasChanged && !IsCopying)
				{
					CopyConsigneeDetails(ConsigneeAddress);
				}
			}
		}

		protected virtual void CopyConsigneeDetails()
		{
			CopyConsigneeDetails(ConsigneeAddressForPortOfDestination, false);
		}

		public virtual void CopyConsigneeDetails(IAddress consigneeAddress, bool copyOrganisationPK = true)
		{
			if (consigneeAddress != null)
			{
				if (copyOrganisationPK)
				{
					CopyConsigneeFK(consigneeAddress);
				}

				CA_ConsigneeName = consigneeAddress.CompanyName.Left(35);
				CA_ConsigneeAddress1 = consigneeAddress.Address1.Left(CA_ConsigneeAddress1Info.MaxLength);
				CA_ConsigneeAddress2 = consigneeAddress.Address2.Left(CA_ConsigneeAddress2Info.MaxLength);
				CA_RN_NKConsigneeCountryCode = consigneeAddress.CountryCode.Left(CA_RN_NKConsigneeCountryCodeInfo.MaxLength);
				if (StateProvinceIsSupported)
				{
					CA_ConsigneeSuburb = consigneeAddress.City.Left(CA_ConsigneeSuburbInfo.MaxLength);
					CA_ConsigneeState = consigneeAddress.State.Left(CA_ConsigneeStateInfo.MaxLength);
				}
				else
				{
					CA_ConsigneeSuburb = consigneeAddress.Address3.Left(CA_ConsigneeSuburbInfo.MaxLength);
				}

				CA_ConsigneePostcode = consigneeAddress.PostCode.Left(CA_ConsigneePostcodeInfo.MaxLength);
				CA_ConsigneeFax = consigneeAddress.Fax.Left(CA_ConsigneeFaxInfo.MaxLength);
				CA_ConsigneePhone = consigneeAddress.Phone.Left(CA_ConsigneePhoneInfo.MaxLength);
			}
		}

		protected virtual void CopyConsigneeFK(IAddress consigneeAddress)
		{
			base.CA_OH_Consignee = consigneeAddress.Organisation?.PK ?? ZGuid.Empty;
		}

		protected virtual void CopyConsigneeDetails(OrgAddress consigneeOrgAddress)
		{
			if (consigneeOrgAddress != null)
			{
				CA_ConsigneeName = consigneeOrgAddress.CompanyName.Left(35);
				CA_ConsigneeAddress1 = consigneeOrgAddress.Address1.Left(CA_ConsigneeAddress1Info.MaxLength);
				CA_ConsigneeAddress2 = consigneeOrgAddress.Address2.Left(CA_ConsigneeAddress2Info.MaxLength);
				CA_RN_NKConsigneeCountryCode = consigneeOrgAddress.OA_RN_NKCountryCode.Left(CA_RN_NKConsigneeCountryCodeInfo.MaxLength);
				if (StateProvinceIsSupported)
				{
					CA_ConsigneeSuburb = consigneeOrgAddress.OA_City.Left(CA_ConsigneeSuburbInfo.MaxLength);
					CA_ConsigneeState = consigneeOrgAddress.OA_State.Left(CA_ConsigneeStateInfo.MaxLength);
				}
				else
				{
					CA_ConsigneeSuburb = ((ZString)(consigneeOrgAddress.OA_City + " " + consigneeOrgAddress.OA_State)).Left(CA_ConsigneeSuburbInfo.MaxLength);
				}

				CA_ConsigneePostcode = consigneeOrgAddress.Postcode.Left(CA_ConsigneePostcodeInfo.MaxLength);
				CA_ConsigneeFax = consigneeOrgAddress.OA_Fax.Left(CA_ConsigneeFaxInfo.MaxLength);
				CA_ConsigneePhone = consigneeOrgAddress.OA_Phone.Left(CA_ConsigneePhoneInfo.MaxLength);
			}
		}

		public IAddress ConsigneeAddressForPortOfDestination
		{
			get
			{
				if (Consignee != null)
				{
					return new PortBasedOrgAddressDecider(Consignee, ConsigneeAddressType, delegate()
					{ return CA_RL_NK_PortOfDestination; });
				}

				return null;
			}
		}

		public virtual CargoAddressType ConsigneeAddressType
		{
			get { return CargoAddressType.Delivery; }
		}

		protected virtual bool ConsigneeAddressLocked
		{
			get { return (Consignee != null) && !MessageAcknowledged; }
		}

		#region Consignee Name and address read only

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeName
		{
			get { return base.CA_ConsigneeName; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeAddress1
		{
			get { return base.CA_ConsigneeAddress1; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeAddress2
		{
			get { return base.CA_ConsigneeAddress2; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeSuburb
		{
			get { return base.CA_ConsigneeSuburb; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneePostcode
		{
			get { return base.CA_ConsigneePostcode; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneePhone
		{
			get { return base.CA_ConsigneePhone; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeFax
		{
			get { return base.CA_ConsigneeFax; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_RN_NKConsigneeCountryCode
		{
			get { return base.CA_RN_NKConsigneeCountryCode; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeContactName
		{
			get { return base.CA_ConsigneeContactName; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLocked))]
		public override ZString CA_ConsigneeState
		{
			get { return base.CA_ConsigneeState; }
		}

		#endregion

		#endregion

		#region Consignor

		public override ZGuid CA_OH_Consignor
		{
			get { return base.CA_OH_Consignor; }
			set
			{
				bool hasChanged = CA_OH_Consignor != value;
				base.CA_OH_Consignor = value;
				if (hasChanged && !IsCopying)
				{
					CopyConsignorDetails();
				}
			}
		}

		public override ZGuid CA_OA_ConsignorAddress
		{
			get => base.CA_OA_ConsignorAddress;
			set
			{
				bool hasChanged = CA_OA_ConsignorAddress != value;
				base.CA_OA_ConsignorAddress = value;
				if (hasChanged && !IsCopying)
				{
					CopyConsignorDetails(ConsignorAddress);
				}
			}
		}

		protected virtual void CopyConsignorDetails()
		{
			CopyConsignorDetails(ConsignorAddressForPortOfOrigin, false);
		}

		public virtual void CopyConsignorDetails(IAddress consignorAddress, bool copyOrganisationPK = true)
		{
			if (consignorAddress != null)
			{
				if (copyOrganisationPK)
				{
					CopyConsignorFK(consignorAddress);
				}

				CA_ConsignorName = consignorAddress.CompanyName.Left(35);
				CA_ConsignorAddress1 = consignorAddress.Address1.Left(CA_ConsignorAddress1Info.MaxLength);
				CA_ConsignorAddress2 = consignorAddress.Address2.Left(CA_ConsignorAddress2Info.MaxLength);
				CA_RN_NKConsignorCountryCode = consignorAddress.CountryCode.Left(CA_RN_NKConsignorCountryCodeInfo.MaxLength);
				if (StateProvinceIsSupported)
				{
					CA_ConsignorSuburb = consignorAddress.City.Left(CA_ConsignorSuburbInfo.MaxLength);
					CA_ConsignorState = consignorAddress.State.Left(CA_ConsignorStateInfo.MaxLength);
				}
				else
				{
					CA_ConsignorSuburb = consignorAddress.Address3.Left(CA_ConsignorSuburbInfo.MaxLength);
				}

				CA_ConsignorPostcode = consignorAddress.PostCode.Left(CA_ConsignorPostcodeInfo.MaxLength);
				CA_ConsignorPhone = consignorAddress.Phone.Left(CA_ConsignorPhoneInfo.MaxLength);
			}
		}

		protected virtual void CopyConsignorFK(IAddress consignorAddress)
		{
			base.CA_OH_Consignor = consignorAddress.Organisation?.PK ?? ZGuid.Empty;
		}

		protected virtual void CopyConsignorDetails(OrgAddress consignorOrgAddress)
		{
			if (consignorOrgAddress != null)
			{
				CA_ConsignorName = consignorOrgAddress.CompanyName.Left(35);
				CA_ConsignorAddress1 = consignorOrgAddress.Address1.Left(CA_ConsignorAddress1Info.MaxLength);
				CA_ConsignorAddress2 = consignorOrgAddress.Address2.Left(CA_ConsignorAddress2Info.MaxLength);
				CA_RN_NKConsignorCountryCode = consignorOrgAddress.OA_RN_NKCountryCode.Left(CA_RN_NKConsignorCountryCodeInfo.MaxLength);
				if (StateProvinceIsSupported)
				{
					CA_ConsignorSuburb = consignorOrgAddress.OA_City.Left(CA_ConsignorSuburbInfo.MaxLength);
					CA_ConsignorState = consignorOrgAddress.OA_State.Left(CA_ConsignorStateInfo.MaxLength);
				}
				else
				{
					CA_ConsignorSuburb = ((ZString)(consignorOrgAddress.OA_City + " " + consignorOrgAddress.OA_State)).Left(CA_ConsignorSuburbInfo.MaxLength);
				}

				CA_ConsignorPostcode = consignorOrgAddress.OA_PostCode.Left(CA_ConsignorPostcodeInfo.MaxLength);
				CA_ConsignorPhone = consignorOrgAddress.OA_Phone.Left(CA_ConsignorPhoneInfo.MaxLength);
			}
		}

		public IAddress ConsignorAddressForPortOfOrigin
		{
			get
			{
				if (Consignor != null)
				{
					return new PortBasedOrgAddressDecider(Consignor, ConsignorAddressType, delegate()
					{ return CA_RL_NK_PortOfOrigin; });
				}

				return null;
			}
		}

		public virtual CargoAddressType ConsignorAddressType
		{
			get { return CargoAddressType.Pickup; }
		}

		protected virtual bool ConsignorAddressLocked
		{
			get { return (Consignor != null) && !MessageAcknowledged; }
		}

		#region Consignor Name and address read only

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorName
		{
			get { return base.CA_ConsignorName; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorAddress1
		{
			get { return base.CA_ConsignorAddress1; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorAddress2
		{
			get { return base.CA_ConsignorAddress2; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorSuburb
		{
			get { return base.CA_ConsignorSuburb; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorPostcode
		{
			get { return base.CA_ConsignorPostcode; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_RN_NKConsignorCountryCode
		{
			get { return base.CA_RN_NKConsignorCountryCode; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorPhone
		{
			get { return base.CA_ConsignorPhone; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorFax
		{
			get { return base.CA_ConsignorFax; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorContactName
		{
			get { return base.CA_ConsignorContactName; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLocked))]
		public override ZString CA_ConsignorState
		{
			get { return base.CA_ConsignorState; }
		}

		#endregion

		#endregion

		#region Notify Party

		public override ZGuid CA_OH_Notify
		{
			get { return base.CA_OH_Notify; }
			set
			{
				bool hasChanged = CA_OH_Notify != value;
				base.CA_OH_Notify = value;
				if (hasChanged && !IsCopying)
				{
					CopyNotifyDetails(value);
				}
			}
		}

		protected void CopyNotifyDetails(ZGuid notifyPK)
		{
			OrgHeader notify = Factory.Load<OrgHeader>(notifyPK);
			if (notify != null)
			{
				CopyNotifyDetails(new OrgAddressDecider(notify, CargoAddressType.Notify), false);
			}
		}

		public void CopyNotifyDetails(IAddress notifyAddress, bool copyOrganisationPK = true)
		{
			if (notifyAddress != null)
			{
				if (copyOrganisationPK)
				{
					base.CA_OH_Notify = notifyAddress.Organisation?.PK ?? ZGuid.Empty;
				}
				base.CA_NotifyName = notifyAddress.CompanyName.Left(35);
				base.CA_NotifyAddress1 = notifyAddress.Address1.Left(CA_NotifyAddress1Info.MaxLength);
				base.CA_NotifyAddress2 = notifyAddress.Address2.Left(CA_NotifyAddress2Info.MaxLength);
				if (StateProvinceIsSupported)
				{
					base.CA_NotifySuburb = notifyAddress.City.Left(CA_NotifySuburbInfo.MaxLength);
					base.CA_NotifyState = notifyAddress.State.Left(CA_NotifyStateInfo.MaxLength);
				}
				else
				{
					base.CA_NotifySuburb = notifyAddress.Address3.Left(CA_NotifySuburbInfo.MaxLength);
				}
				base.CA_NotifyPostcode = notifyAddress.PostCode.Left(CA_NotifyPostcodeInfo.MaxLength);
				base.CA_NotifyFax = notifyAddress.Fax.Left(CA_NotifyFaxInfo.MaxLength);
				base.CA_NotifyPhone = notifyAddress.Phone.Left(CA_NotifyPhoneInfo.MaxLength);
				base.CA_RN_NKNotifyCountryCode = notifyAddress.CountryCode.Left(CA_RN_NKNotifyCountryCodeInfo.MaxLength);
			}
		}

		public IAddress NotifyAddress
		{
			get { return new OrgAddressDecider(Notify, CargoAddressType.Notify); }
		}

		protected virtual bool NotifyAddressLocked
		{
			get { return (Notify != null) && !MessageAcknowledged; }
		}

		#region Notify Name and address read only

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyName
		{
			get { return base.CA_NotifyName; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyAddress1
		{
			get { return base.CA_NotifyAddress1; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyAddress2
		{
			get { return base.CA_NotifyAddress2; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifySuburb
		{
			get { return base.CA_NotifySuburb; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyPostcode
		{
			get { return base.CA_NotifyPostcode; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyPhone
		{
			get { return base.CA_NotifyPhone; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyFax
		{
			get { return base.CA_NotifyFax; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_RN_NKNotifyCountryCode
		{
			get { return base.CA_RN_NKNotifyCountryCode; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyContactName
		{
			get { return base.CA_NotifyContactName; }
		}

		[ReadOnlyMember(nameof(NotifyAddressLocked))]
		public override ZString CA_NotifyState
		{
			get { return base.CA_NotifyState; }
		}

		#endregion

		#endregion

		#region Delivery Party

		public override ZGuid CA_OA_DeliveryAddress
		{
			get
			{
				return base.CA_OA_DeliveryAddress;
			}
			set
			{
				bool hasChanged = CA_OA_DeliveryAddress != value;
				base.CA_OA_DeliveryAddress = value;
				if (hasChanged)
				{
					CopyDeliveryDetails(value);
				}
			}
		}

		protected void CopyDeliveryDetails(ZGuid deliveryPK)
		{
			OrgAddress delivery = Factory.Load<OrgAddress>(deliveryPK);
			if (delivery != null)
			{
				IAddressDetails deliveryAddress = delivery;
				CA_DeliveryName = deliveryAddress.CompanyName.SubstringSafe(0, 35);
				CA_DeliveryAddress1 = deliveryAddress.AddressLine1;
				CA_DeliveryAddress2 = deliveryAddress.AddressLine2;
				if (StateProvinceIsSupported)
				{
					CA_DeliverySuburb = deliveryAddress.City;
					CA_DeliveryState = deliveryAddress.State;
				}
				else
				{
					CA_DeliverySuburb = deliveryAddress.City;
				}
				CA_DeliveryPostcode = deliveryAddress.PostCode;
				CA_DeliveryFax = deliveryAddress.Fax;
				CA_DeliveryPhone = deliveryAddress.Phone;
				CA_RN_NKDeliveryCountryCode = deliveryAddress.Country;
			}
		}

		protected virtual bool DeliveryAddressLocked
		{
			get { return (DeliveryAddress != null) && !MessageAcknowledged; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryName
		{
			get { return base.CA_DeliveryName; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryAddress1
		{
			get { return base.CA_DeliveryAddress1; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryAddress2
		{
			get { return base.CA_DeliveryAddress2; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliverySuburb
		{
			get { return base.CA_DeliverySuburb; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryPostcode
		{
			get { return base.CA_DeliveryPostcode; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryPhone
		{
			get { return base.CA_DeliveryPhone; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryFax
		{
			get { return base.CA_DeliveryFax; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_RN_NKDeliveryCountryCode
		{
			get { return base.CA_RN_NKDeliveryCountryCode; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryContactName
		{
			get { return base.CA_DeliveryContactName; }
		}

		[ReadOnlyMember(nameof(DeliveryAddressLocked))]
		public override ZString CA_DeliveryState
		{
			get { return base.CA_DeliveryState; }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region Loader
		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BaseCusSCAHouse LoadFromShipmentAndApplicationCode(ZGuid shipmentPK, ZString[] applicationCode, bool reloadQuery = false)
			{
				ZQuery query = new ZQuery(CusSCAHouseSchema.CA_JS, shipmentPK);
				return LoadFromQueryAndApplicationCode(query, applicationCode, reloadQuery);
			}

			public BaseCusSCAHouse LoadFromBGMReferenceAndApplicationCode(ZString bGMReference, ZString[] applicationCode, bool reloadQuery = false)
			{
				ZQuery query = new ZQuery(CusSCAHouseSchema.CA_BGMReference, bGMReference);
				return LoadFromQueryAndApplicationCode(query, applicationCode, reloadQuery);
			}

			//BaseCusSCAHouse LoadFromQueryAndApplicationCode(ZQuery Query, ZString[] ApplicationCode)
			//{
			//  ZDBOnlyQuery cusSCAHouseQuery = new ZDBOnlyQuery(typeof(BaseCusSCAHouse));
			//  ZDBOnlySubQuery cusSCAOceanBillQuery = new ZDBOnlySubQuery(typeof(BaseCusSCAOceanBill), CusSCAHouseSchema.CA_CB);
			//  cusSCAOceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, ApplicationCode);
			//  cusSCAHouseQuery.AddSubQuery(cusSCAOceanBillQuery, JoinCondition.And);
			//  Query.AddToFilter(cusSCAHouseQuery);
			//  return Factory.LoadTop1<BaseCusSCAHouse>(Query);
			//}

			BaseCusSCAHouse LoadFromQueryAndApplicationCode(ZQuery query, ZString[] applicationCode, bool reloadQuery = false)
			{
				if (reloadQuery)
				{
					query.ReLoadExistingRows = true;
				}

				foreach (BaseCusSCAHouse house in Factory.Load<BaseCusSCAHouse>(query))
				{
					ZQuery oceanBillQuery = new ZQuery(CusSCAOceanBillSchema.PK, house.CA_CB);
					oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, applicationCode);
					if (Factory.LoadTop1<BaseCusSCAOceanBill>(oceanBillQuery) != null)
					{
						return house;
					}
				}
				return null;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BaseCusSCAHouse);
			}
		}
		#endregion

		public virtual void DefaultFromShipment()
		{
			throw new NotImplementedException("Must implement in subclass");
		}

		#region publish Customs Status changed Event for ETail

		public override void OnSaving()
		{
			base.OnSaving();
			LogCustomsEntryStatusChangedEvent();
		}

		public override void OnSaved(bool savedSucceeded)
		{
			base.OnSaved(savedSucceeded);

			if (!savedSucceeded)
			{
				customsEntryStatusChangedEvent?.Delete();
			}

			customsEntryStatusChangedEvent = null;
		}

		protected void LogCustomsEntryStatusChangedEvent()
		{
			if (CA_IsHVLV && !CA_ShipmentStatus.Equals(CA_ShipmentStatusInfo.OriginalValue))
			{
				var statusHVLV = GetHVLVStatusMapping(CA_ShipmentStatus);
				if (!statusHVLV.IsEmpty)
				{
					var parameters = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, CA_RL_NK_PortOfDestination),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, statusHVLV)
					};

					var reason = GetReason();
					if (!reason.IsEmpty)
					{
						parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason));
					}

					if (ShouldPublishCustomsEntryStatusChangedEvent)
					{
						parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, CustomsStatusLogSubscriber.PublishCustomsStatusChangedEventService));
					}

					customsEntryStatusChangedEvent = Logs.AddNew(AutoEvents.CustomsEntryStatus, ZDateTimeOffset.Now, parameters.ToArray());
				}
			}
		}
		StmALog customsEntryStatusChangedEvent;

		protected virtual bool ShouldPublishCustomsEntryStatusChangedEvent => true;

		protected virtual ZString GetHVLVStatusMapping(ZString status) => ZString.Empty;

		protected virtual ZString GetReason() => ZString.Empty;

		#endregion

		#region IWorkflowProvider

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public bool SupportsWorkflow => SupportsWorkflowCore;

		protected virtual bool SupportsWorkflowCore => false;

		public ZString WorkflowType => WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusSCAHouseProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusSCAHouseProcessTaskCollection() => new CusSCAHouseProcessTaskCollection(this);

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => new ColumnValueRanker();

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return workflowInformationProvider ?? (workflowInformationProvider = new CusSCAHouseWorkflowInformationProvider(this));
		}
		IWorkflowInformationProvider workflowInformationProvider;

		#endregion

		#region IWorkflowProviderEvent

		OrgHeader[] IWorkflowProviderEvent.RecipientOrganisations => Array.Empty<OrgHeader>();

		#endregion

		#region IHVLVCustomsStatusPublisher

		ZString Integration.Customs.IHVLVCustomsStatusPublisher.HouseBillNumber => CA_HouseBill;

		BusinessObject Integration.Customs.IHVLVCustomsStatusPublisher.MasterBill => OceanBill;

		#endregion
	}

	public class BaseCusSCAHouseTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ZGuid cusOceanBillPK = (row != null) ? new ZGuid(row[BaseCusSCAHouse.Schema.CA_CB]) : ZGuid.Empty;
			BaseCusSCAOceanBill cusSCAOceanBill = factory.Load<BaseCusSCAOceanBill>(cusOceanBillPK);
			ZString applicationCode = (cusSCAOceanBill != null) ? cusSCAOceanBill.CB_ApplicationCode : ZString.Empty;
			switch (applicationCode)
			{
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad:
					return ObjectFactory.GetType<Integration.Customs.CA.ICusSCAHouse>();
				case Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAHouse>();
				case Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>();
#if DEBUG
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting:
					return typeof(TestCusSCAHouse);
#endif
				default:
					return typeof(DefaultCusSCAHouse);
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.Australia:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>();
				case Core.Constants.CountryCodes.NewZealand:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAHouse>();
				default:
					return null;
			}
		}
	}

#if DEBUG
	class TestCusSCAHouse : BaseCusSCAHouse
	{
		public TestCusSCAHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetHVLVStatusMapping(ZString status)
		{
			var result = ZString.Empty;
			if (status == CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Cleared)
			{
				result = status;
			}
			return result;
		}

		protected override ZString GetReason()
		{
			return "REASON FOR TEST";
		}

		protected override IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection()
		{
			return Factory.Load<BaseCusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, PK));
		}
	}
#endif
}
