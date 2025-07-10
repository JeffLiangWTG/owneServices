using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class EntryCensusWarningOverride : AutoEntryCensusWarningOverride, ICensusWarningOverride
	{
		public EntryCensusWarningOverride(CusEntryHeader entry, EntryCensusWarningOverrideCollection coll)
			: base(entry.Factory)
		{
			this.Entry = entry;
			this.coll = coll;
		}

		internal readonly CusEntryHeader Entry;
		internal readonly EntryCensusWarningOverrideCollection coll;

		public CusEntryLine EntryLine
		{
			get { return Factory.Load<CusEntryLine>(EntryLinePK); }
		}

		[List(nameof(Lookups) + "." + nameof(EntryCensusWarningOverrideLookups.EntryLines))]
		public override ZGuid EntryLinePK
		{
			get { return base.EntryLinePK; }
			set { base.EntryLinePK = value; }
		}

		[List(nameof(Lookups) + "." + nameof(EntryCensusWarningOverrideLookups.ConditionCodeList))]
		public override ZString ConditionCode
		{
			get { return base.ConditionCode; }
			set { base.ConditionCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(EntryCensusWarningOverrideLookups.CensusOverrideList))]
		public override ZString OverrideCode
		{
			get { return base.OverrideCode; }
			set { base.OverrideCode = value; }
		}

		public EntryCensusWarningOverrideLookups Lookups
		{
			get { return lookups ?? (lookups = new EntryCensusWarningOverrideLookups(this)); }
		}
		EntryCensusWarningOverrideLookups lookups;

		#region ICensusWarningOverride Members

		ZString ICensusWarningOverride.ConditionCode
		{
			get { return ConditionCode; }
		}

		ZString ICensusWarningOverride.OverrideCode
		{
			get { return OverrideCode; }
		}

		#endregion
	}
}
