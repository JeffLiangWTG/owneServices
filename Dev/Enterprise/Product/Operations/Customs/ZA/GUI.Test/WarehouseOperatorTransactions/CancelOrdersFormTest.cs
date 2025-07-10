using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.OperationalActions.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(CancelOrdersForm))]
	class CancelOrdersFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CancelOrdersForm(Factory, ZGuid.Empty, ZGuid.Empty, string.Empty);

		public void TestShowDialog()
		{
			var (_, productOwnerPK, warehousePK, _) = ExportApplicatorTestHelper.SetupTestDataForRecordFilterTesting(Factory);
			ZFormModaliser.ShowDialogsInTest = false;

			var notification = UnitTestUserNotification.Instance;
			notification.AddUserResponse("RefForVAL");
			CancelOrdersForm.ShowDialog(Factory, warehousePK, productOwnerPK);
			AssertType<CancelOrdersForm>("Should show the form with matching owner reference", ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.LastFormShownDialogForTest = null;
			notification.AddUserResponse("INVALID REF");
			CancelOrdersForm.ShowDialog(Factory, warehousePK, productOwnerPK);
			AssertNull("Should not show the form without matching owner reference", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Cannot find unallocated orders for \"INVALID REF\"!", notification.LastMessage.Text);
		}
	}
}
