using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class BaseJobComInvoiceHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public BaseJobComInvoiceHeaderFetchStrategy(BaseJobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new BaseJobComInvoiceHeader BusinessObject
		{
			get { return (BaseJobComInvoiceHeader)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(BaseJobDeclaration), BusinessObject.JZ_JE);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, BusinessObject.PK);

			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, BusinessObject.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);

			if (BusinessObject is ICusAddInfoTypeSupporter)
			{
				Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
			}
			if (BusinessObject is ICusCodeDataTypeSupporter)
			{
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			}

			if (!(BusinessObject.JobDeclaration?.HasFetchForLoadChildEditableObjectsBeenCalled ?? false) && !BusinessObject.HasFetchForLoadChildEditableObjectsBeenCalled)
			{
				AddFetchHintForPackInvoiceHeaderPivot();
				Factory.AddFetchHint(JobComInvoiceHeaderRefsSchema.J2_ClusterKey, BusinessObject.JZ_ClusterKey);
				Factory.AddFetchHint(JobComInvoiceLineSchema.JI_ClusterKey, BusinessObject.JZ_ClusterKey);
				Factory.AddFetchHint(JobComInvLineComponentInventorySchema.JIV_ClusterKey, BusinessObject.JZ_ClusterKey);
				Factory.AddFetchHint(JobComInvLineRefsSchema.JG_ClusterKey, BusinessObject.JZ_ClusterKey);
				Factory.AddFetchHint(CusContainerInvoiceLinePivotSchema.C2_ClusterKey, BusinessObject.JZ_ClusterKey);
				Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_ClusterKey, BusinessObject.JZ_ClusterKey);
				Factory.AddFetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, BusinessObject.JZ_ClusterKey);

				if (AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(BusinessObject.CountryCode))
				{
					Factory.AddFetchHint(CusUnderbondDecSchema.BU_ClusterKey, BusinessObject.JZ_ClusterKey);
				}
			}

			BusinessObject.HasFetchForLoadChildEditableObjectsBeenCalled = true;
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(OrgHeader), BusinessObject.JZ_OH_Supplier);
			AddFetchHintForPackInvoiceHeaderPivot();
			var invoiceLines = BusinessObject.JobComInvoiceLines; // cause JobComInvoiceLines to be part of Children for fetch validation to work
			_ = BusinessObject.Charges; // cause Charges to be part of Children for fetch validation to work

			if (invoiceLines.Any() && invoiceLines[0].UseUniversalConditionCheck)
			{
				var conditionCriteriaSets = GetTariffAndConditionCriteriaSetsForAllInvoiceLines(invoiceLines);
				ConditionLoader.CacheDataForMultipleCriteriaSets(conditionCriteriaSets);
			}
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
			Factory.AddFetchHint(CusPackingListSchema.CUL_JZ, BusinessObject.PK);
			AddFetchHintForPackInvoiceHeaderPivot();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case BaseJobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice:
						Factory.AddFetchHint(JobComInvoiceHeaderSchema.PK, BusinessObject.JZ_JZ_GroupInvoiceFK);
						break;
					case BaseJobComInvoiceHeader.Schema.JZ_Calc_OFTInInvoiceCurrency:
					case BaseJobComInvoiceHeader.Schema.JZ_Calc_ONSInInvoiceCurrency:
						Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
						break;
				}
			}
		}

		void AddFetchHintForPackInvoiceHeaderPivot()
		{
			if (BusinessObject.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				Factory.AddFetchHint(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_ClusterKey, BusinessObject.JZ_ClusterKey);
			}
		}

		ApplicableConditionLoader ConditionLoader => conditionLoader ?? (conditionLoader = new ApplicableConditionLoader(Factory));
		ApplicableConditionLoader conditionLoader;

		IEnumerable<ConditionLoadTariffCriteriaSet> GetTariffAndConditionCriteriaSetsForAllInvoiceLines(BaseJobComInvoiceLineViewCollection invoiceLines)
		{
			var conditionCriteriaSets = new List<ConditionLoadTariffCriteriaSet>();

			foreach (BaseJobComInvoiceLine invoiceLine in invoiceLines)
			{
				var universalTariff = invoiceLine?.UniversalTariff;
				if (universalTariff != null)
				{
					foreach (var conditionSelectionCriteria in invoiceLine.ConditionSelectionCriterias)
					{
						conditionCriteriaSets.Add(new ConditionLoadTariffCriteriaSet(universalTariff, conditionSelectionCriteria));
					}
				}
			}

			return conditionCriteriaSets;
		}
	}
}
