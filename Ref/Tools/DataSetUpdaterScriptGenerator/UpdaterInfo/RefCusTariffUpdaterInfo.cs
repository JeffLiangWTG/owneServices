using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusTariffUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusTariffUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder(SQLBuilder.UpdateReferenceFKStatement<IRefCusRateType, IRefCusRateCode>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusRateType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTariffType, IRefCusTariff>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTariffType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTariffType, IRefCusTariffRelationship>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTariffType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusConditionType, IRefCusCondition>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusConditionType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusConditionValueType, IRefCusConditionValue>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusConditionValueType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusRateCode, IRefCusRate>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusRateCode>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusPreference, IRefCusRate>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusPreference>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusPreference, IRefCusCondition>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusPreference>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusApplicability>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusExcludedTradeGroup>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusTariffUOM>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusVATApplicability>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefCusTariff>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusTariff>(null)));

			foreach (var type in GetTypeByInsertOrder().Skip(1))
			{
				result.AppendLine((string)typeof(ReplaceSQLBuilder).InvokeStaticGenericMethod(nameof(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable), type.Item1, string.Empty));
			}

			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusRateType), typeof(IRefCusRateType));
			yield return Tuple.Create(typeof(IRefCusRateCode), typeof(IRefCusRateCode));
			yield return Tuple.Create(typeof(IRefCusPreference), typeof(IRefCusPreference));
			yield return Tuple.Create(typeof(IRefCusTariffType), typeof(IRefCusTariffType));
			yield return Tuple.Create(typeof(IRefCusConditionType), typeof(IRefCusConditionType));
			yield return Tuple.Create(typeof(IRefCusConditionValueType), typeof(IRefCusConditionValueType));
			yield return Tuple.Create(typeof(IRefCusTradeGroup), typeof(IRefCusTradeGroup));
			yield return Tuple.Create(typeof(IRefLanguageType), typeof(IRefLanguageType));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusTariff), typeof(IRefCusTariff));
			yield return Tuple.Create(typeof(IRefCusTariffNationalCode), typeof(IRefCusTariffNationalCode));
			yield return Tuple.Create(typeof(IRefCusTariffUOM), typeof(IRefCusTariffUOM));
			yield return Tuple.Create(typeof(IRefCusTariffLanguage), typeof(IRefCusTariffLanguage));
			yield return Tuple.Create(typeof(IRefCusTariffRelationship), typeof(IRefCusTariffRelationship));
			yield return Tuple.Create(typeof(IRefCusTariffAttribute), typeof(IRefCusTariffAttribute));
			yield return Tuple.Create(typeof(IRefCusVATApplicability), typeof(IRefCusVATApplicability));
			yield return Tuple.Create(typeof(IRefCusRate), typeof(IRefCusRate));
			yield return Tuple.Create(typeof(IRefCusRateUOM), typeof(IRefCusRateUOM));
			yield return Tuple.Create(typeof(IRefCusCondition), typeof(IRefCusCondition));
			yield return Tuple.Create(typeof(IRefCusConditionValue), typeof(IRefCusConditionValue));
			yield return Tuple.Create(typeof(IRefCusConditionLanguage), typeof(IRefCusConditionLanguage));
			yield return Tuple.Create(typeof(IRefCusTariffAdditionalCode), typeof(IRefCusTariffAdditionalCode));
			yield return Tuple.Create(typeof(IRefCusTariffAdditionalCodeLanguage), typeof(IRefCusTariffAdditionalCodeLanguage));
			yield return Tuple.Create(typeof(IRefCusApplicability), typeof(IRefCusApplicability));
			yield return Tuple.Create(typeof(IRefCusExcludedTradeGroup), typeof(IRefCusExcludedTradeGroup));
			yield return Tuple.Create(typeof(IRefCusTariffBRCharacteristic), typeof(IRefCusTariffBRCharacteristic));
			yield return Tuple.Create(typeof(IRefCusTariffBRCharacteristicAttribute), typeof(IRefCusTariffBRCharacteristicAttribute));
			yield return Tuple.Create(typeof(IRefCusTariffBRCharacteristicValue), typeof(IRefCusTariffBRCharacteristicValue));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}
	}
}
