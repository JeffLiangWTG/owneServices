using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.Warehouse.Transit.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentDocManagerInfo : ShipmentDocManagerInfo
	{
		public ForwardingShipmentDocManagerInfo(ForwardingShipment parent)
			: base(parent) { }

		protected ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			result.AddRange(base.GetRelatedObjects());
			result.AddRange(Shipment.AttachedOrders.ToArray());
			result.AddRange(this.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusHAWB>(), CusHAWBSchema.CS_JS));
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				foreach (ForwardingConsol consol in Shipment.Consols)
				{
					result.AddRange(((IDocManagerSupport)consol).DocManagerInfo.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAOceanBill>(), CusSCAOceanBillSchema.CB_ParentId)
						.Where(x => x is Enterprise.Integration.Customs.AU.ICusSCAOceanBill));
				}
			}
			result.AddRange(this.GetRelatedObjectsChain(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusInBondHeader>(), CusInBondHeaderSchema.BH_ParentID)
				.Where(x => x is Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader || x is Enterprise.Integration.Customs.US.InBond.ICusInBondHeader || x is Enterprise.Integration.Customs.EU.NCTS.IDepartureMovementHeader));
			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(Shipment));
			result.AddRange(TransitWarehouseJobsLoader.GetRelatedTransitWarehouseJobsEDocs(Shipment));
			result.AddRange(Shipment.AttachedWarehouseOrders.Cast<BusinessObject>());
			var relatedReceive = Shipment.RelatedWarehouseReceive;
			if (relatedReceive != null)
			{
				result.Add(relatedReceive);
			}

			result.AddRange(GetRelatedCusExitDetailsEDocs());
			GetRelatedCusExitHeaderEDocs(result);

			result.AddRange(GetRelatedOrderDetailsEDocs());
			result.AddRange(GetRelatedSupplierBookingDetailsEDocs());
			result.AddRange(GetRelatedContainerLoadListHeaderDetailsEDocs());
			result.AddRange(GetRelatedConsolDetailsEDocs());

			return result.ToArray();
		}

		IEnumerable<BusinessObject> GetRelatedConsolDetailsEDocs()
		{
			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.PK);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_JS, Shipment.PK);

			var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JSL_BookingLine);
			containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JL_PackLine, packLineQuery, JoinCondition.And);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JSB_Booking);
			supplierBookingLineQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

			var containerQuery = new ZDBOnlySubQuery(typeof(ForwardingContainer), JobContainerSchema.JC_JK);
			containerQuery.AddSubQuery(JobContainerSchema.JC_JSB_SupplierBooking, supplierBookingLineQuery, JoinCondition.And);

			var consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			consolQuery.AddSubQuery(JobConsolSchema.PK, containerQuery, JoinCondition.And);

			return Shipment.Factory.Load<ForwardingConsol>(consolQuery);
		}

		IEnumerable<BusinessObject> GetRelatedContainerLoadListHeaderDetailsEDocs()
		{
			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.PK);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_JS, Shipment.PK);

			var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_CLH_LoadListHeader);
			containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JL_PackLine, packLineQuery, JoinCondition.And);

			var containerLoadListHeaderQuery = new ZDBOnlyQuery(typeof(CommonContainerLoadList));
			containerLoadListHeaderQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

			return Shipment.Factory.Load<CommonContainerLoadList>(containerLoadListHeaderQuery);
		}

		IEnumerable<BusinessObject> GetRelatedSupplierBookingDetailsEDocs()
		{
			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.PK);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_JS, Shipment.PK);

			var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JSL_BookingLine);
			containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JL_PackLine, packLineQuery, JoinCondition.And);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JSB_Booking);
			supplierBookingLineQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

			var supplierBookingQuery = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			supplierBookingQuery.AddSubQuery(supplierBookingLineQuery, JoinCondition.And);

			return Shipment.Factory.Load<JobSupplierBooking>(supplierBookingQuery);
		}

		IEnumerable<BusinessObject> GetRelatedOrderDetailsEDocs()
		{
			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.PK);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_JS, Shipment.PK);

			var containerLoadListLineQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JSL_BookingLine);
			containerLoadListLineQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JL_PackLine, packLineQuery, JoinCondition.And);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JO_OrderLine);
			supplierBookingLineQuery.AddSubQuery(containerLoadListLineQuery, JoinCondition.And);

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			orderLineQuery.AddSubQuery(supplierBookingLineQuery, JoinCondition.And);

			var orderQuery = new ZDBOnlyQuery(typeof(Order));
			orderQuery.AddSubQuery(orderLineQuery, JoinCondition.And);

			return Shipment.Factory.Load<Order>(orderQuery);
		}

		IEnumerable<BusinessObject> GetRelatedCusExitDetailsEDocs()
		{
			var exitDetailQuery = new ZDBOnlyQuery(typeof(Enterprise.Integration.Customs.EU.ICusExitDetail));
			var exitHeaderFilter = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.EU.ICusExitControlHeader), CusExitControlHeaderSchema.PK);
			exitHeaderFilter.AddToFilter(CusExitControlHeaderSchema.CEH_ParentID, Shipment.PK);
			exitDetailQuery.AddSubQuery(CusExitDetailSchema.CED_CEH, exitHeaderFilter, JoinCondition.And);

			return (IEnumerable<BusinessObject>)Shipment.Factory.Load<Enterprise.Integration.Customs.EU.ICusExitDetail>(exitDetailQuery);
		}

		void GetRelatedCusExitHeaderEDocs(List<BusinessObject> list)
		{
			var exitHeaderLoader = ObjectFactory.Get<Enterprise.Integration.Customs.EUExitControl.ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", Shipment.Factory);
			var exitHeader = exitHeaderLoader.Load(!Shipment.IsInDatabase, Shipment.PK, Shipment.TablePrefix).FirstOrDefault();
			if (exitHeader != null)
			{
				list.Add((BusinessObject)exitHeader);
				if (((IDocManagerSupport)exitHeader).DocManagerInfo is IHaveEDocsChildren childGetter)
				{
					list.AddRange(childGetter.GetEDocsChildrenForAFreightJobToDisplay());
				}
			}
		}

		#region Storage Number

		public override ZBool SupportsStorageNumberGeneration
		{
			get
			{
				return Enterprise.Registry.Business.FreightDataRegistry.Instance.StorageNumberButtonActivation.Value;
			}
		}

		public override ZString GenerateStorageNumber()
		{
			NumberGenerator generator = new NumberGenerator();

			generator.Factory = Shipment.Factory;
			generator.Context = new NumberGeneratorContext();
			generator.BaseFountain = Env.NumberFountains.StorageNumber;
			generator.FountainGetter = Env.NumberFountains.GetStorageNumberGeneratorFountain;
			generator.PrimaryTarget = new StorageNumberGeneratorTarget();
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(Shipment));

			generator.Generate();
			generator.EnforceMaxLengths();

			return generator.PrimaryTarget.Value;
		}

		#endregion
	}
}
