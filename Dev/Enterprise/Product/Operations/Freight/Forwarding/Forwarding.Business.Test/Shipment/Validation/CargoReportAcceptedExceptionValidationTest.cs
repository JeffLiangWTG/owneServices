using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CargoReportAcceptedExceptionValidationTest : ExceptionValidationTest
	{
		public void TestCheckLateCargoReportReason()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			AssertEquals("Correct validation type", typeof(CargoReportAcceptedExceptionValidation), exceptionProcessTask.Validation.GetType());
			((CargoReportAcceptedExceptionValidation)exceptionProcessTask.Validation).ValidateLateCargoReportReason();
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportReasonInfo, "Please enter a value");
			exceptionProcessTask.LateCargoReportReason = "ABC";
			AssertNoErrorContaining(exceptionProcessTask.LateCargoReportReasonInfo, "Please enter a value");
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportReasonInfo, "Enter a valid selection");
			exceptionProcessTask.LateCargoReportReason = "MISCC";
			AssertNoErrors("No Error", exceptionProcessTask.LateCargoReportReasonInfo);
		}

		public void TestCheckCheckLateCargoReportText()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			AssertEquals("Correct validation type", typeof(CargoReportAcceptedExceptionValidation), exceptionProcessTask.Validation.GetType());
			exceptionProcessTask.LateCargoReportReason = "ABCDE";
			((CargoReportAcceptedExceptionValidation)exceptionProcessTask.Validation).ValidateLateCargoReportText();
			AssertNoErrors("No error and no exception", exceptionProcessTask.LateCargoReportTextInfo);
			exceptionProcessTask.LateCargoReportReason = "MISCC";
			((CargoReportAcceptedExceptionValidation)exceptionProcessTask.Validation).ValidateLateCargoReportText();
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportTextInfo, "Please enter a value");
			exceptionProcessTask.LateCargoReportText = "SOME TEXT";
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
			exceptionProcessTask.LateCargoReportReason = "MISCR";
			exceptionProcessTask.LateCargoReportText = "";
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportTextInfo, "Please enter a value");
			exceptionProcessTask.LateCargoReportText = "SOME TEXT";
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
			exceptionProcessTask.LateCargoReportReason = "RLDGI";
			exceptionProcessTask.LateCargoReportText = "";
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportTextInfo, "Please enter a value");
			exceptionProcessTask.LateCargoReportText = "SOME TEXT";
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
			exceptionProcessTask.LateCargoReportReason = "IARRD";
			exceptionProcessTask.LateCargoReportText = "";
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
			exceptionProcessTask.LateCargoReportText = "SOME TEXT";
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
		}

		public void TestValidateLateCargoReport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			AssertEquals("Correct validation type", typeof(CargoReportAcceptedExceptionValidation), exceptionProcessTask.Validation.GetType());
			((CargoReportAcceptedExceptionValidation)exceptionProcessTask.Validation).ValidateLateCargoReport();
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportReasonInfo, "Please enter a value");
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
			exceptionProcessTask.LateCargoReportReason = "IARRD";
			using (exceptionProcessTask.GetValidationSuspender())
			{
				exceptionProcessTask.LateCargoReportReason = "MISCR";
			}
			AssertNoErrors("Pre-condition", exceptionProcessTask.LateCargoReportReasonInfo);
			AssertNoErrors("Pre-condition", exceptionProcessTask.LateCargoReportTextInfo);
			((CargoReportAcceptedExceptionValidation)exceptionProcessTask.Validation).ValidateLateCargoReport();
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportReasonInfo);
			AssertHasErrorContaining(exceptionProcessTask.LateCargoReportTextInfo, "Please enter a value");
			using (exceptionProcessTask.GetValidationSuspender())
			{
				exceptionProcessTask.LateCargoReportText = "SOME TEXT";
			}
			((CargoReportAcceptedExceptionValidation)exceptionProcessTask.Validation).ValidateLateCargoReport();
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportReasonInfo);
			AssertNoErrors("No error", exceptionProcessTask.LateCargoReportTextInfo);
		}
	}
}
