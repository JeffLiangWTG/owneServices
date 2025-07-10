using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationJobDatesProviderTests : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var dateToTest = new ZDateTime(2014, 5, 7);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DateOfArrival = dateToTest;
			var declarationJobDatesProvider = new DeclarationJobDatesProvider(declaration);
			AssertEquals(dateToTest, declarationJobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var dateToTest = new ZDateTime(2014, 5, 7);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ExportDate = dateToTest;
			var declarationJobDatesProvider = new DeclarationJobDatesProvider(declaration);
			AssertEquals(dateToTest, declarationJobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationJobDatesProvider = new DeclarationJobDatesProvider(declaration);

			AssertEquals(false, declarationJobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = new ZDateTime(2014, 5, 7);
			var jobHeader = new JobHeader.Loader(declaration).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, declarationJobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
