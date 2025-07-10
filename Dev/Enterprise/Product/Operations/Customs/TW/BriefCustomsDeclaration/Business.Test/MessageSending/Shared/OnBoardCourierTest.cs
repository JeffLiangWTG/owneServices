using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(OnBoardCourier))]
	sealed class OnBoardCourierTest : TestCaseWithFactory
	{
		public void TestData()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Wang Xiaomei";
			person1.PER_DriversLicenseNumber = "F123456789";
			person1.PER_Passport = "AB7533967";
			person1.PER_PreferredLanguage = Enterprise.Core.SharedConstants.Languages.English;

			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "王小美";
			person2.PER_Passport = "AB7533967";
			person2.PER_PreferredLanguage = Enterprise.Core.SharedConstants.Languages.ChineseTraditional;

			var onBoardCourier = new OnBoardCourier(person1);
			CombineAssertions(() =>
			{
				AssertEquals("ChineseName", ZString.Empty, onBoardCourier.ChineseName);
				AssertEquals("ID", "F123456789", onBoardCourier.ID);
				AssertEquals("Name", "Wang Xiaomei", onBoardCourier.Name);
				AssertEquals("TypeCode", "174", onBoardCourier.TypeCode);
			});

			onBoardCourier = new OnBoardCourier(person2);
			CombineAssertions(() =>
			{
				AssertEquals("ChineseName", "王小美", onBoardCourier.ChineseName);
				AssertEquals("ID", "AB7533967", onBoardCourier.ID);
				AssertEquals("Name", ZString.Empty, onBoardCourier.Name);
				AssertEquals("TypeCode", "53", onBoardCourier.TypeCode);
			});
		}
	}
}
