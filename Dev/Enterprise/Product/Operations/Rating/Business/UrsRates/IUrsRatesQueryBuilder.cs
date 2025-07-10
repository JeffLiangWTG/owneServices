using Urs.Api.Integration.DTOs.Request;

namespace Enterprise.Rating.Business;

public interface IUrsRatesQueryBuilder<TCriteria>
{
	(QueryRequestDto ursRequest, string error) Build(TCriteria criteria);
}
