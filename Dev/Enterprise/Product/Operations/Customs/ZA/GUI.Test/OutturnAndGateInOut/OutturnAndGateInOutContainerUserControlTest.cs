using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing.OutturnAndGateInOut
{
	[TestedType(typeof(OutturnAndGateInOutContainerUserControl))]
	sealed class OutturnAndGateInOutContainerUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumnsCaption()
		{
			var expectedCaptions = new[]
			{
				"Container #",
				"Empty/Full",
				"Cont. Type",
				"Packed/Unpacked DateTime",
				"Seal 1 Number",
				"Seal 1 Party Type",
				"Gate In/Out DateTime",
			};

			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutContainerUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			var containerGrid = userControl.FindSingle<ZGrid>("zGridContainer");
			AssertContainsExactElementsInExactOrder("Columns captions", expectedCaptions, containerGrid.Columns.Select(c => containerGrid.GetColumnCaption(c.ColumnName)));
		}

		public void TestDetailControlsCaption()
		{
			var expectedCaptions = new[]
			{
				"Container Number",
				"Empty/Full Indicator",
				"Container Type",
				"Seal 1 Number",
				"Seal 1 Party Type",
				"Packed/Unpacked DateTime",
				"Gate In/Out DateTime",
			};

			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutContainerUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			var groupBox = userControl.FindSingle<ZGroupBox>("zGroupBoxContainerDetail");
			AssertContainsExactElementsInExactOrder("Controls", expectedCaptions, groupBox.Controls.Cast<Control>().OrderBy(c => c.TabIndex).Select(c => c.GetExtension<ILabelCaptionRenderer>().Caption));
		}
	}
}
