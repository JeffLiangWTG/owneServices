using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class DataProviderHelper
	{
		public static IOrderedEnumerable<string> OrderInclude(IEnumerable<string> includes, string orderBy = "ASC", char splitCharacter = '.')
		{
			Argument.NotNull(includes, nameof(includes));
			return orderBy != "DESC"
				? includes.ToList().OrderBy(x => x.Split(splitCharacter).Length)
				: includes.ToList().OrderByDescending(x => x.Split(splitCharacter).Length);
		}

		public static bool IsExpirableType(Type type)
		{
			var canEnableExpirable = EnableExpireTypeConfiguration.TypesToEnableExpirable.Any(x => x.Name.Equals(type.Name, StringComparison.OrdinalIgnoreCase));
			if (canEnableExpirable)
			{
				Argument.NotNull(DataProviderHelper.enableExpirable, nameof(DataProviderHelper.enableExpirable), $"Please call {nameof(SetEnableExpirable)} first");
			}
			var enableExpirable = !canEnableExpirable || DataProviderHelper.enableExpirable.Value;
			return enableExpirable && type.IsExpirableType();
		}

		public static void AddUpdaterResult(Dictionary<Guid, SafeObjectUpdaterResult> results, object safeObj, string safeObjTblPrefix, ResultAction action, Guid? expirableAncestorPK, Guid? newRecordForCloneActionPK, Guid? datasetPK)
		{
			Argument.NotNull(results, nameof(results));
			Argument.NotNull(safeObj, nameof(safeObj));

			var safeObjPK = safeObj.GetPKValue();
			results.AddIfNotExists(safeObjPK, new SafeObjectUpdaterResult
			{
				ParentPK = safeObjPK,
				ParentCode = safeObjTblPrefix,
				Action = action,
				ExpirableAncestorPK = expirableAncestorPK,
				NewRecordForCloneActionPK = newRecordForCloneActionPK,
				DatasetPK = datasetPK
			});
		}

		public static void SetEnableExpirable(bool doEnableExpirable)
		{
			enableExpirable = doEnableExpirable;
		}
		static bool? enableExpirable;
	}
}
