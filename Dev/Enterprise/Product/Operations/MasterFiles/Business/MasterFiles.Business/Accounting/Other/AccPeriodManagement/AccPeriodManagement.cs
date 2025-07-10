using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Represents a single period.
	/// </summary>
	[SystemDefinedValues]
	public class AccPeriodManagement : AutoAccPeriodManagement
	{
		public new abstract class Schema : AutoAccPeriodManagement.Schema
		{
			public const string ReverseJournalPK = "ReverseJournalPK";
		}

		public AccPeriodManagement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude()]
		public override ZDateTime AM_EndDate
		{
			get
			{
				return base.AM_EndDate;
			}
			set
			{
				if (AM_EndDateOld == null)
				{
					AM_EndDateOld = base.AM_EndDate;
				}
				base.AM_EndDate = value;
			}
		}

		ZDateTime? AM_EndDateOld;

		public override ZDateTime AM_StartDate
		{
			get
			{
				return base.AM_StartDate;
			}
			set
			{
				if (AM_StartDateOld == null)
				{
					AM_StartDateOld = base.AM_StartDate;
				}
				base.AM_StartDate = value;
			}
		}

		ZDateTime? AM_StartDateOld;

		public override ZBool AM_IsGeneralLedgerClosed
		{
			get
			{
				return base.AM_IsGeneralLedgerClosed;
			}
			set
			{
				if (AM_IsGeneralLedgerClosedOld == null)
				{
					AM_IsGeneralLedgerClosedOld = base.AM_IsGeneralLedgerClosed;
				}
				base.AM_IsGeneralLedgerClosed = value;
			}
		}

		ZBool? AM_IsGeneralLedgerClosedOld;

		public override ZBool AM_IsSubLedgerClosed
		{
			get
			{
				return base.AM_IsSubLedgerClosed;
			}
			set
			{
				if (AM_IsSubLedgerClosedOld == null)
				{
					AM_IsSubLedgerClosedOld = base.AM_IsSubLedgerClosed;
				}
				base.AM_IsSubLedgerClosed = value;
			}
		}

		ZBool? AM_IsSubLedgerClosedOld;

		public override ZBool AM_IsSubledgerClosedForAdjustments
		{
			get
			{
				return base.AM_IsSubledgerClosedForAdjustments;
			}
			set
			{
				if (AM_IsSubledgerClosedForAdjustmentsOld == null)
				{
					AM_IsSubledgerClosedForAdjustmentsOld = base.AM_IsSubledgerClosedForAdjustments;
				}
				base.AM_IsSubledgerClosedForAdjustments = value;
			}
		}

		ZBool? AM_IsSubledgerClosedForAdjustmentsOld;

		public ZGuid ReverseJournalPK
		{
			get
			{
				return this.GetSystemDefinedValue<ZGuid>(Schema.ReverseJournalPK);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.ReverseJournalPK, AddOnColumnDataType.Codes.Guid, value);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (AM_StartDateOld != null && !AM_StartDateOld.Value.IsEmpty && AM_StartDateOld != AM_StartDate)
			{
				Logs.AddNew(Events.PeriodDateChanged, string.Format("Start Date changed from {0} to {1}", AM_StartDateOld.Value.Date.ToString(), AM_StartDate.Date.ToString()));
			}

			if (AM_EndDateOld != null && !AM_EndDateOld.Value.IsEmpty && AM_EndDateOld != AM_EndDate)
			{
				Logs.AddNew(Events.PeriodDateChanged, string.Format("End Date changed from {0} to {1}", AM_EndDateOld.Value.Date.ToString(), AM_EndDate.Date.ToString()));
			}

			if (AM_IsGeneralLedgerClosedOld != null && AM_IsGeneralLedgerClosedOld != AM_IsGeneralLedgerClosed)
			{
				if (AM_IsGeneralLedgerClosed)
				{
					Logs.AddNew(Events.PeriodClosed, "General Ledger Closed");
				}
				else
				{
					Logs.AddNew(Events.PeriodReopened, "General Ledger Reopened");
				}
			}

			if (AM_IsSubLedgerClosedOld != null && AM_IsSubLedgerClosedOld != AM_IsSubLedgerClosed)
			{
				if (AM_IsSubLedgerClosed)
				{
					Logs.AddNew(Events.PeriodClosed, "Sub Ledger Closed");
				}
				else
				{
					Logs.AddNew(Events.PeriodReopened, "Sub Ledger Reopened");
				}
			}

			if (AM_IsSubledgerClosedForAdjustmentsOld != null && AM_IsSubledgerClosedForAdjustmentsOld != AM_IsSubledgerClosedForAdjustments)
			{
				if (AM_IsSubledgerClosedForAdjustments)
				{
					Logs.AddNew(Events.PeriodClosed, "Sub Ledger for Adjustments Closed");
				}
				else
				{
					Logs.AddNew(Events.PeriodReopened, "General Ledger for Presentation Adjustments Reopened");
				}
			}

			AM_StartDateOld = null;
			AM_EndDateOld = null;
			AM_IsGeneralLedgerClosedOld = null;
			AM_IsSubLedgerClosedOld = null;
			AM_IsSubledgerClosedForAdjustmentsOld = null;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}
	}
}
