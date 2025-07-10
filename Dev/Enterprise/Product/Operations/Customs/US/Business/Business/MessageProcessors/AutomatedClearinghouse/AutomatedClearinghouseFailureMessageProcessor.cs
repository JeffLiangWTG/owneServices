using CargoWise.Application;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	[TopLevel(typeof(ENSEB))]
	public abstract class AutomatedClearinghouseFailureMessageProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public sealed override void Process()
		{
			HtmlTableCreator errorTable = new HtmlTableCreator(new string[] { "Error Description" });

			foreach (MessageBlock block in messageBlocks)
			{
				IStatusesAndErrors errorBlock = block as IStatusesAndErrors;
				if (errorBlock != null)
				{
					errorTable.WriteRow(errorBlock.NarrativeMessage);
				}
			}

			string url = "";
			string jobNumber = "";
			CusStatementHeader statement = OriginalMessageLinker.Link<CusStatementHeader>(Message);
			if (statement != null)
			{
				statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(statement);
				jobNumber = statement.B2_StatementNumber;
			}

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, MessageTypeDescription, errorTable.ToHtml(), true, branch, statement);
		}

		protected abstract string MessageTypeDescription { get; }
	}
}
