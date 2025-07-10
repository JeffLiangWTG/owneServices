using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(CountryStatesGlbHolidayForm))]
	public class CountryStatesGlbHolidayFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			dataBoundBizO = new CountryStatesGlbHolidayBizo(Factory);
			var form = new CountryStatesGlbHolidayForm(dataBoundBizO);
			form.ControllerID = ControllerIDs.CountryStatesGlbHoliday;

			return form;
		}

		public void TestResize()
		{
			using (var form = new CountryStatesGlbHolidayForm(dataBoundBizO))
			{
				AssertEquals(System.Windows.Forms.AutoSizeMode.GrowAndShrink, form.AutoSizeMode);
				AssertEquals(0, form.MaximumSize.Width);
				AssertEquals(0, form.MaximumSize.Height);
			}
		}

		CountryStatesGlbHolidayBizo dataBoundBizO;

		#endregion
	}
}
