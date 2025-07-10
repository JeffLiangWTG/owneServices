using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.Warehouse.Transit.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolDocManagerInfo : ConsolDocManagerInfo
	{
		public ForwardingConsolDocManagerInfo(ForwardingConsol parent)
			: base(parent, Core.Constants.DocManagerCodes.Consol)
		{
		}

		protected override bool ShouldRecordDocumentCore(IStmMenuItem menuItem)
		{
			return menuItem != null ? menuItem.SU_MenuName != Core.Constants.MenuNameConstantsForPrinting.ConsolJobProfitDocument : base.ShouldRecordDocumentCore(menuItem);
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			List<BusinessObject> related = new List<BusinessObject>(base.GetRelatedObjects());
			related.AddRange(GetARInvoicesForEntireConsol());
			related.AddRange(this.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusMAWB>(), CusMAWBSchema.CM_JK));
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				related.AddRange(this.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAOceanBill>(), CusSCAOceanBillSchema.CB_ParentId)
					.Where(x => x is Enterprise.Integration.Customs.AU.ICusSCAOceanBill));
			}
			related.AddRange(this.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusInBondHeader>(), CusInBondHeaderSchema.BH_ParentID).Where(x => x is Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader));
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				related.AddRange(this.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader>(), AsycudaManifestHeaderSchema.AMA_ParentId).Where(x => x is Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader));
			}

			if (BusinessEntity is ForwardingConsol consol)
			{
				related.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(BusinessEntity as ForwardingConsol));
				if (consol.IsDestinationToCanada() && consol.CusCAeMHMaster is BusinessObject eManifest)
				{
					related.Add(eManifest);
				}

				related.AddRange(TransitWarehouseJobsLoader.GetRelatedTransitWarehouseJobsEDocs(consol.Factory, consol));
			}

			related.AddRange(GetRelatedCusExitDetailsEDocs());
			related.AddRange(GetRelatedCusExitHeaderEDocs());

			related.AddRange(GetRelatedOrdersEDocs());
			related.AddRange(GetRelatedSupplierBookingEDocs());
			related.AddRange(GetRelatedContainerLoadListHeaderDetailsEDocs());
			related.AddRange(GetRelatedShipmentDetailsEDocs());

			return related.Distinct().ToArray();
		}

		#region Implementation

		IEnumerable<BusinessObject> GetRelatedCusExitDetailsEDocs()
		{
			var exitDetailQuery = new ZDBOnlyQuery(typeof(Enterprise.Integration.Customs.EU.ICusExitDetail));
			var exitHeaderFilter = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.EU.ICusExitControlHeader), CusExitControlHeaderSchema.PK);
			exitHeaderFilter.AddToFilter(CusExitControlHeaderSchema.CEH_ParentID, Consol.PK);
			exitDetailQuery.AddSubQuery(CusExitDetailSchema.CED_CEH, exitHeaderFilter, JoinCondition.And);

			return (IEnumerable<BusinessObject>)Consol.Factory.Load<Enterprise.Integration.Customs.EU.ICusExitDetail>(exitDetailQuery);
		}

		IEnumerable<BusinessObject> GetRelatedCusExitHeaderEDocs()
		{
			var list = new List<BusinessObject>();
			var exitHeaderLoader = ObjectFactory.Get<Enterprise.Integration.Customs.EUExitControl.ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", Consol.Factory);
			var exitHeader = exitHeaderLoader.Load(!Consol.IsInDatabase, Consol.PK, Consol.TablePrefix).FirstOrDefault();
			if (exitHeader != null)
			{
				list.Add((BusinessObject)exitHeader);
				if (((IDocManagerSupport)exitHeader).DocManagerInfo is ShipmentDocManagerInfo.IHaveEDocsChildren childGetter)
				{
					list.AddRange(childGetter.GetEDocsChildrenForAFreightJobToDisplay());
				}
			}

			return list;
		}

		IEnumerable<BusinessObject> GetRelatedOrdersEDocs()
		{
			var consol = BusinessEntity as ForwardingConsol;

			if (consol != null)
			{
				var supplierBookingPKs = GetRelatedSupplierBookingPKs(consol);

				if (supplierBookingPKs.Any())
				{
					var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JO_OrderLine);
					supplierBookingLineQuery.AddToFilter(JobSupplierBookingLineSchema.JSL_JSB_Booking, SQLComparisonOperator.Equal, supplierBookingPKs);

					var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
					orderLineQuery.AddSubQuery(JobOrderLineSchema.PK, supplierBookingLineQuery, JoinCondition.And);

					var orderQuery = new ZDBOnlyQuery(typeof(Order));
					orderQuery.AddSubQuery(JobOrderHeaderSchema.PK, orderLineQuery, JoinCondition.And);

					return consol.Factory.Load<Order>(orderQuery);
				}
			}

			return Enumerable.Empty<BusinessObject>();
		}

		IEnumerable<BusinessObject> GetRelatedSupplierBookingEDocs()
		{
			var consol = BusinessEntity as ForwardingConsol;

			if (consol != null)
			{
				var supplierBookingPKs = GetRelatedSupplierBookingPKs(consol);

				if (supplierBookingPKs.Any())
				{
					var supplierBookingQuery = new ZDBOnlyQuery(typeof(JobSupplierBooking));
					supplierBookingQuery.AddToFilter(JobSupplierBookingSchema.PK, SQLComparisonOperator.Equal, supplierBookingPKs);

					return consol.Factory.Load<JobSupplierBooking>(supplierBookingQuery);
				}
			}

			return Enumerable.Empty<BusinessObject>();
		}

		IEnumerable<BusinessObject> GetRelatedContainerLoadListHeaderDetailsEDocs()
		{
			var consol = BusinessEntity as ForwardingConsol;

			if (consol != null)
			{
				var supplierBookingPKs = GetRelatedSupplierBookingPKs(consol);

				if (supplierBookingPKs.Any())
				{
					var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
					supplierBookingLineQuery.AddToFilter(JobSupplierBookingLineSchema.JSL_JSB_Booking, SQLComparisonOperator.Equal, supplierBookingPKs);

					var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_CLH_LoadListHeader);
					containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JSL_BookingLine, supplierBookingLineQuery, JoinCondition.And);

					var containerLoadListHeaderQuery = new ZDBOnlyQuery(typeof(CommonContainerLoadList));
					containerLoadListHeaderQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

					return consol.Factory.Load<CommonContainerLoadList>(containerLoadListHeaderQuery);
				}
			}

			return Enumerable.Empty<BusinessObject>();
		}

		IEnumerable<BusinessObject> GetRelatedShipmentDetailsEDocs()
		{
			var consol = BusinessEntity as ForwardingConsol;

			if (consol != null)
			{
				var supplierBookingPKs = GetRelatedSupplierBookingPKs(consol);

				if (supplierBookingPKs.Any())
				{
					var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
					supplierBookingLineQuery.AddToFilter(JobSupplierBookingLineSchema.JSL_JSB_Booking, SQLComparisonOperator.Equal, supplierBookingPKs);

					var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JL_PackLine);
					containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JSL_BookingLine, supplierBookingLineQuery, JoinCondition.And);

					var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
					packLineQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

					var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
					shipmentQuery.AddSubQuery(packLineQuery, JoinCondition.And);

					return consol.Factory.Load<ForwardingShipment>(shipmentQuery);
				}
			}

			return Enumerable.Empty<BusinessObject>();
		}

		IEnumerable<ZGuid> GetRelatedSupplierBookingPKs(ForwardingConsol consol)
		{
			return consol
				.Shipments
				.OfType<ForwardingShipment>()
				.SelectMany(x => x.OuterPackLines.OfType<ForwardingPackLine>())
				.Select(x => x.GetContainer(consol)?.JC_JSB_SupplierBooking)
				.OfType<ZGuid>();
		}

		BusinessObject[] GetARInvoicesForEntireConsol()
		{
			ForwardingConsol consol = BusinessEntity as ForwardingConsol;
			AccTransactionHeaderCollection loadedInvoices = new InvoiceLoader(consol.Factory).GetInvoicesForUniqueRef(consol.JK_UniqueConsignRef);
			return loadedInvoices.ToArray();
		}

		#endregion
	}
}
