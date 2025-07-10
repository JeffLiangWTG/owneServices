using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Names = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListAttributeNames;
using Values = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListAttributeValues;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	static class RefCusCodeListMapper
	{
		public static RefCusCodeList MapFromRawSupportingDocument(string codeType, IRawSupportingDocument rawSupportingDocument, bool mapAttributes = true, bool alwaysIncludeReferenceNumberAttributeIfPresent = false)
		{
			return new RefCusCodeList
			{
				ZZD_Code = rawSupportingDocument.Code,
				ZZD_Description = rawSupportingDocument.Description,
				ZZD_StartDate = rawSupportingDocument.StartDate.GetDateOrFallbackToMinSmallDateTime(),
				ZZD_EndDate = rawSupportingDocument.EndDate.GetDateOrFallbackToMaxSmallDateTime(),
				ZZD_ZZK_NKCodeType = codeType,
				RefCusCodeListAttributes = mapAttributes ? GetRefCusCodeListAttributes(rawSupportingDocument, alwaysIncludeReferenceNumberAttributeIfPresent) : Array.Empty<RefCusCodeListAttribute>()
			};
		}

		static RefCusCodeListAttribute[] GetRefCusCodeListAttributes(IRawSupportingDocument rawSupportingDocument, bool alwaysIncludeReferenceNumberAttributeIfPresent)
		{
			var attributes = new (bool IncludeAttribute, (string Name, string Value) Attribute)[]
			{
				(rawSupportingDocument.IsRetroActiveRequired, (Names.Retroactive, Values.Yes)),
				(rawSupportingDocument.IsYearRequired, (Names.Year, Values.Yes)),
				(rawSupportingDocument.IsCountryRequired, (Names.Country, Values.Yes)),
				(rawSupportingDocument.IsCertificateIdRequired && (rawSupportingDocument.Type == SupportingDocumentType.National || alwaysIncludeReferenceNumberAttributeIfPresent), (Names.ReferenceNumber, Values.Yes)),
				(rawSupportingDocument.IsQuantityRequired, (Names.Quantity, Values.Yes)),
				(rawSupportingDocument.IsUnitOfQuantityRequired, (Names.UnitOfQuantity, Values.Yes)),
				(rawSupportingDocument.IsElectronicFolderRequired, (Names.ElectronicFolder, Values.Yes)),
				(!string.IsNullOrWhiteSpace(rawSupportingDocument.ElectronicFolderNote), (Names.ElectronicFolderNote, rawSupportingDocument.ElectronicFolderNote)),
				(rawSupportingDocument.IsPaperFolderRequired, (Names.PaperFolder, Values.Yes)),
				(!string.IsNullOrWhiteSpace(rawSupportingDocument.PaperFolderNote), (Names.PaperFolderNote, rawSupportingDocument.PaperFolderNote))
			}
			.Where(x => x.IncludeAttribute)
			.Select(x =>
				new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = x.Attribute.Name,
					ZZE_Value = x.Attribute.Value
				})
			.ToArray();

			return attributes;
		}
	}
}
