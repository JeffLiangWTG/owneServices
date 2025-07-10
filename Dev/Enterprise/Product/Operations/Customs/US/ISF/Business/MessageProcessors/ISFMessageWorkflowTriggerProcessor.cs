using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFMessageWorkflowTriggerProcessor : IProcessor
	{
		public ISFMessageWorkflowTriggerProcessor(CusISFHeader isfHeader)
		{
			this.header = Argument.NotNull(isfHeader, "CusISFHeader");
		}
		readonly CusISFHeader header;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (header.HasChanges)
			{
				try
				{
					header.Factory.Save();
				}
				catch (ZSaveException ex)
				{
					LogExceptionError(notifications, ex.Message);
				}
			}

			var hasMessageBeenGenerated = false;
			if (CanSendISFHeader())
			{
				header.LoadChildEditableObjects();
				header.RunPreSaveValidation();
				if (header.HasErrors)
				{
					notifications.AddError(ZString.Format("System cannot send ISF message because of following errors on Job:{0}, please fix all of them and try again.\r\n{1}", header.BF_JobReference, header.GetErrors().ToUniqueMessageListString())); // text in service tasks
					return;
				}

				try
				{
					var actionCode = header.ShouldSendAdd ? Messaging.Business.UpdateActionCode.Add : Messaging.Business.UpdateActionCode.Replace;
					var builder = new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(header, actionCode);
					builder.PopulateMessage();
					hasMessageBeenGenerated = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					LogExceptionError(notifications, ex.Message);
					return;
				}
			}
			else if (header.IsWaitingForResponse)
			{
				notifications.AddWarning(ZString.Format("System cannot send the ISF message for Job:{0}, please check whether it's waiting for response from customs.", header.BF_JobReference));
			}
			else if (header.BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete)
			{
				notifications.AddWarning(ZString.Format("System cannot send further ISF message for Job:{0}, as this Customs Reference '{1}' has been deleted from Customs system.", header.BF_JobReference, header.BF_CustomsReference));
			}
			else
			{
				notifications.AddWarning(ZString.Format("System cannot send ISF message for Job:{0}", header.BF_JobReference));
			}

			if (hasMessageBeenGenerated)
			{
				try
				{
					header.Factory.Save();
					notifications.Add(NotificationType.Information, ZString.Format("ISF message has been sent to customs for Job:{0}", header.BF_JobReference)); // text in service tasks
				}
				catch (ZSaveException ex)
				{
					LogExceptionError(notifications, ex.Message);
				}
			}
		}

		void LogExceptionError(INotifications notifications, ZString errorMessage)
		{
			notifications.AddError(ZString.Format("There was an exception while attempting sending ISF message for Job:{0}. See below for more information.\r\n{1}\r\n", header.BF_JobReference, errorMessage)); // text in service tasks
		}

		ZBool CanSendISFHeader()
		{
			return !header.IsWaitingForResponse && header.BF_CustomsStatus != MessageStatusList.Codes.ClearISFDelete;
		}
	}
}
