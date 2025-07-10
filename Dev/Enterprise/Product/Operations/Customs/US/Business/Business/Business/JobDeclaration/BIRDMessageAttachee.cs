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
	class BIRDMessageAttachee : IMessageAttacheeInDeclaration
	{
		public BIRDMessageAttachee(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		#region IMessageAttacheeInDeclaration Members

		ZString IMessageAttachee.MessageStatus
		{
			get;
			set;
		}

		public CBPEDIMessageCollection Messages
		{
			get { return null; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return declaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return declaration.JE_DeclarationReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return declaration.Logs; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return declaration.Branch; }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return declaration.RegistryCompanyPK; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return declaration.Factory; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return declaration.PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ((IMessageAttachee)declaration).ControllerID; }
		}

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return MessageAttacheeRecordType.BIRD; }
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.BIRD; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return declaration.JE_DeclarationReference; }
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return declaration.JE_DeclarationReference; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return declaration.PK; }
		}

		ValidationModes IMessageAttacheeInDeclaration.ValidationModes
		{
			get { return declaration.ValidationModes; }
			set { throw new InvalidOperationException("Validation Modes for consolidated BIRD should not be settable. Validation modes should be retrieved from the declaration."); }
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return declaration.ImportEntryNumber; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return declaration.JE_EntryAuthorisationDate; }
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return 0; }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get
			{
				var pKs = new List<ZGuid>();
				pKs.Add(declaration.PK);
				pKs.AddRange(declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Select(ceh => ceh.PK));

				foreach (CusLiquidation liquidation in declaration.Liquidations)
				{
					if (liquidation.Message.IsBIRDTransaction)
					{
						pKs.Add(liquidation.PK);
					}
				}

				return pKs.ToArray();
			}
		}

		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode
		{
			get { return declaration.US_EntryFilerCode; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get { return declaration.ProcessingDistrictPort; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get { return declaration.ProcessingOfficeCode; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return declaration.TransportMode; }
		}

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			return ((IMessageResponseNotificator)declaration).GetFallbackEmailAddressRecipient();
		}

		#endregion
	}
}
