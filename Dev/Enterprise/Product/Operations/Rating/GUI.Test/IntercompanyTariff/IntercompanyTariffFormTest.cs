using System.Windows.Forms;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class IntercompanyTariffFormTest : RatingTestCase
	{
		public void TestFiltersShouldBeEnabledInViewMode()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new IntercompanyTariffForm(intercompanyTariff))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				var filterControl = (IReadOnlyToggleControl)form.rateEntryFilterStripControl;
				AssertEquals(false, filterControl.ReadOnly);
			}
		}
	}

	#region Form Basher

	[TestedType(typeof(IntercompanyTariffForm))]
	public class IntercompanyTariffFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Helper.IsMarkAsNeedingValidationSuspended = true;
			var intercompanyTariff = Helper.NewFullyPopulatedIntercompanyTariff();
			Helper.IsMarkAsNeedingValidationSuspended = false;

			using (var dis = intercompanyTariff.SuspendMarkingAsNeedingValidation())
			{
				Factory.Save();
			}

			return new IntercompanyTariffForm(intercompanyTariff);
		}

		#region Helper

		TestHelper Helper => fHelper ?? (fHelper = new TestHelper(Factory));
		TestHelper fHelper;

		#endregion
	}

	#endregion
}
