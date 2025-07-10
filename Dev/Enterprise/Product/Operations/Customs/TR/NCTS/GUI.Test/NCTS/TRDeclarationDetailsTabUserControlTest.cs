using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.TR.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class TRDeclarationDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestStampDutyGroupBoxAndItsElements()
		{
			using (var userControl = new TRDeclarationDetailsTabUserControl())
			{
				var stampDutyGroupBox = userControl.FindSingle<ZGroupBox>("StampDutyGroupBox");

				CombineAssertions("These Fields Should Be Visible In The Necessary GroupBox", () =>
				{
					AssertGroupBoxVisibility(stampDutyGroupBox, "StampDutyGroupBox");
					AssertElementTypeVisibilityAndExistence<ZDropEdit>(stampDutyGroupBox, "StampDutyStatusDropEdit", true);
					AssertElementTypeVisibilityAndExistence<ZCalcEdit>(stampDutyGroupBox, "StampDutyCalcEdit", true);
					AssertElementTypeVisibilityAndExistence<ZDateEdit>(stampDutyGroupBox, "RegistrationDateEdit", true);
				});
			}
		}

		public void TestDeclarationDetailsFieldsForTR()
		{
			using (var control = new TRDeclarationDetailsTabUserControl())
			{
				CombineAssertions("Existence of the type and the field", () =>
				{
					AssertElementTypeVisibilityAndExistence<ZCheckBox>(control, "BM_MoveToFTZCheckBox", true);
					AssertElementTypeVisibilityAndExistence<ZDropEdit>(control, "BM_CustomsOfficeAtBorderDropEdit", false);
					AssertElementTypeVisibilityAndExistence<ZTextBox>(control, "LrnRegistrationNumberTextBox", true);
					AssertElementTypeVisibilityAndExistence<ZDateEdit>(control, "LrnRegistrationDateEdit", true);
				});
			}
		}

		public void TestCustomsOfficeAtBorderDropEditVisibilityCondition()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();

			using (var frm = new ZForm(header))
			{
				frm.Controls.Add(new TRDeclarationDetailsTabUserControl());
				frm.Show();

				CombineAssertions("Visibility of the elements in order", () =>
				{
					AssertElementTypeVisibilityAndExistence<ZCheckBox>(frm, "BM_MoveToFTZCheckBox", true);
					AssertElementTypeVisibilityAndExistence<ZDropEdit>(frm, "BM_CustomsOfficeAtBorderDropEdit", false);
				});

				header.MovementHeader.MoveToFTZ = CargoWise.Types.ZBool.True;

				CombineAssertions("Visibility of the elements in order (updated)", () =>
				{
					AssertElementTypeVisibilityAndExistence<ZCheckBox>(frm, "BM_MoveToFTZCheckBox", true);
					AssertElementTypeVisibilityAndExistence<ZDropEdit>(frm, "BM_CustomsOfficeAtBorderDropEdit", true);
				});
			}
		}

		public void TestControlLocations()
		{
			using (var userControl = new TRDeclarationDetailsTabUserControl())
			{
				CombineAssertions("Check locations of the controls", () =>
				{
					AssertControlLocation(userControl, "StatusDropEdit", 122, 85);
					AssertControlLocation(userControl, "MessageStatusDropEdit", 122, 111);
					AssertControlLocation(userControl, "SimplifiedNctsProcedureCheckBox", 225, 133);
					AssertControlLocation(userControl, "SafetyAndSecurityCheckBox", 402, 133);
					AssertControlLocation(userControl, "DeclarationTypeDropEdit", 150, 155);
				});
			}
		}

		void AssertControlLocation(Control parent, string controlName, int expectedX, int expectedY)
		{
			var control = parent.Controls.Find(controlName, true).FirstOrDefault();
			AssertNotNull($"{controlName} should exist", control);
			AssertEquals($"{controlName} X location", expectedX, control.Location.X);
			AssertEquals($"{controlName} Y location", expectedY, control.Location.Y);
		}

		void AssertGroupBoxVisibility(ZGroupBox groupBox, string groupBoxName)
		{
			AssertEquals($"{groupBoxName}", true, groupBox.Visible);
		}

		void AssertElementTypeVisibilityAndExistence<T>(Control parent, string elementName, bool expectedVisibility) where T : Control
		{
			var element = parent.Controls.Find(elementName, true).FirstOrDefault();
			AssertNotNull($"{elementName} should exist", element);
			AssertEquals($"{elementName} should be of type {typeof(T).Name}", typeof(T), element.GetType());
			AssertEquals($"{elementName} visibility should be {expectedVisibility}", expectedVisibility, element.Visible);
		}
	}
}
