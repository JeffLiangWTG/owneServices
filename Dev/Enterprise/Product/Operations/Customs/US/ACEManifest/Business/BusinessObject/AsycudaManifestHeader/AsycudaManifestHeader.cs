using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;
using AsycudaPackPackedItemPivotCollection = Enterprise.Customs.ManifestBase.AsycudaPackPackedItemPivotCollection;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader
		, IValidateForCustomsMessagingSupporter
		, ASYCUDA.Business.ISelectionItem
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string EstDateAtFirstArrival = "EstDateAtFirstArrival";
			public const int ACEManifestAMA_VoyageMaxLength = 8;
		}

		internal const string US_Air_AMS = "US Air AMS";

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = $"{US_Air_AMS} {AMA_JobReference}";
				if (!AMA_MasterBill.IsEmpty)
				{
					result += $" - {AMA_MasterBill}";
				}
				return result;
			}
		}

		[ResourceStringData("ACEManifest.Business.AsycudaManifestHeader|AMA_RL_NKPortOfFirstArrival", Caption = "Port of First Arrival", FullDescription = "First Airport of Arrival in the United States")]
		public override ZString AMA_RL_NKPortOfFirstArrival
		{
			get => base.AMA_RL_NKPortOfFirstArrival;
			set => base.AMA_RL_NKPortOfFirstArrival = value;
		}

		[ResourceStringData("ACEManifest.Business.AsycudaManifestHeader|AMA_RL_NKPortOfDischarge", Caption = "Discharge Port", FullDescription = "Discharge Port (PTP port if different to Port of First Arrival)")]
		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set => base.AMA_RL_NKPortOfDischarge = value;
		}

		[ReadOnly(true)]
		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set => base.AMA_TransportMode = value;
		}

		[MaxLength(Schema.ACEManifestAMA_VoyageMaxLength)]
		public override ZString AMA_Voyage
		{
			get => base.AMA_Voyage;
			set => base.AMA_Voyage = value;
		}

		public override ZGuid AMA_GB
		{
			get { return base.AMA_GB; }
			set
			{
				var oldValue = AMA_GB;
				base.AMA_GB = value;
				if (AMA_GB != oldValue && !IsCopying)
				{
					foreach (AsycudaArrivalHeader arrivalHeader in ArrivalHeaders)
					{
						arrivalHeader.MarkTransferBillsAsNeedingValidation();
					}
				}
			}
		}

		public ZBool IsExpressCourier
		{
			get => this.GetSystemDefinedValue<ZBool>(nameof(IsExpressCourier));
			set
			{
				var oldValue = IsExpressCourier;
				this.SetSystemDefinedValue(nameof(IsExpressCourier), value);
				IsExpressCourierInfo.RefreshBinding(oldValue);
				if (!IsExpressCourier)
				{
					Bills.ForEach(x =>
					{
						x.CustomsEntryNumberType = "";
						x.CustomsEntryNumber = "";
					});
				}
			}
		}

		public ZPropertyInfo IsExpressCourierInfo => GetZPropertyInfo(nameof(IsExpressCourier));

		public new ASYCUDA.Business.AsycudaArrivalHeaderCollection<AsycudaArrivalHeader> ArrivalHeaders => (ASYCUDA.Business.AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>)base.ArrivalHeaders;
		protected override ASYCUDA.Business.IAsycudaArrivalHeaderCollection<ASYCUDA.Business.AsycudaArrivalHeader> CreateNewAsycudaArrivalHeaderCollection() => new ASYCUDA.Business.AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>(this);
		protected override Type GetArrivalHeaderTypeCore() => typeof(AsycudaArrivalHeader);

		public new ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		public new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader> Persons => (ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>)base.Persons;
		protected override ASYCUDA.Business.CusPersonCollection CreateNewCusPersonCollection() => new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override Type GetPersonTypeCore() => typeof(CusPerson);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.UnitedStates;
		public override ZString CustomsSystem => "US Air AMS";

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		[ResourceStringData("AsycudaManifestHeader.EstDateAtFirstArrival", Caption = "Estimated Date at First Port", ShortCaption = "Est. First Port")]
		public ZDateTime EstDateAtFirstArrival
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.EstDateAtFirstArrival);
			set
			{
				var oldValue = EstDateAtFirstArrival;
				this.SetSystemDefinedValue(Schema.EstDateAtFirstArrival, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEstDateAtFirstArrival();
				}
				EstDateAtFirstArrivalInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo EstDateAtFirstArrivalInfo => GetZPropertyInfo(Schema.EstDateAtFirstArrival);

		public ZString FIRMSCode => GetCustomsRegNoFromDeconsolidateAddress(OrgCusCode.USACodeTypes.FIRMSCode);

		ZString GetCustomsRegNoFromDeconsolidateAddress(string codeType)
		{
			return DeconsolidateAddress?.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
		}

		public ZString AirAMSOriginatorCode
		{
			get
			{
				var result = GetCustomsRegNoFromDeconsolidateAddress(OrgCusCode.USACodeTypes.AirAMSOriginatorCode);
				if (result.IsEmpty)
				{
					result = GetAirAMSOriginatorCodeFromOrgProxy(GlbBranch.CurrentBranch?.OrgProxy);
				}

				if (result.IsEmpty)
				{
					result = GetAirAMSOriginatorCodeFromOrgProxy(GlbCompany.CurrentCompany?.OrgProxy);
				}
				return result;
			}
		}

		ZString GetAirAMSOriginatorCodeFromOrgProxy(OrgHeader orgProxy)
		{
			var result = ZString.Empty;
			if (orgProxy != null)
			{
				var amoCodes = orgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, Core.Constants.CountryCodes.UnitedStates);
				if (amoCodes.Length == 1)
				{
					result = amoCodes[0].OK_CustomsRegNo;
				}
				else if (amoCodes.Length > 1)
				{
					result = orgProxy.MainAddress?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
				}
			}
			return result;
		}

		#region IAsycudaManifestHeader Members

		ZDateTime Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader.EstDateAtFirstArrival
		{
			get => EstDateAtFirstArrival;
			set => EstDateAtFirstArrival = value;
		}

		#endregion

		#region IOriginatorCodeProvider Members

		ZString Integration.Customs.ASYCUDA.ACEManifest.IOriginatorCodeProvider.AirAMSOriginatorCode => AirAMSOriginatorCode;

		#endregion

		[ResourceStringData("ACEAsycudaManifestHeader.BillStatusDescription", Caption = "Bill Status Desc.")]
		public ZString BillStatusDescription => MasterBill?.ABL_BillStatusDescription ?? ZString.Empty;

		[ResourceStringData("ACEAsycudaManifestHeader.BillStatus", Caption = "Bill Status")]
		public ZString BillStatus => MasterBill?.ABL_BillStatus ?? ZString.Empty;

		public ZInt TotalPieces => Bills.Cast<AsycudaBill>().Sum(bill => bill.ABL_ManifestQty);
		public ZDecimal TotalWeightInKilos => Bills.Cast<AsycudaBill>().Sum(bill => bill.MassInKilos);

		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new ACEManifestMessageSendingNotificationHelper(this);
		}

		protected override ASYCUDA.Business.MessageChooser GetNewMessageChooserCore(IEnumerable<ASYCUDA.Business.ISelectionItem> items, string messageType, bool showStatus)
		{
			switch (messageType)
			{
				case AIMMessageSubTypes.FRI:
				case AIMMessageSubTypes.FXI:
				case AIMMessageSubTypes.FRC:
				case AIMMessageSubTypes.FXC:
				case AIMMessageSubTypes.FRX:
				case AIMMessageSubTypes.FXX:
				case AIMMessageSubTypes.FSQ:
					{
						return new AIMMessageChooser(this, items, messageType);
					}
			}
			return base.GetNewMessageChooserCore(items, messageType, showStatus);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_Nature = ShipmentTypeList.Codes.Import23;
			AMA_TransportMode = Core.Constants.TransportModes.Air;

			var orgProxy = GlbBranch.CurrentBranch?.OrgProxy;
			if (orgProxy != null && orgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, Core.Constants.CountryCodes.UnitedStates).Length > 0)
			{
				AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = orgProxy.PK;
			}
		}

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = AMA_OA_Carrier;
				base.AMA_OA_Carrier = value;
				if (AMA_CarrierCode.IsEmpty && oldValue != AMA_OA_Carrier && (Carrier?.Header?.MiscServ?.IsAirline ?? false))
				{
					var airLineDetail = Carrier.Header.MiscServ.Airline;
					var airlinePrefix2Char = airLineDetail?.RM_TwoCharacterCode ?? string.Empty;
					var airlinePrefix3Char = airLineDetail?.RM_ThreeLetterCode ?? string.Empty;
					AMA_CarrierCode = (airlinePrefix2Char.Length == 2 && (airlinePrefix2Char[1] > '9' || airlinePrefix2Char[1] < '0')) ? airlinePrefix2Char : airlinePrefix3Char;
				}
			}
		}

		[ResourceStringData("ACEManifest.Business.AsycudaManifestHeader.AMA_OA_DeconsolidateAddress", Caption = "CFS Address")]
		public override ZGuid AMA_OA_DeconsolidateAddress
		{
			get => base.AMA_OA_DeconsolidateAddress;
			set => base.AMA_OA_DeconsolidateAddress = value;
		}

		protected override ZAddress GetNewAMA_OA_DeconsolidateAddress_ZAddress()
		{
			var address = base.GetNewAMA_OA_DeconsolidateAddress_ZAddress();
			address.GetDefaultAddress = GetDefaultAddress;
			address.DefaultAddressType = AddressType.DLV;
			address.AddressIsMandatoryIfOrgIsValid = true;
			return address;
		}

		ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			var result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				var amoCodes = organisation.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, Core.Constants.CountryCodes.UnitedStates);
				if (amoCodes.Length == 1)
				{
					result = amoCodes.FirstOrDefault().PremisesAddress.PK;
				}
			}
			return result;
		}

		protected override ZBool IsDeconsolidatorEnabledCore => true;

		#region ISelectionItem

		ZGuid ASYCUDA.Business.ISelectionItem.PK => this.PK;

		string ASYCUDA.Business.ISelectionItem.SelectionDescription(bool showStatus)
		{
			return FormattableString.Invariant($"HAWB Number - {AMA_MasterBill}");
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction) => this;

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging => true;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = ACEManifestTypes.Codes.IAM;
		}
#endif
	}
}
