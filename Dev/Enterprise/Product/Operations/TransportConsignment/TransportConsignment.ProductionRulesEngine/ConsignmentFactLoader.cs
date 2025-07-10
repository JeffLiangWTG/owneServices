using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;
using WTG.ProductionRules.Core;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	class ConsignmentFactLoader : IConsignmentFactLoader
	{
		public IEnumerable<IInputFact> GetFacts(IDtbConsignment entity, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment)
		{
			var factAccumulator = new BranchSelectionFactAccumulator();
			var branchFact = factAccumulator.CreateUniqueBranchFact(loginBranch);
			var departmentFact = factAccumulator.CreateUniqueDepartmentFact(loginDepartment);

			var parentJob = LoadParentJob(entity);
			var parentPlugin = entity as IJobInvoicingPlugIn;
			var invoiceSupporter = parentPlugin.InvoicingSupporter;

			var warehouseJobFact = CreateWarehouseJobFact(entity, parentJob);
			var shipmentJobFact = CreateShipmentJobFact(parentJob, factAccumulator);

			var localClientFact = CreateOrNull(invoiceSupporter.Job.LocalCharges, factAccumulator.CreateUniqueOrganisationFact);
			var companyCountry = loginCompany.Country.Code;
			var consignmentFact = new ConsignmentFact(entity, branchFact, departmentFact, companyCountry, localClientFact, warehouseJobFact, shipmentJobFact);

			return new List<IInputFact> { consignmentFact };
		}

		ILandTransportJobShipmentFact CreateShipmentJobFact(BusinessObject parentJob, BranchSelectionFactAccumulator factAccumulator)
		{
			if (parentJob is IForwardingShipment)
			{
				var shipment = parentJob as AutoJobShipment;
				var invoiceSupporter = new JobInvoicingSupporter(parentJob as IJobHeaderParent);
				var origin = shipment.Origin;
				var originFact = CreateOrNull(origin, factAccumulator.CreateUniqueUNLOCOFact);
				var destination = shipment.Destination;
				var destinationFact = CreateOrNull(destination, factAccumulator.CreateUniqueUNLOCOFact);

				return new LandTransportJobShipmentFact(invoiceSupporter, originFact, destinationFact);
			}
			return null;
		}

		ILandTransportJobWarehouseFact CreateWarehouseJobFact(IDtbConsignment entity, BusinessObject parentJob)
		{
			if (parentJob is IWhsOrder)
			{
				return new LandTransportJobWarehouseFact(entity as IJobInvoicingPlugIn, parentJob);
			}
			return null;
		}

		BusinessObject LoadParentJob(IDtbConsignment entity)
		{
			var factory = entity.Factory;
			var transportBooking = factory?.Load<IDtbBooking>(entity.LTC_KM_Booking);

			if (transportBooking == null)
			{
				return null;
			}

			var bookingConsolidation = transportBooking.ConsolidationSingleJob;

			if (bookingConsolidation == null)
			{
				return null;
			}

			return factory.Load(bookingConsolidation.KB_ParentTableCode, bookingConsolidation.KB_ParentID);
		}

		protected TFact CreateOrNull<TFact, TBizo>(TBizo businessObject, Func<TBizo, TFact> accumulate)
		{
			return businessObject == null ? default : accumulate(businessObject);
		}
	}
}
