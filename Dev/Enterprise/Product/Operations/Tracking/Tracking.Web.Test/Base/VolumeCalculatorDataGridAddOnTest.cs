using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class VolumeCalculatorDataGridAddOnTest : ZDataGridAddOnTest
	{
		#region Test Cases

		public void TestWebServiceMethods()
		{
			AssertNotNull(Control);
			AssertNotNull(Control.WebServiceMethods);
			AssertEquals(1, Control.WebServiceMethods.Count);
			AssertEquals(typeof(VolumeCalculatorWebServiceMethod), Control.WebServiceMethods[0].GetType());
		}

		#endregion

		#region Implementation

		new VolumeCalculatorDataGridAddOn Control
		{
			get { return (VolumeCalculatorDataGridAddOn)base.Control; }
		}

		protected override Control GetNewControl()
		{
			return new VolumeCalculatorDataGridAddOn();
		}

		#endregion
	}
}
