using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedProperty))]
	class ImportedPropertyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValue()
		{
			Dummy.Z0_VarCharMax = "Value";
			AssertEquals("Value", ImportedProperty.Value);
		}

		public void TestDisplayValue()
		{
			Dummy.Z0_VarCharMax = "Value";
			Dummy.Z0_Description = "DisplayValue";
			AssertEquals("DisplayValue", ImportedProperty.DisplayValue);
		}

		public void TestDisplayValue_ForDateTime()
		{
			Dummy.Z0_Date = new ZDateTime(2005, 1, 2, 5, 30, 15);
			ImportedProperty importedProperty = new ImportedProperty(Dummy.Z0_DateInfo, "Date");
			AssertEquals("ZDateTime formatted correctly", "02-Jan-05 05:30", importedProperty.DisplayValue);
		}

		public void TestCaption()
		{
			AssertEquals("Text", ImportedProperty.Caption);
		}

		public void TestState()
		{
			Dummy.Z0_VarCharMax = "New";
			AssertEquals(ImportedPropertyState.New, ImportedProperty.State);

			Dummy.Z0_VarCharMax = "Unchanged";
			Factory.Save();
			AssertEquals(ImportedPropertyState.Unchanged, ImportedProperty.State);

			Dummy.Z0_VarCharMax = "Modified";
			AssertEquals(ImportedPropertyState.Modified, ImportedProperty.State);
		}

		public void TestState_FromConstructor()
		{
			ImportedProperty importedProperty = new ImportedProperty(Dummy.Z0_VarCharMaxInfo, "Caption", ImportedPropertyState.Modified);
			AssertEquals(ImportedPropertyState.Modified, importedProperty.State);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportedProperty(Dummy.Z0_VarCharMaxInfo, "Text", delegate
			{ return Dummy.Z0_Description; });
		}

		ImportedProperty ImportedProperty
		{
			get { return (ImportedProperty)GetNewBusinessObject(); }
		}

		DummyBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyBusinessObject>();
				}
				return dummy;
			}
		}
		DummyBusinessObject dummy;

		#endregion
	}
}
