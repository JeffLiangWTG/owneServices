using System.Web;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.WebCFS.Web
{
	public class ContainerDetailsTest : TestCaseWithFactory
	{
		class PageForTest : ContainerDetails
		{
			public void SetContainerForTest(CFSContainer container)
			{
				fContainerForTest = container;
				LoadOrCreateDataSource();
			}

			protected override BusinessObject GetNewDataSource()
			{
				if (fContainerForTest != null)
				{
					return fContainerForTest;
				}

				return base.GetNewDataSource();
			}
			CFSContainer fContainerForTest;

			public bool ShouldNotShowPackLinesGridForTest
			{
				get
				{
					return ShouldNotShowPackLinesGrid;
				}
			}
		}

		public void TestShouldNotShowPackLinesGrid()
		{
			PageForTest testPage = new PageForTest();
			AssertNotNull("TestPage should be proper type", testPage);
			Assert("Should not show PackLinesGrid when Containe is null", testPage.ShouldNotShowPackLinesGridForTest);
			CFSContainer testContainer = Factory.NewWithValidTestData<CFSContainer>();
			testPage.SetContainerForTest(testContainer);
			testContainer.JC_LCLUnpack = ZDateTime.Empty;
			Assert("Should not show PackLinesGrid when when UnpackDate is empty", testPage.ShouldNotShowPackLinesGridForTest);
			testContainer.JC_LCLUnpack = ZDateTime.Now.AddDays(1);
			Assert("Should not show PackLinesGrid when  UnpackDate has not passed", testPage.ShouldNotShowPackLinesGridForTest);
			testContainer.JC_LCLUnpack = ZDateTime.Now;
			Assert("Should show PackLinesGrid when UnpackDate is now", !testPage.ShouldNotShowPackLinesGridForTest);
			testContainer.JC_LCLUnpack = ZDateTime.Now.AddDays(-1);
			Assert("Should show PackLinesGrid when UnpackDate has passed", !testPage.ShouldNotShowPackLinesGridForTest);
		}

		[HttpContextEnabledTest]

		public void TestGetGuidFromRefParameter()
		{
			var testContainer = Factory.NewWithValidTestData<CFSContainer>();
			Factory.Save();
			HttpContext.Current.Request.QueryString.Add("Ref", testContainer.PK.ToString());
			var page = new PageForTest();

			page.SetContainerForTest(null);

			AssertEquals(testContainer.PK, page.DataSource.PK);
		}

		[HttpContextEnabledTest]

		public void TestGetGuidFromRefParameter_Invalid()
		{
			HttpContext.Current.Request.QueryString.Add("Ref", "NotAGuid");
			var page = new PageForTest();

			page.SetContainerForTest(null);

			AssertNull(page.DataSource);
		}
	}
}
