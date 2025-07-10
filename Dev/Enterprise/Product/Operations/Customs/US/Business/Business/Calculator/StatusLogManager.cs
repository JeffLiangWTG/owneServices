using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class StatusLogManager
	{
		public StatusLogManager(Logs logs, GlbBranch branch)
		{
			if (logs == null)
			{
				throw new ArgumentNullException(nameof(logs));
			}

			this.logs = logs;
			this.branch = branch;
		}
		readonly GlbBranch branch;
		readonly Logs logs;

		#region Add a log

		public StmALog AddALogIfNecessary(ZString oldStatus, ZString newStatus, IStatusList list)
		{
			StmALog result = null;
			if (list != null)
			{
				if (IsStatusChangingFromNotClearToClear(oldStatus, newStatus, list))
				{
					result = AddAClearLogIfNecessary(oldStatus, newStatus, list);
				}
				else
				{
					result = CustomsEntryStatusEvent.AddNew(newStatus, DateTimeParser.GetFromJobBranchCurrentTime(branch));
				}
			}
			return result;
		}

		public StmALog AddAClearLogIfNecessary(ZString oldStatus, ZString newStatus, IStatusList list)
		{
			StmALog result = null;

			if (IsStatusChangingFromNotClearToClear(oldStatus, newStatus, list) || IsChangingFromNotWithdrawnToWithdrawn(oldStatus, newStatus, list))
			{
				if (list.IsArrivalExportBTATransmissionStatus(newStatus))
				{
					result = ArrivalExportBTATransmissionEvent.AddNew(newStatus, DateTimeParser.GetFromJobBranchCurrentTime(branch));
				}
				else if (list.IsWithdrawnStatus(newStatus))
				{
					CustomsEntryStatusEvent.CancelAll();
					ArrivalExportBTATransmissionEvent.CancelAll();

					result = WithdrawnStatusEvent.AddNew(newStatus, DateTimeParser.GetFromJobBranchCurrentTime(branch));
				}
				else
				{
					WithdrawnStatusEvent.CancelAll();

					result = CustomsEntryStatusEvent.AddNew(newStatus, DateTimeParser.GetFromJobBranchCurrentTime(branch));
				}

				foreach (string status in list.AcceptedStatusToCancelRejectStatusInterested)
				{
					if (newStatus == status)
					{
						DeclarationRejectedEvent.CancelAll();
						break;
					}
				}
			}

			return result;
		}

		bool IsChangingFromNotWithdrawnToWithdrawn(ZString oldStatus, ZString newStatus, IStatusList list)
		{
			return list != null && !list.IsWithdrawnStatus(oldStatus) && list.IsWithdrawnStatus(newStatus);
		}

		public StmALog AddARejectLogIfNecessary(ZString oldStatus, ZString newStatus, IStatusList list)
		{
			if (oldStatus != newStatus)
			{
				foreach (string status in list.RejectStatusInterested)
				{
					if (status == newStatus)
					{
						return DeclarationRejectedEvent.AddNew(newStatus, DateTimeParser.GetFromJobBranchCurrentTime(branch));
					}
				}
			}
			return null;
		}

		#endregion

		#region Has a certain log

		public bool HasARejectLog(string[] rejectStatusList)
		{
			foreach (string status in rejectStatusList)
			{
				if (DeclarationRejectedEvent.DescriptionExists(status))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasAClearLog(IEnumerable<string> statusList)
		{
			foreach (string status in statusList)
			{
				if (CustomsEntryStatusEvent.DescriptionExists(status))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasAClearLog(ImportMessageStatusList.MessageType messageType, IStatusList list)
		{
			return HasAClearLog(list.GetFirstClearStatusFor(messageType));
		}

		public bool HasAWithdrawnLog
		{
			get { return WithdrawnStatusEvent.Count > 0; }
		}

		public
#if DEBUG
 virtual
#endif
 bool HasAClearInBondDeparture
		{
			get { return HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal, ImportMessageStatusList.Codes.ClearDeparturePartialOriginal }); }
		}

		public
#if DEBUG
 virtual
#endif
 bool HasAClearInBondArrival
		{
			get { return HasAClearArrivalExportBTATransmissionLog(new string[] { ImportMessageStatusList.Codes.ClearArrival }); }
		}

		public
#if DEBUG
 virtual
#endif
 bool HasAClearInBondExportation
		{
			get { return HasAClearArrivalExportBTATransmissionLog(new string[] { ImportMessageStatusList.Codes.ClearExportation }); }
		}

		public bool HasAClearArrivalExportBTATransmissionLog(string[] statusList)
		{
			foreach (string status in statusList)
			{
				if (ArrivalExportBTATransmissionEvent.DescriptionExists(status))
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region LogsForNominatedEvent

		LogsForNominatedEvent CustomsEntryStatusEvent
		{
			get
			{
				if (fClearStatusEvent == null)
				{
					fClearStatusEvent = new LogsForNominatedEvent(logs, Events.CustomsEntryStatus, true);
				}
				return fClearStatusEvent;
			}
		}
		LogsForNominatedEvent fClearStatusEvent;

		LogsForNominatedEvent WithdrawnStatusEvent
		{
			get
			{
				if (fWithdrawanStatusEvent == null)
				{
					fWithdrawanStatusEvent = new LogsForNominatedEvent(logs, Events.DeclarationCancellationApproved, true);
				}
				return fWithdrawanStatusEvent;
			}
		}
		LogsForNominatedEvent fWithdrawanStatusEvent;

		LogsForNominatedEvent ArrivalExportBTATransmissionEvent
		{
			get
			{
				if (fArrivalExportBTATransmissionEvent == null)
				{
					fArrivalExportBTATransmissionEvent = new LogsForNominatedEvent(logs, Events.UnderbondCustomsApproval, true);
				}
				return fArrivalExportBTATransmissionEvent;
			}
		}
		LogsForNominatedEvent fArrivalExportBTATransmissionEvent;

		LogsForNominatedEvent DeclarationRejectedEvent
		{
			get
			{
				if (fDeclarationRejectedEvent == null)
				{
					fDeclarationRejectedEvent = new LogsForNominatedEvent(logs, Events.DeclarationRejected, true);
				}
				return fDeclarationRejectedEvent;
			}
		}
		LogsForNominatedEvent fDeclarationRejectedEvent;

		#endregion

		bool IsStatusChangingFromNotClearToClear(ZString oldStatus, ZString newStatus, IStatusList list)
		{
			return list != null && !list.IsStatusClear(oldStatus) && list.IsStatusClear(newStatus);
		}
	}
}
