using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailCloner : NonPersistentBusinessObject
	{
		public TradeDetailCloner(OrgOpportunity sourceOpportunity, OrgOpportunity targetOpportunity)
		{
			this.sourceOpportunity = sourceOpportunity;
			this.targetOpportunity = targetOpportunity;
		}

		readonly OrgOpportunity sourceOpportunity;
		readonly OrgOpportunity targetOpportunity;

		public TradeDetailCloneItemCollection SourceTradeDetails
		{
			get
			{
				if (sourceTradeDetails == null)
				{
					sourceTradeDetails = new TradeDetailCloneItemCollection();

					var query = new ZDBOnlyQuery(typeof(OrgTradeDetail));
					var hasAssociationPivotQuery = new ZDBOnlySubQuery(typeof(OrgSalesValueAssociationPivot), OrgSalesValueAssociationPivotSchema.SVP_TradeId);
					hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgTradeDetailSchema.Constants.Prefix);
					hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, sourceOpportunity.TablePrefix);
					hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, sourceOpportunity.PK);
					query.AddSubQuery(hasAssociationPivotQuery, JoinCondition.And);
					var details = sourceOpportunity.Factory.Load<OrgTradeDetail>(query);

					foreach (OrgTradeDetail tradeDetail in details)
					{
						sourceTradeDetails.AddNew(tradeDetail);
					}

					RegisterEditableChildObject(sourceTradeDetails);
				}
				return sourceTradeDetails;
			}
		}
		TradeDetailCloneItemCollection sourceTradeDetails;

		public void SelectAll()
		{
			foreach (TradeDetailCloneItem item in SourceTradeDetails)
			{
				item.Selected = true;
			}
		}

		public void UnselectAll()
		{
			foreach (TradeDetailCloneItem item in SourceTradeDetails)
			{
				item.Selected = false;
			}
		}

		public void SelectUnsuccessful()
		{
			foreach (TradeDetailCloneItem item in SourceTradeDetails)
			{
				item.Selected = item.TradeDetail.IsUnsuccessful;
			}
		}

		public void CopySelectionToTarget()
		{
			var selectedSourceDetails = SourceTradeDetails.Cast<TradeDetailCloneItem>().Where(x => x.Selected);

			foreach (var sourceDetailProductGroup in selectedSourceDetails.GroupBy(x => x.TradeDetail.SalesProduct))
			{
				var salesHeader = ((SalesHeaderCollection)targetOpportunity.ProspectiveSalesHeaderCollection).AddNew((OrgSalesProduct)sourceDetailProductGroup.Key);

				foreach (var sourceDetail in sourceDetailProductGroup)
				{
					var sales = sourceDetail.TradeDetail.Sales;
					var entitySales = EntitySalesWrapper.Get(sales, targetOpportunity);
					salesHeader.EntitySalesCollectionProductView.Add(entitySales);

					var targetDetail = entitySales.EntityTradeDetailsCollection.AddNew();
					targetDetail.CopyPersistentValuesFrom(sourceDetail.TradeDetail, new BusinessObjectCloneArgs(new string[] { OrgTradeDetailSchema.Constants.PA_Status }, true));
					targetOpportunity.OnCopyOrgTradeDetail(targetDetail);

					var targetProspectDetail = targetDetail.ProspectDetail;
					targetProspectDetail.CopyPersistentValuesFrom(sourceDetail.TradeDetail.ProspectDetail,
						new BusinessObjectCloneArgs(new string[] {
						OrgTradeProspectSchema.Constants.PAP_PA,
						OrgTradeProspectSchema.Constants.PAP_ExpectedTradeStartDate,
						OrgTradeProspectSchema.Constants.PAP_ExpiryDate,
						OrgTradeProspectSchema.Constants.PAP_ExpiryReason
						}, true));
					targetProspectDetail.PAP_RecurrenceType = sourceDetail.RecurrenceType;
					targetProspectDetail.PAP_RC_NKContainer = sourceDetail.ContainerType;

					var targetProspectPeriod = targetDetail.CurrentProspectPeriod;
					targetProspectPeriod.CopyPersistentValuesFrom(sourceDetail.TradeDetail.CurrentProspectPeriod,
						new BusinessObjectCloneArgs(new string[] {
						OrgTradePeriodSchema.Constants.PAS_PA,
						OrgTradePeriodSchema.Constants.PAS_IsSuperseded,
						OrgTradePeriodSchema.Constants.PAS_IsExpired
						}, true));
					targetProspectPeriod.PAS_Units = sourceDetail.ContainerCount;
					targetProspectPeriod.PAS_Weight = sourceDetail.Weight;
					targetProspectPeriod.PAS_WeightUQ = sourceDetail.WeightUQ;
					targetProspectPeriod.PAS_Volume = sourceDetail.Volume;
					targetProspectPeriod.PAS_VolumeUQ = sourceDetail.VolumeUQ;
					targetProspectPeriod.PAS_RateOffered = sourceDetail.RateOffered;
					targetProspectPeriod.PAS_RepeatsMnth = sourceDetail.JobCount;
					targetProspectPeriod.PAS_RX_NKCurrency = sourceDetail.Currency;
					targetProspectPeriod.PAS_EstimatedProfit = sourceDetail.EstimatedValue;
					targetOpportunity.OnCopyOrgTradePeriod(targetProspectPeriod);
				}
			}

			targetOpportunity.UpdateEstimatedValue();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			foreach (TradeDetailCloneItem item in SourceTradeDetails)
			{
				item.RunPreSaveValidation();
			}
		}
	}
}
