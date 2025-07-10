using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	sealed public class ShipmentFieldStateChange : IBusinessObjectFieldChangeState
	{
		public bool IsObjectInitialized(BusinessObject bizo)
		{
			return !GetInitializingShipmentService(bizo.Factory).IsInitializing;
		}

		public bool IsRootObject(BusinessObject bizo)
		{
			if (bizo is IWorkflowTriggerEventSource workflowTriggerEventSource)
			{
				if (workflowTriggerEventSource.ParentWorkflowProviders.Count > 0 || !(bizo is IWorkflowProviderCore))
				{
					return false;
				}

				return true;
			}

			return true;
		}

		public void NotifyObjectHooked(BusinessObject bizo)
		{
			var service = GetInitializingShipmentService(bizo.Factory);
			if (service.IsInitializing)
			{
				GetInitializingShipmentService(bizo.Factory).NestedInitializations.Push(bizo);
			}
		}

		public static IDisposable InitializingShipment(BusinessObjectFactory factory)
		{
			var service = GetInitializingShipmentService(factory);

			if (service.IsInitializing)
			{
				// This shipment will be hooked inside NotifyObjectHooked and notified initialized when the outer scope is disposed
				return null;
			}

			service.IsInitializing = true;

			return new DisposableAction(() =>
			{
				service.IsInitializing = false;

				while (service.NestedInitializations.Count != 0)
				{
					var shipment = service.NestedInitializations.Pop();
					shipment.FieldChangeTrackerStateChanged(BusinessObjectFieldStateChangeEvent.ObjectIntialized);
				}
			});
		}

		static InitializingShipmentService GetInitializingShipmentService(BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetService<InitializingShipmentService>();
			if (service == null)
			{
				service = new InitializingShipmentService();
				factory.ServiceContainer.AddService(service);
			}

			return service;
		}

		sealed class InitializingShipmentService : IService
		{
			public bool IsInitializing { get; set; }
			public Stack<BusinessObject> NestedInitializations { get; } = new Stack<BusinessObject>();
		}
	}

	public class ShipmentFieldChangeStateFactory : IBusinessObjectFieldChangeStateFactory
	{
		public IBusinessObjectFieldChangeState BusinessObjectFieldChangeState => new ShipmentFieldStateChange();

		public string ChildTableCodePrefix => JobShipmentSchema.Constants.Prefix;
	}
}
