using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsProductParamsByWhsAndClient : AutoWhsProductParamsByWhsAndClient, IWhsProductParamsByWhsAndClient
	{
		#region Schema

		public abstract new class Schema : AutoWhsProductParamsByWhsAndClient.Schema
		{
			public const string UQ = "UQ";
			public const string PickGroupForBinding = "PickGroupForBinding";
		}

		#endregion

		#region Constructors

		public WhsProductParamsByWhsAndClient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Related Entities

		#region Warehouse

		[ActionFieldFollow(false)]
		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(W3_WW);

		#endregion

		#region StagingLocationBOM

		[ActionFieldFollow(false)]
		public WhsLocation StagingLocationBOM => Factory.Load<WhsLocation>(W3_WL_StagingLocationBOM);

		#endregion

		#region InwardProcessingStagingLocationBOM

		[ActionFieldFollow(false)]
		public WhsLocation InwardProcessingStagingLocationBOM => Factory.Load<WhsLocation>(W3_WL_InwardsProcessingStagingLocationBOM);

		#endregion

		#region DynamicPickFaceArea

		[ActionFieldFollow(false)]
		public WhsArea DynamicPickFaceArea
		{
			get { return Factory.Load<WhsArea>(W3_WA_DynamicPickFaceArea); }
		}

		#endregion

		#region Product

		[ActionFieldFollow(false)]
		public WhsProduct Product
		{
			get
			{
				if (W3_OP.IsValid)
				{
					if (product == null || W3_OP != product.Parent.PK)
					{
						var part = SupplierPart;
						if (part != null)
						{
							product = WhsProduct.GetWhsProduct(part);
						}
					}
				}
				else
				{
					product = null;
				}
				return product;
			}
		}

		WhsProduct product;

		#endregion

		#region Client

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(W3_OH); }
		}

		#endregion

		#endregion

		#region Properties

		#region W3_EconomicQuantity

		public override ZDecimal W3_EconomicQuantity
		{
			get { return base.W3_EconomicQuantity; }
			set
			{
				base.W3_EconomicQuantity = value;

				if (base.W3_EconomicQuantity > 0 && W3_ReplenishmentMultiple <= 0)
				{
					W3_ReplenishmentMultiple = 1;
				}
				Validation.ValidateW3_ReplenishmentMinimum();
				Validation.ValidateW3_ReplenishmentMultiple();
			}
		}

		#endregion

		#region W3_ReplenishmentMinimum

		public override ZDecimal W3_ReplenishmentMinimum
		{
			get { return base.W3_ReplenishmentMinimum; }
			set
			{
				if (base.W3_ReplenishmentMinimum != value)
				{
					base.W3_ReplenishmentMinimum = value;
					Validation.ValidateW3_EconomicQuantity();
				}
			}
		}

		#endregion

		#region W3_EconomicQuantity

		public override ZDecimal W3_ReplenishmentMultiple
		{
			get { return base.W3_ReplenishmentMultiple; }
			set
			{
				if (base.W3_ReplenishmentMultiple != value)
				{
					base.W3_ReplenishmentMultiple = value;
					Validation.ValidateW3_EconomicQuantity();
				}
			}
		}

		#endregion

		#region W3_ABCAnalysisCategory

		public ZString W3_ABCAnalysisCategory
		{
			get
			{
				var relatedABCCategory = RelatedABCCategory;
				return relatedABCCategory != null ? relatedABCCategory.WJ_Category : ZString.Empty;
			}
		}

		#region RelatedABCCategory

		WhsABCCategory RelatedABCCategory => WhsABCCategory.GetABCCategory(Factory, W3_OP, W3_OH, W3_WW);

		#endregion

		#endregion

		#region W3_ABCAnalysisPeriod

		public ZString W3_ABCAnalysisPeriod
		{
			get
			{
				var relatedABCCategory = RelatedABCCategory;
				return relatedABCCategory != null ? relatedABCCategory.ABCAnalysisPeriodAsString : "";
			}
		}

		#endregion

		#region W3_MaximumShelfLife

		[ReadOnlyMember(nameof(MaximumShelfLifeReadOnly))]
		public override ZShort W3_MaximumShelfLife
		{
			get { return base.W3_MaximumShelfLife; }
			set { base.W3_MaximumShelfLife = value; }
		}

		#endregion

		#region W3_PickGroup

		public override ZShort W3_PickGroup
		{
			get { return base.W3_PickGroup; }
			set
			{
				base.W3_PickGroup = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePickGroup();
				}

				PickGroupForBindingInfo.RefreshBinding();
			}
		}

		#endregion

		#region PickGroupForBinding

		[BusinessObjectTestExclude]
		[List("Lookups.PickGroups")]
		[MaxLength(PickGroupHelper.PickGroupForBindingMaxLength)]
		public ZString PickGroupForBinding
		{
			get { return PickGroupHelper.GetPickGroupDescription(W3_PickGroup, Lookups.PickGroups); }
			set { W3_PickGroup = PickGroupHelper.GetPickGroupFromString(value); }
		}

		public ZPropertyInfo PickGroupForBindingInfo
		{
			get { return GetZPropertyInfo(Schema.PickGroupForBinding, W3_PickGroupInfo.HumanReadableName); }
		}

		#endregion

		#region W3_OH

		[ActionField(ReadOnly = true)]
		[List("Lookups.Headers")]
		public override ZGuid W3_OH
		{
			get { return base.W3_OH; }
			set
			{
				var oldValue = base.W3_OH;
				base.W3_OH = value;

				if (value != oldValue)
				{
					SetDefaultExpiryNotificationPeriod();
				}
			}
		}

		#endregion

		#region W3_WW

		[RelatedBusinessObject("Warehouse")]
		[List("Lookups.Warehouses")]
		public override ZGuid W3_WW
		{
			get { return base.W3_WW; }
			set
			{
				if (W3_WW != value)
				{
					base.W3_WW = value;
					SetDefaultExpiryNotificationPeriod();
				}
			}
		}

		void SetDefaultExpiryNotificationPeriod()
		{
			var client = Client;

			if (W3_ExpiryNotificationPeriod == 0
				&& W3_WW.IsValid && !W3_WWInfo.HasErrors()
				&& client != null && !W3_OHInfo.HasErrors())
			{
				W3_ExpiryNotificationPeriod = client.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays;
			}
		}

		#endregion

		#region W3_OP

		public override ZGuid W3_OP
		{
			get { return base.W3_OP; }
			set
			{
				if (value != W3_OP)
				{
					base.W3_OP = value;
					product = null;
				}
			}
		}

		#endregion

		#region W3_WL_StagingLocationBOM

		[List("Lookups.StagingLocationsBOM")]
		public override ZGuid W3_WL_StagingLocationBOM
		{
			get { return base.W3_WL_StagingLocationBOM; }
			set { base.W3_WL_StagingLocationBOM = value; }
		}

		#endregion

		#region W3_WL_InwardsProcessingStagingLocationBOM

		[List("Lookups.StagingLocationsBOM")]
		public override ZGuid W3_WL_InwardsProcessingStagingLocationBOM
		{
			get { return base.W3_WL_InwardsProcessingStagingLocationBOM; }
			set { base.W3_WL_InwardsProcessingStagingLocationBOM = value; }
		}

		#endregion

		#region W3_WA_DynamicPickFaceArea

		[List("Lookups.DynamicPickFaceAreas")]
		public override ZGuid W3_WA_DynamicPickFaceArea
		{
			get { return base.W3_WA_DynamicPickFaceArea; }
			set { base.W3_WA_DynamicPickFaceArea = value; }
		}

		#endregion

		#region W3_StockTakeCycle

		[List("Lookups.StockTakeCycles")]
		public override ZString W3_StockTakeCycle
		{
			get { return base.W3_StockTakeCycle; }
			set { base.W3_StockTakeCycle = value; }
		}

		#endregion

		#region W3_F3_NKReceivedPackType

		[List("Lookups.PackTypes")]
		public override ZString W3_F3_NKReceivedPackType
		{
			get { return base.W3_F3_NKReceivedPackType; }
			set { base.W3_F3_NKReceivedPackType = value; }
		}

		#endregion

		#region W3_F3_NKReleasedPackType

		[List("Lookups.PackTypes")]
		public override ZString W3_F3_NKReleasedPackType
		{
			get { return base.W3_F3_NKReleasedPackType; }
			set { base.W3_F3_NKReleasedPackType = value; }
		}

		#endregion

		#region W3_WPG_PutawayGroup

		[List("Lookups.PutawayGroups")]
		public override ZGuid W3_WPG_PutawayGroup
		{
			get => base.W3_WPG_PutawayGroup;
			set => base.W3_WPG_PutawayGroup = value;
		}

		public WhsPutawayGroup PutawayGroup => Factory.Load<WhsPutawayGroup>(W3_WPG_PutawayGroup);

		#endregion

		#region UQ

		[ReadOnly(true)]
		public ZString UQ
		{
			get { return SupplierPart != null ? SupplierPart.OP_StockKeepingUnit : ZString.Empty; }
		}

		public ZPropertyInfo UQInfo
		{
			get { return GetZPropertyInfo(Schema.UQ); }
		}

		#endregion

		#region ReadOnly

		bool MaximumShelfLifeReadOnly
		{
			get
			{
				var result = false;
				var whsProduct = Product;

				if (whsProduct != null)
				{
					var client = Client;
					result = !whsProduct.IsAJulianBatchNumberAttributeUsed(client)
						&& (!whsProduct.IsPackingDateUsed(client) || !whsProduct.IsExpiryDateUsed(client));
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Delete

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (!CanDelete)
				{
					result = ResString.GetMultilingualString("cd1ac61e-2b0c-4b27-9bd2-745d7f0fda10", "There is current inventory using Julian Batch numbers. This inventory must be removed from the warehouse before this record can be deleted.");
				}
				return result;
			}
		}

		public override bool CanDelete
		{
			get
			{
				var whsProduct = Product;
				var originalClient = Factory.Load<OrgHeader>((ZGuid)W3_OHInfo.OriginalValue);
				var originalWhs = Factory.Load<WhsWarehouse>((ZGuid)W3_WWInfo.OriginalValue);
				return base.CanDelete && (whsProduct == null || !whsProduct.IsAJulianBatchNumberAttributeUsedAndHasAnyStock(originalClient, originalWhs));
			}
		}

		#endregion
	}
}
