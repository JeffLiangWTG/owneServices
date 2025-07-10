using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class OutturnReport : DocDataObject, IDataSourceProvider
	{
		public OutturnReport(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region Containers

		public IReadOnlyCollection<BookingContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<BookingContainer> containers;

		#endregion

		#region GoodsDetails

		public IReadOnlyCollection<GoodsDetail> GoodsDetails
		{
			get => goodsDetails;
			set => goodsDetails = SetChildCollection(goodsDetails, value);
		}

		IReadOnlyCollection<GoodsDetail> goodsDetails;

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region PCS

		public ZString PCS
		{
			get => pcs;
			set
			{
				if (SetNonPersistentPropertyValue(PCSInfo, ref pcs, value))
				{
					Validate(PCSInfo);
				}
			}
		}
		ZString pcs;

		public ZPropertyInfo PCSInfo => GetZPropertyInfo(nameof(PCS));

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ConsolNumber

		public ZString ConsolNumber
		{
			get => consolNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsolNumberInfo, ref consolNumber, value))
				{
					Validate(ConsolNumberInfo);
				}
			}
		}

		ZString consolNumber;

		public ZPropertyInfo ConsolNumberInfo => GetZPropertyInfo(nameof(ConsolNumber));

		#endregion ConsolNumber

		#region BillOfLading

		public ZString BillOfLading
		{
			get => billOfLading;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingInfo, ref billOfLading, value))
				{
					Validate(BillOfLadingInfo);
				}
			}
		}
		ZString billOfLading;

		public ZPropertyInfo BillOfLadingInfo => GetZPropertyInfo(nameof(BillOfLading));

		#endregion

		#region ATATPReferenceP

		public ZString ATPReference
		{
			get => atpReference;
			set
			{
				if (SetNonPersistentPropertyValue(ATPReferenceInfo, ref atpReference, value))
				{
					Validate(ATPReferenceInfo);
				}
			}
		}
		ZString atpReference;

		public ZPropertyInfo ATPReferenceInfo => GetZPropertyInfo(nameof(ATPReference));

		#endregion

		#region LPDReference

		public ZString LPDReference
		{
			get => lpdReference;
			set
			{
				if (SetNonPersistentPropertyValue(LPDReferenceInfo, ref lpdReference, value))
				{
					Validate(LPDReferenceInfo);
				}
			}
		}
		ZString lpdReference;

		public ZPropertyInfo LPDReferenceInfo => GetZPropertyInfo(nameof(LPDReference));

		#endregion

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		#region VesselName

		public ZString VesselName
		{
			get => vesselName;
			set
			{
				if (SetNonPersistentPropertyValue(VesselNameInfo, ref vesselName, value))
				{
					Validate(VesselNameInfo);
				}
			}
		}
		ZString vesselName;

		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(nameof(VesselName));

		#endregion

		#region VoyageFlightNo

		public ZString VoyageFlightNo
		{
			get => voyageFlightNo;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNoInfo, ref voyageFlightNo, value))
				{
					Validate(VoyageFlightNoInfo);
				}
			}
		}
		ZString voyageFlightNo;

		public ZPropertyInfo VoyageFlightNoInfo => GetZPropertyInfo(nameof(VoyageFlightNo));

		#endregion

		#region SendingForwarder

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}

		Address sendingForwarder;

		#endregion

		#region SendingForwarderRegistrationNumbers

		public RegistrationNumber SendingForwarderSON
		{
			get => sendingForwarderSON;
			set => sendingForwarderSON = SetChild(sendingForwarderSON, value);
		}

		RegistrationNumber sendingForwarderSON;

		public RegistrationNumber SendingForwarderCI5
		{
			get => sendingForwarderCI5;
			set => sendingForwarderCI5 = SetChild(sendingForwarderCI5, value);
		}

		RegistrationNumber sendingForwarderCI5;

		#endregion

		#region ReceivingForwarder

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}

		Address receivingForwarder;

		#endregion

		#region ReceivingForwarderRegistrationNumbers

		public RegistrationNumber ReceivingForwarderSON
		{
			get => receivingForwarderSON;
			set => receivingForwarderSON = SetChild(receivingForwarderSON, value);
		}

		RegistrationNumber receivingForwarderSON;

		public RegistrationNumber ReceivingForwarderCI5
		{
			get => receivingForwarderCI5;
			set => receivingForwarderCI5 = SetChild(receivingForwarderCI5, value);
		}

		RegistrationNumber receivingForwarderCI5;

		#endregion

		#region CFS

		public Address CFS
		{
			get => cFS;
			set => cFS = SetChild(cFS, value);
		}

		Address cFS;

		#endregion

		#region CFSRegistrationNUmbers

		public RegistrationNumber CFSSON
		{
			get => cfsSON;
			set => cfsSON = SetChild(cfsSON, value);
		}

		RegistrationNumber cfsSON;

		public RegistrationNumber CFSCI5
		{
			get => cfsCI5;
			set => cfsCI5 = SetChild(cfsCI5, value);
		}

		RegistrationNumber cfsCI5;

		public RegistrationNumber CFSSOW
		{
			get => cfsSOW;
			set => cfsSOW = SetChild(cfsSOW, value);
		}

		RegistrationNumber cfsSOW;

		public RegistrationNumber CFSSOA
		{
			get => cfsSOA;
			set => cfsSOA = SetChild(cfsSOA, value);
		}

		RegistrationNumber cfsSOA;

		#endregion

		#region FormattedCFSProviderID

		public ZString FormattedCFSProviderID
		{
			get => formattedCFSProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedCFSProviderIDInfo, ref formattedCFSProviderID, value))
				{
					Validate(FormattedCFSProviderIDInfo);
				}
			}
		}
		ZString formattedCFSProviderID;

		public ZPropertyInfo FormattedCFSProviderIDInfo => GetZPropertyInfo(nameof(FormattedCFSProviderID));

		#endregion

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region SendingPartyRegistrationNumbers

		public RegistrationNumber SendingPartySON
		{
			get => sendingPartySON;
			set => sendingPartySON = SetChild(sendingPartySON, value);
		}

		RegistrationNumber sendingPartySON;

		public RegistrationNumber SendingPartyCI5
		{
			get => sendingPartyCI5;
			set => sendingPartyCI5 = SetChild(sendingPartyCI5, value);
		}

		RegistrationNumber sendingPartyCI5;

		public RegistrationNumber SendingPartySOW
		{
			get => sendingPartySOW;
			set => sendingPartySOW = SetChild(sendingPartySOW, value);
		}

		RegistrationNumber sendingPartySOW;

		public RegistrationNumber SendingPartySOA
		{
			get => sendingPartySOA;
			set => sendingPartySOA = SetChild(sendingPartySOA, value);
		}

		RegistrationNumber sendingPartySOA;

		#endregion

		#region FormattedSendingPartyProviderID

		public ZString FormattedSendingPartyProviderID
		{
			get => formattedSendingPartyProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedSendingPartyProviderIDInfo, ref formattedSendingPartyProviderID, value))
				{
					Validate(FormattedSendingPartyProviderIDInfo);
				}
			}
		}
		ZString formattedSendingPartyProviderID;

		public ZPropertyInfo FormattedSendingPartyProviderIDInfo => GetZPropertyInfo(nameof(FormattedSendingPartyProviderID));

		#endregion

		#region CurrentUserParty

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		Address currentUser;

		#endregion CurrentUser

		#region CurrentUserRegistrationNumbers

		public RegistrationNumber CurrentUserSON
		{
			get => currentUserSON;
			set => currentUserSON = SetChild(currentUserSON, value);
		}

		RegistrationNumber currentUserSON;

		public RegistrationNumber CurrentUserCI5
		{
			get => currentUserCI5;
			set => currentUserCI5 = SetChild(currentUserCI5, value);
		}

		RegistrationNumber currentUserCI5;

		public RegistrationNumber CurrentUserSOW
		{
			get => currentUserSOW;
			set => currentUserSOW = SetChild(currentUserSOW, value);
		}

		RegistrationNumber currentUserSOW;

		public RegistrationNumber CurrentUserSOA
		{
			get => currentUserSOA;
			set => currentUserSOA = SetChild(currentUserSOA, value);
		}

		RegistrationNumber currentUserSOA;

		#endregion
	}
}
