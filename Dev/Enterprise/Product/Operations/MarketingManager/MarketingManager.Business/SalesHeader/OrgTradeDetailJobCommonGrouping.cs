using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgTradeDetailJobCommonGrouping : AutoOrgTradeDetailJobCommonGrouping, ISalesValue
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OrgTradeDetailJobCommonGrouping(EntityTradeDetailWrapperCollectionCompanyView entityTradeDetails, OrgSalesProduct product)
			: base(entityTradeDetails.Factory)
		{
			Argument.NotNull(entityTradeDetails, "tradeDetailCollection");
			Argument.NotNull(product, "product");

			this.entityTradeDetails = entityTradeDetails;
			this.Product = product;
			this.elements = new OrgTradeDetailJobCommonGroupingElements(this);
			this.RegisterEditableChildObject(this.elements);

			if (Product.MP_Code == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				if (TradeTypes.ContainsCode(OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import))
				{
					TradeType = OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import;
				}
				else if (TradeTypes.Count == 1)
				{
					TradeType = ((ICodeDescription)TradeTypes[0]).Code;
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OrgTradeDetailJobCommonGrouping(EntityTradeDetailWrapperCollectionCompanyView entityTradeDetails,
			OrgSalesProduct product,
			ZString tradeMode,
			ZString tradeType,
			IEnumerable<EntityTradeDetailWrapper> tradeDetails)
			: this(entityTradeDetails, product)
		{
			TradeMode = tradeMode;
			TradeType = tradeType;
			Elements.AddRange(tradeDetails);
			RefreshSalesAssociationPivotCollectionFilters(null, true);
			entityTradeDetails.Factory.Saved += RefreshSalesAssociationPivotCollectionFilters;
		}

		public readonly OrgSalesProduct Product;

		#region Properties

		#region TradeMode

		[List("TradeModes")]
		public override ZString TradeMode
		{
			get { return base.TradeMode; }
			set
			{
				base.TradeMode = value;

				if (Product.MP_Code == SystemDefinedSalesProductList.Codes.ForwardingShipment)
				{
					if (value == Constants.TransportModes.Sea)
					{
						TradeType = Constants.ContainerModes.FCL;
					}
					else if (value == Constants.TransportModes.Air)
					{
						TradeType = Constants.ContainerModes.Loose;
					}
				}

				foreach (OrgTradeDetail tradeDetail in Elements)
				{
					tradeDetail.PA_TradeMode = value;
				}
			}
		}

		public bool TradeMode_ReadOnly
		{
			get { return !IsEditable; }
		}

		#endregion

		#region TradeType

		[List("TradeTypes")]
		public override ZString TradeType
		{
			get { return base.TradeType; }
			set
			{
				base.TradeType = value;
				foreach (OrgTradeDetail tradeDetail in Elements)
				{
					tradeDetail.PA_TradeType = value;
				}
			}
		}

		public bool TradeType_ReadOnly
		{
			get { return !IsEditable; }
		}

		#endregion

		bool IsEditable
		{
			get
			{
				return (EntityTradeDetails.EntitySales == null || EntityTradeDetails.EntitySales.IsEditable)
					|| Elements.Count == 0
					|| Elements.Cast<OrgTradeDetail>().All(x => !x.IsInDatabase);
			}
		}

		#endregion

		#region Collections

		public EntityTradeDetailWrapperCollectionCompanyView EntityTradeDetails => entityTradeDetails;
		readonly EntityTradeDetailWrapperCollectionCompanyView entityTradeDetails;

		public OrgTradeDetailJobCommonGroupingElements Elements => elements;
		readonly OrgTradeDetailJobCommonGroupingElements elements;

		public SalesValueAssociationPivotCollection SalesAssociationPivotCollectionCompanyView
		{
			get { return salesAssociationPivotCollectionCompanyView ?? (salesAssociationPivotCollectionCompanyView = new OrgTradeDetailJobCommonGroupingAssociationPivotCollection(this, applyCompanyFilter: true)); }
		}
		OrgTradeDetailJobCommonGroupingAssociationPivotCollection salesAssociationPivotCollectionCompanyView;

		public SalesValueAssociationPivotCollection SalesAssociationPivotCollectionGlobal
		{
			get { return salesAssociationPivotCollectionGlobal ?? (salesAssociationPivotCollectionGlobal = new OrgTradeDetailJobCommonGroupingAssociationPivotCollection(this, applyCompanyFilter: false)); }
		}
		OrgTradeDetailJobCommonGroupingAssociationPivotCollection salesAssociationPivotCollectionGlobal;

		void RefreshSalesAssociationPivotCollectionFilters(BusinessObjectFactory o, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				return;
			}

			((OrgTradeDetailJobCommonGroupingAssociationPivotCollection)SalesAssociationPivotCollectionCompanyView).SetAdditionalFilter(this);
			((OrgTradeDetailJobCommonGroupingAssociationPivotCollection)SalesAssociationPivotCollectionGlobal).SetAdditionalFilter(this);
		}

		#endregion

		#region Lookups

		#region TradeModes

		public ICodeDescriptionPairList TradeModes
		{
			get { return OrgTradeDetailLookups.GetTradeModes(Product.MP_Code); }
		}

		#endregion

		#region TradeTypes

		public ICodeDescriptionPairList TradeTypes
		{
			get { return OrgTradeDetailLookups.GetTradeTypes(Product.MP_Code, TradeMode); }
		}

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			foreach (var element in Elements.ToArray())
			{
				element.Delete();
			}
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && IsEditable; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("4c83d47d-5278-4df6-b5b8-93cda2def85f", "This trade mode / type is linked to another opportunity"); }
		}

		#endregion

		#region ISalesValue

		string ISalesValue.TablePrefix => OrgTradeDetailSchema.Constants.Prefix;

		ZDateTime IAuditDetails.SystemCreateTimeUtc => ZDateTime.Empty;

		ZString IAuditDetails.SystemCreateUser => ZString.Empty;

		ZDateTime IAuditDetails.SystemLastEditTimeUtc => ZDateTime.Empty;

		ZString IAuditDetails.SystemLastEditUser => ZString.Empty;

		#endregion
	}
}
