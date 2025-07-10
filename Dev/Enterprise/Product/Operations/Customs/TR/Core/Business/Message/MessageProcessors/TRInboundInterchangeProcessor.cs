using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public class TRInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public TRInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => messageCreator ?? (messageCreator = new TRInboundMessageCreator());
		IInboundMessageCreator messageCreator;
	}
}
