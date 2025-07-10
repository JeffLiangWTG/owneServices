using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	internal static class DataModelTestHelper
	{
		public static void RunDataModelTest_ReportErrorWhenSetToEmpty<TBusinessObject>(BusinessObjectFactory factory)
			where TBusinessObject : BusinessObject
			=> RunDataModelTest_ReportErrorWhenSetToEmpty<TBusinessObject>(factory, f => f.NewWithValidTestData<TBusinessObject>());

		public static void RunDataModelTest_ReportErrorWhenSetToEmpty<TBusinessObject>(BusinessObjectFactory factory, Func<BusinessObjectFactory, BusinessObject> getNewBusinessObject)
			where TBusinessObject : BusinessObject
		{
			var businessObject = getNewBusinessObject(factory);
			businessObject[businessObject.TablePrefix + "_DataModel"] = ZString.Empty;
			Assertion.AssertEquals($"{businessObject.TablePrefix}_DataModel should not be set to empty.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public static void RunDataModelTest_ReportErrorWhenUpdated<TBusinessObject>(BusinessObjectFactory factory)
			where TBusinessObject : BusinessObject
			=> RunDataModelTest_ReportErrorWhenUpdated<TBusinessObject>(factory, f => f.NewWithValidTestData<TBusinessObject>());

		public static void RunDataModelTest_ReportErrorWhenUpdated<TBusinessObject>(BusinessObjectFactory factory, Func<BusinessObjectFactory, BusinessObject> getNewBusinessObject)
			where TBusinessObject : BusinessObject
		{
			var businessObject = getNewBusinessObject(factory);
			factory.Save();

			businessObject = factory.Load<TBusinessObject>(businessObject.PK);
			businessObject[businessObject.TablePrefix + "_DataModel"] = "ZZ";
			Assertion.AssertEquals($"{businessObject.TablePrefix}_DataModel can only be set once.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public static void RunDataModelTest_CanSaveTwice<TBusinessObject>(BusinessObjectFactory factory)
			where TBusinessObject : BusinessObject
			=> RunDataModelTest_CanSaveTwice<TBusinessObject>(factory, f => f.NewWithValidTestData<TBusinessObject>());
		public static void RunDataModelTest_CanSaveTwice<TBusinessObject>(BusinessObjectFactory factory, Func<BusinessObjectFactory, BusinessObject> getNewBusinessObject)
			where TBusinessObject : BusinessObject
		{
			getNewBusinessObject(factory);
			Assertion.AssertNoExceptionThrown(() =>
			{
				factory.Save();
				factory.Save();
			});
		}
	}
}
