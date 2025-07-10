using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class AgreementsLAIACodeListParser : BaseRefCusCodeListParser<(Stream, string)>
	{
		public AgreementsLAIACodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.DutyLaiaAgreementCodes.Code;

		protected override bool HasAttributes => true;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists((Stream, string) dataSource)
		{
			var stream = dataSource.Item1;
			Contract.Assume(stream != null);

			var dutyLaiaAgreements = TariffAgreementsCodeListParser.GetLaiaAgreementDTOs(dataSource.Item2);

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(stream);

			Contract.Assume(xml != null);

			var customsAlaiaAgreementsList = xml.Root?.Descendants("AcordoAladi");

			Contract.Assume(customsAlaiaAgreementsList != null);

			foreach (XElement element in customsAlaiaAgreementsList)
			{
				string code = element.GetElementValueAsString("codigo", 35);
				var attributes = dutyLaiaAgreements.FirstOrDefault(x => x.Id == code);

				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = code,
					ZZD_Description = element.GetElementValueAsString("descricao", 2000),
					ZZD_StartDate = element.GetElementValueAsDateTime("inicioVigencia"),
					RefCusCodeListAttributes = GetRefCusCodeListAttributeValue(attributes).ToArray(),
				};

				Contract.Assume(!string.IsNullOrEmpty(refCusCodeList.ZZD_Code));

				result.Add(refCusCodeList);
			}
			return result;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.DutyLaiaAgreementCodes.Description,
			};
		}

		protected override IEnumerable<RefCusCodeListAttributeName> GetRefCusCodeListAttributeNames()
		{
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.AgreementCountry.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.AgreementCountry.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.AgreementCountry.Description
			};
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.AgreementSubject.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.AgreementSubject.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.AgreementSubject.Description
			};
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.AgreementLegalAct.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.AgreementLegalAct.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.AgreementLegalAct.Description
			};
		}

		static IEnumerable<RefCusCodeListAttribute> GetRefCusCodeListAttributeValue(LaiaAgreementDTO attributes)
		{
			if (attributes != null)
			{
				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.AgreementCountry.Code,
					ZZE_Value = attributes.Country
				};
				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.AgreementSubject.Code,
					ZZE_Value = attributes.Subject
				};
				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.AgreementLegalAct.Code,
					ZZE_Value = attributes.LegalAct
				};
			}
		}
	}
}
