using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

[assembly: CBPMessageProcessorProvider(typeof(Enterprise.Customs.US.ISF.Business.ISFProcessor))]
namespace Enterprise.Customs.US.ISF.Business
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse)]
	public class ISFProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			IImporterSecurityFiling parent = LinkToParent();
			ISFMessageParser parser = new ISFMessageParser(parent, messageBlocks.ToArray());
			var iSFTransactionNumber = GetISFTransactionNumber();
			if (parent != null && !iSFTransactionNumber.IsEmpty)
			{
				parent.SFTransactionNumber = iSFTransactionNumber;
			}

			bool isFailure = parser.Status == ABIResponseStatus.Rejected;
			GlbBranch branch = parent != null ? parent.Branch : null;
			GenerateHtmlEmailAndSendToOriginalOrGroup(parser.URI, parser.JobReference, "Importer Security Filing", parser.HtmlData, isFailure, branch, parent as BusinessObject);
			CalculateStatus(parent, parser.Status);
			if (!isFailure)
			{
				UpdateAcceptedDate(parent);
			}
			UpdateMessageApplicationReference(iSFTransactionNumber);
		}

		void UpdateMessageApplicationReference(ZString iSFTransactionNumber)
		{
			var originalMessage = Message.OriginalMessage;
			if (iSFTransactionNumber.IsEmpty)
			{
				if (originalMessage != null && !originalMessage.EM_ApplicationReference.IsEmpty)
				{
					Message.EM_ApplicationReference = originalMessage.EM_ApplicationReference;
				}
			}
			else
			{
				Message.EM_ApplicationReference = iSFTransactionNumber;
				if (originalMessage != null && originalMessage.EM_ApplicationReference.IsEmpty)
				{
					originalMessage.EM_ApplicationReference = iSFTransactionNumber;
				}
			}
		}

		ZString GetISFTransactionNumber()
		{
			ISFSF10 sf10 = GetFirstMessageBlock<ISFSF10>();
			return sf10 == null ? ZString.Empty : sf10.ISFTransactionNumber;
		}

		void UpdateAcceptedDate(IImporterSecurityFiling parent)
		{
			if (parent != null)
			{
				parent.UpdateAcceptedDate(GetMessageDate());
			}
		}

		ZDateTime GetMessageDate()
		{
			ZDateTime acceptedDate = ZDateTime.Empty;
			if (A != null)
			{
				acceptedDate = A.CurrentDate;
			}

			if (acceptedDate.IsEmpty)
			{
				acceptedDate = Message.EM_MessageDateTime;
			}
			return acceptedDate;
		}

		void CalculateStatus(IImporterSecurityFiling header, ABIResponseStatus status)
		{
			if (header != null)
			{
				new ImporterSecurityFilingMessageStatusCalculator(header).CalculateStatus(Message, status);
			}
		}

		IImporterSecurityFiling LinkToParent()
		{
			var result = Message.EM_LinkedObject as IImporterSecurityFiling;
			if (result == null)
			{
				var originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					var bizObj = originalMessage.EM_LinkedObject;
					result = bizObj as IImporterSecurityFiling;
					if (bizObj != null && result != null)
					{
						Message.EM_LinkedObject = bizObj;
						Message.EM_LinkTable = bizObj.TableName;
					}
				}
			}

			return result;
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			var parent = LinkToParent();
			Integration.IRegistryItem notificationRegistryItem = ISFRegistry.Instance.ImporterSecurityFilingMessagesGroup;

			if (parent != null)
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, parent.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferredCode);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "TYP=HVL");

				if (Factory.Exists(typeof(StmALog), query))
				{
					notificationRegistryItem = ISFRegistry.Instance.HVLVImporterSecurityFilingMessagesGroup;
				}
			}

			return notificationRegistryItem;
		}

		protected override List<ZString> ExcludedErrorCodes
		{
			get
			{
				List<ZString> result = base.ExcludedErrorCodes;
				result.Add("108"); // INVALID ISF TRANSACTION NUMBER
				result.Add("113"); // INVALID ISF TRANSACTION NUMBER
				result.Add("117"); // DUPLICATE ISF TRANSACTION
				result.Add("125"); // INVALID ISF IMPORTER NUMBER
				result.Add("127"); // INVALID ISF BOND HOLDER 
				result.Add("128"); // INVALID SURETY CODE
				result.Add("133"); // INVALID ISF IMPORTER NUMBER
				result.Add("154"); // INVALID HOUSE OR REGULAR BOL NUMBER
				result.Add("155"); // INVALID SCAC
				result.Add("156"); // INVALID BILL
				result.Add("316"); // INVALID IRS FORMAT IN ENTITY IDENTIFIER
				result.Add("317"); // INVALID ANI FORMAT IN ENTITY IDENTIFIER
				result.Add("321"); // INVALID DUNS NUMBER IN ENTITY IDENTIFIER
				result.Add("329"); // INVALID ENTITY IDENTIFIER
				return result;
			}
		}
	}
}
