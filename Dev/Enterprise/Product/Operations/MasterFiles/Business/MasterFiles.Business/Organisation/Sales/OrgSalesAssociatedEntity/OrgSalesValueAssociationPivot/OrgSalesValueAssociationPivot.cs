using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesValueAssociationPivot : AutoOrgSalesValueAssociationPivot
	{
		public OrgSalesValueAssociationPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public ISalesValue SalesValue
		{
			get { return Factory.Load(SVP_TradeTableCode, SVP_TradeId) as ISalesValue; }
			set
			{
				if (value != null)
				{
					SVP_TradeId = value.Identifier;
					SVP_TradeTableCode = value.TablePrefix;
				}
				else
				{
					SVP_TradeId = ZGuid.Empty;
					SVP_TradeTableCode = ZString.Empty;
				}
			}
		}

		public ISalesValueAssociatedEntity AssociatedEntity
		{
			get { return Factory.Load(SVP_ActivityTableCode, SVP_ActivityId) as ISalesValueAssociatedEntity; }
			set
			{
				if (value != null)
				{
					SVP_ActivityId = value.Identifier;
					SVP_ActivityTableCode = value.TablePrefix;
				}
				else
				{
					SVP_ActivityId = ZGuid.Empty;
					SVP_ActivityTableCode = ZString.Empty;
				}
			}
		}

		public ZDateTime AssociatedEntityLastEditTime
		{
			get
			{
				if (AssociatedEntity == null)
				{
					return ZDateTime.Empty;
				}

				return AssociatedEntity.SystemLastEditTimeUtc.ToLocalBranchTime();
			}
		}

		public ZString AssociatedEntityLastEditUser
		{
			get
			{
				if (AssociatedEntity == null)
				{
					return ZString.Empty;
				}

				return AssociatedEntity.SystemLastEditUser;
			}
		}

		public ZDateTime AssociatedDate
		{
			get
			{
				var salesValue = SalesValue;
				if (salesValue == null)
				{
					return ZDateTime.Empty;
				}

				var associatedEntity = AssociatedEntity;
				if (associatedEntity == null)
				{
					return ZDateTime.Empty;
				}

				return associatedEntity.GetDateAssociatedToSalesValue(salesValue);
			}
		}

		public ZString AssociatingUser
		{
			get
			{
				var salesValue = SalesValue;
				if (salesValue == null)
				{
					return ZString.Empty;
				}

				var associatedEntity = AssociatedEntity;
				if (associatedEntity == null)
				{
					return ZString.Empty;
				}

				return associatedEntity.GetUserThatAssociatedToSalesValue(salesValue);
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				var tradeLane = SalesValue as OrgSales;
				if (tradeLane != null && SVP_ActivityTableCode == OrgOpportunitySchema.Constants.Prefix)
				{
					tradeLane.OW_LatestProspectDate = ZDate.Today;
				}
			}

			base.OnSaving();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new SalesValueAssociationPivotFetchStrategy(this);
		}

		#endregion

		#region Static Methods

		public static ZQuery GetPivotsQuery(ISalesValueAssociatedEntity entity, ISalesValue salesValue)
		{
			Argument.NotNull(entity, "entity");
			Argument.NotNull(salesValue, "salesValue");

			var query = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, entity.TablePrefix);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, entity.Identifier);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, salesValue.TablePrefix);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeId, salesValue.Identifier);
			return query;
		}

		public static bool HasPivot(ISalesValueAssociatedEntity entity, ISalesValue salesValue)
		{
			Argument.NotNull(entity, "entity");
			Argument.NotNull(salesValue, "salesValue");

			var factory = entity.Factory;
			var pivotsQuery = GetPivotsQuery(entity, salesValue);
			return factory.Load<OrgSalesValueAssociationPivot>(pivotsQuery).Length > 0;
		}

		public static void AddPivotIfNotExist(ISalesValueAssociatedEntity entity, ISalesValue salesValue)
		{
			Argument.NotNull(entity, "entity");
			Argument.NotNull(salesValue, "salesValue");

			if (!HasPivot(entity, salesValue))
			{
				var factory = entity.Factory;
				var pivot = factory.New<OrgSalesValueAssociationPivot>();
				pivot.SVP_ActivityTableCode = entity.TablePrefix;
				pivot.SVP_ActivityId = entity.Identifier;
				pivot.SVP_TradeTableCode = salesValue.TablePrefix;
				pivot.SVP_TradeId = salesValue.Identifier;
			}
		}

		public static void DeleteAll(ISalesValueAssociatedEntity entity, ISalesValue salesValue)
		{
			Argument.NotNull(entity, "entity");
			Argument.NotNull(salesValue, "salesValue");

			var factory = entity.Factory;
			var pivotsQuery = GetPivotsQuery(entity, salesValue);
			foreach (var pivot in factory.Load<OrgSalesValueAssociationPivot>(pivotsQuery))
			{
				pivot.Delete();
			}
		}

		public static void DeleteAll(ISalesValue salesValue)
		{
			Argument.NotNull(salesValue, "salesValue");

			var associationPivotsQuery = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_TradeId, salesValue.Identifier);
			associationPivotsQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, salesValue.TablePrefix);
			var associationPivots = salesValue.Factory.Load<OrgSalesValueAssociationPivot>(associationPivotsQuery);
			foreach (var pivot in associationPivots)
			{
				pivot.Delete();
			}
		}

		#endregion
	}
}
