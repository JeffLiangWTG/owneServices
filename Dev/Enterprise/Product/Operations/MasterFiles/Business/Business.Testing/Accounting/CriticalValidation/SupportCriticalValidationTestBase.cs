using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(
			typeof(ISupportCriticalValidation),
			RequireTestOnlyInFirstSubLevel = true,
			IncludeAbstractClasses = true)]
	public abstract class SupportCriticalValidationTestBase : IConflictWithCriticalFieldsTester
	{
		public void TestSetBusinessContextForBusinessObject()
		{
			var obj = GetNewBusinessObject();

			if (obj is BusinessObject businessObject)
			{
				Assert("Precondition", !businessObject.HasContext(BusinessContext.ConflictWithCriticalFields));
				AssertNoExceptionThrown("Should not throw any exceptions", () => (businessObject as IConflictWithCriticalFields).SetConflictWithCriticalFieldsBusinessContext());
				Assert("Should have Business Context ConflictWithCriticalFields", businessObject.HasContext(BusinessContext.ConflictWithCriticalFields));
			}
			else
			{
				AssertExceptionThrown<NotSupportedException>("SetConflictWithCriticalFieldsBusinessContext will never be called for non BusinessObject", () => obj.SetConflictWithCriticalFieldsBusinessContext());
			}
		}
	}
}
