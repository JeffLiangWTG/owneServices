using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class VerifiedGrossMass : DocDataObject, IDataSourceProvider
	{
		public VerifiedGrossMass(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		readonly ZString sourceType;
		readonly ZString sourceID;

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		ZString IDataSourceProvider.SourceType => sourceType;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region Shipper

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		Address shipper;

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}
		Address carrier;

		#endregion

		#region Consignee

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		Address consignee;

		#endregion

		#region FreightForwarder

		public Address FreightForwarder
		{
			get => freightForwarder;
			set => freightForwarder = SetChild(freightForwarder, value);
		}
		Address freightForwarder;

		#endregion

		#region CarrierHandlingAgent

		public Address CarrierHandlingAgent
		{
			get => carrierHandlingAgent;
			set => carrierHandlingAgent = SetChild(carrierHandlingAgent, value);
		}

		Address carrierHandlingAgent;

		#endregion

		#region CarrierBookingAgent

		public Address CarrierBookingAgent
		{
			get => carrierBookingAgent;
			set => carrierBookingAgent = SetChild(carrierBookingAgent, value);
		}

		Address carrierBookingAgent;

		#endregion

		#region CurrentUser

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		Address currentUser;

		#endregion

		#region Title

		public ZString Title
		{
			get => title;
			set
			{
				if (SetNonPersistentPropertyValue(TitleInfo, ref title, value))
				{
					Validate(TitleInfo);
				}
			}
		}

		ZString title;

		public ZPropertyInfo TitleInfo => GetZPropertyInfo(nameof(Title));

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
					Validate(CarrierBookingReferenceInfo);
				}
			}
		}

		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region BillOfLadingNumber

		public ZString BillOfLadingNumber
		{
			get => billOfLadingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingNumberInfo, ref billOfLadingNumber, value))
				{
					Validate(BillOfLadingNumberInfo);
				}
			}
		}

		ZString billOfLadingNumber;

		public ZPropertyInfo BillOfLadingNumberInfo => GetZPropertyInfo(nameof(BillOfLadingNumber));

		#endregion

		#region FreightForwardersReference

		public ZString FreightForwardersReference
		{
			get => freightForwardersReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwardersReferenceInfo, ref freightForwardersReference, value))
				{
					Validate(FreightForwardersReferenceInfo);
				}
			}
		}

		ZString freightForwardersReference;

		public ZPropertyInfo FreightForwardersReferenceInfo => GetZPropertyInfo(nameof(FreightForwardersReference));

		#endregion

		#region FirstSeaLegForChina

		public Transport FirstSeaLegForChina
		{
			get => firstSeaLegForChina;
			set => firstSeaLegForChina = SetChild(firstSeaLegForChina, value);
		}

		Transport firstSeaLegForChina;

		#endregion

		#region ContainerMode

		public CodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		CodeDescription containerMode;

		#endregion

		#region SignedBy

		public SignatureDetails SignedBy
		{
			get => signedBy;
			set => signedBy = SetChild(signedBy, value);
		}
		SignatureDetails signedBy;

		#endregion

		#region Containers

		public IReadOnlyCollection<VGMMessagingContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<VGMMessagingContainer> containers;

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion

		#region IsRequiredSendAttachment

		public ZBool IsRequiredSendAttachment
		{
			get => isRequiredSendAttachment;
			set
			{
				if (SetNonPersistentPropertyValue(IsRequiredSendAttachmentInfo, ref isRequiredSendAttachment, value))
				{
					Validate(IsRequiredSendAttachmentInfo);
				}
			}
		}
		ZBool isRequiredSendAttachment;

		public ZPropertyInfo IsRequiredSendAttachmentInfo => GetZPropertyInfo(nameof(IsRequiredSendAttachment));

		#endregion
	}
}
