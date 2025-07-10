using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PPQForm368NoticeOfArrivalData))]
	internal class PPQForm368NoticeOfArrivalDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPPQForm368NoticeOfArrivalDataWorksAsProxy()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_PPQForm368Box13A = "Box13A";
			declaration.US_PPQForm368Box13B = "Box13B";
			declaration.US_PPQForm368Box13C = "Box13C";
			IPPQForm368NoticeOfArrivalSupportable noticeOfArrivalSupportable = declaration;
			AssertNotNull("Precondition: JobDeclaration should implement IPPQForm368NoticeOfArrivalSupportable", noticeOfArrivalSupportable);

			PPQForm368NoticeOfArrivalData noticeOfArrivalData = new PPQForm368NoticeOfArrivalData(noticeOfArrivalSupportable);
			AssertEquals("PPQForm368Box13A", "Box13A", noticeOfArrivalSupportable.US_PPQForm368Box13A);
			AssertEquals("PPQForm368Box13A", "Box13B", noticeOfArrivalSupportable.US_PPQForm368Box13B);
			AssertEquals("PPQForm368Box13A", "Box13C", noticeOfArrivalSupportable.US_PPQForm368Box13C);

			noticeOfArrivalData.US_PPQForm368Box13A = "PPQBox13A";
			noticeOfArrivalData.US_PPQForm368Box13B = "PPQBox13B";
			noticeOfArrivalData.US_PPQForm368Box13C = "PPQBox13C";
			AssertEquals("PPQForm368Box13A", "Box13A", declaration.US_PPQForm368Box13A);
			AssertEquals("PPQForm368Box13A", "Box13B", declaration.US_PPQForm368Box13B);
			AssertEquals("PPQForm368Box13A", "Box13C", declaration.US_PPQForm368Box13C);

			noticeOfArrivalData.SaveData();
			AssertEquals("PPQForm368Box13A", "PPQBox13A", declaration.US_PPQForm368Box13A);
			AssertEquals("PPQForm368Box13A", "PPQBox13B", declaration.US_PPQForm368Box13B);
			AssertEquals("PPQForm368Box13A", "PPQBox13C", declaration.US_PPQForm368Box13C);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return new PPQForm368NoticeOfArrivalData(declaration);
		}
	}
}
