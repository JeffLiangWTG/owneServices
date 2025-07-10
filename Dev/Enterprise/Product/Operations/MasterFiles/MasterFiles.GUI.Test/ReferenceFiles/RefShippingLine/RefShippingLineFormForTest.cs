using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefShippingLineFormForTest : RefShippingLineForm
	{
		internal RefShippingLineFormForTest(RefShippingLine shippingLine) : base(shippingLine)
		{
		}

		internal void RSL_IsActiveCheckBox_ClickForTest() => RSL_IsActiveCheckBox_Click(this, null);
	}
}
