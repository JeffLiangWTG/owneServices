using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class CheckEnteredTest : TestCase
	{
		public void TestGetValidator()
		{
			var rule = new CheckEnteredRule();
			Dictionary<string, object> values = new Dictionary<string, object>();
			CustomBusinessObject cusObj = new CustomBusinessObject(null, new CustomPropertyCollectionImpl(
				propertyName =>
				{
					object value;
					return values.TryGetValue(propertyName, out value) ? value : null;
				},
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				})
			{
				{ typeof(ZString), "TestProp", rule.GetValidator(), rule.GetMetaData().ToArray() },
			});

			ZPropertyInfo propInfo = cusObj.FindPropertyInfo("TestProp");
			cusObj.Validation.ValidateAll();
			TestCaseWithFactory.AssertHasError(propInfo, "Please enter a value.");
			cusObj["TestProp"] = "A1";
			TestCaseWithFactory.AssertNoErrors(propInfo);
			cusObj["TestProp"] = "";
			TestCaseWithFactory.AssertHasError(propInfo, "Please enter a value.");
		}

		public void TestCanBeApplied()
		{
			var rule = new CheckEnteredRule();

			Assert(rule.CanBeApplied(typeof(ZString)));
			Assert(!rule.CanBeApplied(typeof(ZBool)));
			Assert(rule.CanBeApplied(typeof(ZDateTime)));
		}
	}
}
