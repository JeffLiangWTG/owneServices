using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var forwardingShipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var bookingShipment = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory).ForwardingShipment;
			var cfsShipment = (BusinessObject)Factory.New<CFS.ICFSShipment>();
			var agencyBooking = (BusinessObject)Factory.New<Integration.Agency.IAgencyBooking>();
			var billOfLading = (BusinessObject)Factory.New<Integration.Agency.IBillOfLading>();
			var webBillOfLading = (BusinessObject)Factory.New<Integration.Agency.IBillOfLading>();
			webBillOfLading[JobShipmentSchema.JS_ShipmentStatus] = ShipmentStatusList.Codes.WebFwdInstruction;
			var rejectedBillOfLading = (BusinessObject)Factory.New<Integration.Agency.IBillOfLading>();
			webBillOfLading[JobShipmentSchema.JS_ShipmentStatus] = ShipmentStatusList.Codes.SIRejected;

			Factory.Save();

			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), forwardingShipment.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), bookingShipment.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<CFS.ICFSShipment>(), cfsShipment.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IAgencyBooking>(), agencyBooking.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IBillOfLading>(), billOfLading.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IBillOfLading>(), webBillOfLading.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IBillOfLading>(), rejectedBillOfLading.PK);

			var newFactory = new BusinessObjectFactory();

			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), forwardingShipment.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), bookingShipment.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<CFS.ICFSShipment>(), cfsShipment.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IAgencyBooking>(), agencyBooking.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IBillOfLading>(), billOfLading.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IBillOfLading>(), webBillOfLading.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IBillOfLading>(), rejectedBillOfLading.PK);
		}

		public void TestGetTypeForLoad_SimultaneousCFSAndForwarding_TryToUseFactoryCacheFirst()
		{
			var shipment = (BusinessObject)Factory.New<CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_IsForwardRegistered] = true;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var explicitlyLoadedForwardingShipment = anotherFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(shipment.PK);
			AssertTypeLoaded(anotherFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), shipment.PK);

			var yetAnotherFactory = new BusinessObjectFactory();
			var explicitlyLoadedCFSShipment = yetAnotherFactory.Load<CFS.ICFSShipment>(shipment.PK);
			AssertTypeLoaded(yetAnotherFactory, ObjectFactory.GetType<CFS.ICFSShipment>(), shipment.PK);
		}

		public void TestGetTypeForLoad_SimultaneousCFSAndForwarding_CheckCFSContextService()
		{
			var shipment = (BusinessObject)Factory.New<CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_IsForwardRegistered] = true;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			AssertEquals("Prerequisite: factory should not be in CFS context by default", FreightDomainContext.Unspecified, anotherFactory.GetFreightDomainContext());
			AssertTypeLoaded(anotherFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), shipment.PK);

			anotherFactory = new BusinessObjectFactory();
			anotherFactory.SetFreightDomainContext(FreightDomainContext.CFS);

			AssertEquals("Prerequisite: factory is in CFS context", FreightDomainContext.CFS, anotherFactory.GetFreightDomainContext());
			AssertTypeLoaded(anotherFactory, ObjectFactory.GetType<CFS.ICFSShipment>(), shipment.PK);
		}

		public void TestGetTypeForLoad_SimultaneousCFSAndForwarding_ViewGenericJob_TryToUseFactoryCacheFirst()
		{
			var shipment = (BusinessObject)Factory.New<CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_IsForwardRegistered] = true;
			var jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.LoadGenericJob<IGenericJob>(jobHeader);
			AssertNoExceptionThrown(() => { anotherFactory.Load<CommonShipment>(shipment.PK); });

			var yetAnotherFactory = new BusinessObjectFactory();
			var explicitlyLoadedCFSShipment = yetAnotherFactory.Load<CFS.ICFSShipment>(shipment.PK);
			AssertTypeLoaded(yetAnotherFactory, ObjectFactory.GetType<CFS.ICFSShipment>(), shipment.PK);
		}

		void AssertTypeLoaded(BusinessObjectFactory factory, Type expectedType, ZGuid shipmentPK)
		{
			AssertEquals("TypeDecider called when loading by type", expectedType, factory.Load<CommonShipment>(shipmentPK).GetType());
			AssertEquals("TypeDecider called when loading by table prefix", expectedType, factory.Load(JobShipmentSchema.Constants.Prefix, shipmentPK).GetType());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>(), new ShipmentTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>(), new ShipmentTypeDecider().GetTypeForBinding());
		}
	}
}
