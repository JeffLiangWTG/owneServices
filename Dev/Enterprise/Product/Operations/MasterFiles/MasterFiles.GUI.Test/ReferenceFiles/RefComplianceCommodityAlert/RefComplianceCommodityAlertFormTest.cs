using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Testing
{
	[TestedType(typeof(RefComplianceCommodityAlertForm))]
	sealed class RefComplianceCommodityAlertFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestRefComplianceCommodityAlertUserControl()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var refComplianceCommodityAlertUserControl = (RefComplianceCommodityAlertUserControl)form.FindSingle<ZUserControl>("ComplianceCommodityAlertUserControl");

				AssertNotNull(refComplianceCommodityAlertUserControl);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			dataBoundRefComplianceCommodityAlert = Factory.New<RefComplianceCommodityAlert>();
			var form = new RefComplianceCommodityAlertForm(dataBoundRefComplianceCommodityAlert);
			form.ControllerID = ControllerIDs.RefComplianceCommodityAlert;

			return form;
		}

		RefComplianceCommodityAlert dataBoundRefComplianceCommodityAlert;

		#endregion
	}
}
