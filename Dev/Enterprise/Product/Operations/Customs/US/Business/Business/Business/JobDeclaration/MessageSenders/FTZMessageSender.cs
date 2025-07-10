using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class FTZMessageSender : MessageSender
	{
		public FTZMessageSender(FTZMessageSendingObject sender)
			: base(sender.Declaration)
		{
			this.actionCode = sender.ActionCode;
			this.sender = sender;
		}

		readonly UpdateActionCode actionCode;
		readonly FTZMessageSendingObject sender;

		public delegate bool PrepareEventHandler();

		public event PrepareEventHandler OnPrepare;

		protected override bool Prepare()
		{
			UpdateFTZYear();
			if (!Job.FTZControlNumberIsAutoAllocated && (Job.FTZZoneID.IsEmpty || Job.FTZControlNumber.IsEmpty))
			{
				Job.MessageInitiator.NotifyUserOfAnInvalidOperation("System cannot send a FTZ " + actionCode + " message as ZoneID or Control number is invalid.");
				return false;
			}
			else if (Job.FTZControlNumberIsAutoAllocated && Job.FTZControlNumber.IsEmpty && !Job.LockImportEntryNumberAllocationMutex)
			{
				var message = Job.GetFTZAdmissionNumberAllocationMutexInfo() + " is in the process of allocating FTZ Control Number for this job; system cannot send the data as it will result in a different FTZ Control Number being allocated.\r\nPlease retry sending when the other user has finished.";
				Job.MessageInitiator.WarnUserAboutSomething(message, "Send Messages");
				return false;
			}
			bool okToSend = true;
			if (Job.FTZEntry == null || Job.MergeManager.RequiresMerge)
			{
				okToSend = Job.DoMerge();
			}

			if (!okToSend)
			{
				return false;
			}

			string notificationText = "";
			bool hasBeenLodged;

			if (MessageManager.CanSendThisMessage(actionCode, out notificationText, out hasBeenLodged))
			{
				if (!hasBeenLodged)
				{
					okToSend = IsCreditCheckOKToSend();
					if (!okToSend)
					{
						return false;
					}
				}

				if (MessageManager.IsWaitingForResponse)
				{
					okToSend = Job.MessageInitiator.YesNoQuery(AwaitingCustomsResponseWarningMessage, "Warning");
				}

				if (okToSend && OnPrepare != null)
				{
					okToSend = OnPrepare();
				}

				if (okToSend && Job.HasChanges)
				{
					try
					{
						Job.Factory.Save();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(e);
						okToSend = false;
					}
				}

				return okToSend;
			}
			else
			{
				Job.MessageInitiator.NotifyUserOfAnInvalidOperation("System cannot send a FTZ " + actionCode + " message as " + notificationText);
				return false;
			}
		}
		const string AwaitingCustomsResponseWarningMessage = "This Job is waiting for Customs response.\r\nAre you sure you you want to resend to Customs?";

		void UpdateFTZYear()
		{
			if (!Job.FTZAdmissionHasBeenLodgedAtCustoms)
			{
				if (Job.FTZYear != ZDate.Today.ToString("yy", System.Globalization.CultureInfo.InvariantCulture))
				{
					Job.FTZYear = ZDate.Today.ToString("yy", System.Globalization.CultureInfo.InvariantCulture);
				}
			}
		}

		protected override bool GenerateMessage()
		{
			MessageManager.PopulateMessage(actionCode);
			if (actionCode == UpdateActionCode.Add)
			{
				Job.LogCustomsCommencedIfNeeded();

				var ftzEntry = Job.ActiveEntryHeaders.FTZEntry;
				if (ftzEntry != null)
				{
					ftzEntry.PopulateEntrySubmittedDateIfRequired();
				}
			}
			return true;
		}

		protected override string SuccessfulSendNotification
		{
			get { return "FTZ " + actionCode + " Message sent"; }
		}

		protected override bool ShouldValidateJob
		{
			get { return true; }
		}

		FTZMessageManager MessageManager
		{
			get { return ftzMessageManager ?? (ftzMessageManager = new FTZMessageManager(sender)); }
		}
		FTZMessageManager ftzMessageManager;
	}
}
