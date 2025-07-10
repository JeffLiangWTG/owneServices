using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Freight.Business.Testing
{
	class TransportParentTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTransportParentSet()
		{
			var rowFactory = new RowFactory(Factory);

			AssertParentIsSet(rowFactory, Factory.New<DummyTransportParent>(), "DMY");

			var dtbBookingConsolidationParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>());
			var whsItemDispatchLoadListParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemDispatchLoadList>());
			var whsItemReceiveASNParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemReceiveASN>());
			var whsItemReceiveConsignmentParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemReceiveConsignment>());
			var jobShipmentPreplanningParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IJobShipmentPreplanning>());
			var jobDeclarationParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IBaseJobDeclaration>());
			var cusISFHeaderParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<ICusISFHeader>());
			var asycudaManifestHeaderParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<ASYCUDA.IAsycudaManifestHeader>());
			var jobComInvoiceHeaderParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IBaseJobComInvoiceHeader>());

			AssertParentIsSet(rowFactory, dtbBookingConsolidationParent, Constants.TransportParentTypes.TransportBooking);
			AssertParentIsSet(rowFactory, whsItemDispatchLoadListParent, Constants.TransportParentTypes.TransitDispatchLoadList);
			AssertParentIsSet(rowFactory, whsItemReceiveASNParent, Constants.TransportParentTypes.TransitReceiveASN);
			AssertParentIsSet(rowFactory, whsItemReceiveConsignmentParent, Constants.TransportParentTypes.TransitReceiveConsignment);
			AssertParentIsSet(rowFactory, jobShipmentPreplanningParent, Constants.TransportParentTypes.ShipmentPreAdvice);
			AssertParentIsSet(rowFactory, jobDeclarationParent, Constants.TransportParentTypes.Declaration);
			AssertParentIsSet(rowFactory, cusISFHeaderParent, Constants.TransportParentTypes.ImporterSecurityFiling);
			AssertParentIsSet(rowFactory, asycudaManifestHeaderParent, Constants.TransportParentTypes.AsycudaManifest);
			AssertParentIsSet(rowFactory, jobComInvoiceHeaderParent, Constants.TransportParentTypes.CommercialInvoice);
		}

		public void TestOtherTypeDeciders()
		{
			var rowFactory = new RowFactory(Factory);

			var agencyShipmentParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IAgencyShipment>());
			var agencyBookingParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IAgencyBooking>());
			var billOfLadingParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<IBillOfLading>());
			var commonConsolParent = Factory.NewWithValidTestData<CommonConsol>();
			var forwardingConsolParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
			var cfsLoadListConsolParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<ICFSLoadListConsol>());
			var commonShipmentParent = Factory.NewWithValidTestData<CommonShipment>();
			var forwardingShipmentParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>());
			var cfsShipmentParent = (ITransportParentCommon)Factory.NewWithValidTestData(ObjectFactory.GetType<ICFSShipment>());

			Factory.Save();

			AssertParentIsSet(rowFactory, agencyShipmentParent, Constants.TransportParentTypes.AgencyShipment);
			AssertParentIsSet(rowFactory, agencyBookingParent, Constants.TransportParentTypes.AgencyShipment);
			AssertParentIsSet(rowFactory, billOfLadingParent, Constants.TransportParentTypes.AgencyShipment);
			AssertParentIsSet(rowFactory, commonConsolParent, Constants.TransportParentTypes.Consol);
			AssertParentIsSet(rowFactory, forwardingConsolParent, Constants.TransportParentTypes.Consol);
			AssertParentIsSet(rowFactory, cfsLoadListConsolParent, Constants.TransportParentTypes.Consol);
			AssertParentIsSet(rowFactory, commonShipmentParent, Constants.TransportParentTypes.Shipment);
			AssertParentIsSet(rowFactory, forwardingShipmentParent, Constants.TransportParentTypes.Shipment);
			AssertParentIsSet(rowFactory, cfsShipmentParent, Constants.TransportParentTypes.Shipment);
		}

		void AssertParentIsSet(RowFactory rowFactory, ITransportParentCommon parent, string parentCode)
		{
			var pk = Guid.NewGuid();
			var row = rowFactory.New(JobConsolTransportSchema.Constants.TableName);
			row[JobConsolTransportSchema.Constants.PK] = pk;
			row[JobConsolTransportSchema.Constants.JW_ParentType] = parentCode;
			row[JobConsolTransportSchema.Constants.JW_ParentGUID] = parent.PK.ToGuid();
			row.Table.Rows.Add(row);

			rowFactory.Save();

			var transport = Factory.Load<DummyTransport>(pk);

			AssertEquals(parent.PK, transport.Parent.PK);
		}
	}
}
