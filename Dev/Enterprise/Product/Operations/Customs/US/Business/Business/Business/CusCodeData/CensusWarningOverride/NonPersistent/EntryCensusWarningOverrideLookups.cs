using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class EntryCensusWarningOverrideLookups : ZLookups
	{
		public EntryCensusWarningOverrideLookups(EntryCensusWarningOverride parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ConditionCodeList
		{
			get { return Factory.GetCachedValue<CensusWarningCodeList>(); }
		}

		public CodeDescriptionPairList CensusOverrideList
		{
			get
			{
				return Factory.GetCachedValue("CensusOverrideCodeList for " + Parent.ConditionCode, delegate
				{
					return CensusOverrideCodeList.GetListFor(Parent.ConditionCode);
				}
				);
			}
		}

		public ActiveBusinessObjectCollection<CusEntryLine> EntryLines
		{
			get
			{
				if (activeCusEntryLines == null)
				{
					activeCusEntryLines = new ActiveBusinessObjectCollection<CusEntryLine>(Factory, new ZQuery(CusEntryLineSchema.PK, Parent.Entry.MergedLines.OfType<CusEntryLine>().Where(x => !x.IsSupTariffLine()).Select(x => x.PK)));
				}
				return activeCusEntryLines;
			}
		}
		ActiveBusinessObjectCollection<CusEntryLine> activeCusEntryLines;

		#region Implementation

		protected new EntryCensusWarningOverride Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (EntryCensusWarningOverride)base.Parent; }
		}

		#endregion
	}
}
