using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRSurveyFilterControl))]
	public class MNRSurveyFilterControlTest : TestCaseWithFactory
	{
		#region FilterControlFileds

		public void TestFilterControlFileds()
		{
			using (var surveyFilterControl = new MNRSurveyFilterControl(GetNewGridCollection(), (MNRSurveyFilterBusinessObject)GetNewFilterBusinessObject()))
			{
				AssertEquals("MNRSurveyFilterControl", surveyFilterControl.Name);
				AssertEquals(5, surveyFilterControl.Grid.ColumnStyles.Count);
			}
		}

		#endregion

		#region GetSomeThing

		protected FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new MNRSurveyFilterBusinessObject();
		}

		protected IBusinessObjectCollection GetNewGridCollection()
		{
			return new MNRSurveyCollection(Factory);
		}

		#endregion
	}
}
