using System;
using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYCInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public UYCInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes) { }

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => messageCreator ?? (messageCreator = new UYInboundMessageCreator());
		IInboundMessageCreator messageCreator;

		protected override Type TypeOfInterchangeToCreate() => typeof(UYCInterchange);
	}
}
