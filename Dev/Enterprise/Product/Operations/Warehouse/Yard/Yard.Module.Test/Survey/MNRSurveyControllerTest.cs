using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRSurveyController))]
	public class MNRSurveyControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MNRSurvey;
		}

		#region ModuleID

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.MNRSurvey, new MNRSurveyController().ModuleID);
		}

		#endregion
	}
}
