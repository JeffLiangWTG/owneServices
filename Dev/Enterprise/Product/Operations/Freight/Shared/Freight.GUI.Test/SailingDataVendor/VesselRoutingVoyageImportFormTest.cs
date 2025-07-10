using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.GUI.Testing
{
	[TestedType(typeof(VesselRoutingVoyageImportForm))]
	sealed class VesselRoutingVoyageImportFormTest : ZFormBasherTest
	{
		public void TestAppearanceOnShow()
		{
			Form.Show();
			Application.DoEvents();

			AssertEquals("Progress bar animated during import", ProgressBarStyle.Marquee, Form.ProgressBar.Style);
			AssertEquals("CloseButton not enabled during import", false, Form.CloseButton.Enabled);
			AssertEquals("KeepTickedPortPairsCheckBox not enabled during import", false, Form.KeepTickedPortPairsCheckBox.Enabled);
			AssertEquals("Importing Sailing Schedules...", Form.ImportStatusLabel.Text);
		}

		public void TestAppearanceAfterSuccessfulImport()
		{
			Form.Show();
			Application.DoEvents();

			Form.NotifyImportCompleted();
			AssertEquals("Progress bar not animated after import", ProgressBarStyle.Blocks, Form.ProgressBar.Style);
			AssertEquals("CloseButton enabled after import", true, Form.CloseButton.Enabled);

			AssertEquals("KeepTickedPortPairsCheckBox enabled after successful import", true, Form.KeepTickedPortPairsCheckBox.Enabled);
			AssertEquals("Default value of KeepTickedPortPairs after successful import", false, Form.KeepTickedPortPairs);
			AssertEquals("Import completed successfully", Form.ImportStatusLabel.Text);
		}

		public void TestAppearanceAfterFailedImport()
		{
			Form.Show();
			Application.DoEvents();
			Form.Notify(new ErrorNotification(ErrorType.Error, "Failed"));

			Form.NotifyImportCompleted();
			AssertEquals("Progress bar not animated after import", ProgressBarStyle.Blocks, Form.ProgressBar.Style);
			AssertEquals("CloseButton enabled after import", true, Form.CloseButton.Enabled);

			AssertEquals("KeepTickedPortPairsCheckBox cannot be changed after failed import", false, Form.KeepTickedPortPairsCheckBox.Enabled);
			AssertEquals("KeepTickedPortPairs after failed import", true, Form.KeepTickedPortPairs);
			AssertEquals("Import completed with errors", Form.ImportStatusLabel.Text);
		}

		public void TestFormCantBeClosedDuringImport()
		{
			Form.Show();
			Application.DoEvents();

			Form.Close();
			AssertEquals("Form cannot be closed during import", false, Form.IsDisposed);

			Form.NotifyImportCompleted();
			Form.Close();
			AssertEquals("Form can be closed after import has completed", true, Form.IsDisposed);
		}

		#region Test Classes

		class TestVesselRoutingVoyageImportForm : VesselRoutingVoyageImportForm
		{
			public new RichTextBox ProgressTextBox
			{
				get { return base.ProgressTextBox; }
			}

			public new ZButton CloseButton
			{
				get { return base.CloseButton; }
			}

			public new ProgressBar ProgressBar
			{
				get { return base.ProgressBar; }
			}

			public new ZLabel ImportStatusLabel
			{
				get { return base.ImportStatusLabel; }
			}

			public new ZCheckBox KeepTickedPortPairsCheckBox
			{
				get { return base.KeepTickedPortPairsCheckBox; }
			}
		}

		#endregion

		#region Implementation

		TestVesselRoutingVoyageImportForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new TestVesselRoutingVoyageImportForm();
				}
				return fForm;
			}
		}
		TestVesselRoutingVoyageImportForm fForm;

		protected override Form GetFormToBashCore()
		{
			return new TestVesselRoutingVoyageImportForm();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fForm != null)
			{
				fForm.Dispose();
			}
		}

		#endregion
	}
}
