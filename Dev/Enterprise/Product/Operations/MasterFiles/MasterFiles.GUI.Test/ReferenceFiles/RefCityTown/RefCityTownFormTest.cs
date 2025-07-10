using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefCityTownForm))]
	sealed class RefCityTownFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefCityTownForm(Factory.New<RefCityTown>());
		}

		public void TestFormCaption()
		{
			using (RefCityTownForm form = new RefCityTownForm(Factory.New<RefCityTown>()))
			{
				AssertEquals("City Town", Res.GetString("RefCityTownForm|67e6d094-c1c2-4894-87b6-3f68ed950935", "City Town"));
			}
		}

		public void TestDetachEventHasError()
		{
			var postcode = Factory.NewWithValidTestData<RefPostCode>();
			var cityTown = Factory.NewWithValidTestData<RefCityTown>();

			postcode.RK_CityTownPostCode = "2000";
			cityTown.R9_InternationalName = "SYDNEY";

			cityTown.PostCodes.Add(postcode);

			var pivot = (RefCityPCodePivot)((ManyToManyRelationship)cityTown.PostCodes.Relationship).GetPivotObject(postcode);
			pivot.R0_IsSystem = true;

			Factory.Save();

			var errorString = "This city/postcode relationship is system defined and therefore the selected postcode cannot be detached from this city.";

			using (var form = new RefCityTownForm(cityTown))
			{
				form.Show();
				var moduleButtonGrid = (ZModuleButtonGrid)form.Controls.Find("zModuleButtonGrid1", true)[0];
				moduleButtonGrid.InnerGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.DetachSelectedElement();
				AssertEquals(errorString, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, cityTown.PostCodes.Contains(postcode));
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			pivot.R0_IsSystem = false;
			Factory.Save();
			using (var form = new RefCityTownForm(cityTown))
			{
				form.Show();
				var moduleButtonGrid = (ZModuleButtonGrid)form.Controls.Find("zModuleButtonGrid1", true)[0];
				moduleButtonGrid.InnerGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.DetachSelectedElement();
				AssertNotEquals(errorString, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, cityTown.PostCodes.Contains(postcode));
			}
		}
	}
}
