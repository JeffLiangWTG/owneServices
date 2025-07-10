using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseExportNotification : DocDataObject, IDataSourceProvider
	{
		public PortbaseExportNotification(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region IDataSourceProvider members

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

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

		#endregion

		#region BookingConfirmationReference

		public ZString BookingConfirmationReference
		{
			get => bookingConfirmationReference;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationReferenceInfo, ref bookingConfirmationReference, value))
				{
					Validate(BookingConfirmationReferenceInfo);
				}
			}
		}
		ZString bookingConfirmationReference;

		public ZPropertyInfo BookingConfirmationReferenceInfo => GetZPropertyInfo(nameof(BookingConfirmationReference));

		#endregion

		#region OperationalPort
		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion OperationalPort

		#region DepartureCTO

		public Address DepartureCTO
		{
			get => departureCTO;
			set => departureCTO = SetChild(departureCTO, value);
		}
		Address departureCTO;

		#endregion

		#region TerminalFenexRegNo

		public ZString TerminalFenexRegNo
		{
			get => terminalFenexRegNo;
			set
			{
				if (SetNonPersistentPropertyValue(TerminalFenexRegNoInfo, ref terminalFenexRegNo, value))
				{
					Validate(TerminalFenexRegNoInfo);
				}
			}
		}
		ZString terminalFenexRegNo;

		public ZPropertyInfo TerminalFenexRegNoInfo => GetZPropertyInfo(nameof(TerminalFenexRegNo));

		#endregion

		#region TerminalDescription

		public ZString TerminalDescription
		{
			get => terminalDescription;
			set
			{
				if (SetNonPersistentPropertyValue(TerminalDescriptionInfo, ref terminalDescription, value))
				{
					Validate(TerminalDescriptionInfo);
				}
			}
		}
		ZString terminalDescription;

		public ZPropertyInfo TerminalDescriptionInfo => GetZPropertyInfo(nameof(TerminalDescription));

		#endregion

		#region IsFerryTerminal

		public ZBool IsFerryTerminal
		{
			get => isFerryTerminal;
			set
			{
				if (SetNonPersistentPropertyValue(IsFerryTerminalInfo, ref isFerryTerminal, value))
				{
					Validate(IsFerryTerminalInfo);
				}
			}
		}

		ZBool isFerryTerminal;

		public ZPropertyInfo IsFerryTerminalInfo => GetZPropertyInfo(nameof(IsFerryTerminal));

		#endregion

		#region SendersCustomsNo

		public ZString SendersCustomsNo
		{
			get => sendersCustomsNo;
			set
			{
				if (SetNonPersistentPropertyValue(SendersCustomsNoInfo, ref sendersCustomsNo, value))
				{
					Validate(SendersCustomsNoInfo);
				}
			}
		}
		ZString sendersCustomsNo;

		public ZPropertyInfo SendersCustomsNoInfo => GetZPropertyInfo(nameof(SendersCustomsNo));

		#endregion

		#region Documents

		public IReadOnlyCollection<PortbaseDocument> Documents
		{
			get => documents;
			set => documents = SetChildCollection(documents, value);
		}
		IReadOnlyCollection<PortbaseDocument> documents;

		#endregion

		#region CurrentUser

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		Address currentUser;

		#endregion

		#region SelectAllToSend

		public ZBool SelectAllToSend
		{
			get => selectAllToSend;
			set
			{
				if (SetNonPersistentPropertyValue(SelectAllToSendInfo, ref selectAllToSend, value)
					&& documents != null)
				{
					Validate(SelectAllToSendInfo);

					foreach (var document in documents)
					{
						document.IsSelectedToSend = value;
					}
				}
			}
		}

		ZBool selectAllToSend;

		public ZPropertyInfo SelectAllToSendInfo => GetZPropertyInfo(nameof(SelectAllToSend));

		#endregion
	}
}
