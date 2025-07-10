using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefSysConfig))]
	class RefSysConfigTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIRefSysConfig()
		{
			AssertEquals(typeof(RefSysConfig), ObjectFactory.GetType<Integration.Customs.Shared.IRefSysConfig>());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RefSysConfig>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var refSysConfigType = factory.New<RefSysConfigType>();
			refSysConfigType.ZRT_ConfigCode = "TRETIGWMAX";
			refSysConfigType.ZRT_Description = "TR E-Trade Maximum Import Gross Weight Limit";
			refSysConfigType.ZRT_LongDescription = "There is a maximum import weight limit for TR Customs E-Trade";
			var businessObject = factory.New<RefSysConfig>();
			businessObject.ZRC_ZRT_NKConfigCode = refSysConfigType.ZRT_ConfigCode;
			businessObject.ZRC_DecimalValue = (ZDecimal)30;
			businessObject.ZRC_StartDate = ZDateTime.Now.AddDays(-1);
			businessObject.ZRC_EndDate = ZDateTime.Now;
			return businessObject;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
