using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class CustomsOfficeCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public CustomsOfficeCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.CustomsOffice.Code;

		protected override bool HasAttributes => true;

		protected override XmlWriterConfiguration GetRefCusCodeListWriterConfiguration()
		{
			return Helper.GetRefCusCodeListWriterConfiguration(CodeType, HasAttributes, true);
		}

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var offices = xml.Root.Descendants(CustomsOfficeCodeListContants.TagCustomsOffice);

			foreach (XElement element in offices)
			{
				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = element.GetElementValueAsString(CustomsOfficeCodeListContants.TagCustomsOfficeCode, 35),
					ZZD_Description = element.GetElementValueAsString(CustomsOfficeCodeListContants.TagCustomsOfficeDescription, 2000)
				};

				refCusCodeList.ZZD_StartDate = element.GetElementValueAsDateTime(CustomsOfficeCodeListContants.TagCustomsOfficeStartDate);

				var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();

				var refCusCodeListAttributeUnloco = new RefCusCodeListAttribute
				{
					ZZE_Value = getCategoria(element.GetElementValueAsString(CustomsOfficeCodeListContants.TagAttributePortValue, 32)),
					ZZE_ZXE_NKName = CustomsOfficeCodeListContants.TagAttributePort
				};
				addIfValueIsNotNull(refCusCodeListAttributeUnloco, refCusCodeListAttributes);

				var refCusCodeListAttributeShitType = new RefCusCodeListAttribute
				{
					ZZE_Value = CustomsOfficeCodeListContants.TagAttributeExportDirectionValue,
					ZZE_ZXE_NKName = CustomsOfficeCodeListContants.TagAttributeDirection
				};
				addIfValueIsNotNull(refCusCodeListAttributeShitType, refCusCodeListAttributes);

				var refCusCodeListAttributeImportShitType = new RefCusCodeListAttribute
				{
					ZZE_Value = CustomsOfficeCodeListContants.TagAttributeImportDirectionValue,
					ZZE_ZXE_NKName = CustomsOfficeCodeListContants.TagAttributeDirection
				};
				addIfValueIsNotNull(refCusCodeListAttributeImportShitType, refCusCodeListAttributes);

				refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
				result.Add(refCusCodeList);
			}
			return result;
		}

		static string getCategoria(string value)
		{
			string prefix = CustomsOfficeCodeListContants.CategoriaPrefix;
			if (value == null)
			{
				return "";
			}
			else if (value.Contains("/"))
			{
				return prefix + value.Split('/')[1];
			}
			return "";
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.CustomsOffice.Description,
				ZZK_MaxLength = 7
			};
		}
	}
}
