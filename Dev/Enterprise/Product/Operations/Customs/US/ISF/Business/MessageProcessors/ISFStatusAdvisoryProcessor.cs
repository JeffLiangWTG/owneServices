using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory)]
	public class ISFStatusAdvisoryProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			ISFSA10 isfsa10 = GetFirstMessageBlock<ISFSA10>();
			IImporterSecurityFiling parent = LinkToParent(isfsa10);

			ISFSA30 isfsa30 = null;
			ISFSA50 isfsa50 = null;
			foreach (MessageBlock block in messageBlocks)
			{
				if (block is ISFSA30)
				{
					isfsa30 = (ISFSA30)block;
					isfsa50 = null;
				}
				else if (block is ISFSA50 && isfsa30 != null)
				{
					isfsa50 = (ISFSA50)block;
					UpdateBillStatus(parent, isfsa30, isfsa50);
				}
			}

			ISFStatusAdvisoryMessageParser parser = new ISFStatusAdvisoryMessageParser(parent, messageBlocks.ToArray());
			GlbBranch branch = parent != null ? parent.Branch : null;
			var parentAsBizObj = parent as BusinessObject;

			if (branch != null && branch.PK.IsValid)
			{
				TemporaryUserContext context = new TemporaryUserContext();
				context.BranchPK = branch.PK.ToGuid();
				using (context.Set())
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup(parser.URI, parser.JobReference, "Importer Security Filing Status Advisory", parser.HtmlData, false, branch, parentAsBizObj);
				}
			}
			else
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(parser.URI, parser.JobReference, "Importer Security Filing Status Advisory", parser.HtmlData, false, branch, parentAsBizObj);
			}
			UpdateMessageApplicationReference(isfsa10.ISFTransactionNumber);
		}

		void UpdateMessageApplicationReference(ZString iSFTransactionNumber)
		{
			Message.EM_ApplicationReference = iSFTransactionNumber;
		}

		void UpdateBillStatus(IImporterSecurityFiling parent, ISFSA30 isfsa30, ISFSA50 isfsa50)
		{
			if (parent != null)
			{
				parent.UpdateBill(isfsa30.BillNumber, isfsa50.DispositionCode, GetMessageDate());
			}
		}

		ZDateTime GetMessageDate()
		{
			ZDateTime statusDate = ZDateTime.Empty;
			if (A != null)
			{
				statusDate = A.CurrentDate;
			}

			if (statusDate.IsEmpty)
			{
				statusDate = Message.EM_MessageDateTime;
			}
			return statusDate;
		}

		IImporterSecurityFiling LinkToParent(ISFSA10 isfsa10)
		{
			var result = Message.EM_LinkedObject as IImporterSecurityFiling;
			if (result == null)
			{
				BusinessObject bizObj = null;
				MQEDIMessage originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					bizObj = originalMessage.EM_LinkedObject;
				}
				else
				{
					if (isfsa10 == null)
					{
						throw new InvalidMessageFormatException("First record is not a 'ISFSA10' record.");
					}

					ZQuery query = new ZQuery(CusISFHeaderSchema.BF_CustomsReference, isfsa10.ISFTransactionNumber);
					bizObj = Factory.LoadTop1<CusISFHeader>(query);
				}

				result = bizObj as IImporterSecurityFiling;
				if (bizObj != null && result != null)
				{
					Message.EM_LinkedObject = bizObj;
					Message.EM_LinkTable = bizObj.TableName;
				}
			}

			return result;
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			var isfsa10 = GetFirstMessageBlock<ISFSA10>();
			var parent = LinkToParent(isfsa10);
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
	}
}
