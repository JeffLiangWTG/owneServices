using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDocketLineDataObjectWriter<T> : DataObjectWriter<T, OrderLine>
		where T : WhsDocketLine
	{
		protected WhsDocketLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region Export Job

		protected sealed override OrderLine PopulateDataObject(T docketLineBO)
		{
			var docketLineDataObject = new OrderLine(writeManager.WriterStrategy);
			var part = docketLineBO.SupplierPart;

			if (part != null)
			{
				docketLineDataObject.Commodity = ListHelper.GetWithDescription<Commodity>(docketLineBO.CommodityCode, part.Lookups.CommodityCodes);
				docketLineDataObject.SetUNDGCollection(() => ProcessCollection(part.UNDGs, new UNDGDataObjectWriter(writeManager)));
			}

			docketLineDataObject.ExpiryDate = docketLineBO.WE_ExpiryDate;
			docketLineDataObject.LineComment = docketLineBO.WE_LineComment;
			docketLineDataObject.LineNumber = docketLineBO.WE_LineNo;
			docketLineDataObject.PackingDate = docketLineBO.WE_PackingDate;
			docketLineDataObject.PackageQty = docketLineBO.WE_PackQuantity;
			docketLineDataObject.PackageQtyUnit = ListHelper.GetWithDescription<PackageType>(docketLineBO.WE_F3_NKPackType, docketLineBO.Lookups.PackTypes);
			docketLineDataObject.PartAttribute1 = docketLineBO.WE_PartAttrib1;
			docketLineDataObject.PartAttribute2 = docketLineBO.WE_PartAttrib2;
			docketLineDataObject.PartAttribute3 = docketLineBO.WE_PartAttrib3;
			docketLineDataObject.SerialNumber = docketLineBO.WE_SerialNumber;
			docketLineDataObject.Product = docketLineBO.ProductCode.IsEmpty ? null : new Product { Code = docketLineBO.ProductCode, Description = docketLineBO.ProductDesc };
			docketLineDataObject.SubLineNumber = docketLineBO.WE_SubLineNo;
			docketLineDataObject.OrderedQty = docketLineBO.WE_TransactionQuantity;
			docketLineDataObject.OrderedQtyUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(docketLineBO.ProductUQ, docketLineBO.Lookups.PackTypes);

			PopulateDataObject(docketLineDataObject, docketLineBO);
			PopulateCustomsData(docketLineBO, docketLineDataObject);
			PopulateOrganisationLevelCustomFields(docketLineBO, docketLineDataObject);

			return docketLineDataObject;
		}

		void PopulateCustomsData(T docketLineBO, OrderLine docketLineDataObject)
		{
			if (docketLineBO.IsCustomsTransaction)
			{
				PopulateCustomsDataCore(docketLineBO, docketLineDataObject);
			}
		}

		protected virtual void PopulateCustomsDataCore(T docketLineBO, OrderLine docketLineDataObject)
		{
			var writer = new WhsBondedWarehouseAttributeDataObjectWriter(writeManager);
			docketLineDataObject.CustomsData = writer.GetDataObject(docketLineBO.CustomsData);
		}

		void PopulateOrganisationLevelCustomFields(T docketLine, OrderLine docketLineDataObject)
		{
			var docket = docketLine.Docket;
			if (docket != null)
			{
				CustomLabelsCustomizedFieldDataObjectWriter.Write(WhsDocketLineSchema.Instance, docketLine, docketLineDataObject, new WhsDocketLine.CustomLabelsProvider(docket));
			}
		}

		protected abstract void PopulateDataObject(OrderLine docketLineDataObject, T whsDocketLineBO);

		#endregion
	}
}
