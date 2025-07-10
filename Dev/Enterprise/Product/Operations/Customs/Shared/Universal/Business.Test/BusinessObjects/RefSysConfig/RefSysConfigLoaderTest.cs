using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefSysConfig.Loader))]
	public class RefSysConfigLoaderTest : LoaderTestCase
	{
		public void TestIRefSysConfigLoader()
		{
			AssertType<RefSysConfig.Loader>(ObjectFactory.Get<Integration.Customs.Shared.IRefSysConfigLoader>("IRefSysConfigLoader", Factory));
		}

		public void TestGetDecimalValue()
		{
			var date = ZDateTime.UtcToday;
			var typeName = "TESTC";
			var loader = new RefSysConfig.Loader(Factory);
			AssertEquals(3m, loader.GetDecimalValue(typeName, 3m, date));
			var expectedType = Factory.New<RefSysConfigType>();
			expectedType.ZRT_ConfigCode = typeName;
			expectedType.ZRT_Description = "TEST CASE";
			expectedType.ZRT_LongDescription = "TEST CASE";
			var expectedConfig = Factory.New<RefSysConfig>();
			expectedConfig.ZRC_ZRT_NKConfigCode = expectedType.ZRT_ConfigCode;
			expectedConfig.ZRC_DecimalValue = 4m;
			expectedConfig.ZRC_StartDate = date.AddDays(-10);
			expectedConfig.ZRC_EndDate = date.AddDays(10);
			Factory.Save();
			AssertEquals(3m, loader.GetDecimalValue(typeName, 3m, date));
			date = date.AddDays(1);
			AssertEquals(4m, loader.GetDecimalValue(typeName, 3m, date));
			expectedConfig.ZRC_DecimalValue = 5m;
			Factory.Save();
			AssertEquals(4m, loader.GetDecimalValue(typeName, 3m, date));
			AssertEquals(5m, new RefSysConfig.Loader(new BusinessObjectFactory()).GetDecimalValue(typeName, 3m));
			AssertEquals(3m, loader.GetDecimalValue(typeName, 3m));
			AssertEquals(5m, loader.GetDecimalValue(typeName, 3m, date.AddDays(1)));
		}

		public void TestGetStringValue()
		{
			var date = ZDateTime.UtcToday;
			var typeName = "TESTC";
			var loader = new RefSysConfig.Loader(Factory);
			AssertEquals("", loader.GetStringValue(typeName, date));
			var expectedType = Factory.New<RefSysConfigType>();
			expectedType.ZRT_ConfigCode = typeName;
			expectedType.ZRT_Description = "TEST CASE";
			expectedType.ZRT_LongDescription = "TEST CASE";
			var expectedConfig = Factory.New<RefSysConfig>();
			expectedConfig.ZRC_ZRT_NKConfigCode = expectedType.ZRT_ConfigCode;
			expectedConfig.ZRC_DecimalValue = 0m;
			expectedConfig.ZRC_StartDate = date.AddDays(-10);
			expectedConfig.ZRC_EndDate = date.AddDays(10);
			expectedConfig.ZRC_StringValue = "20210602";
			Factory.Save();
			AssertEquals("", loader.GetStringValue(typeName, date));
			date = date.AddDays(1);
			AssertEquals("20210602", loader.GetStringValue(typeName, date));
			expectedConfig.ZRC_StringValue = "20210603";
			Factory.Save();
			AssertEquals("20210602", loader.GetStringValue(typeName, date));
			AssertEquals("20210603", new RefSysConfig.Loader(new BusinessObjectFactory()).GetStringValue(typeName));
			AssertEquals("", loader.GetStringValue(typeName));
			AssertEquals("20210603", loader.GetStringValue(typeName, date.AddDays(1)));
		}

		public void TestGetBoolValue()
		{
			var date = ZDateTime.UtcToday;
			var typeName = "TESTB";
			var loader = new RefSysConfig.Loader(Factory);
			AssertEquals(false, loader.GetBoolValue(typeName, date));
			var expectedType = Factory.New<RefSysConfigType>();
			expectedType.ZRT_ConfigCode = typeName;
			expectedType.ZRT_Description = "TEST CASE";
			expectedType.ZRT_LongDescription = "TEST CASE";
			var expectedConfig = Factory.New<RefSysConfig>();
			expectedConfig.ZRC_ZRT_NKConfigCode = expectedType.ZRT_ConfigCode;
			expectedConfig.ZRC_DecimalValue = 0m;
			expectedConfig.ZRC_StartDate = date.AddDays(-10);
			expectedConfig.ZRC_EndDate = date.AddDays(10);
			expectedConfig.ZRC_BitValue = true;
			Factory.Save();
			AssertEquals(false, loader.GetBoolValue(typeName, date));
			date = date.AddDays(1);
			AssertEquals(true, loader.GetBoolValue(typeName, date));
			expectedConfig.Delete();
			Factory.Save();
			AssertEquals(true, loader.GetBoolValue(typeName, date));
			AssertEquals(false, new RefSysConfig.Loader(new BusinessObjectFactory()).GetBoolValue(typeName));
			AssertEquals(false, loader.GetBoolValue(typeName));
			AssertEquals(false, loader.GetBoolValue(typeName, date.AddDays(1)));
		}

		public void TestLoadByDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refsysconfigtype = helper.CreateRefSysConfigType("TRETIGWMAX", "TR E-Trade Maximum Import Gross Weight Limit", "There is a maximum import weight limit for TR Customs E-Trade.");
			var refsysconfigtype2 = helper.CreateRefSysConfigType("TRETIGVMAX", "TR E-Trade Maximum Import Goods Value", "There is a maximum import Goods Value limit for TR Customs E-Trade. The limit currency code is EUR");
			helper.CreateRefSysConfig(refsysconfigtype.ZRT_ConfigCode, 30, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateRefSysConfig(refsysconfigtype2.ZRT_ConfigCode, 1500, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
			var loader = new RefSysConfig.Loader(Factory);
			CombineAssertions(() =>
			{
				var refsysconfig1 = loader.Load("TRETIGWMAX", ZDateTime.Now);
				AssertNotNull(refsysconfig1);
				AssertEquals((ZDecimal)30, refsysconfig1.ZRC_DecimalValue);
				var refsysconfig2 = loader.Load("TRETIGVMAX", ZDateTime.Now.AddDays(-2));
				AssertNull(refsysconfig2);
				AssertNull(loader.Load("", ZDateTime.Now));
				AssertNull(loader.Load("TRETIGVMAX", ZDateTime.Empty));
				AssertNull(loader.Load("XZY", ZDateTime.Now));
			}

			);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefSysConfig.Loader(Factory);
		}
	}
}
