using System.Linq;
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
	public abstract class WhsAdjustmentAndPickableDocketLineDataObjectReader<TDocket, TDocketLine> : WhsDocketLineDataObjectReader<TDocket, TDocketLine>
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		protected WhsAdjustmentAndPickableDocketLineDataObjectReader(OrderLine docketLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, TDocket parent)
			: base(docketLineDataObject, logger, factory, parent)
		{
		}

		#region Matching Job (Line)

		protected override TDocketLine GetExistingBusinessObject()
		{
			return (dataObject.LineNumber.HasValue && dataObject.SubLineNumber.HasValue) ? (TDocketLine)GetChildDocketLineCollection().FirstOrDefault(l => l.WE_LineNo == dataObject.LineNumber.Value && l.WE_SubLineNo == dataObject.SubLineNumber.Value) : null;
		}

		protected virtual WhsDocketLineCollection GetChildDocketLineCollection() => Parent.Lines;

		#endregion

		#region Create / Update Job (Line)

		protected override void PopulateBusinessObjectCore(TDocketLine line)
		{
			SetValueWhenNotReadOnlyAndAllowToChange(line, WhsDocketLineSchema.WE_ExpiryDate, dataObject.ExpiryDate);
			if (ImportUnitPriceFields)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_ExtendedLinePrice, dataObject.ExtendedLinePrice);
			}

			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_LineComment, dataObject.LineComment);
			SetValue(line, WhsDocketLineSchema.WE_LineNo, dataObject.LineNumber);
			SetValueWhenNotReadOnlyAndAllowToChange(line, WhsDocketLineSchema.WE_PackingDate, dataObject.PackingDate);

			if (line.SupplierPart.IsInDatabase)
			{
				SetValueWhenNotReadOnlyAndAllowToChange(line, WhsDocketLineSchema.WE_PartAttrib1, dataObject.PartAttribute1);
				SetValueWhenNotReadOnlyAndAllowToChange(line, WhsDocketLineSchema.WE_PartAttrib2, dataObject.PartAttribute2);
				SetValueWhenNotReadOnlyAndAllowToChange(line, WhsDocketLineSchema.WE_PartAttrib3, dataObject.PartAttribute3);
				SetValueWhenNotReadOnlyAndAllowToChange(line, WhsDocketLineSchema.WE_SerialNumber, dataObject.SerialNumber);
			}
			else
			{
				SetValue(line, WhsDocketLineSchema.WE_PartAttrib1, dataObject.PartAttribute1);
				SetValue(line, WhsDocketLineSchema.WE_PartAttrib2, dataObject.PartAttribute2);
				SetValue(line, WhsDocketLineSchema.WE_PartAttrib3, dataObject.PartAttribute3);
				SetValue(line, WhsDocketLineSchema.WE_SerialNumber, dataObject.SerialNumber);
			}

			var subLineNo = dataObject.SubLineNumber;
			if (subLineNo == null)
			{
				line.WE_SubLineNo = -1;
				subLineNo = line.GetSubLineNo();
			}
			SetValue(line, WhsDocketLineSchema.WE_SubLineNo, subLineNo);
			SetTransactionQuantity(line);
			if (dataObject.PackageQtyUnit != null && dataObject.PackageQtyUnit.Code.HasValue && !dataObject.PackageQtyUnit.Code.Value.IsEmpty)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_F3_NKPackType, line.Lookups.PackTypesWithStandardUnits, dataObject.PackageQtyUnit);
			}
		}

		protected virtual bool ImportUnitPriceFields => true;

		#region PopulateProduct

		protected override void SetProduct(TDocketLine docketLine, OrgSupplierPart product)
		{
			SetValueWhenNotReadOnlyAndAllowToChange(docketLine, WhsDocketLineSchema.WE_OP, product.PK);
		}

		#endregion

		#region SetTransactionQuantity

		void SetTransactionQuantity(TDocketLine line)
		{
			CheckTransactionQuantity(line);
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_TransactionQuantity, dataObject.OrderedQty);
		}

		protected virtual void CheckTransactionQuantity(TDocketLine line)
		{
		}

		#endregion

		#endregion

		#region SetValueWhenNotReadOnlyAndAllowToChange

		protected virtual void SetValueWhenNotReadOnlyAndAllowToChange(TDocketLine line, SchemaGuidColumn column, ZGuid? value) => SetValueWhenNotReadOnlyOrCustomsTransaction(line, column, value);

		protected virtual void SetValueWhenNotReadOnlyAndAllowToChange(TDocketLine line, SchemaStringColumn column, ZString? value) => SetValueWhenNotReadOnlyOrCustomsTransaction(line, column, value);

		protected virtual void SetValueWhenNotReadOnlyAndAllowToChange(TDocketLine line, SchemaDateTimeColumn column, ZDateTime? value) => SetValueWhenNotReadOnlyOrCustomsTransaction(line, column, value);

		#endregion
	}
}
