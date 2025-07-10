using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class CodeListDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override bool GenerateFiles(DateTime publicationDate, ref Errors error)
		{
			var result = CodeTypeGenerator?.GenerateFiles(publicationDate,ref error) ?? true;
			foreach (var attributeNameGenerator in AttributeNameGenerators)
			{
				result = result && attributeNameGenerator.GenerateFiles(publicationDate, ref error);
			}
			return result && base.GenerateFiles(publicationDate, ref error);
		}

		protected sealed override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true); 
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			if (GetStartDateFromInputFile)
			{
				codeList.IncludeColumn(x => x.ZZD_StartDate, false);
			}
			else
			{
				codeList.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, UniversalDataHelper.MinimumDateTime);
			}
			if (GetEndDateFromInputFile)
			{
				codeList.IncludeColumn(x => x.ZZD_EndDate, false);
			}
			else
			{
				codeList.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, UniversalDataHelper.MaximumDateTime);
			}
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, UniversalDataHelper.MaximumDateTime);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, CodeType);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, DataGrouping);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			if (HasAttributes)
			{
				codeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
				var codeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				codeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				codeListAttribute.IncludeColumn(x => x.ZZE_Value, true);
				xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeListAttribute);
			}
			return xmlWriterConfiguration;
		}

		protected virtual RefCusCodeTypeGenerator CodeTypeGenerator => null;

		protected virtual bool HasAttributes => false;

		protected virtual BaseCodeListAttributeNameGenerator[] AttributeNameGenerators => Array.Empty<BaseCodeListAttributeNameGenerator>();

		protected override Dependency[] GetDependencies(DateTime publicationTime)
		{
			var result = base.GetDependencies(publicationTime).ToList();
			if (CodeTypeGenerator != null)
			{
				result.Add(new Dependency(CodeTypeGenerator.DataSource, publicationTime, DependencyType.Required));
			}
			foreach (var attributeNameGenerator in AttributeNameGenerators)
			{
				result.Add(new Dependency(attributeNameGenerator.DataSource, publicationTime, DependencyType.Required));
			}
			return result.ToArray();
		}

		public sealed override string DataSource => $"{DataGrouping} - {CodeType} Code Lists";

		protected abstract string DataGrouping { get; }
		protected abstract string CodeType { get; }
		protected abstract IEnumerable<string> InputFileNames { get; }
		protected virtual bool GetStartDateFromInputFile => false;
		protected virtual bool GetEndDateFromInputFile => false;
	}
}
