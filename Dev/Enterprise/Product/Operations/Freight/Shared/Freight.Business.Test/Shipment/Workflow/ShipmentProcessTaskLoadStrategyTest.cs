using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_InvalidID()
		{
			ShipmentProcessTaskLoadStrategy strategy = new ShipmentProcessTaskLoadStrategy();
			Type expectedType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(JobShipmentSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertEquals(expectedType, strategy.GetTypeForLoad(JobShipmentSchema.Constants.Prefix, ZGuid.Invalid, Factory));
		}

		public void TestGetTypeForLoad()
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			ShipmentProcessTaskLoadStrategy strategy = new ShipmentProcessTaskLoadStrategy();
			Type expectedType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));

			shipment = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			expectedType = ObjectFactory.GetType<Integration.Agency.IBillOfLadingProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));

			shipment = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
			expectedType = ObjectFactory.GetType<Integration.Agency.IAgencyBookingProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));

			shipment = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			expectedType = ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));

			shipment = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			shipment.JS_IsForwardRegistered = true;
			expectedType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));

			shipment = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			shipment.JS_IsForwardRegistered = false;
			expectedType = ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));

			shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			expectedType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory));
		}

		public void TestGetTypeForLoad_GivenParentInUnusualState_ReturnsCurrentType()
		{
			var aB = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
			var boL = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			var fS = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			Type aBPTType = ObjectFactory.GetType<Integration.Agency.IAgencyBookingProcessTask>();
			Type boLPTType = ObjectFactory.GetType<Integration.Agency.IBillOfLadingProcessTask>();
			Type fSPTType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();

			ShipmentProcessTaskLoadStrategy strategy = new ShipmentProcessTaskLoadStrategy();

			AssertEquals(aBPTType, strategy.GetTypeForLoad(aB.TablePrefix, aB.PK, Factory));
			aB.JS_ShipmentStatus = "CNF";
			AssertEquals(aBPTType, strategy.GetTypeForLoad(aB.TablePrefix, aB.PK, Factory));
			aB.JS_IsShipping = false;
			aB.JS_IsCFSRegistered = true;
			AssertEquals(aBPTType, strategy.GetTypeForLoad(aB.TablePrefix, aB.PK, Factory));
			aB.JS_IsCFSRegistered = false;
			aB.JS_IsForwardRegistered = true;
			AssertEquals(aBPTType, strategy.GetTypeForLoad(aB.TablePrefix, aB.PK, Factory));

			AssertEquals(boLPTType, strategy.GetTypeForLoad(boL.TablePrefix, boL.PK, Factory));
			boL.JS_ShipmentStatus = "BKD";
			AssertEquals(boLPTType, strategy.GetTypeForLoad(boL.TablePrefix, boL.PK, Factory));
			boL.JS_IsShipping = false;
			boL.JS_IsCFSRegistered = true;
			AssertEquals(boLPTType, strategy.GetTypeForLoad(boL.TablePrefix, boL.PK, Factory));
			boL.JS_IsCFSRegistered = false;
			boL.JS_IsForwardRegistered = true;
			AssertEquals(boLPTType, strategy.GetTypeForLoad(boL.TablePrefix, boL.PK, Factory));

			AssertEquals(fSPTType, strategy.GetTypeForLoad(fS.TablePrefix, fS.PK, Factory));
			fS.JS_IsForwardRegistered = false;
			fS.JS_IsCFSRegistered = true;
			AssertEquals(fSPTType, strategy.GetTypeForLoad(fS.TablePrefix, fS.PK, Factory));
			fS.JS_IsCFSRegistered = false;
			fS.JS_IsShipping = true;
			fS.JS_ShipmentStatus = "BKD";
			AssertEquals(fSPTType, strategy.GetTypeForLoad(fS.TablePrefix, fS.PK, Factory));
			fS.JS_ShipmentStatus = "CNF";
			AssertEquals(fSPTType, strategy.GetTypeForLoad(fS.TablePrefix, fS.PK, Factory));
		}

		public void TestGetTypeForLoad_GivenParentInUnusualState_ReturnsCurrentType_CFS()
		{
			var cfs = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			Factory.Save();

			var factoryNew = new BusinessObjectFactory();
			factoryNew.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(cfs.PK);
			factoryNew.Load<Integration.CFS.ICFSShipment>(cfs.PK);

			var fSPTType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			var cFSTType = ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();

			var strategy = new ShipmentProcessTaskLoadStrategy();

			AssertEquals(cFSTType, strategy.GetTypeForLoad(cfs.TablePrefix, cfs.PK, factoryNew));

			cfs.JS_IsForwardRegistered = true;
			Factory.Save();
			AssertEquals(fSPTType, strategy.GetTypeForLoad(cfs.TablePrefix, cfs.PK, factoryNew));
		}

		public void TestGetTypeForLoad_ShipmentIsCFS_ReturnCFSShipmentProcessTask()
		{
			var shipment = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();

			Factory.Save();

			var strategyToTest = new ShipmentProcessTaskLoadStrategy();

			var expectedType = ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();
			var actualType = strategyToTest.GetTypeForLoad(shipment.TablePrefix, shipment.PK, Factory);

			AssertEquals(expectedType, actualType);
		}

		public void TestGetTypeForLoad_ShipmentIsBothCFSAndForwarding_ReturnForwardingShipmentProcessTask()
		{
			var factory1 = new BusinessObjectFactory();
			var shipment = (CommonShipment)factory1.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.Load<Integration.CFS.ICFSShipment>(shipment.PK);
			factory2.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(shipment.PK);

			var strategyToTest = new ShipmentProcessTaskLoadStrategy();

			var expectedType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			var actualType = strategyToTest.GetTypeForLoad(shipment.TablePrefix, shipment.PK, factory2);

			AssertEquals(expectedType, actualType);
		}

		public void TestGetTypeForLoad_ParentNotInCache()
		{
			var aB = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
			var boL = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			var cFS = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			var fS = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			Factory.Save();

			Type aBPTType = ObjectFactory.GetType<Integration.Agency.IAgencyBookingProcessTask>();
			Type boLPTType = ObjectFactory.GetType<Integration.Agency.IBillOfLadingProcessTask>();
			Type cFSPTType = ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();
			Type fSPTType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();

			ShipmentProcessTaskLoadStrategy strategy = new ShipmentProcessTaskLoadStrategy();

			var factory2 = new BusinessObjectFactory();
			AssertEquals(aBPTType, strategy.GetTypeForLoad(aB.TablePrefix, aB.PK, factory2));
			AssertEquals(boLPTType, strategy.GetTypeForLoad(boL.TablePrefix, boL.PK, factory2));
			AssertEquals(cFSPTType, strategy.GetTypeForLoad(cFS.TablePrefix, cFS.PK, factory2));
			AssertEquals(fSPTType, strategy.GetTypeForLoad(fS.TablePrefix, fS.PK, factory2));
		}

		public void TestAddAdditionalParentFilters()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			CommonShipment forwardingShipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			forwardingShipment.FillWithValidTestData();
			ProcessTask forwardingShipmentTask = ((IWorkflowProvider)forwardingShipment).WorkflowItems.AddNew();
			CommonShipment billOfLading = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			billOfLading.FillWithValidTestData();
			ProcessTask billOfLadingTask = ((IWorkflowProvider)billOfLading).WorkflowItems.AddNew();
			CommonShipment agencyBooking = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
			agencyBooking.FillWithValidTestData();
			ProcessTask agencyBookingTask = ((IWorkflowProvider)agencyBooking).WorkflowItems.AddNew();
			Factory.Save();
			CommonShipment cfsShipment = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			cfsShipment.FillWithValidTestData();
			ProcessTask cfsShipmentTask = ((IWorkflowProvider)cfsShipment).WorkflowItems.AddNew();
			Factory.Save();

			ProcessTask[] tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(JobInvoicingConsumerTypes.Shipment.Code));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(forwardingShipmentTask, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(billOfLadingTask, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(agencyBookingTask, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(JobInvoicingConsumerTypes.CFSShipment.Code));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(cfsShipmentTask, tasks);
		}

		public void TestGetTypeForLoad_CancelledShipment()
		{
			var ab = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
			var bol = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
			var cfs = (CommonShipment)Factory.New<Integration.CFS.ICFSShipment>();
			var fs = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			ab.JS_IsCancelled = true;
			bol.JS_IsCancelled = true;
			cfs.JS_IsCancelled = true;
			fs.JS_IsCancelled = true;

			Factory.Save();

			Type abPTType = ObjectFactory.GetType<Integration.Agency.IAgencyBookingProcessTask>();
			Type bolPTType = ObjectFactory.GetType<Integration.Agency.IBillOfLadingProcessTask>();
			Type cfsPTType = ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();
			Type fsPTType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();

			ShipmentProcessTaskLoadStrategy strategy = new ShipmentProcessTaskLoadStrategy();

			var factory2 = new BusinessObjectFactory();
			AssertEquals(abPTType, strategy.GetTypeForLoad(ab.TablePrefix, ab.PK, factory2));
			AssertEquals(bolPTType, strategy.GetTypeForLoad(bol.TablePrefix, bol.PK, factory2));
			AssertEquals(cfsPTType, strategy.GetTypeForLoad(cfs.TablePrefix, cfs.PK, factory2));
			AssertEquals(fsPTType, strategy.GetTypeForLoad(fs.TablePrefix, fs.PK, factory2));
		}

		ZDBOnlyQuery GetNewQueryForTestAddAdditionalParentFilters(string workflowTypeCode)
		{
			WorkflowDescriptor descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowTypeCode);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CommonShipment), ProcessTasksSchema.P9_ParentID);
			ShipmentProcessTaskLoadStrategy strategy = new ShipmentProcessTaskLoadStrategy();
			strategy.AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}
	}
}
