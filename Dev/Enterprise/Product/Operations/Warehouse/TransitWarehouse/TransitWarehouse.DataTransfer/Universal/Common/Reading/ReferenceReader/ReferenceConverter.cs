using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class ReferenceConverter
	{
		public ReferenceConverter(IEnumerable<PortReference> portReferencesToConvert,
			IEnumerable<AdditionalReference> additionalReferencesToConvert,
			IEnumerable<EntryNumber> entryNumbersToConvert)
		{
			AllReferences = new List<ReferenceElement>();
			ConvertPortReferences(portReferencesToConvert ?? System.Array.Empty<PortReference>());
			ConvertAdditionalReferences(additionalReferencesToConvert ?? System.Array.Empty<AdditionalReference>());
			ConvertEntryNumbersReferences(entryNumbersToConvert ?? System.Array.Empty<EntryNumber>());
		}
		readonly List<ReferenceElement> AllReferences;

		void ConvertPortReferences(IEnumerable<PortReference> portReferences)
		{
			foreach (var reference in portReferences)
			{
				AllReferences.Add(BuildReferenceElementFromPortReferences(reference));
			}
		}

		void ConvertEntryNumbersReferences(IEnumerable<EntryNumber> entryNumbers)
		{
			foreach (var entryNumber in entryNumbers)
			{
				AllReferences.Add(BuildReferenceElementFromEntryNumber(entryNumber));
			}
		}

		void ConvertAdditionalReferences(IEnumerable<AdditionalReference> additionalReferences)
		{
			foreach (var reference in additionalReferences)
			{
				AllReferences.Add(BuildReferenceElementFromAdditionalReferences(reference));
			}
		}

		ReferenceElement BuildReferenceElementFromPortReferences(PortReference portReference)
		{
			return new ReferenceElement
			{
				Category = TransitWarehouseReferenceCategories.Codes.PortReference,
				Reference = portReference.Reference.GetValueOrDefault(),
				Type = portReference.Type?.Code.GetValueOrDefault() ?? ZString.Empty,
				Status = portReference.Status?.Code.GetValueOrDefault() ?? ZString.Empty,
				CountryCode = portReference.Country?.Code.GetValueOrDefault() ?? ZString.Empty,
			};
		}

		ReferenceElement BuildReferenceElementFromAdditionalReferences(AdditionalReference additionalReference)
		{
			return new ReferenceElement
			{
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Reference = additionalReference.ReferenceNumber.GetValueOrDefault(),
				Type = additionalReference.Type?.Code.GetValueOrDefault() ?? ZString.Empty,
				IssueDate = additionalReference.IssueDate.GetValueOrDefault(),
				ContextInformation = additionalReference.ContextInformation.GetValueOrDefault(),
				CountryCode = additionalReference.CountryOfIssue?.Code.GetValueOrDefault() ?? ZString.Empty
			};
		}

		ReferenceElement BuildReferenceElementFromEntryNumber(EntryNumber entryNumber)
		{
			return new ReferenceElement
			{
				Category = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				Type = entryNumber.Type?.Code.GetValueOrDefault() ?? ZString.Empty,
				Reference = entryNumber.Number.GetValueOrDefault(),
				Status = entryNumber.EntryStatus?.Code.GetValueOrDefault() ?? ZString.Empty,
				CountryCode = entryNumber.CountryOfIssue?.Code.GetValueOrDefault() ?? ZString.Empty,
				IssueDate = entryNumber.IssueDate.GetValueOrDefault(),
				ExpiryDate = entryNumber.ExpiryDate.GetValueOrDefault(),
				EntryIsSystemGenerated = entryNumber.EntryIsSystemGenerated.GetValueOrDefault(),
				EntryLineReference = entryNumber.EntryLineReference.GetValueOrDefault(),
			};
		}

		public (List<PortReference> portReferences,
			List<AdditionalReference> additionalReferences,
			List<(ZString SourceType, EntryNumber EntryNumber)> entryNumbers)
			ConvertReferences(IEnumerable<ReferenceTypeMappingInfo> mappingInfos, ZString direction)
		{
			var portReferences = new List<PortReference>();
			var additionalReferences = new List<AdditionalReference>();
			var entryNumberReferences = new List<(ZString SourceType, EntryNumber EntryNumber)>();
			foreach (var reference in AllReferences)
			{
				var matchingMappingElement =
					mappingInfos.FirstOrDefault(m => m.FromReferenceCategory == reference.Category && m.FromReferenceType == reference.Type && m.Direction == direction) ??
					mappingInfos.FirstOrDefault(m => m.FromReferenceCategory == reference.Category && m.FromReferenceType == reference.Type && string.IsNullOrEmpty(m.Direction));
				if (matchingMappingElement != null)
				{
					reference.Category = matchingMappingElement.ToReferenceCategory;
					reference.Type = matchingMappingElement.ToReferenceType;
					if ((matchingMappingElement.ToReferenceType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber || matchingMappingElement.ToReferenceType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber) && (matchingMappingElement.FromReferenceType == WarehouseAdditionalReferenceTypes.Codes.T1 || matchingMappingElement.FromReferenceType == WarehouseAdditionalReferenceTypes.Codes.T2 || matchingMappingElement.FromReferenceType == WarehouseAdditionalReferenceTypes.Codes.T2L))
					{
						reference.SourceType = matchingMappingElement.FromReferenceType;
					}
				}

				AddToRelevantReferenceCollection(reference, portReferences, additionalReferences, entryNumberReferences);
			}

			return (portReferences, additionalReferences, entryNumberReferences);
		}

		void AddToRelevantReferenceCollection(ReferenceElement reference, List<PortReference> portReferences, List<AdditionalReference> additionalReferences, List<(ZString SourceType, EntryNumber EntryNumber)> entryNumbers)
		{
			if (reference.Category == TransitWarehouseReferenceCategories.Codes.PortReference)
			{
				portReferences.Add(CreatePortReference(reference));
			}
			else if (reference.Category == TransitWarehouseReferenceCategories.Codes.AdditionalReference)
			{
				additionalReferences.Add(CreateAdditionalReference(reference));
			}
			else
			{
				entryNumbers.Add((reference.SourceType, CreateEntryNumber(reference)));
			}
		}

		PortReference CreatePortReference(ReferenceElement element)
		{
			return new PortReference
			{
				Type = new PortReferenceType { Code = element.Type },
				Reference = element.Reference,
				Country = new Country { Code = element.CountryCode },
				Status = new PortReferenceStatus { Code = element.Status }
			};
		}

		AdditionalReference CreateAdditionalReference(ReferenceElement element)
		{
			return new AdditionalReference
			{
				Type = new EntryType { Code = element.Type },
				ReferenceNumber = element.Reference,
				ContextInformation = element.ContextInformation,
				IssueDate = element.IssueDate,
				CountryOfIssue = new Country { Code = element.CountryCode }
			};
		}

		EntryNumber CreateEntryNumber(ReferenceElement element)
		{
			return new EntryNumber
			{
				Type = new EntryType { Code = element.Type },
				Number = element.Reference,
				EntryLineReference = element.EntryLineReference,
				EntryStatus = new EntryStatus { Code = element.Status },
				EntryIsSystemGenerated = element.EntryIsSystemGenerated,
				CountryOfIssue = new Country { Code = element.CountryCode },
				ExpiryDate = element.ExpiryDate,
				IssueDate = element.IssueDate
			};
		}
	}
}
