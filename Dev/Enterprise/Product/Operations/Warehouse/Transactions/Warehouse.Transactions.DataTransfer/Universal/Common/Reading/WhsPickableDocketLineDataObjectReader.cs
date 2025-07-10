using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsPickableDocketLineDataObjectReader<TDocket, TDocketLine> : WhsAdjustmentAndPickableDocketLineDataObjectReader<TDocket, TDocketLine>
		where TDocket : WhsPickableDocket
		where TDocketLine : WhsPickableDocketLine
	{
		protected WhsPickableDocketLineDataObjectReader(
			OrderLine docketLineDataObject,
			IXmlImportLogger logger,
			UniversalObjectFactory factory,
			TDocket parent,
			IEnumerable<TDocketLine> matchedLines)
			: base(docketLineDataObject, logger, factory, parent)
		{
			MatchedLines = matchedLines;
		}
		protected IEnumerable<TDocketLine> MatchedLines { get; }

		#region PopulateBusinessObjectCore

		protected override void PopulateBusinessObjectCore(TDocketLine orderLine)
		{
			base.PopulateBusinessObjectCore(orderLine);
			if (ImportUnitPriceFields)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_RecommendedUnitPrice, dataObject.UnitPriceRecommended);
				SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_RX_NKUnitPriceCurrency, dataObject.UnitPriceCurrency);
				SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_UnitDiscountAmount, dataObject.UnitPriceDiscountAmount);
				SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_UnitDiscountPercent, dataObject.UnitPriceDiscountPercent);
				SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_UnitPriceAfterDiscount, dataObject.UnitPriceAfterDiscount);
			}
			SetValueWhenNotReadOnlyOrCustomsTransaction(orderLine, WhsDocketLineSchema.WE_TransactionQuantity, GetWE_TransactionQuantity(orderLine));
		}

		protected override void SetProduct(TDocketLine orderLine, OrgSupplierPart product)
		{
			using (orderLine.SetIsPopulatingFromUniversalDataObjectReader())
			{
				base.SetProduct(orderLine, product);
			}
		}

		protected override void CheckTransactionQuantity(TDocketLine line)
		{
			if (dataObject.OrderedQty < 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("58667eca-9aba-44fd-83c7-7fb4a75aaf98", "Cannot Import Order Line {0}:\r\nQuantity cannot be negative.",
					dataObject.LineNumber.GetValueOrDefault()));
			}
		}

		#endregion

		#region Shipment

		protected Shipment Shipment
		{
			get
			{
				var shipment = (Shipment)logger?.TopLevelDataObject;
				return shipment != null ? Shipment.GetSourceDataObject(shipment) : shipment;
			}
		}

		#endregion

		#region CustomsHelper

		protected override bool IsDataSourceCustoms => CustomsHelper?.IsDataSourceCustoms ?? false;

		protected CustomsDataSourceHelper<TDocket> CustomsHelper
		{
			get
			{
				if (customsHelper == null && logger?.TopLevelDataObject != null && logger?.TopLevelDataContext != null)
				{
					customsHelper = GetNewCustomsDataSourceHelper(Shipment, logger.TopLevelDataContext);
				}

				return customsHelper;
			}
		}

		CustomsDataSourceHelper<TDocket> customsHelper;
		protected abstract CustomsDataSourceHelper<TDocket> GetNewCustomsDataSourceHelper(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext);

		#endregion

		#region SetValueWhenNotReadOnlyAndAllowToChange

		protected override void SetValueWhenNotReadOnlyAndAllowToChange(TDocketLine line, SchemaGuidColumn column, ZGuid? value)
		{
			if (!IsSameValue(line, column, value))
			{
				base.SetValueWhenNotReadOnlyOrCustomsTransaction(line, column, value);
			}
		}

		protected bool IsSameValue(TDocketLine line, SchemaColumn column, IZType value)
		{
			var currentValue = line[column];
			var valueIsSame = value == null || currentValue.Equals(value);
			if (!valueIsSame)
			{
				RaiseAnExceptionIfNotAllowedToChangeRestrictedFields(line, column.Name);
			}
			return valueIsSame;
		}

		void RaiseAnExceptionIfNotAllowedToChangeRestrictedFields(TDocketLine line, string columnName)
		{
			if (line.IsInDatabase && IsColumnReadonly(line, columnName) && !IsAllowToChangeRestrictedFields)
			{
				throw GetNotAllowedToChangeRestrictedFieldsException();
			}
		}

		protected DataObjectReadFailureException GetNotAllowedToChangeRestrictedFieldsException() => new DataObjectReadFailureException(Res.GetString("4e7f5fb4-c38d-4a87-85d2-a15cb2739550", "Cannot Import {0} Line {1}\r\n{2}", DocketLineType, dataObject.Link, NotAllowedToChangeRestrictedFieldsMessage));

		protected abstract string NotAllowedToChangeRestrictedFieldsMessage { get; }

		protected virtual bool IsColumnReadonly(TDocketLine line, string columnName) => line.ZPropertyInfoHash[columnName].ReadOnly;

		bool IsAllowToChangeRestrictedFields => !Parent.WD_WP.IsValid || IsDataSourceCustoms;

		#endregion
	}
}
