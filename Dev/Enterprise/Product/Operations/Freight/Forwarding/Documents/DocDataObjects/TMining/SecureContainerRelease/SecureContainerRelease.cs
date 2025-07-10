using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SecureContainerRelease : DocDataObject, IDataSourceProvider
	{
		public SecureContainerRelease(ZString sourceType, ZString sourceID)
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

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion

		#region SendingPartyId

		public RegistrationNumber SendingPartyIdEOR
		{
			get => sendingPartyIdEOR;
			set => sendingPartyIdEOR = SetChild(sendingPartyIdEOR, value);
		}

		RegistrationNumber sendingPartyIdEOR;

		public RegistrationNumber SendingPartyIdDUN
		{
			get => sendingPartyIdDUN;
			set => sendingPartyIdDUN = SetChild(sendingPartyIdDUN, value);
		}

		RegistrationNumber sendingPartyIdDUN;

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

		public RegistrationNumber ForwarderIdEOR
		{
			get => forwarderIdEOR;
			set => forwarderIdEOR = SetChild(forwarderIdEOR, value);
		}

		RegistrationNumber forwarderIdEOR;

		public RegistrationNumber ForwarderIdDUN
		{
			get => forwarderIdDUN;
			set => forwarderIdDUN = SetChild(forwarderIdDUN, value);
		}

		RegistrationNumber forwarderIdDUN;

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

		public RegistrationNumber TransportCompanyIdEOR
		{
			get => transportCompanyIdEOR;
			set => transportCompanyIdEOR = SetChild(transportCompanyIdEOR, value);
		}

		RegistrationNumber transportCompanyIdEOR;

		public RegistrationNumber TransportCompanyIdDUN
		{
			get => transportCompanyIdDUN;
			set => transportCompanyIdDUN = SetChild(transportCompanyIdDUN, value);
		}

		RegistrationNumber transportCompanyIdDUN;

		#endregion

		#region Containers

		public IReadOnlyCollection<SecureContainerReleaseContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<SecureContainerReleaseContainer> containers;

		#endregion

		#region SelectedContainers

		public IEnumerable<SecureContainerReleaseContainer> SelectedContainers => Containers.Where(c => c.FormMode == FormMode);

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
