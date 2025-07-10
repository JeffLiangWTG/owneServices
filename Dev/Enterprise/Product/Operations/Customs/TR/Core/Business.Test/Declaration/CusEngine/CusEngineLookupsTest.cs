using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusEngineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEngineTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(EngineTypeList.Codes._1, EngineTypeList.Descriptions._1),
				new CodeDescriptionPair(EngineTypeList.Codes._2, EngineTypeList.Descriptions._2),
				new CodeDescriptionPair(EngineTypeList.Codes._3, EngineTypeList.Descriptions._3),
			}, Lookups.EngineTypeList);
		}

		CusEngineLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					var engine = Factory.New<CusEngine>();
					lookups = new CusEngineLookups(engine);
				}
				return lookups;
			}
		}
		CusEngineLookups lookups;
	}
}
