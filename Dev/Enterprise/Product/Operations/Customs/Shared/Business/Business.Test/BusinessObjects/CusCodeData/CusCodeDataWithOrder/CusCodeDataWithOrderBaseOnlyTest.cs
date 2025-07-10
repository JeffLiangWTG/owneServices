using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusCodeDataWithOrder))]
	sealed class CusCodeDataWithOrderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			var code = new CusCodeDataWithOrder.Loader(Factory)
				.LoadOrCreate<CusCodeDataWithOrderForTest, BaseCusClassification>(classification, 1, "TES");
			AssertEquals("Code with order", code.HumanReadableName);
		}
	}

	public class CusCodeDataWithOrderForTest : CusCodeDataWithOrder
	{
		public CusCodeDataWithOrderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string CusCodeDataType => "SUP";

		protected internal override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(BaseCusClassification)); }
		}
	}
}
