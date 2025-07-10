using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class CertifiedPickup : DocDataObject, IDataSourceProvider
	{
		public CertifiedPickup(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region Terminal

		public ZString Terminal
		{
			get => terminal;
			set
			{
				if (SetNonPersistentPropertyValue(TerminalInfo, ref terminal, value))
				{
					Validate(TerminalInfo);
				}
			}
		}

		ZString terminal;

		public ZPropertyInfo TerminalInfo => GetZPropertyInfo(nameof(Terminal));

		#endregion

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

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion

		#region SendingPartyId

		public RegistrationNumber SendingPartyId
		{
			get => sendingPartyId;
			set => sendingPartyId = SetChild(sendingPartyId, value);
		}

		RegistrationNumber sendingPartyId;

		#endregion

		#region Forwarder

		public Address Forwarder
		{
			get => forwarder;
			set => forwarder = SetChild(forwarder, value);
		}

		Address forwarder;

		#endregion

		#region ForwarderId

		public RegistrationNumber ForwarderId
		{
			get => forwarderId;
			set => forwarderId = SetChild(forwarderId, value);
		}

		RegistrationNumber forwarderId;

		#endregion

		#region TransportCompany

		public IAddress TransportCompany
		{
			get => transportCompany;
			set => transportCompany = SetChild(transportCompany, value);
		}

		IAddress transportCompany;

		#endregion

		#region TransportCompanyId

		public RegistrationNumber TransportCompanyId
		{
			get => transportCompanyId;
			set => transportCompanyId = SetChild(transportCompanyId, value);
		}

		RegistrationNumber transportCompanyId;

		#endregion

		#region CarrierIdentificationId

		public RegistrationNumber CarrierIdentificationId
		{
			get => carrierIdentificationId;
			set => carrierIdentificationId = SetChild(carrierIdentificationId, value);
		}

		RegistrationNumber carrierIdentificationId;

		#endregion

		#region Containers

		public IReadOnlyCollection<CertifiedPickupContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<CertifiedPickupContainer> containers;

		#endregion

		#region SelectedContainers

		public IEnumerable<CertifiedPickupContainer> SelectedContainers => Containers.Where(c => c.FormMode == FormMode);

		#endregion

		#region DataObjectWriter Fields

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
					Validate(ErrorPlaceHolderInfo);
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion ErrorPlaceHolder

		#region Form Mode

		public const string FormModeAcceptDecline = "AcceptDecline"; // Untranslatable const
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable const")]
		public const string FormModeTransfer = "Transfer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable const")]
		public const string FormModeRevoke = "Revoke";
		public const string FormModeNotApplicable = "NotApplicable"; // Untranslatable const

		public string FormMode { get; set; }

		public bool IsTransferMode => FormMode == FormModeTransfer;
		public bool IsRevokeMode => FormMode == FormModeRevoke;

		#endregion
	}
}
