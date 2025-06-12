using System.ServiceModel;
using CargoWise.eHub.Shared.RoutingRuleEngine;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	[ServiceContract]
	public interface IRoutingRuleValidationWebService
	{
		[OperationContract]
		Result[] Evaluate(RoutingEvaluationGenericInput routingEvaluationInput);

		[OperationContract]
		Result[] EvaluateOCM(RoutingEvaluationOCMInput routingEvaluationInput);
	}
}
