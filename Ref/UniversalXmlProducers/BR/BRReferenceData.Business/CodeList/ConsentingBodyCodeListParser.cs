using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ConsentingBodyCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public ConsentingBodyCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.CustomsConsentingBody.Code;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var customsConsentingBodyList = xml.Root?.Descendants("OrgaoAnuente");

			Contract.Assume(customsConsentingBodyList != null);

			foreach (XElement element in customsConsentingBodyList)
			{
				string code = element.GetElementValueAsString("codigo", 35);

				if (!string.IsNullOrEmpty(code))
				{
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = code,
						ZZD_Description = element.GetElementValueAsString("descricao", 2000),
						ZZD_StartDate = element.GetElementValueAsDateTime("inicioVigencia")
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
				ZZK_Description = Constants.RefCusCodeTypes.CustomsConsentingBody.Description,
			};
		}
	}
}
