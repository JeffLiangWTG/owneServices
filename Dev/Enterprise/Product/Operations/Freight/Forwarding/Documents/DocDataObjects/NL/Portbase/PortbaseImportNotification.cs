using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseImportNotification : DocDataObject, IDataSourceProvider
	{
		public PortbaseImportNotification(ZString sourceType, ZString sourceID, ZString documentName)
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

		#region TransportMode

		public ICodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(transportMode, value);
		}
		ICodeDescription transportMode;

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

		#region Documents

		public IReadOnlyCollection<PortbaseDocument> Documents
		{
			get => documents;
			set => documents = SetChildCollection(documents, value);
		}
		IReadOnlyCollection<PortbaseDocument> documents;

		#endregion

		#region CurrentUser

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		IAddress currentUser;

		#endregion

		#region CTO

		public IAddress CTO
		{
			get => cto;
			set => cto = SetChild(cto, value);
		}
		IAddress cto;

		#endregion

		#region ReceivingPort

		public IUnloco ReceivingPort
		{
			get => receivingPort;
			set => receivingPort = SetChild(receivingPort, value);
		}

		IUnloco receivingPort;

		#endregion

		#region IsFerryTeminal

		public ZBool IsFerryTeminal
		{
			get => isFerryTeminal;
			set
			{
				if (SetNonPersistentPropertyValue(IsFerryTeminalInfo, ref isFerryTeminal, value))
				{
					Validate(IsFerryTeminalInfo);
				}
			}
		}

		ZBool isFerryTeminal;

		public ZPropertyInfo IsFerryTeminalInfo => GetZPropertyInfo(nameof(IsFerryTeminal));

		#endregion

		#region CarrierBookingRef

		public ZString CarrierBookingRef
		{
			get => carrierBookingRef;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingRefInfo, ref carrierBookingRef, value))
				{
					Validate(CarrierBookingRefInfo);
				}
			}
		}
		ZString carrierBookingRef;

		public ZPropertyInfo CarrierBookingRefInfo => GetZPropertyInfo(nameof(CarrierBookingRef));

		#endregion
	}
}
