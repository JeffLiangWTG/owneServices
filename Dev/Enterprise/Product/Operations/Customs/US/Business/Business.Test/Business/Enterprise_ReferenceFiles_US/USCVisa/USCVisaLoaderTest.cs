using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCVisa.Loader))]
	class USCVisaLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			AssertEquals(visa1, new USCVisa.Loader(Factory).Load("100", "KR", new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 31)));

			AssertEquals(visa2, new USCVisa.Loader(Factory).Load("100", "KR", new ZDateTime(2007, 1, 1), new ZDateTime(2007, 12, 31)));

			AssertEquals(visa2, new USCVisa.Loader(Factory).Load("100", "KR", new ZDateTime(2007, 1, 1)));
		}

		USCVisa visa1;
		USCVisa visa2;
		USCVisa visa3;
		USCVisa visa4;

		protected override void SetUp()
		{
			base.SetUp();
			visa1 = Factory.New<USCVisa>();
			visa1.UO_BeginDate = new ZDateTime(2006, 1, 1);
			visa1.UO_EndDate = new ZDateTime(2006, 12, 31);
			visa1.UO_TextileCategoryNo = "100";
			visa1.UO_UC_NKOriginCountry = "KR";

			visa2 = Factory.New<USCVisa>();
			visa2.UO_BeginDate = new ZDateTime(2007, 1, 1);
			visa2.UO_EndDate = new ZDateTime(2007, 12, 31);
			visa2.UO_TextileCategoryNo = "100";
			visa2.UO_UC_NKOriginCountry = "KR";

			visa3 = Factory.New<USCVisa>();
			visa3.UO_BeginDate = new ZDateTime(2006, 1, 1);
			visa3.UO_EndDate = new ZDateTime(2006, 12, 31);
			visa3.UO_TextileCategoryNo = "200";
			visa3.UO_UC_NKOriginCountry = "KR";

			visa4 = Factory.New<USCVisa>();
			visa4.UO_BeginDate = new ZDateTime(2006, 1, 1);
			visa4.UO_EndDate = new ZDateTime(2006, 12, 31);
			visa4.UO_TextileCategoryNo = "100";
			visa4.UO_UC_NKOriginCountry = "JP";
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new USCVisa.Loader(Factory);
		}
	}
}
