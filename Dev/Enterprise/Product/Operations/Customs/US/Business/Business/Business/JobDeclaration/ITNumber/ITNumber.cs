using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ITNumber : NonPersistentBusinessObject,
		IObsoleteValidation,
		IMessageAttacheeInDeclaration,
		ICargoManifestStatusQueryData
	{
		public ITNumber(ITAndSplitDetails billITNumber)
		{
			this.billITNumber = billITNumber;
		}
		readonly ITAndSplitDetails billITNumber;

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = billITNumber.Bill.Declaration); }
		}
		JobDeclaration declaration;

		#region ICargoManifestStatusQueryData Members

		CargoManifestQueryActionType ICargoManifestStatusQueryData.QueryActionType
		{
			get { return CargoManifestQueryActionType.InBond; }
		}

		string ICargoManifestStatusQueryData.TableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		ZString ICargoManifestStatusQueryData.HumanFriendlyReference
		{
			get { return ((IMessageAttacheeInDeclaration)this).HumanFriendlyReference; }
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return ((IMessageAttacheeInDeclaration)this).EntryNumber; }
		}

		ZString ICargoManifestStatusQueryData.JobReferenceNumber
		{
			get { return ((IMessageAttacheeInDeclaration)this).JobReferenceNumber; }
		}

		ZString ICargoManifestStatusQueryData.EntryOrInBondNumber
		{
			get { return billITNumber.US_ITNumber; }
		}

		ZString ICargoManifestStatusQueryData.MasterAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.HouseAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		bool ICargoManifestStatusQueryData.HasPGAData
		{
			get { return false; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillIssuerCode
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		void ICargoManifestStatusQueryData.LinkToMessage(EDIMessage message)
		{
			((IMessageAttacheeInDeclaration)this).Messages.Add(message);
		}

		ZGuid ICargoManifestStatusQueryData.MessageAttacheePK
		{
			get { return Declaration != null ? Declaration.PK : ZGuid.Empty; }
		}

		ZBool ICargoManifestStatusQueryData.IsRelevantFor(ZString actionCode)
		{
			return actionCode.IsEmpty || actionCode == CargoManifestStatusQueryActionList.Codes.InBond;
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return Declaration; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		#region IMessageAttacheeInDeclaration Members

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return MessageAttacheeRecordType.ITNumber; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.ITNumber; }
		}

		public ZString EntryFilerCode
		{
			get { return Declaration.US_EntryFilerCode; }
		}

		public ZString ProcessingDistrictPort
		{
			get { return ZString.Empty; }
		}

		public ZString ProcessingOfficeCode
		{
			get { return ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return billITNumber.US_ITNumber; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return 0; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return billITNumber.Bill.CU_JE; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return Declaration.Branch; }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return Declaration.RegistryCompanyPK; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return Declaration.TransportMode; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Declaration.Messages; }
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { Declaration.PK }; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return ((IMessageAttachee)Declaration).MessageStatus; }
			set { ((IMessageAttachee)Declaration).MessageStatus = value; }
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return ((IMessageAttacheeInDeclaration)Declaration).MessageStatusDescription; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Declaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Declaration != null ? Declaration.Logs : null; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return billITNumber.Factory; }
		}

		ValidationModes IMessageAttacheeInDeclaration.ValidationModes
		{
			get { return Declaration.ValidationModes; }
			set { Declaration.ValidationModes = value; }
		}

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			var cusAgent = Declaration.CusAgent;
			return cusAgent != null ? cusAgent.GS_EmailAddress : ZString.Empty;
		}

		#endregion

		#endregion
	}
}
