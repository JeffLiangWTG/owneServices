using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System.Collections.Generic;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Services.Dictionaries;
using static CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure.Constants;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure
{
	public static class CusProcedureUniversalReferenceDataXmlGenerator
	{
		static class MetaData
		{
			public const string DataSource = "PL CusProcedure";
			public const string OutputPath = "PLCusProcedure.xml";
		}

		public static bool GenerateCusProcedureUniversalReferenceData()
		{
			try
			{
				var dictionariesHandler = new Dictionaries.Dictionaries(DictionaryFromUrlProvider.GetDictionaryFromUrl);

				var allowedCombinations = GetGetDictionariesAsRefCusCodeList(dictionariesHandler, PuescDictionaryId.AllowedRequestedProcedureAndPreviousProcedureCombinations, RefDataType.RefCusCodeList);

				var result = GenerateRefCusProcedures(dictionariesHandler, allowedCombinations, DictionariesConstants.SupportedPuescDictionaries.ImportProcedureCodes, DictionariesConstants.SupportedPuescDictionaries.ImportPreviousProcedureCodes, DictionariesConstants.SupportedPuescDictionaries.ImportConcessions, isImport: true);
				result.AddRange(GenerateRefCusProcedures(dictionariesHandler, allowedCombinations, DictionariesConstants.SupportedPuescDictionaries.ExportProcedureCodes, DictionariesConstants.SupportedPuescDictionaries.ExportPreviousProcedureCodes, DictionariesConstants.SupportedPuescDictionaries.ExportConcessions, isImport: false));

				var saveFullPath = CommonHelper.GetOutputFilePath(MetaData.OutputPath);
				XmlWriterConfig.ExportToXmlFile(MetaData.DataSource, saveFullPath, XmlWriterConfig.GetRefCusProcedureWriterConfiguration(), DateTime.UtcNow, result);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return false;
			}

			return true;
		}

		static List<RefCusProcedure> GenerateRefCusProcedures(Dictionaries.Dictionaries dictionariesHandler,
			IReadOnlyCollection<RefCusCodeList> allowedCombinations,
			string procedureCodesDictionaryId,
			string previousProcedureDictionaryId,
			string concessionDictionaryId,
			bool isImport) => CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(allowedCombinations,
				GetGetDictionariesAsRefCusCodeList(dictionariesHandler, procedureCodesDictionaryId),
				GetGetDictionariesAsRefCusCodeList(dictionariesHandler, previousProcedureDictionaryId),
				GetGetDictionariesAsRefCusCodeList(dictionariesHandler, concessionDictionaryId),
				isImport);

		static IReadOnlyCollection<RefCusCodeList> GetGetDictionariesAsRefCusCodeList(Dictionaries.Dictionaries dictionariesHandler, string dictionaryId, RefDataType dataType = RefDataType.RefCusCodeListMerged)
		{
			var result = dictionariesHandler.GetDictionariesAsRefCusCodeList(GetNewDictionaryDataForCusProcedure());
			if (result == null
				|| result.Count == 0)
			{
				throw new Exception($"Failed to download '{dictionaryId}' or dictionary is empty, when it should not be");
			}

			return result;

			DictionaryData GetNewDictionaryDataForCusProcedure() => new DictionaryData(dictionaryId,
				cw1Code: null,
				type: dataType,
				publishedByTestPuesc: true);

		}
	}
}
