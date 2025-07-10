using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class PisCofinsLegalBasisCodeListParser : BaseRefCusCodeListParser<Stream>
	{

		public PisCofinsLegalBasisCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.PisCofinsLegalBasis.Code;

		protected override bool HasAttributes => true;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var customsPisCofins = xml.Root.Descendants(PisCofinsLegalBasisCodeListConstants.TagPisCofins);

			foreach (XElement element in customsPisCofins)
			{
				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = element.GetElementValueAsString(PisCofinsLegalBasisCodeListConstants.TagPisCofinsCode, 35),
					ZZD_Description = element.GetElementValueAsString(PisCofinsLegalBasisCodeListConstants.TagPisCofinsDescription, 2000)
				};

				refCusCodeList.ZZD_StartDate = element.GetElementValueAsDateTime(PisCofinsLegalBasisCodeListConstants.TagPisCofinsStartDate);

				var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
				var attributeElement = element.GetElement(PisCofinsLegalBasisCodeListConstants.TagPisCofinsAttribute);

				var refCusCodeListAttribute = new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.PisCofinsLegalBase.Code,
					ZZE_Value = attributeElement.GetElementValueAsString(PisCofinsLegalBasisCodeListConstants.TagPisCofinsAttributeCode, 255)
				};

				addIfValueIsNotNull(refCusCodeListAttribute, refCusCodeListAttributes);
				refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
				result.Add(refCusCodeList);
			}
			return result;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.PisCofinsLegalBasis.Description
			};
		}

		protected override IEnumerable<RefCusCodeListAttributeName> GetRefCusCodeListAttributeNames()
		{
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.PisCofinsLegalBase.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.PisCofinsLegalBase.Description,
				ZXE_ZZK_NKCodeType = Constants.RefCusCodeTypes.PisCofinsLegalBasis.Code,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = true,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.PisCofinsLegalBase.Description
			};
		}
	}
}
