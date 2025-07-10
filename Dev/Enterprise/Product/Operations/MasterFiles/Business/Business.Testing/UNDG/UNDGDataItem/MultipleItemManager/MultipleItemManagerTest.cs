using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MultipleItemManager))]
	sealed class MultipleItemManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValue()
		{
			DummyBusinessObjectCollection coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
			AssertEquals(ZString.Empty, manager.Value);

			DummyBusinessObject bizo = coll.AddNew();
			bizo.Z0_Code = "XYZ";
			AssertEquals("XYZ", manager.Value);

			manager.Value = "ABC";
			AssertEquals("ABC", bizo.Z0_Code);
			AssertEquals(1, coll.Count);

			DummyBusinessObject bizo2 = coll.AddNew();
			bizo2.Z0_Code = "DEF";
			AssertEquals("Many", manager.Value);
		}

		public void TestDefaultValueIsZeroForNumericType()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			var manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Decimal, false);
			AssertEquals(ZString.Empty, manager.Value);

			DummyBusinessObject bizo = coll.AddNew();
			bizo.Z0_Decimal = 123;
			AssertEquals("123", manager.Value);

			manager.Value = "";
			AssertEquals(ZDecimal.Zero, bizo.Z0_Decimal);
			AssertEquals(1, coll.Count);
		}

		public void TestDecimalValueSupportLocalization()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			var manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_AnotherDecimal, false);
			AssertEquals(ZString.Empty, manager.Value);

			DummyBusinessObject bizo = coll.AddNew();
			bizo.Z0_AnotherDecimal = 123;
			AssertEquals("123", manager.Value);

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			using (Culture.SetTemporarily(new CultureInfo("de-DE")))
			{
				bizo.Z0_AnotherDecimal = 1234.23;
				AssertEquals("1234,23", manager.Value);

				manager.Value = "1,23";
			}
			AssertEquals(new ZDecimal(1.23), bizo.Z0_AnotherDecimal);
		}

		public void TestDateValueSupportLocalization()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			var manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_DateOnly, false);
			AssertEquals(ZString.Empty, manager.Value);

			DummyBusinessObject bizo = coll.AddNew();
			bizo.Z0_DateOnly = new ZDate(2020, 2, 1);
			AssertEquals("01-Feb-20", manager.Value);

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			using (Culture.SetTemporarily(new CultureInfo("en-AU")))
			{
				manager.Value = "3/4/2020";
			}

			AssertEquals(new ZDate(2020, 4, 3), bizo.Z0_DateOnly);

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			using (Culture.SetTemporarily(new CultureInfo("en-US")))
			{
				manager.Value = "3/4/2020";
			}
			AssertEquals(new ZDate(2020, 3, 4), bizo.Z0_DateOnly);
		}

		public void TestDateValue()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			var manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_DateOnly, false);
			AssertEquals(ZString.Empty, manager.Value);

			DummyBusinessObject bizo = coll.AddNew();
			bizo.Z0_DateOnly = new ZDate(2020, 2, 1);
			AssertEquals("01-Feb-20", manager.Value);
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			using (Culture.SetTemporarily(new CultureInfo("en-AU")))
			{
				manager.Value = "3/4/2020";
				AssertEquals("03-Apr-20", manager.Value);
				manager.Value = "02-01-2019";
				AssertEquals("02-Jan-19", manager.Value);
				manager.Value = "05/09/18";
				AssertEquals("05-Sep-18", manager.Value);
				manager.Value = "30-Jun-01";
				AssertEquals("30-Jun-01", manager.Value);
			}
		}

		public void TestSetDecimalValueNullOrOverflow()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			var manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_AnotherDecimal, false);
			AssertEquals(ZString.Empty, manager.Value);

			manager.Value = null;
			manager.Value = "80000000000000000000000000000";
		}

		public void TestValue_MultipleItems()
		{
			DummyBusinessObjectCollection coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
			AssertEquals(ZString.Empty, manager.Value);

			DummyBusinessObject bizo = coll.AddNew();
			DummyBusinessObject bizo2 = coll.AddNew();
			manager.Value = "LMN";
		}

		public void TestValue_NoItems()
		{
			DummyBusinessObjectCollection coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
			AssertEquals(ZString.Empty, manager.Value);
			manager.Value = "XXX";
			AssertEquals("XXX", manager.Value);
			AssertEquals(1, coll.Count);
		}

		public void TestValue_WhenEmptyDoesNotCreateNewItem_String()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
			AssertEquals(ZString.Empty, manager.Value);
			manager.Value = "";
			AssertEquals("", manager.Value);
			AssertEquals(0, coll.Count);
		}

		public void TestValue_WhenEmptyDoesNotCreateNewItem_Decimal()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Decimal, false);
			AssertEquals(ZString.Empty, manager.Value);
			manager.Value = "0";
			AssertEquals("", manager.Value);
			AssertEquals(0, coll.Count);
		}

		public void TestValue_WhenEmptyDoesNotCreateNewItem_Guid()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Guid, false);
			AssertEquals(ZString.Empty, manager.Value);
			manager.Value = ZGuid.Empty.ToString();
			AssertEquals("", manager.Value);
			AssertEquals(0, coll.Count);
		}

		public void TestValue_ReadOnly()
		{
			var coll = new DummyBusinessObjectCollection(Factory);

			var manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
			manager.ReadOnlyWhenMany = true;
			Assert("Zero items - not read only", !manager.Value_ReadOnly);

			var bizo = coll.AddNew();
			Assert("One item - not read only", !manager.Value_ReadOnly);

			var bizo2 = coll.AddNew();
			Assert("More than one item - read only", manager.Value_ReadOnly);

			manager.ReadOnlyWhenMany = false;
			Assert("Not ReadOnlyWhenMany - not read only", !manager.Value_ReadOnly);
		}

		public void TestSetValue_ShouldNotChangeTheCurrentValue_WhenNewValueIsMany()
		{
			var source = new MultipleItemManager(new DummyBusinessObjectCollection(Factory), DummyBizoSchema.Z0_Code, false);
			var firstElement = source.Collection.AddNew();
			var secondElement = source.Collection.AddNew();

			var dest = new MultipleItemManager(new DummyBusinessObjectCollection(Factory), DummyBizoSchema.Z0_Code, false);
			dest.Collection.Clear();
			var temp = source.Value;
			AssertNoExceptionThrown(
				() =>
				{
					dest.Value = temp;
				});
			AssertEquals(0, dest.Collection.Count);
		}

		public void TestItemIsDeletedWhenEmpty()
		{
			DummyBusinessObjectWithUNDGs bO = Factory.New<DummyBusinessObjectWithUNDGs>();

			var testSubs = Factory.New<UNDGSubstance>();
			testSubs.DG_Code = "Test";

			var manager = bO.UNDGs.UNDGTechnicalNameManager;

			AssertEquals("Precondition: Value", ZString.Empty, manager.Value);
			AssertEquals("Precondition: Count", 0, bO.UNDGs.Count);

			UNDGDataItem undg = bO.UNDGs.AddNew();
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGFlashPoint = 100;
			undg.DI_DGVolume = 1;
			undg.DI_DGWeight = 2;
			undg.DI_MPMarinePollutant = "Y";
			undg.DI_OC_DGContact = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_UnitOfWeight = "KG";
			manager.Value = "Test";

			AssertEquals("Test", undg.DI_TechnicalName);
			AssertEquals("UNDG Count = 1", 1, bO.UNDGs.Count);

			undg.DI_IsLimitedQuantity = false;
			undg.DI_DGFlashPoint = 0;
			undg.DI_DGVolume = 0;
			undg.DI_DGWeight = 0;
			undg.DI_MPMarinePollutant = ZString.Empty;
			undg.DI_OC_DGContact = ZGuid.Empty;
			undg.DI_TechnicalName = ZString.Empty;
			undg.DI_UnitOfVolume = ZString.Empty;
			undg.DI_UnitOfWeight = ZString.Empty;
			manager.Value = ZString.Empty;

			AssertEquals(0, bO.UNDGs.Count);
		}

		public void TestFieldColumnType()
		{
			DummyBusinessObjectCollection coll = new DummyBusinessObjectCollection(Factory);

			MultipleItemManager manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
			AssertEquals(nameof(FieldType.Text), manager.FieldColumnType);

			manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, true);
			AssertEquals(nameof(FieldType.TextCodeFindBox), manager.FieldColumnType);

			manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Decimal, false);
			AssertEquals(nameof(FieldType.Decimal), manager.FieldColumnType);

			manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Guid, false);
			AssertEquals(nameof(FieldType.Guid), manager.FieldColumnType);

			manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_DateOnly, false);
			AssertEquals(nameof(FieldType.Date), manager.FieldColumnType);

			manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, true, true);
			AssertEquals(nameof(FieldType.TextDropEdit), manager.FieldColumnType);

			coll.AddNew();
			manager = new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, true);
			AssertEquals(nameof(FieldType.TextCodeFindBox), manager.FieldColumnType);

			coll.AddNew();
			AssertEquals("> 1 item, therefore Link Label", nameof(FieldType.LinkLabel), manager.FieldColumnType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObjectCollection coll = new DummyBusinessObjectCollection(Factory);
			return new MultipleItemManager(coll, DummyBizoSchema.Z0_Code, false);
		}

		sealed class DummyBusinessObjectWithUNDGs : DummyBusinessObject, IUNDGDataItemProvider
		{
			public DummyBusinessObjectWithUNDGs(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public UNDGDataItemCollection UNDGs
			{
				get { return undgs ?? (undgs = new UNDGDataItemCollection(this)); }
			}
			UNDGDataItemCollection undgs;

			bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;
		}
	}
}
