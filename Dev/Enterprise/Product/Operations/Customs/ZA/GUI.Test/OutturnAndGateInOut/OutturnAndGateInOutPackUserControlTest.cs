using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing.OutturnAndGateInOut
{
	[TestedType(typeof(OutturnAndGateInOutPackUserControl))]
	sealed class OutturnAndGateInOutPackUserControlTest : TestCaseWithFactory
	{
		public void TestCargoType()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthCargoType");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_CargoType");
			AssertEquals("Caption", "Cargo Type", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestContainer()
		{
			var control = userControl.FindSingle<ZGuidDropEditWithFixedWidth>("zGuidDropEditWithFixedWidthContainerPK");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.ContainerPK");
			AssertEquals("Caption", "Container", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestGoodsDescription()
		{
			var control = userControl.FindSingle<ZTextBox>("zTextBoxAPA_GoodsDescription");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_GoodsDescription");
			AssertEquals("Caption", "Goods Description", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestMarksAndNumbers()
		{
			var control = userControl.FindSingle<ZTextBox>("zTextBoxMarksAndNumbers");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_MarksAndNumbers");
			AssertEquals("Caption", "Marks and Numbers", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestContentsShouldBe()
		{
			var control = userControl.FindSingle<ZTextBox>("zTextBoxContShouldBe");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.ContShouldBe");
			AssertEquals("Caption", "Contents Should Be", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestWeightManifested()
		{
			var control = userControl.FindSingle<ZCalcEdit>("zCalcEditWeight");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_Weight");
			AssertEquals("Caption", "Weight Manifested", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestWeightManifestedUQ()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthWeightUQ");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();
			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_WeightUQ");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestVolumeManifested()
		{
			var control = userControl.FindSingle<ZCalcEdit>("zCalcEditVolume");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_Volume");
			AssertEquals("Caption", "Volume Manifested", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestVolumeManifestedUQ()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthVolumeUQ");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();
			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_VolumeUQ");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestNumberOfPacks()
		{
			var control = userControl.FindSingle<ZCalcEdit>("zCalcEditPackQty");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_PackQty");
			AssertEquals("Caption", "No. Of Packs", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestNumberOfPacksUQ()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthPackUQ");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.APA_PackUQ");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestSealIntact()
		{
			var control = userControl.FindSingle<ZCheckBox>("zCheckBoxSealIntactIndicator");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_SealIntactIndicator");
			AssertEquals("Caption", "Seal Intact", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestContentsActually()
		{
			var control = userControl.FindSingle<ZTextBox>("zTextBoxGoodsDescription");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_GoodsDescription");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestWeightActually()
		{
			var control = userControl.FindSingle<ZCalcEdit>("zCalcEditWeightOutturned");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_WeightOutturned");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestWeightActuallyUQ()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthWeightOutturnedUQ");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_WeightOutturnedUQ");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestVolumeActually()
		{
			var control = userControl.FindSingle<ZCalcEdit>("zCalcEditVolumeOutturned");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_VolumeOutturned");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestVolumeActuallyUQ()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthVolumeOutturnedUQ");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_VolumeOutturnedUQ");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestNumberOfPacksActually()
		{
			var control = userControl.FindSingle<ZCalcEdit>("zCalcEditPackagesOutturned");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_PackagesOutturned");
			AssertEquals("Caption visible", false, labelCaptionRenderer.Visible);
		}

		public void TestPackCondition()
		{
			var control = userControl.FindSingle<ZDropEdit>("zDropEditPackageCondition");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.C5_PackageCondition");
			AssertEquals("Caption", "Pack Condition", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestPackConditionDescription()
		{
			var control = userControl.FindSingle<ZTextBox>("zTextBoxPackCondDesc");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.PackCondDesc");
			AssertEquals("Caption", "Pack Condition Description", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestExcessShortIndicator()
		{
			var control = userControl.FindSingle<ZDropEditWithFixedWidth>("zDropEditWithFixedWidthExcessShortInd");
			var labelCaptionRenderer = control.GetExtension<ILabelCaptionRenderer>();

			AssertEquals("Binding", control.BindTo, "Bills.Packs.Outturn.ExcessShortInd");
			AssertEquals("Caption", "Excess/Short Indicator", labelCaptionRenderer.Caption);
			AssertEquals("Caption visible", true, labelCaptionRenderer.Visible);
		}

		public void TestManifestedShouldBeLabel()
		{
			var label = userControl.FindSingle<ZLabel>("zLabelManifestedShouldBe");
			AssertEquals("Caption", "Manifested should be", label.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		public void TestActuallyFoundToBeLabel()
		{
			var label = userControl.FindSingle<ZLabel>("zLabelActuallyFoundToBe");
			AssertEquals("Caption", "Actually found to be", label.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		public void TestDiscrepancyReportLabel()
		{
			var label = userControl.FindSingle<ZLabel>("zLabelDiscrepancyReport");
			AssertEquals("Caption", "Discrepancy Report", label.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		public void TestTabIndex()
		{
			var expectedControls = new string[]
			{
				"zDropEditWithFixedWidthCargoType",
				"zGuidDropEditWithFixedWidthContainerPK",
				"zCheckBoxSealIntactIndicator",
				"zTextBoxAPA_GoodsDescription",
				"zTextBoxMarksAndNumbers",
				"zTextBoxContShouldBe",
				"zTextBoxGoodsDescription",
				"zCalcEditWeight",
				"zDropEditWithFixedWidthWeightUQ",
				"zCalcEditWeightOutturned",
				"zDropEditWithFixedWidthWeightOutturnedUQ",
				"zCalcEditVolume",
				"zDropEditWithFixedWidthVolumeUQ",
				"zCalcEditVolumeOutturned",
				"zDropEditWithFixedWidthVolumeOutturnedUQ",
				"zCalcEditPackQty",
				"zDropEditWithFixedWidthPackUQ",
				"zCalcEditPackagesOutturned",
				"zDropEditPackageCondition",
				"zTextBoxPackCondDesc",
				"zDropEditWithFixedWidthExcessShortInd",
			};

			var controls = expectedControls
				.Select(c => userControl.FindSingle<Control>(c))
				.OrderBy(c => c.TabIndex)
				.Select(c => c.Name)
				.ToArray();

			AssertContainsExactElementsInExactOrder("Controls", expectedControls, controls);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new OutturnAndGateInOutPackUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		OutturnAndGateInOutPackUserControl userControl;
	}
}
