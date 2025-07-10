using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using WTG.NUnit;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		#region AssertLoadingAsWrongType

		public void AssertLoadingAsWrongType(Type[] typesToTestLoadAs, bool factorySaveBeforeLoad)
		{
			var bizO = (EnterpriseBusinessObject)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			if (factorySaveBeforeLoad)
			{
				Factory.Save();
			}

			var bizoType = bizO.GetType();
			foreach (var loadType in typesToTestLoadAs)
			{
				if (bizoType.IsAssignableFrom(loadType))
				{
					AssertNoExceptionThrown("Compatible load was run, should not throw exception", () => Factory.Load(loadType, bizO.PK));
				}
				else
				{
					NUnit.Framework.Assert.That(delegate
					{
						Factory.Load(loadType, bizO.PK);
					}, CustomConstraints.InnermostExceptionThrown(typeof(NotSupportedException)), "Incompatible load was run, should throw NotSupportedException");
				}
			}
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			var bizO = (EnterpriseBusinessObject)GetNewBusinessObject();

			var bindingFlags = IsValidationOverridden
				? BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
				: BindingFlags.Instance | BindingFlags.Public;

			var validationInfo = bizO.GetType().GetProperty("Validation", bindingFlags);
			var validation = (ZValidation)validationInfo.GetValue(bizO, null);
			AssertEquals(ExpectedValidationType, validation.GetType());
		}

		protected abstract Type ExpectedValidationType
		{
			get;
		}

		protected virtual bool IsValidationOverridden
		{
			get { return true; }
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			var bizO = (EnterpriseBusinessObject)GetNewBusinessObject();

			var bindingFlags = IsLookupsOverridden
				? BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
				: BindingFlags.Instance | BindingFlags.Public;

			var lookupsInfo = bizO.GetType().GetProperty("Lookups", bindingFlags);
			var lookups = (ZLookups)lookupsInfo.GetValue(bizO, null);
			AssertEquals(ExpectedLookupsType, lookups.GetType());
		}

		protected virtual bool IsLookupsOverridden
		{
			get { return true; }
		}

		protected abstract Type ExpectedLookupsType
		{
			get;
		}

		#endregion

		#region Implementation

		protected TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}

		protected TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = GetNewTestHelper()); }
		}

		protected virtual TransportCommonTestHelper GetNewTestHelper()
		{
			return new TransportCommonTestHelper(Factory);
		}

		TestNotificationBuffer notify;
		TransportCommonTestHelper helper;

		#endregion
	}
}
