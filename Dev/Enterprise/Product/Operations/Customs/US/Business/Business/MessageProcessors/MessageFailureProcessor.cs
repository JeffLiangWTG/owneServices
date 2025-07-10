using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AllMessages)]
	[TopLevel(typeof(ENSEB))]
	public class AllMessageFailureProcessor : ACSMessageFailureProcessor
	{
		protected override void SetFailStatus(BusinessObject bizObj)
		{
			IMessageFailStatusManager messageFailStatusManager = bizObj as IMessageFailStatusManager;
			if (messageFailStatusManager != null && messageFailStatusManager.IsMessageTypeSupported(Message.EM_MessageType))
			{
				messageFailStatusManager.SetFailStatus(Message);
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[TopLevel(typeof(AABIX0), typeof(AABIOutputX1))]
	public class FTZMessageFailureProcessor : AllACEMessageFailureProcessor
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling)]
	[TopLevel(typeof(AABIX0), typeof(AABIOutputX1))]
	public class ISFMessageFailureProcessor : AllMessageFailureProcessor
	{
	}

	public abstract class ACSMessageFailureProcessor : MessageFailureProcessor<APLA, APLB, APLY>
	{
	}

	public abstract class ACEMessageFailureProcessor : MessageFailureProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class MessageFailureProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
			HtmlTableCreator errorTable = new HtmlTableCreator(new string[] { Res.GetString("{63C60ACA-5D2D-4d0c-A3FC-1C1F84032C29}", "Error Description") });

			foreach (MessageBlock block in messageBlocks)
			{
				IStatusesAndErrors errorBlock = block as IStatusesAndErrors;
				if (errorBlock != null)
				{
					errorTable.WriteRow(errorBlock.NarrativeMessage);
				}
			}

			string jobNumber = "";
			string uri = "";
			GlbBranch branch = null;
			BusinessObject bizObj = OriginalMessageLinker.Link(Message);
			if (bizObj != null)
			{
				IMessageAttachee messageAttachee = GetMessageAttachee(bizObj);

				if (messageAttachee != null)
				{
					jobNumber = messageAttachee.TopLevelBizObjReferenceNumber;
					if (!string.IsNullOrEmpty(jobNumber))
					{
						uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(messageAttachee);
					}
					branch = messageAttachee.Branch;
				}
				else
				{
					jobNumber = bizObj.HumanReadableName;
				}
				SetFailStatus(bizObj);
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, GetMessageTypeDescription(), errorTable.ToHtml(), true, branch, bizObj);
		}

		IMessageAttachee GetMessageAttachee(BusinessObject bizObj)
		{
			IMessageAttachee result = bizObj as IMessageAttachee;

			switch (bizObj.TablePrefix)
			{
				case OrgHeaderSchema.Constants.Prefix:
					result = OrgHeaderWrapper.New((OrgHeader)bizObj);
					break;
			}

			return result;
		}

		protected virtual void SetFailStatus(BusinessObject bizObj)
		{
		}

		protected virtual string GetMessageTypeDescription()
		{
			var result = Factory.GetCachedValue<ApplicationIdentifierCodeList>().GetDescriptionFromCode(Message.EM_MessageType);

			if (string.IsNullOrEmpty(result))
			{
				result = Factory.GetCachedValue<ACEApplicationIdentifierCodeList>().GetDescriptionFromCode(Message.EM_MessageType);
			}

			if (string.IsNullOrEmpty(result))
			{
				result = "Unknown Message Type '" + Message.EM_MessageType + "'";
			}

			return result;
		}
	}
}
