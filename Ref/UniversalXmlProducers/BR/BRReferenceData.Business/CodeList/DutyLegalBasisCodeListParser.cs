using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class DutyLegalBasisCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public DutyLegalBasisCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.DutyLegalBasis.Code;

		protected override bool HasAttributes => true;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Contract.Assume(dataSource != null);

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var xmlList = xml.Root?.Descendants("FundamentoLegalRegimeTributacaoII");

			var result = new HashSet<RefCusCodeList>();
			foreach (XElement element in xmlList)
			{
				var code = element.GetElementValueAsString("codigo", 35);
				var description = element.GetElementValueAsString("descricao", 2000);
				var startDate = element.GetElementValueAsDateTime("inicioVigencia");
				var taxDutyRegimeLegalBase = element.Elements("listaRegimeTributacao");

				if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description))
				{
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = code,
						ZZD_Description = description,
						ZZD_StartDate = startDate,
						RefCusCodeListAttributes = GetRefCusCodeListAttributeValue(taxDutyRegimeLegalBase).ToArray(),
					};

					result.Add(refCusCodeList);
				}
			}

			return result;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.DutyLegalBasis.Description,
				ZZK_MaxLength = 2,
			};
		}

		protected override IEnumerable<RefCusCodeListAttributeName> GetRefCusCodeListAttributeNames()
		{
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.TaxDutyRegimeLegalBase.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.TaxDutyRegimeLegalBase.Description,
				ZXE_ZZK_NKCodeType = Constants.RefCusCodeTypes.DutyLegalBasis.Code,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = true,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.TaxDutyRegimeLegalBase.Description
			};
		}

		private static IEnumerable<RefCusCodeListAttribute> GetRefCusCodeListAttributeValue(IEnumerable<XElement> taxDutyRegimeLegalBase)
		{
			foreach (var element in taxDutyRegimeLegalBase)
			{
				var code = int.Parse(Regex.Match(element.GetElementValueAsString("codigo", 35), @"\d+").Value, CultureInfo.CurrentCulture);
				var description = element.GetElementValueAsString("descricao", 255);

				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.TaxDutyRegimeLegalBase.Code,
					ZZE_Value = code + " - " + description
				};
			}
		}
	}
}
