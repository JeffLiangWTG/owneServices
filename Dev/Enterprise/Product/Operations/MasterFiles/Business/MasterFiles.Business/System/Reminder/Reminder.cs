using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class Reminder : CargoWise.Services.Calendar.ReminderBase
	{
		public Reminder(string identifier, ZGuid parentPK, ZString slTable, DateTimeKind dateTimeKind, ZDateTime fromDate, ZDateTime toDate, string subject, string body, string htmlBody = "", ITimeZone timeZoneOverride = null, BusinessObjectFactory factory = null)
			: base(identifier, dateTimeKind, fromDate, toDate, subject, body, htmlBody, timeZoneOverride, factory)
		{
			if (slTable.IsEmpty)
			{
				throw new ArgumentException("May not be empty", nameof(slTable));
			}
			if (!parentPK.IsValid)
			{
				throw new ArgumentException("May not be empty or invalid", nameof(parentPK));
			}

			this.parentPK = parentPK;
			this.slTable = slTable;
		}

		public Reminder(string identifier, ZGuid parentPK, ZString slTable, DateTimeKind dateTimeKind, DateTime fromDate, TimeSpan duration, string subject, string body, string htmlBody = "", ITimeZone timeZoneOverride = null)
			: this(identifier, parentPK, slTable, dateTimeKind, fromDate, fromDate.Add(duration), subject, body, htmlBody, timeZoneOverride)
		{
		}

		protected readonly ZGuid parentPK;
		protected readonly ZString slTable;

		public override void CreateAppointment()
		{
			try
			{
				base.CreateAppointment();
			}
			catch (EmailSendFailedException ex) when (Globals.IsUserInteractive)
			{
				NotificationHandler.Instance.ReportError(
					Res.GetString("BA7D627F-67FC-4BA9-8659-945B795725F4", "Could not create appointment due to the following error:\r\n\r\n{0}", ex.Message),
					Res.GetString("07FAAA7A-D4FB-4D87-9BB8-83C262965653", "Error create appointment"));
			}
		}

		protected override uint GenerateSequenceNumber()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var filter = new ZQuery(StmALogSchema.SL_Parent, parentPK);
			filter.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, Constants.EventCode));
			filter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Constants.ReferencePrefix + ID);

			return (uint)factory.GetDatabaseCount(typeof(StmALog), filter);
		}

		protected override void OnAppointmentCreated()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			if (Factory == null || BaseStmALog.GetMaster(parentPK, slTable, newFactory) != null)
			{
				AddLogs(newFactory);
				newFactory.Save();
			}
			else if (BaseStmALog.GetMaster(parentPK, slTable, Factory) != null)
			{
				AddLogs(Factory);
			}
		}

		void AddLogs(BusinessObjectFactory factory)
		{
			var log = factory.New<StmALog>();
			using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
			{
				log.SL_Parent = parentPK;
				log.SL_SE_NKEvent = Constants.EventCode;
				log.SL_Reference = GenerateReference(Sequence);
				log.SL_Table = slTable;
			}
		}

		string GenerateReference(uint sequence)
		{
			return string.Format("{0}{1}|{2}", Constants.ReferencePrefix, ID, sequence);
		}

		internal class Constants
		{
			internal const string EventCode = "EMS";
			internal const string ReferencePrefix = "REMINDER:";
		}
	}
}
