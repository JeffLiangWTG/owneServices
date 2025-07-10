using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ImportSiscomexProcedureParser : BaseParser
	{
		public ImportSiscomexProcedureParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream streamDutyLegalBasis, Stream streamPisCofinsLegalBasis, Stream streamDeclarationTypeTaxRegime, string outputFileName, DateTime publicationTime)
		{
			var refCusProcedureLists = GetRefCusProcedureList(streamDutyLegalBasis, streamPisCofinsLegalBasis, streamDeclarationTypeTaxRegime);
			var writer = Helper.GetRefCusProcedureWriterConfiguration(Constants.ShipmentTypes.ImportSiscomex);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusProcedureLists);
		}

		public static List<RefCusProcedure> GetRefCusProcedureList(Stream streamDutyLegalBasis, Stream streamPisCofinsLegalBasis, Stream streamDeclarationTypeTaxRegime)
		{
			Contract.Assume(streamDutyLegalBasis != null && streamPisCofinsLegalBasis != null && streamDeclarationTypeTaxRegime != null);

			var xmlDutyLegalBasis = XDocument.Load(streamDutyLegalBasis);
			var xmlPisCofinsLegalBasis = XDocument.Load(streamPisCofinsLegalBasis);
			var xmlDeclarationTypeTaxRegime = XDocument.Load(streamDeclarationTypeTaxRegime);

			Contract.Assume(xmlDutyLegalBasis != null && xmlPisCofinsLegalBasis != null && xmlDeclarationTypeTaxRegime != null);
			var xmlDutyLegalBasisList = xmlDutyLegalBasis.Root?.Descendants("FundamentoLegalRegimeTributacaoII");
			var xmlPisCofinsLegalBasisList = xmlPisCofinsLegalBasis.Root?.Descendants("FundamentoLegalRegimeTributacaoPisCofins");
			var xmlDeclarationTypeTaxRegimeList = xmlDeclarationTypeTaxRegime.Root?.Descendants("TipoDeclaracaoRegimeTributario");

			var result = new List<RefCusProcedure>();

			result.AddRange(GetRefCusProcedures(xmlDutyLegalBasisList, xmlDeclarationTypeTaxRegimeList, Constants.Rates.Types.Duty));
			result.AddRange(GetRefCusProcedures(xmlPisCofinsLegalBasisList, xmlDeclarationTypeTaxRegimeList, Constants.Rates.Types.PIS));

			return result;
		}

		static IEnumerable<RefCusProcedure> GetRefCusProcedures(IEnumerable<XElement> xml, IEnumerable<XElement> xmlDeclarationTypeTaxRegimeList, string rateType)
		{
			var result = new List<RefCusProcedure>();

			var childNodeName = rateType == Constants.Rates.Types.Duty ? "listaRegimeTributacao" : "regimesTributacao";

			foreach (XElement element in xml)
			{
				var concession = element.GetElementValueAsString("codigo", 35).Trim();
				var startDate = element.GetElementValueAsString("inicioVigencia", 35).Trim();

				var innerObjectList = element.Descendants(childNodeName);
				foreach (var innerObject in innerObjectList)
				{
					var innerObjectCode = innerObject.GetElementValueAsString("codigo", 35).Trim();
					var innerObjectDescription = innerObject.GetElementValueAsString("descricao", 100).Trim();
					var calculateDuty = innerObjectCode.Equals("1", StringComparison.Ordinal) || innerObjectCode.Equals("4", StringComparison.Ordinal);

					var procedureCodeList = GetProcedureCode(xmlDeclarationTypeTaxRegimeList, innerObjectCode);

					if (procedureCodeList.Any())
					{
						foreach (var procedureCode in procedureCodeList)
						{
							var refCusProcedure = new RefCusProcedure
							{
								ZZ6_ProcedureCode = procedureCode,
								ZZ6_PreviousProcedureCode = innerObjectCode,
								ZZ6_Category = rateType,
								ZZ6_Concession = concession,
								ZZ6_Description = innerObjectDescription + " - " + concession,
								ZZ6_CalculateDuty = calculateDuty,
								ZZ6_StartDate = Convert.ToDateTime(startDate, CultureInfo.CurrentCulture)
							};

							result.Add(refCusProcedure);
						}
					}
				}
			}

			return result;
		}

		static IEnumerable<string> GetProcedureCode(IEnumerable<XElement> xmlDeclarationTypeTaxRegimeList, string previousProcedureCode)
		{
			List<string> procedureCodeList = new List<string>();

			foreach (XElement element in xmlDeclarationTypeTaxRegimeList)
			{
				var elementPreviousProcedureCode = element.GetElementValueAsString("codigo", 35).Trim().Last().ToString();

				if (elementPreviousProcedureCode == previousProcedureCode)
				{
					procedureCodeList.Add(element.GetElementValueAsString("codigo", 35).Trim().Substring(0, 2));
				}
			}

			return procedureCodeList;
		}
	}
}
