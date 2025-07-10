using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public class LiquidationWithMessagesToShow : NonPersistentBusinessObject, IObsoleteValidation, IMessageAttacheeInDeclaration
	{
		public LiquidationWithMessagesToShow(CusLiquidationCollection liquidations) : base(liquidations.Factory)
		{
			this.liquidations = liquidations;
		}
		readonly CusLiquidationCollection liquidations;

		JobDeclaration Declaration
		{
			get { return liquidations.Master; }
		}

		#region IMessageAttacheeInDeclaration Members

		public CBPEDIMessageCollection Messages
		{
			get { throw new InvalidOperationException("Messages collection not relevant for this Liquidation Object. This Liquidation contains all messages from Liquidations which belongs to Declaration."); }
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return liquidations.GetPKs().ToArray(); }
		}

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return MessageAttacheeRecordType.Liquidation; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.Liquidation; }
		}

		public ZString EntryFilerCode
		{
			get { return Declaration != null ? Declaration.US_EntryFilerCode : ZString.Empty; }
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
			get { return GetEntryNumber(); }
		}

		ZString GetEntryNumber()
		{
			return Declaration != null ? Declaration.ImportEntryNumber : ZString.Empty;
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
			get
			{
				return liquidations.Where(x => x.B8_NoOfSuspensions != 0).OrderByDescending(l => l.B8_SystemCreateDate).FirstOrDefault()?.B8_NoOfSuspensions ?? 0;
			}
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return GetEntryNumber(); }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return Declaration != null ? Declaration.JE_DeclarationReference : ZString.Empty; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return Declaration != null ? Declaration.PK : ZGuid.Empty; }
		}

		ValidationModes IMessageAttacheeInDeclaration.ValidationModes
		{
			get { return Declaration != null ? Declaration.ValidationModes : ValidationModes.None; }
			set { throw new InvalidOperationException("Validation Modes for liquidation should not be settable. Validation moved should be retrieved from the declaration."); }
		}

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			ZString result = ZString.Empty;
			if (Declaration != null)
			{
				var cusAgent = Declaration.CusAgent;
				result = cusAgent != null ? cusAgent.GS_EmailAddress : ZString.Empty;
			}
			return result;
		}

		#endregion

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return this; }
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

		GlbBranch IMessageAttachee.Branch
		{
			get { return Declaration != null ? Declaration.Branch : GlbBranch.CurrentBranch; }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return Declaration?.RegistryCompanyPK ?? GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return Declaration?.TransportMode ?? ZString.Empty; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get;
			set;
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
