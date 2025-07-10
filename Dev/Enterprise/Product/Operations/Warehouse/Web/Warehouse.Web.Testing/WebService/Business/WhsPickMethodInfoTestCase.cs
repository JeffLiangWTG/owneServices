using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsPickMethodInfo))]
	public class WhsPickMethodInfoTestCase : DataObjectInfoTestCase<WhsPickMethodInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			WhsPickMethodInfo pickMethod = new WhsPickMethodInfo("Code1", "Descr1", false);
			AssertEquals("Code1", pickMethod.Code);
			AssertEquals("Descr1", pickMethod.Description);
			AssertEquals(false, pickMethod.IsDefault);

			pickMethod = new WhsPickMethodInfo("Code2", "Descr2", true);
			AssertEquals("Code2", pickMethod.Code);
			AssertEquals("Descr2", pickMethod.Description);
			AssertEquals(true, pickMethod.IsDefault);
		}

		public void TestCode()
		{
			AssertEquals("", Parent.Code);

			Parent.Code = "1234";
			AssertEquals("1234", Parent.Code);

			Parent.Code = "4321";
			AssertEquals("4321", Parent.Code);
		}

		public void TestDescription()
		{
			AssertEquals("", Parent.Description);

			Parent.Description = "1234";
			AssertEquals("1234", Parent.Description);

			Parent.Description = "4321";
			AssertEquals("4321", Parent.Description);
		}

		public void TestIsDefault()
		{
			AssertEquals(false, Parent.IsDefault);

			Parent.IsDefault = true;
			AssertEquals(true, Parent.IsDefault);

			Parent.IsDefault = false;
			AssertEquals(false, Parent.IsDefault);
		}

		#endregion

		#region Implementation

		protected new WhsPickMethodInfo Parent
		{
			get
			{
				return (WhsPickMethodInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsPickMethodInfo();
		}

		#endregion
	}
}
