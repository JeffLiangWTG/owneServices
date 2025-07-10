using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SalesHeaderCollection : NonPersistentBusinessObjectCollection<SalesHeader>, ISalesHeaderCollection
	{
		#region Constructor

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public SalesHeaderCollection(ISalesValueAssociatedEntity entity, bool includeTraded = true)
			: base(entity.Factory)
		{
			Argument.NotNull(entity, "entity");

			this.entity = entity;
			this.includeTraded = includeTraded;

			if (entity is OrgHeader)
			{
				Org = (OrgHeader)entity;
			}
			else if (entity.OrgPkInfo != null)
			{
				Org = Factory.Load<OrgHeader>((ZGuid)entity.OrgPkInfo.Value);

				entity.OrgPkInfo.ValueChanged += (sender, e) =>
				{
					if (entity.OrgPkInfo != null)
					{
						Org = Factory.Load<OrgHeader>((ZGuid)entity.OrgPkInfo.Value);
					}
				};
			}
		}

		readonly bool includeTraded;

		#endregion

		#region Indexer

		ISalesHeader ISalesHeaderCollection.this[int i]
		{
			get { return base[i]; }
		}

		#endregion

		#region Allowed Actions

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region Entity

		public ISalesValueAssociatedEntity Entity
		{
			get { return entity; }
		}
		readonly ISalesValueAssociatedEntity entity;

		#endregion

		#region Org

		public OrgHeader Org
		{
			get { return org; }
			private set
			{
				if (org != value)
				{
					org = value;
					Rebuild();
				}
			}
		}
		OrgHeader org;

		#endregion

		#region SalesCollection

		void SetSalesCollections(EntitySalesWrapperCollection prospectCollection, TradedSalesCollection tradedCollection)
		{
			if (entitySalesCollection != null)
			{
				((IBindingList)entitySalesCollection).ListChanged -= SalesCollection_ListChanged;
			}
			if (tradedSalesCollection != null)
			{
				((IBindingList)tradedSalesCollection).ListChanged -= SalesCollection_ListChanged;
			}

			RemoveAll();
			entitySalesCollection = prospectCollection;
			tradedSalesCollection = tradedCollection;
			Refresh();

			if (entitySalesCollection != null)
			{
				((IBindingList)entitySalesCollection).ListChanged += SalesCollection_ListChanged;
			}
			if (tradedSalesCollection != null)
			{
				((IBindingList)tradedSalesCollection).ListChanged += SalesCollection_ListChanged;
			}
		}

		public EntitySalesWrapperCollection EntitySalesCollection
		{
			get { return entitySalesCollection; }
		}
		EntitySalesWrapperCollection entitySalesCollection;

		TradedSalesCollection TradedSalesCollection
		{
			get { return tradedSalesCollection; }
		}
		TradedSalesCollection tradedSalesCollection;

		void SalesCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!inRefresh && e.ListChangedType == ListChangedType.Reset)
			{
				Refresh(true);
			}
		}

		#endregion

		#region Refresh/Rebuild

		public void Rebuild()
		{
			var prospectCollection = org != null ? new EntitySalesWrapperCollection(entity) : null;
			var tradedCollection = includeTraded && org != null ? new TradedSalesCollection(org) : null;
			SetSalesCollections(prospectCollection, tradedCollection);
		}

		public void Refresh(bool fromListChangedEvent = false)
		{
			try
			{
				inRefresh = true;

				using (SuspendListChanged())
				{
					var existingHeaderProductsBySalesHeader = this.Cast<SalesHeader>().ToDictionary(x => x.SalesProduct);

					if (!fromListChangedEvent && EntitySalesCollection != null)
					{
						EntitySalesCollection.Load();
					}

					var entitySalesProductCodes = EntitySalesCollection == null ?
						Enumerable.Empty<ZGuid>() :
						EntitySalesCollection.Cast<EntitySalesWrapper>().Select(x => x.OW_MP_Product).Where(x => x.IsValid);
					var tradedSalesProductCodes = TradedSalesCollection == null ?
						Enumerable.Empty<ZGuid>() :
						TradedSalesCollection.Cast<OrgSales>().Select(x => x.OW_MP_Product).Where(x => x.IsValid);

					var targetProductCodes = entitySalesProductCodes.Union(tradedSalesProductCodes).Distinct();
					var targetProducts = Factory.Load<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.PK, targetProductCodes));

					foreach (var targetProduct in targetProducts.Where(x => x != null))
					{
						if (!existingHeaderProductsBySalesHeader.ContainsKey(targetProduct))
						{
							AddNew(targetProduct);
						}

						existingHeaderProductsBySalesHeader.Remove(targetProduct);
					}

					if (!fromListChangedEvent)
					{
						foreach (var unusedSalesHeader in existingHeaderProductsBySalesHeader.Values)
						{
							Remove(unusedSalesHeader);
						}
					}

					if (!IsValidationSuspended)
					{
						ValidateAllCurrencies();
					}
				}
			}
			finally
			{
				inRefresh = false;
			}
		}
		bool inRefresh;

		public void ValidateAllCurrencies()
		{
			if (Entity is OrgOpportunity)
			{
				foreach (SalesHeader salesHeader in this)
				{
					foreach (var sales in salesHeader.EntitySales)
					{
						Factory.AddFetchHint(OrgTradeDetailSchema.PA_OW, sales.PK);
					}
				}

				foreach (SalesHeader salesHeader in this)
				{
					foreach (var sales in salesHeader.EntitySales)
					{
						foreach (var tradeDetail in sales.EntityTradeDetails)
						{
							Factory.AddFetchHint(OrgTradePeriodSchema.PAS_PA, tradeDetail.PK);
						}
					}
				}

				foreach (SalesHeader salesHeader in this)
				{
					foreach (var sales in salesHeader.EntitySales)
					{
						foreach (var tradeDetail in sales.EntityTradeDetails)
						{
							Factory.AddFetchHint(RefCurrencySchema.RX_Code, tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency);
						}
					}
				}

				foreach (SalesHeader salesHeader in this)
				{
					foreach (var sales in salesHeader.EntitySales)
					{
						foreach (var tradeDetail in sales.EntityTradeDetails)
						{
							((EntityTradePeriod)tradeDetail.CurrentProspectPeriod).Validation.ValidatePAS_RX_NKCurrency();
						}
					}
				}
			}
		}

		#endregion

		#region AddNew

		public SalesHeader AddNew(OrgSalesProduct product)
		{
			var result = new SalesHeader(EntitySalesCollection, TradedSalesCollection, product, entity);
			Add(result);
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Only allowed to add SalesHeaders with a specified sales product");
		}

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("c27bd97a-8986-4593-afb5-e988aac6176d", "Trade Lanes"); }
		}

		#endregion

		#region DataTable

		protected override ZDataTable Table
		{
			get { return new ZDataTable("SalesHeader"); }
		}

		#endregion

		#region Superceding Info

		public ZString SupercedingWarningMessage
		{
			get
			{
				var builder = new ZStringBuilder();

				var newCreatedOrStatusUpdatedDetails = EntitySalesCollection
					.Cast<EntitySalesWrapper>()
					.SelectMany(x => x.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>())
					.Where(x => !x.IsInDatabase || (x.PA_StatusInfo.HasChanges && x.PA_Status == OpportunityTradeStatus.Codes.Successful));

				foreach (var detail in newCreatedOrStatusUpdatedDetails)
				{
					var superceder = new OrgTradeDetailValueSuperceder(detail, detail.ProspectPeriodStart);
					var message = superceder.SupercedingWarningMessage;
					if (!message.IsEmpty)
					{
						builder.Append(message);
					}
				}
				return builder.ToString();
			}
		}

		#endregion

		#region ISalesHeaderCollection Members

		public ZDecimal TotalValue => this.Cast<SalesHeader>().Sum(x => x.TotalEstimatedAnnualValue);
		public ZDecimal CommittedValue => this.Cast<SalesHeader>().Sum(x => x.CommittedAnnualValue);
		public ZDecimal PipelineValue => this.Cast<SalesHeader>().Sum(x => x.PipelineValue);
		public ZDecimal UnsuccessfulValue => this.Cast<SalesHeader>().Sum(x => x.UnsuccessfulValue);

		#endregion
	}
}
