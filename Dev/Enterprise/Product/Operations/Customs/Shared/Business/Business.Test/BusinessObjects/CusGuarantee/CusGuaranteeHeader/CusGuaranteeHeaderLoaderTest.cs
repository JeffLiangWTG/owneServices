using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGuaranteeHeader.Loader))]
	sealed class CusGuaranteeHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoadByGuaranteeNumber()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			permit.CPH_Number = "ABC001";
			var guarantee1 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee1.CPH_Number = "ABC001";
			guarantee1.CPH_Type = "T12";
			guarantee1.CPH_SubType = "S12";
			var guarantee2 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee2.CPH_Number = "ABC001";
			guarantee2.CPH_Type = "T13";
			guarantee2.CPH_SubType = "S13";
			var guarantee3 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee3.CPH_Number = "ABC001";
			guarantee3.CPH_Type = "T12";
			guarantee3.CPH_SubType = "S14";
			var guarantee4 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee4.CPH_Number = "ABC001";
			guarantee4.CPH_Type = "T12";
			guarantee4.CPH_SubType = "S15";
			guarantee4.CPH_IsActive = false;

			var loader = new BaseCusGuaranteeHeader.Loader(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { guarantee1, guarantee2, guarantee3 }, loader.Load<BaseCusGuaranteeHeader>("ABC001"));
			AssertContainsExactElementsInAnyOrder(new[] { guarantee1, guarantee3 }, loader.Load<BaseCusGuaranteeHeader>("ABC001", new ZQuery(CusPermitHeaderSchema.CPH_Type, "T12")));
		}

		public void TestGetLoadQuery()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			permit.CPH_Number = "ABC001";
			var guarantee1 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee1.CPH_Number = "ABC001";
			guarantee1.CPH_Type = "T12";
			guarantee1.CPH_SubType = "S12";
			var guarantee2 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee2.CPH_Number = "ABC001";
			guarantee2.CPH_Type = "T13";
			guarantee2.CPH_SubType = "S13";
			var guarantee3 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee3.CPH_Number = "ABC001";
			guarantee3.CPH_Type = "T12";
			guarantee3.CPH_SubType = "S14";
			var guarantee4 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee4.CPH_Number = "ABC001";
			guarantee4.CPH_Type = "T12";
			guarantee4.CPH_SubType = "S15";
			guarantee4.CPH_IsActive = false;

			var loader = new BaseCusGuaranteeHeader.Loader(Factory);
			CombineAssertions("No additional filter", () =>
			{
				var query = loader.GetLoadQuery("ABC001");
				AssertEquals("permit", false, permit.MatchesFilter(query));
				AssertEquals("guarantee1", true, guarantee1.MatchesFilter(query));
				AssertEquals("guarantee2", true, guarantee2.MatchesFilter(query));
				AssertEquals("guarantee3", true, guarantee3.MatchesFilter(query));
				AssertEquals("guarantee4", false, guarantee4.MatchesFilter(query));
			});
			CombineAssertions("filter by type", () =>
			{
				var query = loader.GetLoadQuery("ABC001", new ZQuery(CusPermitHeaderSchema.CPH_Type, "T12"));
				AssertEquals("permit", false, permit.MatchesFilter(query));
				AssertEquals("guarantee1", true, guarantee1.MatchesFilter(query));
				AssertEquals("no match guarantee2", false, guarantee2.MatchesFilter(query));
				AssertEquals("guarantee3", true, guarantee3.MatchesFilter(query));
				AssertEquals("guarantee4", false, guarantee4.MatchesFilter(query));
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BaseCusGuaranteeHeader.Loader(Factory);
		}
	}
}
