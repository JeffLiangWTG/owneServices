using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderLineDataObjectReader : WhsPickableDocketLineDataObjectReader<WhsOrder, WhsOrderLine>
	{
		internal WhsOrderLineDataObjectReader(OrderLine orderLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsOrder parent, IEnumerable<WhsOrderLine> matchedLines, ExtraOrderLineDetails extraOrderLineDetails = null)
			: base(orderLineDataObject, logger, factory, parent, matchedLines)
		{
			ExtraOrderLineDetails = extraOrderLineDetails;
		}

		ExtraOrderLineDetails ExtraOrderLineDetails { get; }

		#region Create / Update Job (Line)

		protected override void PopulateBusinessObjectCore(WhsOrderLine orderLine)
		{
			base.PopulateBusinessObjectCore(orderLine);

			if (IsDataSourceCustoms)
			{
				orderLine.WE_AllocationKey = ExtraOrderLineDetails?.AllocationKey ?? ZString.Empty;
			}

			PopulateHoldCode(orderLine);
			SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_PalletID, dataObject.PalletID);
		}

		protected override void SetBondedEntryKey(WhsOrderLine line)
		{
			var previousBondedEntryKey = dataObject.CustomsData?.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine();
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_BondedEntryKey, previousBondedEntryKey); // order stores bondedEntryKey on Custom Attributs
		}

		protected override void ChangesAfterLinePopulation(WhsOrderLine line)
		{
			if (string.IsNullOrEmpty(line.CustomsData.WB_EntryKey) && GetParentCanFinaliseWithoutCustomsClearance())
			{
				line.WhsOrderLineToBeClearedLater = ZBool.True;
			}
		}

		bool GetParentCanFinaliseWithoutCustomsClearance() => factory.GetCachedValue(WhsOrderLine.Schema.WhsOrderLineToBeClearedLater + Parent.PK, () => WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(Parent));

		protected override void SetProduct(WhsOrderLine orderLine, OrgSupplierPart product)
		{
			base.SetProduct(orderLine, product);

			if (Parent != null)
			{
				var orderLineRow = GetColumnIndexerFromRow(orderLine);
				var orderRow = GetColumnIndexerFromRow(Parent);
				var defaultPickGroup = PickGroupLoader.GetDefaultPickGroup(factory.RowFactory, product.PK, orderRow.GetValue(WhsDocketSchema.WD_WW_Whs), orderRow.GetValue(WhsDocketSchema.WD_OH_Client));
				if (defaultPickGroup > 0)
				{
					SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, orderLineRow, WhsDocketLineSchema.WE_PickGroup, defaultPickGroup);
				}
			}
		}

		protected override string DocketLineType => Res.GetString("fc68ee86-bbca-45e9-ae1e-659ee4a09ac3", "Order");

		#endregion

		#region PopulateHoldCode

		void PopulateHoldCode(WhsOrderLine orderLine)
		{
			if (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value)
			{
				var orderedHoldCode = dataObject.CurrentHoldCode?.Code.GetValueOrDefault();
				if (orderedHoldCode.HasValue)
				{
					if (orderLine.IsInDatabase && IsColumnReadonly(orderLine, WhsDocketLineSchema.WE_WHC_NKOrderedHeldCode.Name))
					{
						throw GetNotAllowedToChangeRestrictedFieldsException();
					}
					else if (!orderLine.Lookups.InventoryHeldCodeCollection.ContainsCode(orderedHoldCode.Value))
					{
						throw new DataObjectReadFailureException(Res.GetString("ddb4305e-bff8-48a0-9554-d26e79c801f5", "Imported Hold Code '{0}' is not valid on this system.", orderedHoldCode.Value));
					}

					SetValueIfNotReadOnly(orderLine, WhsDocketLineSchema.WE_WHC_NKOrderedHeldCode, orderedHoldCode.Value);
				}
			}
		}

		#endregion

		#region Matching

		protected override WhsOrderLine GetExistingBusinessObject()
		{
			WhsOrderLine line;
			if (Parent.IsInDatabase && IsDataSourceCustoms && !CustomsHelper.IsWarehouseBondedChangeOfInventory)
			{
				line = ObjectFactory.Get<IWhsOrderLineMatcher>().FindExistingBizOByOrderLineNo(Parent, dataObject, GetProduct(), MatchedLines);
			}
			else
			{
				line = base.GetExistingBusinessObject();
			}

			return line;
		}

		#endregion

		#region ClientDescriptionForErrorMessage

		protected override ZString ClientDescriptionForErrorMessage => CustomsHelper?.IsWarehouseBondedChangeOfOwnership ?? false ? (ZString)Res.GetString("WhsOrderLineDataObjectReader|ChangeOfOwnership|OwnerDescription", "Old Owner") : base.ClientDescriptionForErrorMessage;

		#endregion

		#region CustomsHelper

		protected override CustomsDataSourceHelper<WhsOrder> GetNewCustomsDataSourceHelper(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext)
			=> new CustomsDataSourceHelperForOrder(Shipment, logger.TopLevelDataContext);

		#endregion

		#region SetValueWhenNotReadOnlyAndAllowToChange

		protected override void SetValueWhenNotReadOnlyAndAllowToChange(WhsOrderLine line, SchemaStringColumn column, ZString? value)
		{
			if (!IsSameValue(line, column, value))
			{
				base.SetValueWhenNotReadOnlyOrCustomsTransaction(line, column, value);
			}
		}

		protected override void SetValueWhenNotReadOnlyAndAllowToChange(WhsOrderLine line, SchemaDateTimeColumn column, ZDateTime? value)
		{
			if (!IsSameValue(line, column, value))
			{
				base.SetValueWhenNotReadOnlyOrCustomsTransaction(line, column, value);
			}
		}

		protected override string NotAllowedToChangeRestrictedFieldsMessage => Res.GetString("9dc48e9b-6dcb-483f-8770-3c20afc177a3", "Cannot change product or attributes for allocated order.");

		#endregion
	}
}
