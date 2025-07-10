using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CreateDeclarationHelperForTesting : CreateDeclarationHelper
	{
		protected override ICollection<CreateBrokerageQuestion> GetQuestionsToCreateDeclarationCore()
		{
			return new List<CreateBrokerageQuestion>()
			{
				new CreateBrokerageQuestion()
				{
					Question = "If this is a Air Shipment, do you want to create a declaration for it?",
					Message = "This is a Air Shipment.",
					DefaultAnswer = true,
					ConditionToAsk = delegate(ForwardingShipment shipment) { return shipment.IsAir; }
				}
			};
		}
	}
}
