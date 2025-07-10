using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDocketLineDataObjectReader<TDocket, TDocketLine> : DataObjectReader<OrderLine, TDocketLine>
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		protected WhsDocketLineDataObjectReader(OrderLine docketLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, TDocket parent)
			: base(docketLineDataObject, logger, factory)
		{
			Parent = Argument.NotNull(parent, typeof(TDocket).Name + " parent");
		}

		protected TDocket Parent { get; }
		protected abstract bool IsDataSourceCustoms { get; }

		#region Create / Update Job (Line)

		protected sealed override void PopulateBusinessObject(TDocketLine line)
		{
			line.WE_WD = Parent.PK;
			EnsureValidOrderLineDataObject(line);
			PopulateBusinessObjectCore(line);

			PopulateCustomsData(line);
			PopulateCustomFields(line);
			ChangesAfterLinePopulation(line);
		}

		#region EnsureValidOrderLineDataObject

		void EnsureValidOrderLineDataObject(TDocketLine docketLine)
		{
			var isValidProduct = false;

			var productCode = GetProduct().GetCodeAsUpperCase();
			if (!productCode.IsEmpty)
			{
				var product = WhsProductLoader.GetMatchedProductOrMaybeCreateNew(DocketLineType, dataObject.Link, productCode, dataObject, Parent.Client, CanCreateNewProducts, factory, logger);
				if (product != null)
				{
					SetProduct(docketLine, product);
					isValidProduct = true;
				}
			}

			if (!isValidProduct)
			{
				var secondaryExceptionText = dataObject.Product == null
					? Res.GetString("57b9cb6c-a0e8-4cac-a97e-39dab7a16969", "No Product was provided.")
					: Res.GetString("e397a87b-73f8-4f45-a749-42346c8308c2", "Unable to match Product: {0} for {1} {2}.", GetProduct().ToStringContents(), ClientDescriptionForErrorMessage, Parent.Client.OH_Code);

				throw new DataObjectReadFailureException(Res.GetString("4e7f5fb4-c38d-4a87-85d2-a15cb2739550", "Cannot Import {0} Line {1}\r\n{2}", DocketLineType, dataObject.Link, secondaryExceptionText));
			}
		}

		protected virtual ZString ClientDescriptionForErrorMessage => Res.GetString("WhsDocketLineDataObjectReader|ClientDescriptionForErrorMessage", "Client");

		protected virtual Product GetProduct() => dataObject.Product;

		protected virtual bool CanCreateNewProducts => SystemDataRegistry.Instance.CreateMissingWarehouseProduct.Value;

		protected abstract void SetProduct(TDocketLine docketLine, OrgSupplierPart product);

		protected abstract string DocketLineType { get; }

		#endregion

		#region PopulateBusinessObjectCore

		protected abstract void PopulateBusinessObjectCore(TDocketLine line);

		#endregion

		#region PopulateCustomsData

		void PopulateCustomsData(TDocketLine line)
		{
			if (ShouldPopulateCustomsData)
			{
				if (dataObject.CustomsData != null)
				{
					PopulateCustomsDataCore(line);
				}

				SetBondedEntryKey(line);
			}
		}

		protected virtual void PopulateCustomsDataCore(TDocketLine line)
		{
			new WhsBondedWarehouseAttributeDataObjectReader(logger).ReadEntryKeyAndOutwardTypeIntoBusinessObject(dataObject.CustomsData, line.CustomsData);
			PopulateWhsBondedWarehouseAttributeData(dataObject.CustomsData, line.CustomsData);
		}

		void PopulateWhsBondedWarehouseAttributeData(CustomsEntryInfo customsData, WhsBondedWarehouseAttribute customsDataBO)
		{
			new WhsBondedWarehouseAttributeDataObjectReader(logger).ReadCustomsInfoIntoBusinessObject(customsData, customsDataBO, factory);
		}

		protected abstract void SetBondedEntryKey(TDocketLine line);

		protected virtual bool ShouldPopulateCustomsData => true;

		#endregion

		#region PopulateCustomFields

		void PopulateCustomFields(TDocketLine line)
		{
			new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(WhsDocketLineSchema.Instance, line, dataObject, new WhsDocketLine.CustomLabelsProvider(Parent));
		}

		#endregion

		#region ChangesLineAfterPopulation

		protected virtual void ChangesAfterLinePopulation(TDocketLine line) { }

		#endregion

		#region SetValueWhenNotReadOnlyOrCustomsTransaction

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaStringColumn column, ICodeDataObject property)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(line, column, property);
			}
			else
			{
				ZString code;
				if (property.TryGetCodeAsUpperCase(out code))
				{
					SetValueIfNotReadOnly(line, column, code);
				}
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaStringColumn column, ICodeDescriptionPairList lookupList, ICodeDataObject value)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(line, column, value);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, lookupList, value);
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaStringColumn column, ZString? value)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(line, column, value);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, value);
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaGuidColumn column, ZGuid? value)
		{
			if (IsDataSourceCustoms || !line.IsInDatabase)
			{
				SetValue(line, column, value);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, value);
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaDecimalColumn column, ZDecimal? value)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(line, column, value);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, value);
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaDateTimeColumn column, ZDateTime? value)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(line, column, value);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, value);
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, SchemaDateTimeOffsetColumn column, ZDateTimeOffset? value)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(line, column, value);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, value);
			}
		}

		protected void SetValueWhenNotReadOnlyOrCustomsTransaction(TDocketLine line, IColumnIndexer row, SchemaShortColumn column, ZShort? valueSource)
		{
			if (IsDataSourceCustoms)
			{
				SetValue(row, column, valueSource);
			}
			else
			{
				SetValueIfNotReadOnly(line, column, valueSource);
			}
		}

		#endregion

		#region GetWE_TransactionQuantity

		protected ZDecimal? GetWE_TransactionQuantity(WhsDocketLine docketLine)
		{
			return ((dataObject.OrderedQty == null || (dataObject.OrderedQty == 0 && dataObject.PackageQty > 0))
							? docketLine.GetOrderedQuantityFromPackageQuantity(dataObject.PackageQty ?? 0)
							: dataObject.OrderedQty);
		}

		#endregion

		#endregion
	}
}
