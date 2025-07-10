using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ModuleTextFilter))]
	public class RefCusTariffFilterStripTest : ModuleTextFilterTest
	{
		public void TestGetCurrentFilterControls()
		{
			using (var strip = new RefCusTariffFilterStrip())
			{
				var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "", null, null);
				var filterBO = new RefCusTariffFilterStripBusinessObject();
				var filterStrip = filterBO.FilterStrips.AddNew();
				strip.SetDataBinding(filterStrip, "");
				filterStrip.FilterDescription = Constants.RefCusTariffFilters.TariffType;
				var tariffTypeStrip = strip.Controls.Find("DataGroupingRelatedFilterControl", true)[0];
				AssertType<DataGroupingRelatedFilterControl>(tariffTypeStrip);
			}
		}
	}
}
