using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRSurvey))]
	class MNRSurveyTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<MNRSurvey>();
		}

		#region TestYardUnitState

		public void TestYardUnitState()
		{
			var cydYardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			var survey = (MNRSurvey)GetNewBusinessObject();
			survey.MRS_ParentID = cydYardUnitState.PK;
			AssertEquals(cydYardUnitState, survey.YardUnitState);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var survey = Factory.New<MNRSurvey>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
					PredefinedNoteTypes.Instance.ContainerComment,
					PredefinedNoteTypes.Instance.SurveyInstruction },
				survey.NoteTypes);
		}

		#endregion

		#region TestIEDocsProvider
		public void TestGetEDocsProviderSupport()
		{
			var businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<MNRSurvey>();
			AssertEquals(Constants.DocManagerCodes.MNRSurvey, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion
	}
}
