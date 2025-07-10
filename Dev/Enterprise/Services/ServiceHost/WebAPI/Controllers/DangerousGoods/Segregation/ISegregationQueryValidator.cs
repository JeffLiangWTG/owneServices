using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public interface ISegregationQueryValidator
	{
		(bool validationResult, string validationMessage) IsQueryValid(SegregationQuery segregationQuery);

		(bool validationResult, string validationMessage) IsEntityQueryValid(SegregationEntityQuery segregationEntityQuery);
	}
}
