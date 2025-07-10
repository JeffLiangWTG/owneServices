using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services;

public abstract class BaseCodeListAttributeNameGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeListAttributeName>
{
	public override string OutputFile => $"{DataSource}.xml";

	public sealed override string DataSource => $"{DataGrouping} {CodeType} Attribute Name";

	protected override List<RefCusCodeListAttributeName> GetDataCollection(DateTime publicationDate, ref Errors error)
	{
		var result = new List<RefCusCodeListAttributeName>();
		foreach (var attribute in Attributes)
		{
			result.Add(new RefCusCodeListAttributeName
			{
				ZXE_Name = attribute.Name,
				ZXE_Description = attribute.Description,
				ZXE_ValueDataType = attribute.DataType,
				ZXE_IsMandatory = attribute.IsMandatory,
				ZXE_AllowDuplicates = attribute.AllowDuplicates,
				ZXE_IsValueMandatory = attribute.IsValueMandatory,
			});
		}
		return result;
	}

	protected override XmlWriterConfiguration GetXmlWriterConfiguration()
	{
		var xmlWriterConfiguration = new XmlWriterConfiguration();

		var codeType = new EntityTypeConfiguration<RefCusCodeListAttributeName>(true);
		codeType.IncludeColumn(x => x.ZXE_Name, true);
		codeType.IncludeColumnWithConstantValue(x => x.ZXE_ZZK_NKCodeType, true, CodeType);
		codeType.IncludeColumn(x => x.ZXE_Description, false);
		codeType.IncludeColumn(x => x.ZXE_ValueDataType, false);
		codeType.IncludeColumn(x => x.ZXE_IsMandatory, false);
		codeType.IncludeColumn(x => x.ZXE_AllowDuplicates, false);
		codeType.IncludeColumn(x => x.ZXE_IsValueMandatory, false);
		codeType.IncludeColumnWithConstantValue(x => x.ZXE_ZZZ_NKDataGrouping, true, DataGrouping);
		xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeType);

		return xmlWriterConfiguration;
	}

	internal protected abstract string DataGrouping { get; }

	internal protected abstract string CodeType { get; }

	protected abstract (string Name, string Description, string DataType, bool IsMandatory, bool AllowDuplicates, bool IsValueMandatory)[] Attributes { get; }
}
