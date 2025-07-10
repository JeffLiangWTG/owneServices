using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusNomenclatureGroupUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusNomenclatureGroupUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder(SQLBuilder.UpdateReferenceFKStatement<IRefCusConditionType, IRefCusCondition>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusConditionType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusConditionValueType, IRefCusConditionValue>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusConditionValueType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusApplicability>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusExcludedTradeGroup>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefCusNomenclatureGroup>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusNomenclatureGroup>(null)));

			foreach (var type in GetTypeByInsertOrder().Skip(1))
			{
				result.AppendLine((string)typeof(ReplaceSQLBuilder).InvokeStaticGenericMethod(nameof(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable), type.Item1, string.Empty));
			}

			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusConditionType), typeof(IRefCusConditionType));
			yield return Tuple.Create(typeof(IRefCusConditionValueType), typeof(IRefCusConditionValueType));
			yield return Tuple.Create(typeof(IRefCusTradeGroup), typeof(IRefCusTradeGroup));
			yield return Tuple.Create(typeof(IRefLanguageType), typeof(IRefLanguageType));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusNomenclatureGroup), typeof(IRefCusNomenclatureGroup));
			yield return Tuple.Create(typeof(IRefCusNomenclatureGroupNote), typeof(IRefCusNomenclatureGroupNote));
			yield return Tuple.Create(typeof(IRefCusNomenclatureLanguage), typeof(IRefCusNomenclatureLanguage));
			yield return Tuple.Create(typeof(IRefCusCondition), typeof(IRefCusCondition));
			yield return Tuple.Create(typeof(IRefCusConditionValue), typeof(IRefCusConditionValue));
			yield return Tuple.Create(typeof(IRefCusConditionLanguage), typeof(IRefCusConditionLanguage));
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
