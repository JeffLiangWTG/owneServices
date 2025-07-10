using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	sealed class ContainerUserControlTest : Customs.GUI.Testing.BaseCustomsCusContainersWithTrackingUserControlTest
	{
		public void TestSecondSealNumber()
		{
			using (ContainerUserControl userControl = new ContainerUserControl())
			{
				var columnStyle = userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainerSchema.CO_SecondSeal.Name);
				AssertNotNull("There is a column bound to CO_SecondSeal", columnStyle);
				AssertEquals(CharacterCasing.Upper, columnStyle.CharacterCasing);
			}
		}

		public void TestDeclarationIsSetToContainerTrackingUserControl()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ContainerUserControl userControl = new ContainerUserControl())
			{
				userControl.JobDeclaration = declaration;
				AssertEquals(declaration, ((InBondContainerTrackingUserControl)userControl.containersUserControl1).Declaration);
			}
		}
	}
}
