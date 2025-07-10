using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class ConsignorRelationshipsUserControlTest : TestCaseWithFactory
	{
		public void TestBuyingCommissionPercentageNumericUpDown()
		{
			using (var form = new ZForm())
			using (var control = new ConsignorRelationshipsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertNotNull("Should be able to find BuyingCommissionPercentageNumericUpDown.", control.FindSingle<ZNumericUpDown>("BuyingCommissionPercentageNumericUpDown"));
			}
		}
	}
}
