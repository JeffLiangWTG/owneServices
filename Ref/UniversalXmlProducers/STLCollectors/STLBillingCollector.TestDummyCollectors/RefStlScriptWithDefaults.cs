using System;


namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors
{
	public abstract class RefStlScriptWithDefaults : IRefStlScript
	{
		public virtual string ActiveOn => RefActiveOn.All;

		public abstract string FeatureCode { get; }

		public abstract string FeatureName { get; }

		public virtual string DataGranularity => RefStlItemGrain.Transactional;

		public abstract string TransactionDateUtc { get; }

		public abstract string GuidReference { get; }

		public virtual string TransactionCount => "1";

		public abstract string FromClause { get; }

		public abstract string RoleName { get; }

		public abstract string ModuleName { get; }

		public abstract string FunctionName { get; }

		public abstract string CompanyCode { get; }

		public abstract string BranchCode { get; }

		public abstract string BillingReference1 { get; }
		public virtual string BillingReference2 => string.Empty;
		public virtual string BillingReference3 => string.Empty;
		public virtual string BillingReference4 => string.Empty;

		public abstract string WhereClause { get; }
		public virtual string CreatingUserCode => string.Empty;
		public virtual string AdditionalRefs => string.Empty;
		public virtual string PreparationScript => string.Empty;

		public virtual bool WithOptionRecompile => false;

		public virtual bool UsedInBilling => true;
		public virtual string MinCW1Version => string.Empty;
		public virtual string MaxCW1Version => string.Empty;

		public virtual string DateType => RefStlDateType.DateTime;

		public virtual DateTime CollectionStartDateUtc => DateTime.MinValue;
	}
}
