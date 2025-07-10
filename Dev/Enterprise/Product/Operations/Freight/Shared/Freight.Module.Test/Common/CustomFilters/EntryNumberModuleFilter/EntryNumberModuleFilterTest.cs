using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(EntryNumberModuleFilter))]
	sealed class EntryNumberModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			AssertEquals(ZString.Empty, Filter.EntryType);
			AssertEquals(ZString.Empty, Filter.Property);

			Filter.EntryType = "CAN";
			Filter.Property = "123";

			AssertEquals("CAN", Filter.EntryType);
			AssertEquals("123", Filter.Property);

			Filter.EntryType = "EXDC";
			AssertEquals(ZString.Empty, Filter.Property);
			AssertEquals(true, Filter.PropertyInfo.ReadOnly);
		}

		public void TestClear()
		{
			Filter.EntryType = "CAN";
			Filter.Property = "123";

			AssertEquals("Precondition", "CAN", Filter.EntryType);
			AssertEquals("Precondition", "123", Filter.Property);

			Filter.Clear();
			AssertEquals("", Filter.EntryType);
			AssertEquals("", Filter.Property);
		}

		public void TestIsEmpty()
		{
			Filter.EntryType = "";
			Filter.Property = "";
			AssertEquals(true, filter.IsEmpty);

			Filter.Property = "123";
			AssertEquals(false, Filter.IsEmpty);

			Filter.EntryType = "CAN";
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property = "";
			AssertEquals(false, Filter.IsEmpty);
		}

		public void TestValidation()
		{
			AssertEquals("Should be EntryNumberModuleFilterValidation", typeof(EntryNumberModuleFilterValidation), Filter.Validation.GetType());
		}

		#region Implementation

		public EntryNumberModuleFilter Filter
		{
			get { return filter ?? (filter = new EntryNumberModuleFilter("text", delegate { return null; })); }
		}
		EntryNumberModuleFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntryNumberModuleFilter("text", delegate
			{ return null; });
		}

		#endregion
	}
}
