using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class WarehousingSectorsCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public WarehousingSectorsCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.WarehousingSectorsCodes.Code;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Contract.Assume(dataSource != null);

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);

			var warehousingSectorsList = xml.Root?.Descendants("SetorLotacao");

			Contract.Assume(warehousingSectorsList != null);

			foreach (XElement element in warehousingSectorsList)
			{
				string agencyCode = element.GetElementValueAsString("codigoOrgao", 35);
				string enclosureCode = element.GetElementValueAsString("codigoRecinto", 35);
				string code = element.GetElementValueAsString("codigo", 35);

				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = $"{agencyCode};{enclosureCode};{code}",
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
				ZZK_Description = Constants.RefCusCodeTypes.WarehousingSectorsCodes.Description,
			};
		}
	}
}
