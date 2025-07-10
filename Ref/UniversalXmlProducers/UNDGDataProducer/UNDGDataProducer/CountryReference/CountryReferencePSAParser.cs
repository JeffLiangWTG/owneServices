using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CountryReferencePSAParser
	{
		readonly string _zipFilePath;
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;
		readonly IEnumerable<UNDGCountryReference> _additionalReferences;

		public CountryReferencePSAParser(IFileDownloaderWrapper fileDownloaderWrapper, string zipFilePath, IEnumerable<UNDGCountryReference> additionalReferences)
		{
			_fileDownloaderWrapper = fileDownloaderWrapper;
			_zipFilePath = zipFilePath;
			_additionalReferences = additionalReferences;
		}

		public async Task ParseAndSaveXmlAsync(string filePath)
		{
			var substances = new IMOParser(_fileDownloaderWrapper, _zipFilePath).ParseZipAndExtractUNDGSbustances();
			if (substances == null || !substances.Any())
			{
				Console.Error.WriteLine("Error extract IMO records from zip file");
				return;
			}

			await using var webScraper = await WebScraper.CreateAsync();

			var pSARecords =  await GetPSARecordsAsync(substances, webScraper);

			var references = MergePSARecordsToUNDGSubstances(substances, pSARecords);

			XmlWriterHelper.ExportToXml(CountryReferenceXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), references, filePath);
		}

		public IEnumerable<UNDGCountryReference> MergePSARecordsToUNDGSubstances(IEnumerable<UNDGSubstance> substances, IEnumerable<PSARecord> psaRecords)
		{
			var referenceToPivotDictionary = new Dictionary<UniquePSAReference, List<UNDGCountryReferencePivot>>();
			var countryReferences = new List<UNDGCountryReference>();

			foreach (var substance in substances)
			{
				var result = new UNDGSubstance { DG_UNNO = substance.DG_UNNO, DG_Variant = substance.DG_Variant };
				var matchedPSARecords = psaRecords.Where(x => x.UNNO == substance.DG_UNNO && x.IMOClass == substance.DG_Class && (x.PackagingGroup == substance.DG_PG || x.PackagingGroup == "-") && PSNFuzzyMatch(substance.DG_PSN, x.PSN, false));

				if (!matchedPSARecords.Any())
				{
					matchedPSARecords = psaRecords.Where(x => x.UNNO == substance.DG_UNNO && x.IMOClass == substance.DG_Class && (x.PackagingGroup == substance.DG_PG || x.PackagingGroup == "-") && PSNFuzzyMatch(substance.DG_PSN, x.PSN, true));
				}

				if (!matchedPSARecords.Any())
				{
					continue;
				}

				foreach (var psaRecord in matchedPSARecords)
				{
					var hasFlashPoint = decimal.TryParse(psaRecord.FlashPointLower, out var flashPointLower);
					var referenceKey = new UniquePSAReference(psaRecord.PSAGroup, hasFlashPoint, flashPointLower);

					var undgCountryReferencePivot = new UNDGCountryReferencePivot()
					{
						DCP_UNNO = substance.DG_UNNO,
						DCP_Variant = substance.DG_Variant,
						DCP_Standard = "IMO"
					};

					if (referenceToPivotDictionary.TryGetValue(referenceKey, out var pivots))
					{
						pivots.Add(undgCountryReferencePivot);
					}
					else
					{
						referenceToPivotDictionary.Add(referenceKey, new List<UNDGCountryReferencePivot> { undgCountryReferencePivot });
						countryReferences.Add(new UNDGCountryReference()
						{
							DCR_Code = psaRecord.PSAGroup,
							DCR_HasFlashPointLower = string.IsNullOrWhiteSpace(psaRecord.FlashPointLower) ? false : true,
							DCR_FlashPointLowerCentigrade = string.IsNullOrWhiteSpace(psaRecord.FlashPointLower) ? 0 : Convert.ToDecimal(psaRecord.FlashPointLower, CultureInfo.InvariantCulture),
							DCR_HasFlashPointUpper = string.IsNullOrWhiteSpace(psaRecord.FlashPointUpper) ? false : true,
							DCR_FlashPointUpperCentigrade = string.IsNullOrWhiteSpace(psaRecord.FlashPointUpper) ? 0 : Convert.ToDecimal(psaRecord.FlashPointUpper, CultureInfo.InvariantCulture),
							DCR_RN_NKCountry = "SG",
							DCR_Type = "PSA"
						});
					}

					foreach (var countryReference in countryReferences)
					{
						referenceToPivotDictionary.TryGetValue(new UniquePSAReference(countryReference.DCR_Code, countryReference.DCR_HasFlashPointLower, countryReference.DCR_FlashPointLowerCentigrade), out pivots);
						countryReference.UNDGCountryReferencePivots = pivots.ToArray();
					}
				}
			}

			countryReferences = countryReferences.OrderBy(x => x.DCR_Code).ThenBy(x => x.DCR_FlashPointLowerCentigrade).ToList();
			countryReferences.AddRange(_additionalReferences);
			return countryReferences;
		}

		static bool PSNFuzzyMatch(string pSNIMO, string pSNPSA, bool isContains)
		{
			pSNIMO = pSNIMO.Trim().ToUpper(CultureInfo.InvariantCulture).Replace(",", " ").Replace("S ", string.Empty).Replace(" ", string.Empty);
			pSNPSA = pSNPSA.Trim().ToUpper(CultureInfo.InvariantCulture).Replace(",", " ").Replace("S ", string.Empty).Replace(" ", string.Empty);

			return isContains ? pSNPSA.Contains(pSNIMO) : pSNIMO == pSNPSA;
		}

		public static async Task<IEnumerable<PSARecord>> GetPSARecordsAsync(IEnumerable<UNDGSubstance> originalRecords, IWebScraper webScraper)
		{
			return await PSAGroupParser.GetPSARecordsAsync(originalRecords, webScraper);
		}
	}
}
