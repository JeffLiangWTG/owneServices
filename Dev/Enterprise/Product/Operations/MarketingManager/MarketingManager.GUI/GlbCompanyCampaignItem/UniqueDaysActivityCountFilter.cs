using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class UniqueDaysActivityCountFilter : CampaignContactNumberFilter
	{
		public UniqueDaysActivityCountFilter(ZString description, SchemaNumericColumn filterColumn)
			: base(description, filterColumn)
		{
			DefaultProperty = 1;
		}

		protected UniqueDaysActivityCountFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public UniqueDaysActivityCountFilter(ZString description, GetDecimalQuery queryDelegate)
			: base(description, queryDelegate)
		{
			DefaultProperty = 1;
		}

		public override ZString ComparisonOperator
		{
			get
			{
				return base.ComparisonOperator;
			}
			set
			{
				base.ComparisonOperator = value;
				if (Property == 0 && !base.ComparisonOperator.EqualsIgnoringCase(ComparisonConstants.Exact))
				{
					Property = DefaultProperty;
				}
				Validation.ValidateAll();
			}
		}

		#region Validation

		public new UniqueDaysActivityCountFilterValidation Validation
		{
			get { return (UniqueDaysActivityCountFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new UniqueDaysActivityCountFilterValidation(this);
		}

		#endregion
	}
}
