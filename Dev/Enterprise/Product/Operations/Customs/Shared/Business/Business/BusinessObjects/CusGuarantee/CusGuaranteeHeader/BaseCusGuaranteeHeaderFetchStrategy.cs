using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	class BaseCusGuaranteeHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public BaseCusGuaranteeHeaderFetchStrategy(BaseCusGuaranteeHeader baseCusGuaranteeHeader)
			: base(baseCusGuaranteeHeader)
		{
		}

		protected new BaseCusGuaranteeHeader BusinessObject
		{
			get { return (BaseCusGuaranteeHeader)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(BaseCusGuaranteeHeader.AuthLatestValueBalance):
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.AuthLatestValueBalanceWithMsg):
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.CPH_Calc_OpeningBalance):
						Factory.AddFetchHint(CusPermitRuleSchema.CPR_CPH_PermitHeader, BusinessObject.PK);
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.CPH_Calc_PendingBalanceDecimal):
						Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.CPH_OH_PermitHolder);
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal):
						Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.CPH_OH_PermitHolder);
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.CPH_Calc_UsedBalance):
						Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.CPH_OH_PermitHolder);
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.MainAccessCode):
					case nameof(BaseCusGuaranteeHeader.MainAccessPersonName):
						Factory.AddFetchHint(CusPermitRuleSchema.CPR_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.UnsavedTransactionTotal):
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
					case nameof(BaseCusGuaranteeHeader.ValueBalance):
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
