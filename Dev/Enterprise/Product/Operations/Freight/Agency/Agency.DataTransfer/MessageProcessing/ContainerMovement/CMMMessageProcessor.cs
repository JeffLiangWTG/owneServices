using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public abstract class CMMMessageProcessor : MessageProcessor
	{
		protected CMMMessageProcessor(LoggingInformation logger)
			: base(logger, EDIMessage.ApplicationCodes.ContainerManagement, Res.GetString("21170e20-225c-42eb-a947-53940c70d4d7", "Container Management Message Processor"))
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			string result = EDIMessage.Status.Failed;
			try
			{
				var messageAdapter = GetCMMProcessingAdapter(message);
				messageAdapter.Load();
				message.EM_Status = EDIMessage.Status.Recognised;
				var emailBuilder = new CMMEmailGenerator();
				var processor = new ContainerMovementMessageProcessor(messageAdapter);
				processor.ProcessMessage(emailBuilder);
				EmailDef email;
				if ((email = emailBuilder.ToAckEmail()) != null)
				{
					processor.AttachMessageToEmail(email, "message.edi");
					SendAcknowledgementReport(null, email);
				}

				if ((email = emailBuilder.ToWarningEmail()) != null)
				{
					processor.AttachMessageToEmail(email, "message.edi");
					SendImpedimentReport(null, email);
				}

				result = message.EM_Status;
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				var errorEmail = ContainerMovementMessageProcessor.GenerateErrorEmail(GetCMMProcessingAdapter(message), ex.Message);
				SendErrorReport(null, errorEmail);
			}

			return result;
		}

		protected abstract ICMMProcessingAdapter GetCMMProcessingAdapter(EDIMessage message);

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return AgencyRegistry.Instance.CMMAcknowledgementEmailGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Constants.EmailTo.NominatedGroup; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return AgencyRegistry.Instance.CMMDiscrepanciesEmailGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Constants.EmailTo.NominatedGroup; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return AgencyRegistry.Instance.CMMErrorEmailGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return Constants.EmailTo.NominatedGroup; }
		}
	}
}
