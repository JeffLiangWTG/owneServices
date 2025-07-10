using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class SubscriptionRequesteEventTransformerTest : TestCaseWithFactory
	{
		public void TestTransformWithEventTypeSBR()
		{
			var universalEvent = new UniversalEvent();
			var referenceContext = new Context() { Type = new ContextType { Type = "Reference" }, Value = "reference" };
			var c1cContext = new Context() { Type = new ContextType { Type = "CarrierC1CCode" }, Value = "C1CO" };
			universalEvent.ContextCollection = new List<Context> { referenceContext, c1cContext };

			var eventValue = new EventValue(Events.SubscriptionRequested);
			var result = SubscriptionRequesteEventTransformer.Transform(eventValue, universalEvent);

			AssertEquals("Shipment Visibility", result.Parameters[EventReferenceParameters.Codes.Type]);
			AssertEquals(null, result.Parameters[EventReferenceParameters.Codes.MessageType]);
			AssertEquals("reference", result.Parameters[EventReferenceParameters.Codes.InterchangeNumber]);
			AssertEquals("C1CO", result.Parameters[EventReferenceParameters.Codes.Organization]);
		}
	}
}
