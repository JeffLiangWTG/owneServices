using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	[TestedType(typeof(ZForm))]
	public class MAFeBACCaContainerUserControlTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			Factory.Save();
			ZForm form = new ZForm(declaration.CusContainers);
			form.CaptionRenderingEnabled = true;
			form.Size = new System.Drawing.Size(1012, 551);
			MAFeBACCaContainerUserControl control = new MAFeBACCaContainerUserControl(declaration);
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);
			return form;
		}

		public void TestControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			using (var containerControl = new MAFeBACCaContainerUserControl(declaration))
			{
				AssertEquals("ContainerTypeDropEdit", true, (containerControl.FindSingle<ZDropEdit>("ContainerTypeDropEdit")).Visible);
				AssertEquals("ContainerTypeDropEdit - Caption when legacy declaration", "MPI Container Type", (containerControl.FindSingle<ZDropEdit>("ContainerTypeDropEdit")).CaptionResourceString.Caption);
				AssertEquals("MPINumberTextBox", false, (containerControl.FindSingle<ZTextBox>("MPINumberTextBox")).Visible);
				AssertEquals("ContainerPackingAddressGroupBox", false, (containerControl.FindSingle<ZGroupBox>("ContainerPackingAddressGroupBox")).Visible);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (var containerControl = new MAFeBACCaContainerUserControl(declaration))
			{
				AssertEquals("ContainerTypeDropEdit", true, (containerControl.FindSingle<ZDropEdit>("ContainerTypeDropEdit")).Visible);
				AssertEquals("ContainerTypeDropEdit - Caption when TSW Declaration", "TSW Container Type", (containerControl.FindSingle<ZDropEdit>("ContainerTypeDropEdit")).CaptionResourceString.Caption);
				AssertEquals("MPINumberTextBox should be visible for TSW Declaration", true, (containerControl.FindSingle<ZTextBox>("MPINumberTextBox")).Visible);
				AssertEquals("ContainerPackingAddressGroupBox should be visible for TSW Declaration", true, (containerControl.FindSingle<ZGroupBox>("ContainerPackingAddressGroupBox")).Visible);
			}
		}
	}
}
