using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Customs.Module
{
	public class GuaranteesModuleTextFilterValidation : ModuleTextFilterValidation
	{
		public GuaranteesModuleTextFilterValidation(GuaranteesModuleTextFilter parent, FilterStripBusinessObject filterBusinessObject)
			: base(parent)
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		protected override void CheckProperty()
		{
			base.CheckProperty();
			var hasValidDateFilter = false;

			foreach (var filterStrip in filterBusinessObject.FilterStrips)
			{
				var strip = filterStrip as FilterStrip;
				if (strip.IsDateFilter)
				{
					var dateFilter = strip.CurrentModuleFilter as ModuleDateFilter;
					if (dateFilter.IsDateRangeWithin3Months)
					{
						hasValidDateFilter = true;
					}
				}
			}

			if (!hasValidDateFilter)
			{
				Parent.PropertyInfo.AddError(ResString.GetMultilingualString("ADAD0CD3-0AB1-4D8A-860D-53F3C95ED352", "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Transaction Date based filter with a range of 3 months or less."));
			}
		}
	}
}
