using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class DeliveryDueDateWithExceptionsTestHelper
	{
		public static ForwardingShipment CreateShipmentForTest(BusinessObjectFactory factory, ZDateTime deliveryDueDate)
		{
			var exceptionType = factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Description = "Act of God";
			exceptionType.WET_Code = "AOG";

			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
			shipment.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "I want to override";

			shipment.JS_DeliveryDueDate = deliveryDueDate;
			var deliveryCFS = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(factory, "CIA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington",  "AUCNN");
			var deliveryAgent = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(factory, "KGB", "20/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			factory.Save();

			var deliveryCFSAddress = deliveryCFS.Addresses.Find(x => x.Address1 == "21/8 Camillo St.").FirstOrDefault();
			var deliveryAgentAdress = deliveryAgent.Addresses.Find(x => x.Address1 == "20/8 Camillo St.").FirstOrDefault();
			deliveryAgentAdress.AddAddressType(OrgAddressType.Delivery);
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_OH_DeliveryAgent = deliveryAgentAdress.OA_OH;
			factory.Save();

			return shipment;
		}

		public static ProcessTask AddException(IWorkflowProvider workflowProvider, ZInt durationHours, ZString exceptionTypeCode)
		{
			var exception = workflowProvider.WorkflowItems.Exceptions.AddNew();
			exception.ExceptionTypeCode = exceptionTypeCode;
			exception.P9_ActualDateOffset = ZDateTimeOffset.Now;
			exception.P9_Description = nameof(exception);
			exception.P9_ExceptionDurationHours = durationHours;
			return exception;
		}
	}
}
