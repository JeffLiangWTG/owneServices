using System.Collections.Generic;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public class RefCusRateCode : ITableScript
	{
		public string TableName => nameof(RefCusRateCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusTariffNationalCode : ITableScript
	{
		public string TableName => nameof(RefCusTariffNationalCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusExcludedTradeGroup : ITableScript
	{
		public string TableName => nameof(RefCusExcludedTradeGroup);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCondition : ITableScript
	{
		public string TableName => nameof(RefCusCondition);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusConditionValueType : ITableScript
	{
		public string TableName => nameof(RefCusConditionValueType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConditionValue : ITableScript
	{
		public string TableName => nameof(RefCusConditionValue);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConditionType : ITableScript
	{
		public string TableName => nameof(RefCusConditionType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusConditionCode : ITableScript
	{
		public string TableName => nameof(RefCusConditionCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConditionCodeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusConditionCodeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}


	public class RefCusApplicability : ITableScript
	{
		public string TableName => nameof(RefCusApplicability);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) },
		};
	}

	public class RefCusPreference : ITableScript
	{
		public string TableName => nameof(RefCusPreference);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefDataGrouping : ITableScript
	{
		public string TableName => nameof(RefDataGrouping);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusRateType : ITableScript
	{
		public string TableName => nameof(RefCusRateType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusRateTypeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusRateTypeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffType : ITableScript
	{
		public string TableName => nameof(RefCusTariffType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariff : ITableScript
	{
		public string TableName => nameof(RefCusTariff);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusTariffRelationship : ITableScript
	{
		public string TableName => nameof(RefCusTariffRelationship);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusRate : ITableScript
	{
		public string TableName => nameof(RefCusRate);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffUOM : ITableScript
	{
		public string TableName => nameof(RefCusTariffUOM);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
		};
	}

	public class RefCusTariffAttribute : ITableScript
	{
		public string TableName => nameof(RefCusTariffAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffAttributeName : ITableScript
	{
		public string TableName => nameof(RefCusTariffAttributeName);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>()
		{
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1)}
		};
	}

	public class RefCusTariffBRCharacteristicValue : ITableScript
	{
		public string TableName => nameof(RefCusTariffBRCharacteristicValue);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffBRCharacteristic : ITableScript
	{
		public string TableName => nameof(RefCusTariffBRCharacteristic);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusTariffBRCharacteristicAttribute : ITableScript
	{
		public string TableName => nameof(RefCusTariffBRCharacteristicAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProfileType : ITableScript
	{
		public string TableName => nameof(RefCusProfileType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusProfile : ITableScript
	{
		public string TableName => nameof(RefCusProfile);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusProfileAttribute : ITableScript
	{
		public string TableName => nameof(RefCusProfileAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProfileQuestion : ITableScript
	{
		public string TableName => nameof(RefCusProfileQuestion);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusProfileQuestionAnswerList : ITableScript
	{
		public string TableName => nameof(RefCusProfileQuestionAnswerList);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProfileQuestionAttribute : ITableScript
	{
		public string TableName => nameof(RefCusProfileQuestionAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProfileQuestionLanguage : ITableScript
	{
		public string TableName => nameof(RefCusProfileQuestionLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProfileQuestionAnswerListLanguage : ITableScript
	{
		public string TableName => nameof(RefCusProfileQuestionAnswerListLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProfileQuestionPathway : ITableScript
	{
		public string TableName => nameof(RefCusProfileQuestionPathway);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusNomenclatureGroup : ITableScript
	{
		public string TableName => nameof(RefCusNomenclatureGroup);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusNomenclatureGroupNote : ITableScript
	{
		public string TableName => nameof(RefCusNomenclatureGroupNote);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCarrierCode : ITableScript
	{
		public string TableName => nameof(RefCarrierCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCarrierCodeAttribute : ITableScript
	{
		public string TableName => nameof(RefCarrierCodeAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCarrierCodeLanguage : ITableScript
	{
		public string TableName => nameof(RefCarrierCodeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProcedure : ITableScript
	{
		public string TableName => nameof(RefCusProcedure);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusTradeGroup : ITableScript
	{
		public string TableName => nameof(RefCusTradeGroup);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTradeGroupCountry : ITableScript
	{
		public string TableName => nameof(RefCusTradeGroupCountry);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeType : ITableScript
	{
		public string TableName => nameof(RefCusCodeType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusCodeTypeAttribute : ITableScript
	{
		public string TableName => nameof(RefCusCodeTypeAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeTypeAttributeName : ITableScript
	{
		public string TableName => nameof(RefCusCodeTypeAttributeName);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeList : ITableScript
	{
		public string TableName => nameof(RefCusCodeList);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusCodeListAttribute : ITableScript
	{
		public string TableName => nameof(RefCusCodeListAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusTaxOrFee : ITableScript
	{
		public string TableName => nameof(RefCusTaxOrFee);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusMap : ITableScript
	{
		public string TableName => nameof(RefCusMap);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusMapType : ITableScript
	{
		public string TableName => nameof(RefCusMapType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefVesselArrival : ITableScript
	{
		public string TableName => nameof(RefVesselArrival);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefVesselZZ : ITableScript
	{
		public string TableName => nameof(RefVesselZZ);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefExchangeRateZZ : ITableScript
	{
		public string TableName => nameof(RefExchangeRateZZ);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) },
			{ 4, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 4) }
	};
	}

	public class RefCarrierVesselPivot : ITableScript
	{
		public string TableName => nameof(RefCarrierVesselPivot);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusVATApplicability : ITableScript
	{
		public string TableName => nameof(RefCusVATApplicability);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusCodeOrAttributeTransportMode : ITableScript
	{
		public string TableName => nameof(RefCusCodeOrAttributeTransportMode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusNomenclatureGroupType : ITableScript
	{
		public string TableName => nameof(RefCusNomenclatureGroupType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusRateCodeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusRateCodeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefLanguageType : ITableScript
	{
		public string TableName => nameof(RefLanguageType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeListLanguage : ITableScript
	{
		public string TableName => nameof(RefCusCodeListLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusNomenclatureLanguage : ITableScript
	{
		public string TableName => nameof(RefCusNomenclatureLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffLanguage : ITableScript
	{
		public string TableName => nameof(RefCusTariffLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusPreferenceLanguage : ITableScript
	{
		public string TableName => nameof(RefCusPreferenceLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProcedureAttribute : ITableScript
	{
		public string TableName => nameof(RefCusProcedureAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefAccessorial : ITableScript
	{
		public string TableName => nameof(RefAccessorial);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefAccElectronicProcessingFee : ITableScript
	{
		public string TableName => nameof(RefAccElectronicProcessingFee);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);
		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefAccTaxRate : ITableScript
	{
		public string TableName => nameof(RefAccTaxRate);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);
		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTradeGroupLanguage : ITableScript
	{
		public string TableName => nameof(RefCusTradeGroupLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTaxOrFeeType : ITableScript
	{
		public string TableName => nameof(RefCusTaxOrFeeType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeListAttributeName : ITableScript
	{
		public string TableName => nameof(RefCusCodeListAttributeName);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefCusRuling : ITableScript
	{
		public string TableName => nameof(RefCusRuling);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusRulingConfig : ITableScript
	{
		public string TableName => nameof(RefCusRulingConfig);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffAdditionalCodeCategory : ITableScript
	{
		public string TableName => nameof(RefCusTariffAdditionalCodeCategory);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusTariffAdditionalCode : ITableScript
	{
		public string TableName => nameof(RefCusTariffAdditionalCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) },
		};
	}

	public class RefCusTariffAdditionalCodeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusTariffAdditionalCodeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefHarbourRate : ITableScript
	{
		public string TableName => nameof(RefHarbourRate);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
		};
	}

	public class RefCusAUNexdocECMCode : ITableScript
	{
		public string TableName => nameof(RefCusAUNexdocECMCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefDbVersionControl : ITableScript
	{
		public string TableName => nameof(RefDbVersionControl);
		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusRateUOM : ITableScript
	{
		public string TableName => nameof(RefCusRateUOM);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeListAttributeNameLanguage : ITableScript
	{
		public string TableName => nameof(RefCusCodeListAttributeNameLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusCodeTypeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusCodeTypeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class UNDGSubstanceADR : ITableScript
	{
		public string TableName => nameof(UNDGSubstanceADR);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class UNDGSubstanceCFR : ITableScript
	{
		public string TableName => nameof(UNDGSubstanceCFR);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) },
			{ 4, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 4) },
			{ 5, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 5) }
		};
	}

	public class UNDGAttributeZZ : ITableScript
	{
		public string TableName => nameof(UNDGAttributeZZ);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class UNDGVersion : ITableScript
	{
		public string TableName => nameof(UNDGVersion);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefDocOrgCusCode : ITableScript
	{
		public string TableName => nameof(RefDocOrgCusCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefMessagingBussAttributeInfo : ITableScript
	{
		public string TableName => nameof(RefMessagingBussAttributeInfo);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefMessagingBussCarrierInfoAttribute : ITableScript
	{
		public string TableName => nameof(RefMessagingBussCarrierInfoAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefMessagingBussPackageInfoAttribute : ITableScript
	{
		public string TableName => nameof(RefMessagingBussPackageInfoAttribute);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefMessagingBussCarrierInfo : ITableScript
	{
		public string TableName => nameof(RefMessagingBussCarrierInfo);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefMessagingBussPackageInfo : ITableScript
	{
		public string TableName => nameof(RefMessagingBussPackageInfo);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefMessagingBussPackageVersion : ITableScript
	{
		public string TableName => nameof(RefMessagingBussPackageVersion);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefSysConfigType : ITableScript
	{
		public string TableName => nameof(RefSysConfigType);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefSysConfig : ITableScript
	{
		public string TableName => nameof(RefSysConfig);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class UNDGSubstanceADN : ITableScript
	{
		public string TableName => nameof(UNDGSubstanceADN);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class UNDGSubstanceRID : ITableScript
	{
		public string TableName => nameof(UNDGSubstanceRID);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefCusTaxOrFeeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusTaxOrFeeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusProcedureLanguage : ITableScript
	{
		public string TableName => nameof(RefCusProcedureLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConditionTypeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusConditionTypeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConditionValueTypeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusConditionValueTypeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class UNDGSubstanceJTT : ITableScript
	{
		public string TableName => nameof(UNDGSubstanceJTT);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) }
		};
	}

	public class RefAirlineUNDGRule : ITableScript
	{
		public string TableName => nameof(RefAirlineUNDGRule);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConditionLanguage : ITableScript
	{
		public string TableName => nameof(RefCusConditionLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefAirlineCommodityCode : ITableScript
	{
		public string TableName => nameof(RefAirlineCommodityCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefStlScript : ITableScript
	{
		public string TableName => nameof(RefStlScript);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) },
			{ 3, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 3) },
			{ 4, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 4) },
			{ 5, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 5) }
		};
	}

	public class RefCusTariffTypeLanguage : ITableScript
	{
		public string TableName => nameof(RefCusTariffTypeLanguage);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusConfiguration : ITableScript
	{
		public string TableName => nameof(RefCusConfiguration);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefAirlineProductCode : ITableScript
	{
		public string TableName => nameof(RefAirlineProductCode);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefAirlineProductCodeCommodityCodePivot : ITableScript
	{
		public string TableName => nameof(RefAirlineProductCodeCommodityCodePivot);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefCusQuota : ITableScript
	{
		public string TableName => nameof(RefCusQuota);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefStlFieldMapping : ITableScript
	{
		public string TableName => nameof(RefStlFieldMapping);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) },
			{ 2, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 2) }
		};
	}

	public class RefUNLOCO : ITableScript
	{
		public string TableName => nameof(RefUNLOCO);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>()
		{
		};
	}

	public class RefUNLOCOUtcOffset : ITableScript
	{
		public string TableName => nameof(RefUNLOCOUtcOffset);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefClient : ITableScript
	{
		public string TableName => nameof(RefClient);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefUNLOCORelatedPort : ITableScript
	{
		public string TableName => nameof(RefUNLOCORelatedPort);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class RefGlbReleaseNote : ITableScript
	{
		public string TableName => nameof(RefGlbReleaseNote);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>
		{
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}

	public class UNDGSubstanceTDG : ITableScript
	{
		public string TableName => nameof(UNDGSubstanceTDG);

		public string CreateTableScript => SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(SharedDbSchemaChange.TableSubfolder, TableName);

		public Dictionary<int, string> TableViewScriptDictionary => new Dictionary<int, string>() {
			{ 1, SharedDbSchemaChange.GetTableViewSqlScript(TableName, 1) }
		};
	}
}
