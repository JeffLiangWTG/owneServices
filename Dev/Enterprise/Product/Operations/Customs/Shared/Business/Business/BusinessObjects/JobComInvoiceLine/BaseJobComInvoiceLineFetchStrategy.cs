using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class JobComInvoiceLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(BaseJobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected new internal BaseJobComInvoiceLine BusinessObject
		{
			get { return (BaseJobComInvoiceLine)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(BaseJobComInvoiceHeader), BusinessObject.JI_JZ);
			if (BusinessObject.JI_ParentID.IsEmpty)
			{
				var partPK = BusinessObject.JI_OP;
				if (partPK.IsValid)
				{
					Factory.AddFetchHint(typeof(OrgSupplierPart), partPK);
					Factory.AddFetchHint(OrgPartRelationSchema.OU_OP, partPK);
					var query = new ZQuery(CusClassPartPivotSchema.CI_OP, partPK);
					query.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, BusinessObject.CustomsCountryCode);
					Factory.AddFetchHint(typeof(BaseCusClassPartPivot), query);
				}
				Factory.AddFetchHint(OrgSupplierPartSchema.OP_PartNum, BusinessObject.JI_PartNo);
			}
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(BaseCusClassification), BusinessObject.JI_CC);
			Factory.AddFetchHint(typeof(CusEntryLine), BusinessObject.JI_CL);
			Factory.AddFetchHint(RefCountrySchema.RN_Code, BusinessObject.JI_CountryOfOrigin);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, BusinessObject.PK);
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);

			if (BusinessObject.SupportRulingConfigurations)
			{
				Factory.AddFetchHint(CusRulingConfigCombinedSchema.ZZY_JI_InvoiceLine, BusinessObject.PK);
			}
			if (BusinessObject.SupportsAdditionalTariffs)
			{
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
			}

			if (BusinessObject is ICusAddInfoTypeSupporter)
			{
				Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
			}
			if (BusinessObject is ICusCodeDataTypeSupporter)
			{
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			}
			if (BusinessObject.VehicleRelationship != VehicleRelationshipType.None)
			{
				Factory.AddFetchHint(CusVehicleSchema.CVH_ParentID, BusinessObject.PK);
			}

			if (!(BusinessObject.InvoiceHeader?.HasFetchForLoadChildEditableObjectsBeenCalled ?? false) &&
				!(BusinessObject.Declaration?.HasFetchForLoadChildEditableObjectsBeenCalled ?? false))
			{
				var fetchOnlyFromLocalCache = !BusinessObject.IsInDatabase;
				Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(JobComInvLineComponentInventory), JobComInvLineComponentInventorySchema.JIV_ClusterKey, JobComInvLineComponentInventorySchema.JIV_JI, fetchOnlyFromLocalCache);

				if (BusinessObject.SupportInvoiceLineRefs)
				{
					Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(JobComInvLineRefs), JobComInvLineRefsSchema.JG_ClusterKey, JobComInvLineRefsSchema.JG_JI, fetchOnlyFromLocalCache);
				}

				if (!BusinessObject.ContainersPivotIsLoaded)
				{
					Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(CusContainerInvoiceLinePivot), CusContainerInvoiceLinePivotSchema.C2_ClusterKey, CusContainerInvoiceLinePivotSchema.C2_JI, fetchOnlyFromLocalCache);
				}

				if (BusinessObject.SupportsChcPivotBetweenInvoiceLineAndPacking)
				{
					Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(InvoiceLinePackagePivot), CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, CusHouseContPackInvoiceLinePivotSchema.CHC_JI, fetchOnlyFromLocalCache);
				}

				if (BusinessObject.Declaration?.SupportsJobComInvoiceLineTax ?? false)
				{
					Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(JobComInvoiceLineTax), JobComInvoiceLineTaxSchema.JLT_ClusterKey, JobComInvoiceLineTaxSchema.JLT_JI, fetchOnlyFromLocalCache);
				}

				if (BusinessObject.Declaration?.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine ?? false)
				{
					Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(AdditionalInvoiceLineEntryLineLink), CusUnderbondDecSchema.BU_ClusterKey, CusUnderbondDecSchema.BU_JI, fetchOnlyFromLocalCache);
				}
			}

			if (BusinessObject.JI_ParentID.IsValid)
			{
				Factory.AddFetchHint(typeof(BaseJobComInvoiceLine), BusinessObject.JI_ParentID);
			}
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(BaseJobComInvoiceLine), BusinessObject.JI_ParentID);
			if (BusinessObject.VehicleRelationship != VehicleRelationshipType.None)
			{
				Factory.AddFetchHint(CusVehicleSchema.CVH_ParentID, BusinessObject.PK);
			}
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			if (!BusinessObject.HasAddFetchForDeleteChildren)
			{
				BusinessObject.HasAddFetchForDeleteChildren = true;
				BusinessObject.FetchForLoadChildEditableObjectsIfNeeded();
				Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusPackableItemSchema.CUI_JI, BusinessObject.PK);
				FetchForDeleteChildren();
			}
		}

		#region FetchForDeleteChildren

		void FetchForDeleteChildren()
		{
			FetchForDeleteChildren(BusinessObject, false);
		}

		void FetchForDeleteChildren(BusinessObject bizObj, bool runFetch)
		{
			if (runFetch)
			{
				bizObj.FetchStrategy.FetchForDelete();
			}
			foreach (IBusiness child in ((IBusiness)bizObj).Children)
			{
				var childBO = child as BusinessObject;
				var childCollection = child as IBusinessObjectCollection;
				if (childBO != null)
				{
					FetchForDeleteChildren(childBO, true);
				}
				else if (childCollection != null)
				{
					foreach (BusinessObject bizo in childCollection)
					{
						FetchForDeleteChildren(bizo, true);
					}
				}
			}
		}

		#endregion

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case BaseJobComInvoiceLine.Schema.JI_Calc_Balance:
					case BaseJobComInvoiceLine.Schema.JI_Calc_LinesTotal:
						Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
						Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.JI_JZ);
						break;
				}
			}
		}
	}
}
