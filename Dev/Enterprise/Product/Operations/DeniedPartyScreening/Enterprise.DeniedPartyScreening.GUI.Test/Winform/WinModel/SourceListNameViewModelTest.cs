using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class SourceListNameWinModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var model = new SourceListNameWinModel(Factory, new DpsComplianceListItem("ABC", "U.S.Entity List", "U.S.Entity List Description", null, null));

			AssertEquals("U.S.Entity List", model.Name);
			AssertEquals("U.S.Entity List Description", model.Description);
		}

		public void TestConstructor_ScreenedPartyIsNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new SourceListNameWinModel(Factory, new DpsComplianceListItem("", "U.S.Entity List", "U.S.Entity List Description", null, null)));
			AssertExceptionThrown<ArgumentException>(() => new SourceListNameWinModel(Factory, new DpsComplianceListItem("ABC", "", "U.S.Entity List Description", null, null)));
		}

		public void TestConstructor_DescriptionIsNull()
		{
			AssertNoExceptionThrown(() => new SourceListNameWinModel(Factory, new DpsComplianceListItem("ABC", "U.S.Entity List", "U.S.Entity List Description", null, null)));
		}

		public void TestOpenComplianceFormWithMatchedCode()
		{
			var complianceListItem = Factory.NewWithValidTestData<RefComplianceList>();
			complianceListItem.RCL_ListCode = "ABC";
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			var model = new SourceListNameWinModel(Factory, new DpsComplianceListItem("ABC", "U.S.Entity List", "U.S.Entity List Description", null, null));
			model.OpenComplianceForm();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			using (var form = Application.OpenForms.OfType<RefComplianceListForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}
		}

		public void TestOpenComplianceFormWithUnmatchedCode()
		{
			var complianceListItem = Factory.NewWithValidTestData<RefComplianceList>();
			complianceListItem.RCL_ListCode = "ABC";
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			var model = new SourceListNameWinModel(Factory, new DpsComplianceListItem("DEF", "U.S.Entity List", "U.S.Entity List Description", null, null));
			model.OpenComplianceForm();
			AssertEquals("This record is currently unavailable via the Compliance Lists (Party Screening) module. Please raise a customer support incident if this issue persists.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, Application.OpenForms.OfType<RefComplianceListForm>().Any());
		}
	}
}
