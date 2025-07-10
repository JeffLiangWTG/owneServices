using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DummyIInBondWP : DummyBusinessObject
			, IInBondWPHeader
			, IInBondArriveExportTOLHeader
			, IMessageActionHeader
			, IMessageAttacheeInDeclaration
	{
		public DummyIInBondWP(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsActive
		{
			get { return true; }
		}

		public ZString ActionCode
		{
			get { return fActionCode; }
			set { fActionCode = value; }
		}
		ZString fActionCode;

		public ZString InBondNumber
		{
			get { return fInbondNumber; }
			set { fInbondNumber = value; }
		}
		ZString fInbondNumber;

		public ZString JobReferenceNumber
		{
			get { return "Blah"; }
		}

		public ZString EntryNumber
		{
			get { return "Blah"; }
		}

		public ZString ArrivalFirmsCode
		{
			get { return "Blah"; }
		}

		public ZString MasterBillIssuerCode
		{
			get { return fMasterBillIssuerCode; }
			set { fMasterBillIssuerCode = value; }
		}
		ZString fMasterBillIssuerCode;

		public ZString MasterBillNumber
		{
			get { return fMasterBillNumber; }
			set { fMasterBillNumber = value; }
		}
		ZString fMasterBillNumber;

		public ZString ContainerNumber
		{
			get { return fContainerNumber; }
			set { fContainerNumber = value; }
		}
		ZString fContainerNumber;

		public ZDateTime ArrivalDateTime
		{
			get { return fArrivalDateTime; }
			set { fArrivalDateTime = value; }
		}
		ZDateTime fArrivalDateTime;

		public ZString ScheduleDPortOfArrival
		{
			get { return scheduleDPortOfArrival; }
			set { scheduleDPortOfArrival = value; }
		}
		ZString scheduleDPortOfArrival;

		public ZDateTime ExportDateTime
		{
			get { return fExportDateTime; }
			set { fExportDateTime = value; }
		}
		ZDateTime fExportDateTime;

		public ZString PortOfExport
		{
			get { return fPortOfExport; }
			set { fPortOfExport = value; }
		}
		ZString fPortOfExport;

		GlbBranch IMessageAttachee.Branch
		{
			get { return Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return ZString.Empty; }
		}
		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return null; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return null; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return Guid.Empty; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return ""; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return null; }
		}

		public ZString EntryStatus
		{
			get { return entryStatus; }
			set { entryStatus = value; }
		}
		ZString entryStatus;

		public ZString RecordTypeDescription
		{
			get { return recordTypeDescription; }
			set { recordTypeDescription = value; }
		}
		ZString recordTypeDescription = "RecordTypeDescription";

		public ZString AdditionalReferenceInformation
		{
			get { return "AdditionalReferenceInformation"; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get { return ZString.Empty; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get { return ZString.Empty; }
		}

		public ZGuid InBondCarrier
		{
			get { return fInBondCarrier; }
			set { fInBondCarrier = value; }
		}
		ZGuid fInBondCarrier;

		public ZPropertyInfo InBondCarrierInfo
		{
			get { return GetZPropertyInfo(nameof(InBondCarrier)); }
		}

		public ZString InBondCarrierCode
		{
			get { return fInBondCarrierCode; }
			set { fInBondCarrierCode = value; }
		}
		ZString fInBondCarrierCode;

		public ZString BondedCarrierID
		{
			get { return fBondedCarrierID; }
			set { fBondedCarrierID = value; }
		}
		ZString fBondedCarrierID;

		public void RecalculateValidationModesOnDeclaration()
		{
		}

		public ZDateTime TOLDateTime
		{
			get { return fTOLDateTime; }
			set { fTOLDateTime = value; }
		}
		ZDateTime fTOLDateTime;

		public ZString CityName
		{
			get { return fCityName; }
			set { fCityName = value; }
		}
		ZString fCityName;

		public ZPropertyInfo CityNameInfo
		{
			get { return GetZPropertyInfo(nameof(CityName)); }
		}

		public ZString StateCode
		{
			get { return fStateCode; }
			set { fStateCode = value; }
		}
		ZString fStateCode;

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StateCode)); }
		}

		public ZString InBondExportTransportMode
		{
			get { return fExportTransportMode; }
			set { fExportTransportMode = value; }
		}
		ZString fExportTransportMode;

		public ZPropertyInfo ExportTransportModeInfo
		{
			get { return GetZPropertyInfo("ExportTransportMode"); }
		}
		public ZString InBondImportTransportMode
		{
			get { return fImportTransportMode; }
			set { fImportTransportMode = value; }
		}
		ZString fImportTransportMode;

		public ZPropertyInfo ImportTransportModeInfo
		{
			get { return GetZPropertyInfo("ImportTransportMode"); }
		}

		public ZString ArrivalConveyance
		{
			get { return fArrivalConveyance; }
			set { fArrivalConveyance = value; }
		}
		ZString fArrivalConveyance;

		public ZString ExportConveyance
		{
			get { return fExportConveyance; }
			set { fExportConveyance = value; }
		}
		ZString fExportConveyance;

		public ZPropertyInfo ExportConveyanceInfo
		{
			get { return GetZPropertyInfo(nameof(ExportConveyance)); }
		}

		public ZString MessageStatus
		{
			get { return fStatus; }
			set { fStatus = value; }
		}
		ZString fStatus;

		public ZString MessageStatusDescription
		{
			get { return StatusList.GetDescriptionFromCode(MessageStatus); }
		}

		ImportMessageStatusList StatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		public ValidationModes ValidationModes
		{
			get { return fValidationMode; }
			set { fValidationMode = value; }
		}
		ValidationModes fValidationMode;

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return ZGuid.Empty; }
		}

		void IInBondArriveExportTOLHeader.UpdateLinkBusinessObject(MQEDIMessage message)
		{
		}

		public ZString EntryFilerCode
		{
			get { return EntryFilerCodeForTesting; }
		}
		public ZString EntryFilerCodeForTesting;

		public bool HasAClearInBondDepartureLog
		{
			get { return fHasAClearInBondDepartureLog; }
			set { fHasAClearInBondDepartureLog = value; }
		}
		bool fHasAClearInBondDepartureLog;

		public bool HasAClearInBondArrivalLog
		{
			get { return fHasAClearInBondArrivalLog; }
			set { fHasAClearInBondArrivalLog = value; }
		}
		bool fHasAClearInBondArrivalLog;

		public NotificationInfo[] NotificationInfos = Array.Empty<NotificationInfo>();
		public NotificationInfo[] GetMessageSendingNotificationsFor(string actionCode)
		{
			return NotificationInfos;
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		public void AddAndLinkMessage(MQEDIMessage messageToAdd)
		{
			Messages.Add(messageToAdd);
			messageToAdd.EM_LinkedObject = this;
		}

		public ZString HumanFriendlyReference
		{
			get { return fHumanFriendlyReference; }
			set { fHumanFriendlyReference = value; }
		}
		ZString fHumanFriendlyReference;

		public MessageAttacheeRecordType RecordType
		{
			get { return fRecordType; }
			set { fRecordType = value; }
		}
		MessageAttacheeRecordType fRecordType;

		public ZDateTime ReleaseDate
		{
			get { return releaseDate; }
			set { releaseDate = value; }
		}
		ZDateTime releaseDate;

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return ZInt.Zero; }
		}

		public IReadOnlyList<IMessageAttacheeInDeclaration> MessageAttachees
		{
			get
			{
				if (fMessageAttachees == null)
				{
					fMessageAttachees = Array.Empty<IMessageAttacheeInDeclaration>();
				}
				return fMessageAttachees;
			}
			set
			{
				fMessageAttachees = value;
			}
		}
		IReadOnlyList<IMessageAttacheeInDeclaration> fMessageAttachees;

		public ZString JobNumber
		{
			get { return fJobNumber; }
			set { fJobNumber = value; }
		}
		ZString fJobNumber;

		#region ICusCodeDataParentBizObj Members

		public ZString ParentTableCode
		{
			get { return "Z0"; }
		}

		#endregion

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			return ZString.Empty;
		}

		#endregion
	}
}
