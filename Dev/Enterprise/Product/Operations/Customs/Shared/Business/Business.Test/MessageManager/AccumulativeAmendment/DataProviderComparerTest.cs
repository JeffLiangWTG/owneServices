using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DataProviderComparerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareZTypeMembersWhenEqual()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummyHeader1 = new DummyEntryHeader() { EntryNumber = "1", LoadingDate = new DateTime(2020, 1, 1), IsPersonalItem = true, PackageCount = 2, TotalValue = 20.250m };
			var dummyHeader2 = new DummyEntryHeader() { EntryNumber = "1", LoadingDate = new DateTime(2020, 1, 1), IsPersonalItem = true, PackageCount = 2, TotalValue = 20.250m };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("NoChange.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareZTypeMembersWhenDifferent()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummyHeader1 = new DummyEntryHeader() { EntryNumber = "1a", LoadingDate = new ZDateTime(2020, 1, 1), PackageCount = 1, IsPersonalItem = true };
			var dummyHeader2 = new DummyEntryHeader() { EntryNumber = "1A", LoadingDate = new ZDateTime(2020, 3, 3), PackageCount = 2, TotalValue = 20.250m };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("UpdateFields.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareOrganisationsWhenEquals()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var declarant1 = new DummyOrganization() { Role = RoleType.Declarant, CompanyName = "AAA", AddressLine = "1 King St" };
			var manufacturer1 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "BBB", AddressLine = "1 Queen St" };
			var supplier1 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "CCC", AddressLine = "1 Alpha Rd" };
			var dummyHeader1 = new DummyEntryHeader() { Declarant = declarant1, Manufacturer = manufacturer1, Supplier = supplier1 };

			var declarant2 = new DummyOrganization() { Role = RoleType.Declarant, CompanyName = "AAA", AddressLine = "1 King St" };
			var manufacturer2 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "BBB", AddressLine = "1 Queen St" };
			var supplier2 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "CCC", AddressLine = "1 Alpha Rd" };
			var dummyHeader2 = new DummyEntryHeader() { Declarant = declarant2, Manufacturer = manufacturer2, Supplier = supplier2 };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("NoChange.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareOrganisationsWhenDifferent()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var declarant = new DummyOrganization() { Role = RoleType.Declarant, CompanyName = "AAA", RepresentativeName = "James" };
			var manufacturer1 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "BBB" };
			var dummyHeader1 = new DummyEntryHeader() { Manufacturer = manufacturer1, Declarant = declarant };

			var supplier = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "", AddressLine = "1 Alpha Rd" };
			var manufacturer2 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "bbb", AddressLine = "1 Queen St" };
			var dummyHeader2 = new DummyEntryHeader() { Manufacturer = manufacturer2, Supplier = supplier };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("UpdateEntities.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubinesWhenEquals()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Ingredient" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU", SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine2 = new DummySubLine() { SubLineNo = 1, Ingredient = "Ingredient" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU", SubLines = new DummySubLine[] { dummySubLine2 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("NoChange.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubLinesWhenDifferent()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 3 };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 4, Ingredient = "Woods" };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 5, Ingredient = "Plastics" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "33333", SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2 } };

			var dummySubLine4 = new DummySubLine() { SubLineNo = 3, Ingredient = "Iron" };
			var dummySubLine5 = new DummySubLine() { SubLineNo = 4 };
			var dummySubLine6 = new DummySubLine() { SubLineNo = 5, Ingredient = "plastics" };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 1, CountryOfOrigin = "AU", SubLines = new DummySubLine[] { dummySubLine4, dummySubLine5 } };
			var dummyLine4 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "55555", SubLines = new DummySubLine[] { dummySubLine6 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine3, dummyLine4 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("UpdateEntityWithID.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubLinesWhenAdded()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 3 };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine2 = new DummySubLine() { SubLineNo = 3 };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 4, Ingredient = "Glass" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine2, dummySubLine3 } };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU" };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2, dummyLine3 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("AddEntityWithID.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubLinesWhenDeleted()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 3 };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 4, Ingredient = "Glass" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU" };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2 } };

			var dummySubLine3 = new DummySubLine() { SubLineNo = 3 };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine3 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("DeleteEntityWithID.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareOrganisationInIEnumerableList()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var supplier1 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "AAA", AddressLine = "1 King St" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, Supplier = supplier1 };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var supplier2 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "BBB", AddressLine = "1 Queen St" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, Supplier = supplier2 };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("UpdateEntitiesInList.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubLinesBothAdded_OriginalListIsNull()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 2, Ingredient = "Glass" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2 } };

			var dummySubLine2_1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummyLine2_1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_1 } };
			var dummySubLine2_3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2_2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_3 } };
			var dummySubLine2_2 = new DummySubLine() { SubLineNo = 1, Ingredient = "Glass" };
			var dummyLine2_3 = new DummyEntryLine() { EntryLineNo = 3, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_2 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2_1, dummyLine2_2, dummyLine2_3 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("AddEntryLineAndSubLine.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubLinesBothDeleted()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 1, Ingredient = "Glass" };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 3, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2, dummyLine3 } };

			var dummySubLine2_1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummySubLine2_2 = new DummySubLine() { SubLineNo = 2, Ingredient = "Glass" };
			var dummyLine2_2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_1, dummySubLine2_2 } };
			var dummySubLine2_3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2_1 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_3 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2_1, dummyLine2_2 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("DeleteEntryLineAndSubLine.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareEntryLinesSubLinesWhenDeleted_CurrentListIsNull()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 2, Ingredient = "Woods" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1" };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("DeleteSubLineCurrnetListNull.xml"));
			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestXmlIgnore()
		{
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var dummyData1 = new DummyData() { ProductName = "Bin", Ingredient = "Plastics" };
			var dummyData2 = new DummyData() { ProductName = "Desk", Ingredient = "Wood" };
			var testFile = embeddedResourceRetriever.GetStream(GetEmbeddedResourcePath("XmlIgnore.xml"));
			var result = new DataProviderComparer<IDummyData>().Compare(dummyData1, dummyData2);
			var strActual = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(result);
			using (var streamExpected = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				AssertXMLEquals(streamExpected.WriteToString(), strActual);
			}
		}

		public void TestCompareWhenNumericFieldsAddedWithEmpty()
		{
			var dummyHeader1 = new DummyEntryHeader();

			var dummyLine21 = new DummyEntryLine() { EntryLineNo = 1, };
			var dummyLine22 = new DummyEntryLine() { EntryLineNo = 1, Price = 0 };
			var dummyLine23 = new DummyEntryLine() { EntryLineNo = 1, Price = 10 };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine21, dummyLine22, dummyLine23 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);

			CombineAssertions(() =>
			{
				var subEntitiys = result.SubEntity;
				AssertEquals(3, subEntitiys.Length);

				AssertEquals(EntityAmendType.Add, subEntitiys[0].AmendType);
				AssertEquals(1, subEntitiys[0].Field.Length);
				AssertEquals("EntryLineNo", subEntitiys[0].Field[0].Name);

				AssertEquals(EntityAmendType.Add, subEntitiys[1].AmendType);
				AssertEquals(1, subEntitiys[1].Field.Length);
				AssertEquals("EntryLineNo", subEntitiys[1].Field[0].Name);

				AssertEquals(EntityAmendType.Add, subEntitiys[2].AmendType);
				AssertEquals(2, subEntitiys[2].Field.Length);
				AssertEquals("EntryLineNo", subEntitiys[2].Field[0].Name);
				AssertEquals("Price", subEntitiys[2].Field[1].Name);
				AssertEquals("SYSTEM_NULL_VALUE", subEntitiys[2].Field[1].Before);
				AssertEquals("10", subEntitiys[2].Field[1].After);
			});
		}

		public void TestCompareWhenDateTimeFieldsAddedWithEmpty()
		{
			var dummyHeader1 = new DummyEntryHeader();

			var dummyLine21 = new DummyEntryLine() { EntryLineNo = 1, };
			var dummyLine22 = new DummyEntryLine() { EntryLineNo = 1, ApprovalDate = ZDateTime.Empty };
			var dummyLine23 = new DummyEntryLine() { EntryLineNo = 1, ApprovalDate = ZDateTime.Today };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine21, dummyLine22, dummyLine23 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(dummyHeader1, dummyHeader2);

			CombineAssertions(() =>
			{
				var subEntitiys = result.SubEntity;
				AssertEquals(3, subEntitiys.Length);

				AssertEquals(EntityAmendType.Add, subEntitiys[0].AmendType);
				AssertEquals(1, subEntitiys[0].Field.Length);
				AssertEquals("EntryLineNo", subEntitiys[0].Field[0].Name);

				AssertEquals(EntityAmendType.Add, subEntitiys[1].AmendType);
				AssertEquals(1, subEntitiys[1].Field.Length);
				AssertEquals("EntryLineNo", subEntitiys[1].Field[0].Name);

				AssertEquals(EntityAmendType.Add, subEntitiys[2].AmendType);
				AssertEquals(2, subEntitiys[2].Field.Length);
				AssertEquals("EntryLineNo", subEntitiys[2].Field[0].Name);
				AssertEquals("ApprovalDate", subEntitiys[2].Field[1].Name);
				AssertEquals("SYSTEM_NULL_VALUE", subEntitiys[2].Field[1].Before);
				AssertEquals(ZDateTime.Today.ToString(ZDateTime.LongTimeFormat), subEntitiys[2].Field[1].After);
			});
		}

		public void TestDerivedInterfaceCompare()
		{
			var dummyHeader1 = new DummyDerivedEntryHeader();

			var dummyLine21 = new DummyEntryLine() { EntryLineNo = 1, };
			var dummyLine22 = new DummyEntryLine() { EntryLineNo = 1, ApprovalDate = ZDateTime.Empty };
			var dummyLine23 = new DummyEntryLine() { EntryLineNo = 1, ApprovalDate = ZDateTime.Today };
			var dummyHeader2 = new DummyDerivedEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine21, dummyLine22, dummyLine23 }, EntryStatus = "XXX" };

			var result = new DataProviderComparer<IDummyDerivedEntryHeader>().Compare(dummyHeader1, dummyHeader2);

			CombineAssertions(() =>
			{
				var subEntities = result.SubEntity;
				AssertEquals(3, subEntities.Length);

				AssertEquals(EntityAmendType.Add, subEntities[0].AmendType);
				AssertEquals(1, subEntities[0].Field.Length);
				AssertEquals("EntryLineNo", subEntities[0].Field[0].Name);

				AssertEquals(EntityAmendType.Add, subEntities[1].AmendType);
				AssertEquals(1, subEntities[1].Field.Length);
				AssertEquals("EntryLineNo", subEntities[1].Field[0].Name);

				AssertEquals(EntityAmendType.Add, subEntities[2].AmendType);
				AssertEquals(2, subEntities[2].Field.Length);
				AssertEquals("EntryLineNo", subEntities[2].Field[0].Name);
				AssertEquals("ApprovalDate", subEntities[2].Field[1].Name);
				AssertEquals("SYSTEM_NULL_VALUE", subEntities[2].Field[1].Before);
				AssertEquals(ZDateTime.Today.ToString(ZDateTime.LongTimeFormat), subEntities[2].Field[1].After);

				var fields = result.Field;
				AssertEquals(1, fields.Length);
				AssertEquals("EntryStatus", fields[0].Name);
				AssertEquals("", fields[0].Before);
				AssertEquals("XXX", fields[0].After);
			});
		}

		class DummyData : IDummyData
		{
			public ZString ProductName { get; set; }
			public ZString Ingredient { get; set; }
		}

		interface IDummyData
		{
			[XmlIgnore]
			ZString ProductName { get; }
			ZString Ingredient { get; }
		}

		class DummySubLine : IDummySubLine
		{
			public ZInt SubLineNo { get; set; }
			public ZString Ingredient { get; set; }
		}

		interface IDummySubLine
		{
			[ID()]
			ZInt SubLineNo { get; }
			ZString Ingredient { get; }
		}

		class DummyEntryLine : IDummyEntryLine
		{
			public ZInt EntryLineNo { get; set; }
			public ZString CountryOfOrigin { get; set; }
			public ZString HSCode { get; set; }
			public ZDecimal Price { get; set; }
			public ZDateTime ApprovalDate { get; set; }
			public IDummyOrganization Supplier { get; set; }
			public IEnumerable<IDummySubLine> SubLines { get; set; }
		}

		interface IDummyEntryLine
		{
			[ID()]
			ZInt EntryLineNo { get; }
			ZString CountryOfOrigin { get; }
			ZString HSCode { get; }
			ZDecimal Price { get; }
			ZDateTime ApprovalDate { get; }
			IDummyOrganization Supplier { get; }
			IEnumerable<IDummySubLine> SubLines { get; }
		}

		class DummyOrganization : IDummyOrganization
		{
			public RoleType Role { get; set; }
			public ZString CompanyName { get; set; }
			public ZString RepresentativeName { get; set; }
			public ZString AddressLine { get; set; }
		}

		interface IDummyOrganization
		{
			[ID()]
			RoleType Role { get; }
			ZString CompanyName { get; }
			ZString RepresentativeName { get; }
			ZString AddressLine { get; }
		}

		class DummyEntryHeader : IDummyEntryHeader
		{
			public ZString EntryNumber { get; set; }
			public ZDateTime LoadingDate { get; set; }
			public ZInt PackageCount { get; set; }
			public ZDecimal TotalValue { get; set; }
			public ZBool IsPersonalItem { get; set; }
			public IDummyOrganization Declarant { get; set; }
			public IDummyOrganization Supplier { get; set; }
			public IDummyOrganization Manufacturer { get; set; }
			public IEnumerable<IDummyEntryLine> EntryLines { get; set; }
		}

		interface IDummyEntryHeader
		{
			ZString EntryNumber { get; }
			ZDateTime LoadingDate { get; }
			ZInt PackageCount { get; }
			ZDecimal TotalValue { get; }
			ZBool IsPersonalItem { get; }
			IDummyOrganization Declarant { get; }
			IDummyOrganization Supplier { get; }
			IDummyOrganization Manufacturer { get; }
			IEnumerable<IDummyEntryLine> EntryLines { get; }
		}

		class DummyDerivedEntryHeader : DummyEntryHeader, IDummyDerivedEntryHeader
		{
			public ZString EntryStatus { get; set; }
		}

		interface IDummyDerivedEntryHeader : IDummyEntryHeader
		{
			ZString EntryStatus { get; }
		}

		enum RoleType
		{
			None,
			Buyer,
			Declarant,
			Exporter,
			Importer,
			Manufacturer,
			Payer,
			Seller,
			Supplier
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.Business.Testing.MessageManager.AccumulativeAmendment.TestFiles." + fileName;
	}
}
