using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ReasonTemporaryAdmissionCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public ReasonTemporaryAdmissionCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.CustomsReasonTemporaryAdmission.Code;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Contract.Assume(dataSource != null);

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);

			var customsReasonTemporaryAdmissionList = xml.Root?.Descendants("MotivoAdmissaoTemporaria");

			Contract.Assume(customsReasonTemporaryAdmissionList != null);

			foreach (XElement element in customsReasonTemporaryAdmissionList)
			{
				string code = element.GetElementValueAsString("codigo", 35);

				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = code,
					ZZD_Description = element.GetElementValueAsString("descricao", 2000),
					ZZD_StartDate = element.GetElementValueAsDateTime("inicioVigencia")
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
				ZZK_Description = Constants.RefCusCodeTypes.CustomsReasonTemporaryAdmission.Description,
			};
		}
	}
}
