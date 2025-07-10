using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class CusPermitFilterControl : ZFilterStripControl
	{
		public CusPermitFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CusPermitModuleStrip();
		}

		protected override void AddAlwaysVisibleFilterStrips()
		{
			base.AddAlwaysVisibleFilterStrips();
			var collection = GridCollection as Business.PermitFindBoxCollection;
			if (collection != null)
			{
				if (FilterBusinessObject.ModuleFilters[CusPermitFilterStripBusinessObject.Schema.PermitRule] != null)
				{
					foreach (var rule in collection.GetFilterRules())
					{
						var strip = FilterBusinessObject.FilterStrips.AddNew();
						strip.FilterDescription = CusPermitFilterStripBusinessObject.Schema.PermitRule;

						var permitRuleFilter = strip.CurrentModuleFilter as PermitRuleModuleFilter;
						if (permitRuleFilter != null)
						{
							permitRuleFilter.Property1 = rule.Key.Code;
							permitRuleFilter.Property2 = rule.Value;
							permitRuleFilter.Visibility = FilterVisibility.AlwaysVisible;

							AddFilterStrip(strip);
						}
					}
				}
			}
		}
	}
}
