using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	class EntryNoOnReconciliationFilter : ModuleTextFilter
	{
		public EntryNoOnReconciliationFilter(ZString description)
			: base(description, DummyQuery)
		{
		}

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString EntryFilerCode
		{
			get => entryFilerCode;
			set
			{
				CheckMaximumLength(EntryFilerCodeInfo, value);
				entryFilerCode = value;
				EntryFilerCodeInfo.RefreshBinding();
				InvalidateCachedQuery();
			}
		}
		ZString entryFilerCode;

		public ZPropertyInfo EntryFilerCodeInfo => GetZPropertyInfo(nameof(EntryFilerCode));

		protected override void ClearCore()
		{
			base.ClearCore();
			EntryFilerCode = ZString.Empty;
		}

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();

			if (!Property.IsEmpty || !EntryFilerCode.IsEmpty)
			{
				query.AddToFilter(GetEntryNumberQuery());
			}
			return query;
		}

		ZDBOnlyQuery GetEntryNumberQuery()
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Recon);

			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry);

			if (EntryFilerCode.IsEmpty)
			{
				entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.Contains, Property);
			}
			else
			{
				entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, EntryFilerCode + Property);
			}
			result.AddSubQuery(entryQuery, JoinCondition.And);

			return result;
		}

		static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery();
	}
}
