using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.Business.Testing
{
	class CW1UserDefinedPropertyLoaderTest : TestCaseWithFactory
	{
		public void TestGetNonPersistentPropertyGetter_Bool() => TestGetNonPersistentPropertyGetter("BOO", true, false);
		public void TestGetNonPersistentPropertyGetter_Integer() => TestGetNonPersistentPropertyGetter("INT", 2, 3);
		public void TestGetNonPersistentPropertyGetter_Decimal() => TestGetNonPersistentPropertyGetter("DEC", 3.1415m, 2.7182m);
		public void TestGetNonPersistentPropertyGetter_String() => TestGetNonPersistentPropertyGetter("STR", "TEST", "NOPE");

		void TestGetNonPersistentPropertyGetter<T>(string type, T data1, T data2)
		{
			const string propertyName = "TestProperty";

			Helper.CreateUserDefinedProperty(type, propertyName);
			Factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();
			var getter = loader.GetUserDefinedPropertyGetter(param, propertyName);
			AssertNotNull("Should find a valid property.", getter);

			var fact1 = new EmployeeWithUDFFactDummy();
			var fact2 = new EmployeeWithUDFFactDummy();

			var getterFunc = Expression.Lambda<Func<EmployeeWithUDFFactDummy, T>>(getter, param).Compile();
			AssertEquals("Should return the correct data.", default(T), getterFunc(fact1));
			AssertEquals("Should return the correct data.", default(T), getterFunc(fact2));

			fact1.SetUserDefinedProperty(propertyName, "test", data1);
			fact2.SetUserDefinedProperty(propertyName, "test", data2);
			AssertEquals("Should return the correct data.", data1, getterFunc(fact1));
			AssertEquals("Should return the correct data.", data2, getterFunc(fact2));
		}

		public void TestGetNonPersistentPropertyGetter_NoPropertiesSetup()
		{
			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));

			var loader = new CW1UserDefinedPropertyLoader();
			var getter = loader.GetUserDefinedPropertyGetter(param, "Test");

			AssertNull("Should *not* find a property.", getter);
		}

		public void TestGetNonPersistentPropertyGetter_CaseInsensitive()
		{
			Helper.CreateUserDefinedProperty("BOO", "Test");
			Factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();
			var getter1 = loader.GetUserDefinedPropertyGetter(param, "Test");
			AssertNotNull("Precondition: Should find a property.", getter1);

			var getter2 = loader.GetUserDefinedPropertyGetter(param, "tEsT");
			AssertNotNull("Should find a property despite casing problems.", getter2);
		}

		public void TestGetNonPersistentPropertyGetter_DifferentName()
		{
			const string propertyName1 = "TestProperty1";
			const string propertyName2 = "TestProperty2";

			Helper.CreateUserDefinedProperty("BOO", propertyName1);
			Factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();
			var getter1 = loader.GetUserDefinedPropertyGetter(param, propertyName1);
			AssertNotNull("Precondition: Should find a property.", getter1);

			var getter2 = loader.GetUserDefinedPropertyGetter(param, propertyName2);
			AssertNull("Should *not* find a property.", getter2);
		}

		public void TestGetNonPersistentPropertyGetter_DifferentKey()
		{
			const string propertyName1 = "TestProperty1";
			const string propertyName2 = "TestProperty2";

			var userProperty = Helper.CreateUserDefinedProperty("BOO", propertyName1);
			Factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();
			var getter1 = loader.GetUserDefinedPropertyGetter(param, propertyName1);
			AssertNotNull("Precondition: Should find a property.", getter1);

			userProperty.XC_ParentID = Guid.NewGuid();
			var getter2 = loader.GetUserDefinedPropertyGetter(param, propertyName2);
			AssertNull("Should *not* find a property.", getter2);
		}

		public void TestGetNonPersistentPropertyGetter_MultipleColumns()
		{
			const string propertyName1 = "TestProperty1";
			const string propertyName2 = "TestProperty2";

			Helper.CreateUserDefinedProperty("BOO", propertyName1);
			Helper.CreateUserDefinedProperty("INT", propertyName2);

			var other = Guid.NewGuid();
			var error = Guid.NewGuid();
			Helper.CreateUserDefinedProperty("INT", propertyName1).XC_ParentID = other;
			Helper.CreateUserDefinedProperty("STR", propertyName2).XC_ParentID = other;
			Helper.CreateUserDefinedProperty("STR", propertyName1).XC_ParentID = error;
			Helper.CreateUserDefinedProperty("STR", propertyName2).XC_ParentID = error;
			Factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();
			var getter = loader.GetUserDefinedPropertyGetter(param, propertyName1);
			AssertNotNull("Precondition: Should find a property.", getter);

			var fact = new EmployeeWithUDFFactDummy();
			fact.SetUserDefinedProperty(propertyName1, "test", true);
			var getterFunc = Expression.Lambda<Func<EmployeeWithUDFFactDummy, bool>>(getter, param).Compile();
			AssertEquals("Should return the correct data.", true, getterFunc(fact));
		}

		public void TestGetNonPersistentPropertyGetter_InvalidFactType()
		{
			const string propertyName = "TestProperty1";

			var userProperty = Helper.CreateUserDefinedProperty("BOO", propertyName);
			userProperty.XC_ParentID = Guid.NewGuid();
			Factory.Save();

			var param = Expression.Parameter(typeof(ICannotHaveUserDefinedProperties));
			var loader = new CW1UserDefinedPropertyLoader();
			var getter = loader.GetUserDefinedPropertyGetter(param, propertyName);
			AssertNull("Should *not* find a property.", getter);
		}

		public void TestGetNonPersistentPropertyGetter_MultipleTypes()
		{
			const string propertyName = "TestProperty1";

			Helper.CreateUserDefinedProperty("INT", propertyName);
			Helper.CreateUserDefinedProperty("INT", propertyName, "TST");
			Factory.Save();

			var loader = new CW1UserDefinedPropertyLoader();
			var param1 = Expression.Parameter(typeof(ICanHaveUserDefinedProperties));
			var getter1 = loader.GetUserDefinedPropertyGetter(param1, propertyName);
			AssertNotNull("Should find a property.", getter1);
			var getterFunc1 = Expression.Lambda<Func<CanHaveUserDefinedProperties, int>>(getter1, param1).Compile();

			var param2 = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var getter2 = loader.GetUserDefinedPropertyGetter(param2, propertyName);
			AssertNotNull("Should find a property.", getter2);
			var getterFunc2 = Expression.Lambda<Func<IEmployeeWithUDFFactDummy, int>>(getter2, param2).Compile();

			var fact1 = new CanHaveUserDefinedProperties();
			var fact2 = new EmployeeWithUDFFactDummy();
			fact1.SetUserDefinedProperty(propertyName, "test1", 7);
			fact2.SetUserDefinedProperty(propertyName, "test1", 3);
			AssertEquals("Should return the correct data.", 7, getterFunc1(fact1));
			AssertEquals("Should return the correct data.", 3, getterFunc2(fact2));
		}

		public void TestGetNonPersistentPropertyGetter_InvalidPropertyType()
		{
			const string propertyName = "TestProperty1";

			var customColumn = Helper.CreateUserDefinedProperty("LOL", propertyName);
			Factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();
			AssertExceptionThrown<ArgumentException>(() => loader.GetUserDefinedPropertyGetter(param, propertyName));
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}

	[UseSnapshotProtection]
	class ConcurrentCW1UserDefinedPropertyLoaderTest : TestCase
	{
		public void TestGetNonPersistentPropertyGetter_ThreadSafe()
		{
			var factory = new BusinessObjectFactory();
			var helper = new Helper(factory);
			helper.CreateUserDefinedProperty("BOO", "Test");
			factory.Save();

			var param = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var loader = new CW1UserDefinedPropertyLoader();

			Parallel.For(0, 100, i =>
			{
				var getter = loader.GetUserDefinedPropertyGetter(param, "Test");
				AssertNotNull("Precondition: Should find a property.", getter);
			});
		}

		public void TestGetNonPersistentPropertyGetter_ThreadSafe_MultipleTypes()
		{
			var factory = new BusinessObjectFactory();
			var helper = new Helper(factory);
			helper.CreateUserDefinedProperty("BOO", "Test");
			helper.CreateUserDefinedProperty("BOO", "Test", "TST");
			factory.Save();

			var param1 = Expression.Parameter(typeof(IEmployeeWithUDFFactDummy));
			var param2 = Expression.Parameter(typeof(ICanHaveUserDefinedProperties));
			var loader = new CW1UserDefinedPropertyLoader();

			Parallel.For(0, 100, i =>
			{
				var getter = loader.GetUserDefinedPropertyGetter(i % 2 == 0 ? param1 : param2, "Test");
				AssertNotNull("Precondition: Should find a property.", getter);
			});
		}
	}

	[FactUniqueKey("ERR")]
	interface ICannotHaveUserDefinedProperties : IFact
	{
	}

	[FactUniqueKey("TST")]
	interface ICanHaveUserDefinedProperties : IInputFactWithUserDefinedProperties
	{
	}

	class CanHaveUserDefinedProperties : InputFactWithUserDefinedProperties, ICanHaveUserDefinedProperties
	{
	}

	class EmployeeWithUDFFactDummy : InputFactWithUserDefinedProperties, IEmployeeWithUDFFactDummy
	{
		public int StaffID => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();
	}

	[FactUniqueKey("INV")]
	interface IEmployeeWithUDFFactDummy : IInputFactWithUserDefinedProperties
	{
		int StaffID { get; }
		string Name { get; }
	}
}
