using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageOrderReferenceDataObjectWriter : DataObjectWriter<PkgPackageOrderReference, UniversalShipment>
	{
		public PkgPackageOrderReferenceDataObjectWriter(IDataWritingManager writeManager, Dictionary<ZGuid, ZInt> orderLineDictionary) : base(writeManager)
		{
			OrderLineDictionary = Argument.NotNull(orderLineDictionary, nameof(orderLineDictionary));
		}

		protected override UniversalShipment PopulateDataObject(PkgPackageOrderReference packageOrderReferenceBO)
		{
			var pkgPackageJobData = new UniversalShipment(writeManager.WriterStrategy);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.PkgPackage, string.Empty);
			pkgPackageJobData.DataContext = dataContext;

			pkgPackageJobData.Order = new Order(writeManager.WriterStrategy);
			pkgPackageJobData.Order.OrderNumber = packageOrderReferenceBO.KPO_OrderNumber;
			pkgPackageJobData.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine> {
				new OrderLine(writeManager.WriterStrategy)
				{
					Product = new Product() { Code = packageOrderReferenceBO.KPO_SKUPartNumber },
					Link = OrderLineDictionary.TryGetValue(packageOrderReferenceBO.KPO_KP_Package, out var link) ? link : 0,
					CommercialInvoiceNumber = packageOrderReferenceBO.KPO_CommercialInvoiceNumber,
					LineReference = packageOrderReferenceBO.KPO_LineReference,
					ExpiryDate = packageOrderReferenceBO.KPO_ExpiryDate,
					SerialNumber = packageOrderReferenceBO.KPO_SerialNumber,
					BatchNumber = packageOrderReferenceBO.KPO_BatchNumber,
				}
			});

			return pkgPackageJobData;
		}

		readonly Dictionary<ZGuid, ZInt> OrderLineDictionary;
	}
}
