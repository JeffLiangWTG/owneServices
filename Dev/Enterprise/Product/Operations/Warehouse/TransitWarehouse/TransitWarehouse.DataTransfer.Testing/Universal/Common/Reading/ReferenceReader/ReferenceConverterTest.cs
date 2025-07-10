using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ReferenceConverterTest : IntegrationTestCaseWithFactory
	{
		[TestDate(2021, 05, 10)]
		public void TestConvertReferences_NoMappers()
		{
			var now = ZDateTime.Now;

			var prtRef = Helper.CreatePortReference("ABC", "ABC Desc", "AU", "ABC Ref", "CLR");
			var entryRef = Helper.CreateEntryReference("ABC", "ABC Desc", "ABC CUS Ref", "Ref Details", true, "Entry Status", ZDateTime.Now, ZDateTime.Now, "AU");
			var adtRef = Helper.CreateAdditionalReference("XYZ", "XYZ Desc", "Reference", "Context Info", now.AddDays(-1));

			var portReferences = new[] { prtRef };
			var entryReferences = new[] { entryRef };
			var adtReferences = new[] { adtRef };
			var converter = new ReferenceConverter(portReferences, adtReferences, entryReferences);
			var emptyMapper = converter.ConvertReferences(System.Array.Empty<ReferenceTypeMappingInfo>(), "");
			AssertEquals(1, emptyMapper.portReferences.Count);
			AssertEquals(1, emptyMapper.entryNumbers.Count);
			AssertEquals(1, emptyMapper.additionalReferences.Count);
			var actualPRTRef = emptyMapper.portReferences.Single();
			var actualOHTRef = emptyMapper.additionalReferences.Single();
			var actualCUSRef = emptyMapper.entryNumbers.Single().EntryNumber;
			Helper.AssertPortReference(actualPRTRef, prtRef.Type.Code, prtRef.Reference.Value, prtRef.Status.Code.Value, prtRef.Country.Code.Value);
			Helper.AssertAdditionalReference(actualOHTRef, actualOHTRef.Type.Code.Value, actualOHTRef.ReferenceNumber.Value, actualOHTRef.ContextInformation.Value, actualOHTRef.IssueDate.Value);
			Helper.AssertEntryReference(actualCUSRef, entryRef.Type.Code, entryRef.Number.Value, entryRef.EntryLineReference.Value, entryRef.EntryStatus.Code.Value, entryRef.EntryIsSystemGenerated.Value, entryRef.CountryOfIssue.Code.Value, entryRef.IssueDate.GetValueOrDefault(), entryRef.ExpiryDate.GetValueOrDefault());
		}

		[TestDate(2021, 05, 10)]
		public void TestConvertReferences_MappedToSameCategory()
		{
			var now = ZDateTime.Now;

			var mappingToPRT = CreateMapping(TransitWarehouseReferenceCategories.Codes.PortReference, "ABC", TransitWarehouseReferenceCategories.Codes.PortReference, "DEF");
			var mappingToCUS = CreateMapping(TransitWarehouseReferenceCategories.Codes.CustomsReference, "XYZ", TransitWarehouseReferenceCategories.Codes.CustomsReference, "GHI");
			var mappingToOTH = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, "SDF", TransitWarehouseReferenceCategories.Codes.AdditionalReference, "TES");

			var prtToPRT = Helper.CreatePortReference("ABC", "AAA Desc", "NZ", "AAA Ref", "BBB");
			var cusToCUS = Helper.CreateEntryReference("XYZ", "ABC Desc", "ABC CUS Ref", "Ref Details", true, "Entry Status", ZDateTime.Now, ZDateTime.Now, "AU");
			var othToOTH = Helper.CreateAdditionalReference("SDF", "XYZ Desc", "Reference", "Context Info", now.AddDays(-1));

			var portReferences = new[] { prtToPRT };
			var entryNumbers = new[] { cusToCUS };
			var adtReferences = new[] { othToOTH };
			var converter = new ReferenceConverter(portReferences, adtReferences, entryNumbers);
			var mapper = converter.ConvertReferences(new ReferenceTypeMappingInfo[] { mappingToPRT, mappingToCUS, mappingToOTH }, "");
			AssertEquals(1, mapper.portReferences.Count);
			AssertEquals(1, mapper.entryNumbers.Count);
			AssertEquals(1, mapper.additionalReferences.Count);
			Helper.AssertPortReference(mapper.portReferences.Single(), "DEF", prtToPRT.Reference.Value, prtToPRT.Status.Code.Value, prtToPRT.Country.Code.Value);
			Helper.AssertEntryReference(mapper.entryNumbers.Single().EntryNumber, "GHI", cusToCUS.Number.Value, cusToCUS.EntryLineReference.Value, cusToCUS.EntryStatus.Code.Value, cusToCUS.EntryIsSystemGenerated.Value, cusToCUS.CountryOfIssue.Code.Value, cusToCUS.IssueDate.GetValueOrDefault(), cusToCUS.ExpiryDate.GetValueOrDefault());
			Helper.AssertAdditionalReference(mapper.additionalReferences.Single(), "TES", othToOTH.ReferenceNumber.Value, othToOTH.ContextInformation.Value, othToOTH.IssueDate.Value);
		}

		[TestDate(2021, 05, 10)]
		public void TestConvertPortReferences()
		{
			var now = ZDateTime.Now;

			var portReferenceToOTH = CreateMapping(TransitWarehouseReferenceCategories.Codes.PortReference, "ABC", TransitWarehouseReferenceCategories.Codes.AdditionalReference, "DEF");
			var portReferenceToCUS = CreateMapping(TransitWarehouseReferenceCategories.Codes.PortReference, "XYZ", TransitWarehouseReferenceCategories.Codes.CustomsReference, "GHI");

			var mappedToOTH = Helper.CreatePortReference("ABC", "ABC Desc", "AU", "ABC Ref", "CLR");
			var mappedToCUS = Helper.CreatePortReference("XYZ", "XYZ Desc", "NZ", "XYZ Ref", "HLD");

			var portReferences = new PortReference[] { mappedToOTH, mappedToCUS };
			var entryNumbers = System.Array.Empty<EntryNumber>();
			var adtReferences = System.Array.Empty<AdditionalReference>();
			var converter = new ReferenceConverter(portReferences, adtReferences, entryNumbers);
			var mapper = converter.ConvertReferences(new ReferenceTypeMappingInfo[] { portReferenceToOTH, portReferenceToCUS }, "");
			AssertEquals(0, mapper.portReferences.Count);
			AssertEquals(1, mapper.entryNumbers.Count);
			AssertEquals(1, mapper.additionalReferences.Count);
			Helper.AssertEntryReference(mapper.entryNumbers.Single().EntryNumber, "GHI", mappedToCUS.Reference.Value, "", mappedToCUS.Status.Code, false, mappedToCUS.Country.Code, ZDateTime.Empty, ZDateTime.Empty);
			Helper.AssertAdditionalReference(mapper.additionalReferences.Single(p => p.Type.Code.Value == "DEF"), "DEF", mappedToOTH.Reference.Value, "", ZDateTime.Empty);
		}

		[TestDate(2021, 05, 10)]
		public void TestConvertCustomsReferences()
		{
			var now = ZDateTime.Now;

			var cusToOTH = CreateMapping(TransitWarehouseReferenceCategories.Codes.CustomsReference, "ABC", TransitWarehouseReferenceCategories.Codes.AdditionalReference, "DEF");
			var cusToPRT = CreateMapping(TransitWarehouseReferenceCategories.Codes.CustomsReference, "XYZ", TransitWarehouseReferenceCategories.Codes.PortReference, "GHI");

			var mappedToPRT = Helper.CreateEntryReference("XYZ", "XYZ Desc", "XYZ CUS Ref", "XYZ Details", true, "Entry Status", ZDateTime.Now, ZDateTime.Now, "AU");
			var mappedToOTH = Helper.CreateEntryReference("ABC", "ABC Desc", "ABC CUS Ref", "Ref Details", true, "E1", ZDateTime.Now, ZDateTime.Now, "AU");

			var portReferences = System.Array.Empty<PortReference>();
			var entryReferences = new[] { mappedToOTH, mappedToPRT };
			var adtReferences = System.Array.Empty<AdditionalReference>();
			var converter = new ReferenceConverter(portReferences, adtReferences, entryReferences);
			var mapper = converter.ConvertReferences(new ReferenceTypeMappingInfo[] { cusToOTH, cusToPRT }, "");
			AssertEquals(1, mapper.portReferences.Count);
			AssertEquals(0, mapper.entryNumbers.Count);
			AssertEquals(1, mapper.additionalReferences.Count);
			Helper.AssertPortReference(mapper.portReferences.Single(p => p.Type.Code.Value == "GHI"), "GHI", mappedToPRT.Number.Value, mappedToPRT.EntryStatus.Code.Value, mappedToPRT.CountryOfIssue.Code.Value);
			Helper.AssertAdditionalReference(mapper.additionalReferences.Single(p => p.Type.Code.Value == "DEF"), "DEF", mappedToOTH.Number.Value, "", mappedToOTH.IssueDate.Value);
		}

		[TestDate(2021, 05, 10)]
		public void TestMappingSourceType()
		{
			var now = ZDateTime.Now;
			var othToCEN = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, "ABC", TransitWarehouseReferenceCategories.Codes.CustomsReference, "CEN", "IMP");
			var othToCRN = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, "DEF", TransitWarehouseReferenceCategories.Codes.CustomsReference, "CRN", "EXP");
			var othToXYZ = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, "GHI", TransitWarehouseReferenceCategories.Codes.CustomsReference, "XYZ");

			var mappedToCEN = Helper.CreateAdditionalReference("ABC", "CEN Desc", "ReferenceCEN", "Context Info", now.AddDays(-1));
			var mappedToCRN = Helper.CreateAdditionalReference("DEF", "CRN Desc", "ReferenceCRN", "Context Info", now.AddDays(-1));
			var mappedToXYZ = Helper.CreateAdditionalReference("GHI", "XYZ Desc", "ReferenceXYZ", "Context Info", now.AddDays(-1));

			var portReferences = System.Array.Empty<PortReference>();
			var entryReferences = System.Array.Empty<EntryNumber>();
			var adtReferences = new AdditionalReference[] { mappedToCEN, mappedToCRN, mappedToXYZ };
			var converter = new ReferenceConverter(portReferences, adtReferences, entryReferences);
			var mapper = converter.ConvertReferences(new ReferenceTypeMappingInfo[] { othToCEN, othToCRN, othToXYZ }, "EXP");
			AssertEquals(0, mapper.portReferences.Count);
			AssertEquals(1, mapper.additionalReferences.Count);
			AssertEquals(2, mapper.entryNumbers.Count);

			Helper.AssertAdditionalReference(mapper.additionalReferences.Single(p => p.Type.Code.Value == "ABC"), "ABC", mappedToCEN.ReferenceNumber.Value, "Context Info", mappedToCEN.IssueDate.Value);
			Helper.AssertEntryReference(mapper.entryNumbers.Single(p => p.EntryNumber.Type.Code.Value == "CRN").EntryNumber, "CRN", mappedToCRN.ReferenceNumber.Value, "", "", false, "", mappedToCRN.IssueDate.GetValueOrDefault(), ZDateTime.Empty);
			AssertEquals(mapper.entryNumbers.Single(p => p.EntryNumber.Type.Code.Value == "CRN").SourceType, "");
			Helper.AssertEntryReference(mapper.entryNumbers.Single(p => p.EntryNumber.Type.Code.Value == "XYZ").EntryNumber, "XYZ", mappedToXYZ.ReferenceNumber.Value, "", "", false, "", mappedToXYZ.IssueDate.GetValueOrDefault(), ZDateTime.Empty);
			AssertEquals(mapper.entryNumbers.Single(p => p.EntryNumber.Type.Code.Value == "XYZ").SourceType, "");
		}

		[TestDate(2021, 05, 10)]
		public void TestConvertOtherReferences()
		{
			var now = ZDateTime.Now;

			var othToPRT = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, "ABC", TransitWarehouseReferenceCategories.Codes.PortReference, "DEF");
			var othToCUS = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, "XYZ", TransitWarehouseReferenceCategories.Codes.CustomsReference, "GHI");

			var mappedToPRT = Helper.CreateAdditionalReference("ABC", "ABC Desc", "ABC Reference", "ABC Context Info", now.AddDays(-1));
			var mappedToCUS = Helper.CreateAdditionalReference("XYZ", "XYZ Desc", "XYZ Reference", "XYZ Context Info", now.AddDays(-2));

			var portReferences = System.Array.Empty<PortReference>();
			var entryReferences = System.Array.Empty<EntryNumber>();
			var adtReferences = new[] { mappedToPRT, mappedToCUS };
			var converter = new ReferenceConverter(portReferences, adtReferences, entryReferences);
			var mapper = converter.ConvertReferences(new ReferenceTypeMappingInfo[] { othToPRT, othToCUS }, "");
			AssertEquals(1, mapper.portReferences.Count);
			AssertEquals(1, mapper.entryNumbers.Count);
			AssertEquals(0, mapper.additionalReferences.Count);
			Helper.AssertPortReference(mapper.portReferences.Single(p => p.Type.Code.Value == "DEF"), "DEF", mappedToPRT.ReferenceNumber.Value, "", "");
			Helper.AssertEntryReference(mapper.entryNumbers.Single().EntryNumber, "GHI", mappedToCUS.ReferenceNumber.Value, "", "", false, "", mappedToCUS.IssueDate.GetValueOrDefault(), ZDateTime.Empty);
		}

		ReferenceTypeMappingInfo CreateMapping(string fromCategory, string fromType, string toCategory, string toType, string direction = "")
		{
			return new ReferenceTypeMappingInfo
			{
				FromReferenceCategory = fromCategory,
				FromReferenceType = fromType,
				ToReferenceCategory = toCategory,
				ToReferenceType = toType,
				Direction = direction
			};
		}

		[TestDate(2021, 05, 10)]
		public void TestConvertReferences_AddNewCondition_FromReferenceType()
		{
			var now = ZDateTime.Now;
			var othToCRN1 = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, WarehouseAdditionalReferenceTypes.Codes.T1, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var othToCRN2 = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, WarehouseAdditionalReferenceTypes.Codes.T2, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var othToCRN3 = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, WarehouseAdditionalReferenceTypes.Codes.T2L, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			var othToCRN4 = CreateMapping(TransitWarehouseReferenceCategories.Codes.AdditionalReference, WarehouseAdditionalReferenceTypes.Codes.TransportMode, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);

			var mappedToCRN1 = Helper.CreateAdditionalReference(WarehouseAdditionalReferenceTypes.Codes.T1, "CRN Desc", "ReferenceCRN", "Context Info", now.AddDays(-1));
			var mappedToCRN2 = Helper.CreateAdditionalReference(WarehouseAdditionalReferenceTypes.Codes.T2, "CRN Desc", "ReferenceCRN", "Context Info", now.AddDays(-1));
			var mappedToCRN3 = Helper.CreateAdditionalReference(WarehouseAdditionalReferenceTypes.Codes.T2L, "CRN Desc", "ReferenceCRN", "Context Info", now.AddDays(-1));
			var mappedToCRN4 = Helper.CreateAdditionalReference(WarehouseAdditionalReferenceTypes.Codes.TransportMode, "CRN Desc", "ReferenceCRN", "Context Info", now.AddDays(-1));

			var portReferences = System.Array.Empty<PortReference>();
			var entryReferences = System.Array.Empty<EntryNumber>();
			var adtReferences = new AdditionalReference[] { mappedToCRN1, mappedToCRN2, mappedToCRN3, mappedToCRN4 };
			var converter = new ReferenceConverter(portReferences, adtReferences, entryReferences);
			var mapper = converter.ConvertReferences(new ReferenceTypeMappingInfo[] { othToCRN1, othToCRN2, othToCRN3, othToCRN4 }, "");

			AssertEquals(1, mapper.entryNumbers.Count(item => item.SourceType == WarehouseAdditionalReferenceTypes.Codes.T1));
			AssertEquals(1, mapper.entryNumbers.Count(item => item.SourceType == WarehouseAdditionalReferenceTypes.Codes.T2));
			AssertEquals(1, mapper.entryNumbers.Count(item => item.SourceType == WarehouseAdditionalReferenceTypes.Codes.T2L));
			AssertEquals(0, mapper.entryNumbers.Count(item => item.SourceType == WarehouseAdditionalReferenceTypes.Codes.TransportMode));
		}
	}
}
