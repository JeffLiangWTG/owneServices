using System;
using System.Data;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class ConditionLoadTariffCriteriaSet : TariffCriteriaSet<RefCusCondition>
	{
		public override DataRow AddNewCriteriaSetRow(DataTable criteriaParameterTable, DataTable additionalCodesParameterTable, DataTable secondTradeGroupsDataTable, Guid criteriaId, ZGuid tariffPk, IZZApplicabilitySelectionCriteria selectionCriteria)
		{
			var criteriaRow = base.AddNewCriteriaSetRow(criteriaParameterTable, additionalCodesParameterTable, secondTradeGroupsDataTable, criteriaId, tariffPk, selectionCriteria);

			var selectionConditionCriteria = (IZZConditionSelectionCriteria)selectionCriteria;
			criteriaRow[TvpConditionSelectionCriteria.Columns.IsImport] = selectionConditionCriteria.Direction == ConditionChecker.ConditionDirection.Import || selectionConditionCriteria.Direction == ConditionChecker.ConditionDirection.Either;
			criteriaRow[TvpConditionSelectionCriteria.Columns.IsExport] = selectionConditionCriteria.Direction == ConditionChecker.ConditionDirection.Export || selectionConditionCriteria.Direction == ConditionChecker.ConditionDirection.Either;
			criteriaRow[TvpConditionSelectionCriteria.Columns.ConditionClass] = selectionConditionCriteria.ConditionClass;
			criteriaRow[TvpConditionSelectionCriteria.Columns.ConditionType] = selectionConditionCriteria.ConditionType;

			return criteriaRow;
		}

		public ConditionLoadTariffCriteriaSet(TariffView tariff, IZZConditionSelectionCriteria selectionCriteria)
		: base(tariff, selectionCriteria)
		{
		}

		protected override string QualifiedName => "ConditionLoadTariffCriteriaSet";

		protected override string ExtraKeyString(IZZApplicabilitySelectionCriteria criteria)
		{
			var conditonCriteria = (IZZConditionSelectionCriteria)criteria;
			return $"_{conditonCriteria.Direction}_{conditonCriteria.ConditionClass}_{conditonCriteria.ConditionType}";
		}
	}
}
