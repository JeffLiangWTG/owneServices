using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class EnterpriseBusinessObjectExtensionsTest : TestCaseWithFactory
	{
		public void TestGetValue_ReLoadData()
		{
			const string propertyName = "Greeting";
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var value1 = new ZString("HI");
			var value2 = new ZString("HELLO");
			var bizObjInFactory1 = factory1.New<SystemAndUserDefined>();
			bizObjInFactory1.SetSystemDefinedValue(propertyName, value1);
			factory1.Save();
			var bizObjInFactory2 = factory2.Load<SystemAndUserDefined>(bizObjInFactory1.PK);
			bizObjInFactory1.SetSystemDefinedValue(propertyName, value2);
			AssertEquals(value2, bizObjInFactory1.GetSystemDefinedValue<ZString>(propertyName));
			AssertEquals(value1, bizObjInFactory2.GetSystemDefinedValue<ZString>(propertyName));

			factory1.Save();
			AssertEquals(value2, bizObjInFactory1.GetSystemDefinedValue<ZString>(propertyName));
			AssertEquals(value1, bizObjInFactory2.GetSystemDefinedValue<ZString>(propertyName));
			AssertEquals("With reload", value2, bizObjInFactory2.GetSystemDefinedValue<ZString>(propertyName, true));
			AssertEquals(value2, bizObjInFactory2.GetSystemDefinedValue<ZString>(propertyName));
		}

		public void TestDynamicProperty()
		{
			Check(ZBool.True, ZBool.False, ZBool.False);
			Check(new ZDecimal(1.2m), new ZDecimal(2.5m), ZDecimal.Zero);
			Check(new ZInt(5), new ZInt(20), ZInt.Zero);
			Check(ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Empty);
			Check(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.Empty);
		}

		void Check<T>(T nonDefaultValue, T otherNonDefaultValue, T defaultValue) where T : IZType
		{
			CheckSystemDefinedValue(nonDefaultValue, otherNonDefaultValue, defaultValue);
			CheckUserDefinedValue(nonDefaultValue, otherNonDefaultValue, defaultValue);
		}

		void CheckSystemDefinedValue<T>(T nonDefaultValue, T otherNonDefaultValue, T defaultValue) where T : IZType
		{
			const string propertyName = "Hello";

			var bizO = Factory.New<SystemAndUserDefined>();
			bizO.SetSystemDefinedValue(propertyName, null, nonDefaultValue);
			AssertEquals(nonDefaultValue, bizO.GetSystemDefinedValue<T>(propertyName));
			AssertEquals("HasChanges is set when system value is set", true, bizO.HasChanges);
			Factory.Save();

			bizO = new BusinessObjectFactory().Load<SystemAndUserDefined>(bizO.PK);
			bizO.SetSystemDefinedValue(propertyName, null, otherNonDefaultValue);
			AssertEquals(otherNonDefaultValue, bizO.GetSystemDefinedValue<T>(propertyName));
			AssertEquals("HasChanges is set when system value is changed in new factory", true, bizO.HasChanges);

			bizO = new BusinessObjectFactory().Load<SystemAndUserDefined>(bizO.PK);
			bizO.SetSystemDefinedValue(propertyName, null, defaultValue);
			AssertEquals(defaultValue, bizO.GetSystemDefinedValue<T>(propertyName));
			AssertEquals("HasChanges is set when system value is changed to default in new factory", true, bizO.HasChanges);
		}

		void CheckUserDefinedValue<T>(T nonDefaultValue, T otherNonDefaultValue, T defaultValue) where T : IZType
		{
			const string propertyName = "Hello";

			var bizO2 = Factory.New<SystemAndUserDefined>();
			bizO2.SetUserDefinedValue(propertyName, null, nonDefaultValue);
			AssertEquals(nonDefaultValue, bizO2.GetUserDefinedValue<T>(propertyName));
			AssertEquals("HasChanges is set when user defined value is set", true, bizO2.HasChanges);
			Factory.Save();

			bizO2 = new BusinessObjectFactory().Load<SystemAndUserDefined>(bizO2.PK);
			bizO2.SetUserDefinedValue(propertyName, null, otherNonDefaultValue);
			AssertEquals(otherNonDefaultValue, bizO2.GetUserDefinedValue<T>(propertyName));
			AssertEquals("HasChanges is set when user defined value is changed in new factory", true, bizO2.HasChanges);

			bizO2 = new BusinessObjectFactory().Load<SystemAndUserDefined>(bizO2.PK);
			bizO2.SetUserDefinedValue(propertyName, null, defaultValue);
			AssertEquals(defaultValue, bizO2.GetUserDefinedValue<T>(propertyName));
			AssertEquals("HasChanges is set when user defined value is changed to default in new factory", true, bizO2.HasChanges);
		}
	}
}
