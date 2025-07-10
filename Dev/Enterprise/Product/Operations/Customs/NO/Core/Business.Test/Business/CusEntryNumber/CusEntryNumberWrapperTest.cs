using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class CusEntryNumberWrapperTest : TestCaseWithFactory
	{
		public void TestExistsCusEntryNumber()
		{
			CombineAssertions(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123");
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123");
				AssertEquals("CusEntryNumber doesn't exist", false, wrapper.ExistsCusEntryNumber);
				AssertNull("Getter doesn't create CusEntryNumber", CusEntryNumber.Load(parent, EntryType, CountryCode));

				_ = wrapper.EntryNumber;
				AssertNull("EntryNumber getter doesn't create CusEntryNumber", CusEntryNumber.Load(parent, EntryType, CountryCode));

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123");
				AssertEquals("CusEntryNumber exists", true, wrapper.ExistsCusEntryNumber);

				cusEntryNumber.Delete();
				AssertEquals("CusEntryNumber deleted", false, wrapper.ExistsCusEntryNumber);

				wrapper.SetEntryNumberPart("test value", nameof(parent.Property1), parent.Property1Info);
				AssertEquals("CusEntryNumber exists", true, wrapper.ExistsCusEntryNumber);
			});
		}

		public void TestEntryNumber()
		{
			CombineAssertions(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123");
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123");
				AssertEquals("CusEntryNumber doesn't exist", ZString.Empty, wrapper.EntryNumber);
				AssertNull("Getter doesn't create CusEntryNumber", CusEntryNumber.Load(parent, EntryType, CountryCode));

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123");
				AssertEquals("CusEntryNumber exists", "ITMRN123", wrapper.EntryNumber);

				cusEntryNumber.Delete();
				AssertEquals("CusEntryNumber deleted", ZString.Empty, wrapper.EntryNumber);
			});
		}

		public void TestSetEntryNumberPart()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, wrapper.ExistsCusEntryNumber);
				AssertEquals(ZString.Empty, wrapper.EntryNumber);

				wrapper.SetEntryNumberPart("Property3", nameof(parent.Property3), parent.Property3Info);
				Assert(wrapper.ExistsCusEntryNumber);
				AssertEquals(";;Property3", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property3), parent.Property3Info);
				wrapper.SetEntryNumberPart("Property2", nameof(parent.Property2), parent.Property2Info);
				AssertEquals(";Property2;", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart("Property3", nameof(parent.Property3), parent.Property3Info);
				AssertEquals(";Property2;Property3", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property3), parent.Property3Info);
				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property2), parent.Property2Info);
				wrapper.SetEntryNumberPart("Property1", nameof(parent.Property1), parent.Property1Info);
				AssertEquals("Property1;;", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart("Property3", nameof(parent.Property3), parent.Property3Info);
				AssertEquals("Property1;;Property3", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property3), parent.Property3Info);
				wrapper.SetEntryNumberPart("Property2", nameof(parent.Property2), parent.Property2Info);
				AssertEquals("Property1;Property2;", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart("Property3", nameof(parent.Property3), parent.Property3Info);
				AssertEquals("Property1;Property2;Property3", wrapper.EntryNumber);

				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property3), parent.Property3Info);
				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property2), parent.Property2Info);
				wrapper.SetEntryNumberPart(ZString.Empty, nameof(parent.Property1), parent.Property1Info);
				AssertEquals(";;", wrapper.EntryNumber);
			});
		}

		[ExpectNoExceptions]
		public void TestSetEntryNumberPart_WrongPart()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, wrapper.ExistsCusEntryNumber);
				AssertEquals(ZString.Empty, wrapper.EntryNumber);

				wrapper.SetEntryNumberPart("val for wrong part", "Property4", parent.Property3Info);
				AssertEquals(false, wrapper.ExistsCusEntryNumber);
				AssertEquals(ZString.Empty, wrapper.EntryNumber);
			});
		}

		public void TestSetEntryNumberPart_ParentHasChanges()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Before EntryNumber is updated", false, parent.HasChanges);
				wrapper.SetEntryNumberPart("123", nameof(parent.Property1), parent.Property1Info);
				AssertEquals("After EntryNumber is updated", true, parent.HasChanges);
			});
		}

		public void TestGetEntryNumberPart()
		{
			CombineAssertions(() =>
			{
				wrapper.SetEntryNumberPart("Val2", nameof(parent.Property2), parent.Property2Info);
				AssertEquals("Val2", wrapper.GetEntryNumberPart(nameof(parent.Property2)));

				wrapper.SetEntryNumberPart("Val3", nameof(parent.Property3), parent.Property3Info);
				AssertEquals("Val3", wrapper.GetEntryNumberPart(nameof(parent.Property3)));

				wrapper.SetEntryNumberPart("Val1", nameof(parent.Property1), parent.Property1Info);
				AssertEquals("Val1", wrapper.GetEntryNumberPart(nameof(parent.Property1)));
			});
		}

		[ExpectNoExceptions]
		public void TestGetEntryNumberPart_WrongPart()
		{
			AssertEquals(ZString.Empty, wrapper.GetEntryNumberPart("Property4"));
		}

		CusEntryNumber CreateCusEntryNumber(BusinessObject parent, string entryType, string countryCode, string entryNum)
		{
			var cusEntryNumber = CusEntryNumber.New(parent, entryType, countryCode);
			cusEntryNumber.CE_EntryNum = entryNum;
			return cusEntryNumber;
		}

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<CusEntryNumberWrapperParentForTest>();
			wrapper = new CusEntryNumberWrapper(parent, EntryType, new Dictionary<ZString, ZInt> { { "Property1", 0 }, { "Property2", 1 }, { "Property3", 2 } });
		}
		CusEntryNumberWrapperParentForTest parent;
		CusEntryNumberWrapper wrapper;

		const string EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		const string CountryCode = Core.Constants.CountryCodes.Norway;
	}

	class CusEntryNumberWrapperParentForTest : DummyBusinessObject
	{
		public CusEntryNumberWrapperParentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Property1 => ZString.Empty;
		public ZPropertyInfo Property1Info => GetZPropertyInfo(nameof(Property1));

		public ZString Property2 => ZString.Empty;
		public ZPropertyInfo Property2Info => GetZPropertyInfo(nameof(Property2));

		public ZString Property3 => ZString.Empty;
		public ZPropertyInfo Property3Info => GetZPropertyInfo(nameof(Property3));
	}
}
