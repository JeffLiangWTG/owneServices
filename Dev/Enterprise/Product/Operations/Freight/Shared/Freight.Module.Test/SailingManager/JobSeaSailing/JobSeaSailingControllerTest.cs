using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobSeaSailingController))]
	sealed class JobSeaSailingControllerTest : JobSailingControllerTest<JobSeaSailingController>
	{
		#region TestShowCopyForm_Ok

		public void TestShowCopyForm_Ok()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZController controller = ZControllerFactory.Create(ControllerIDs.JobSeaSailing);

			using (IZForm form = controller.ShowTemplateCopyForm(voyage))
			{
				AssertNotNull("Should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have shown the correct form", typeof(SailingTemplateCopyDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());

				AssertNotNull("Should have returned a form", form);
				AssertEquals("Should have returned the correct form", typeof(ZJobVoyageForm), form.GetType());
			}
		}

		#endregion

		#region TestShowCopyForm_Cancel

		public void TestShowCopyForm_Cancel()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZController controller = ZControllerFactory.Create(ControllerIDs.JobSeaSailing);

			using (IZForm form = controller.ShowTemplateCopyForm(voyage))
			{
				AssertNotNull("Should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have shown the correct form", typeof(SailingTemplateCopyDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());

				AssertNull("Should have returned null as the voyage form should not be shown", form);
			}
		}

		#endregion

		#region Implementation

		protected override ZString TransportType
		{
			get { return Constants.TransportModes.Sea; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobSeaSailing;
		}

		#endregion

		protected override void InitializeLine(OrgHeader line) => line.OH_IsShippingLine = true;

		protected override ZString TransportMode => Constants.TransportModes.Sea;
		protected override SecurityCheckpoint CheckPointForCreateFromJob => Env.Security.SailingScheduleCreateFromJob;
	}
}
