using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(CusEntryNumberManager))]
sealed class CusEntryNumberManagerTest : TestCaseWithFactory
{
	public void TestCreateOrUpdateEntry_Parameters() => CombineAssertions(() =>
	{
		CusEntryNumber entryNumber = null;
		var parent = Factory.NewMoq<AsycudaManifestHeader>().Object;
		AssertExceptionThrown<ArgumentNullException>("When parent is null", () => CusEntryNumberManager.CreateOrUpdateEntry(null, "TYP", "VAL", "NO", ref entryNumber));
		AssertExceptionThrown<ArgumentException>("When entryType is null", () => CusEntryNumberManager.CreateOrUpdateEntry(parent, null, "VAL", "NO", ref entryNumber));
		AssertExceptionThrown<ArgumentException>("When entryType is empty", () => CusEntryNumberManager.CreateOrUpdateEntry(parent, null, "VAL", "NO", ref entryNumber));
		AssertExceptionThrown<ArgumentException>("When countryCode is null", () => CusEntryNumberManager.CreateOrUpdateEntry(parent, "TYP", "VAL", ZString.Empty, ref entryNumber));
	});

	public void TestCreateOrUpdateEntry_ForAnExistingEntryAndValue()
	{
		var (entry, parent) = CreateEntryWithParent("MRN", "123");
		var entryPK = entry.PK;
		CusEntryNumberManager.CreateOrUpdateEntry(parent, "MRN", "456", CountryCode, ref entry);

		AssertNotNull("CusEntryNumber", entry);

		CombineAssertions(() =>
		{
			AssertEquals("PK of CusEntryNumber", entryPK, entry.PK);
			AssertEquals("Entry Number Value", "456", entry.CE_EntryNum);
		});
	}

	public void TestCreateOrUpdateEntry_ForDeletedExistingEntryAndValue()
	{
		var (entry, parent) = CreateEntryWithParent("MRN", "123");
		var entryPK = entry.PK;
		entry.Delete();

		CusEntryNumberManager.CreateOrUpdateEntry(parent, "MRN", "456", CountryCode, ref entry);
		AssertNotNull("CusEntryNumber", entry);

		CombineAssertions(() =>
		{
			AssertNotEquals("PK of CusEntryNumbers", entryPK, entry.PK);
			AssertEquals("Entry Number Value", "456", entry.CE_EntryNum);
		});
	}

	public void TestCreateOrUpdateEntry_ForDeletedExistingEntryAndEmptyValue()
	{
		var (entry, parent) = CreateEntryWithParent("MRN", "123");
		var entryPK = entry.PK;
		entry.Delete();

		CusEntryNumberManager.CreateOrUpdateEntry(parent, "MRN", ZString.Empty, CountryCode, ref entry);
		AssertEquals("PK of CusEntryNumber", entryPK, entry.PK);
	}

	public void TestCreateOrUpdateEntry_ForNonExistingEntryAndValue()
	{
		var parent = Factory.NewMoq<AsycudaManifestHeader>().Object;
		CusEntryNumber entry = null;
		CusEntryNumberManager.CreateOrUpdateEntry(parent, "MRN", "456", CountryCode, ref entry);
		AssertNotNull("CusEntryNumber", entry);
		AssertEquals("Entry Number Value", "456", entry.CE_EntryNum);
	}

	public void TestLoad_Parameters() => CombineAssertions(() =>
	{
		CusEntryNumber entryNumber = null;
		var parent = Factory.NewMoq<AsycudaManifestHeader>().Object;
		AssertExceptionThrown<ArgumentNullException>("When parent is null", () => CusEntryNumberManager.Load(null, "TYP", "NO", ref entryNumber));
		AssertExceptionThrown<ArgumentException>("When entryType is null", () => CusEntryNumberManager.Load(parent, null, "NO", ref entryNumber));
		AssertExceptionThrown<ArgumentException>("When entryType is empty", () => CusEntryNumberManager.Load(parent, null, "NO", ref entryNumber));
		AssertExceptionThrown<ArgumentException>("When countryCode is empty", () => CusEntryNumberManager.Load(parent, "TYP", ZString.Empty, ref entryNumber));
	});

	public void TestLoad_ExistingEntryNumber()
	{
		var (entry, parent) = CreateEntryWithParent("TES", "VAL");
		CusEntryNumber entryToBeLoaded = null;
		CusEntryNumberManager.Load(parent, "TES", CountryCode, ref entryToBeLoaded);
		AssertNotNull("Entry", entryToBeLoaded);
		AssertSame(entry, entryToBeLoaded);
	}

	string CountryCode => countryCode ??= GlbCompany.CurrentCompany.Country.Code;
	string countryCode;

	(CusEntryNumber Entry, BusinessObject Parent) CreateEntryWithParent(string entryType, string value)
	{
		var parent = Factory.NewMoq<AsycudaManifestHeader>().Object;
		var entry = CusEntryNumber.LoadOrCreate(parent, entryType, CountryCode);
		entry.CE_EntryNum = value;
		return (entry, parent);
	}
}
