using System.Collections.Generic;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public interface ISegregationRule
	{
		IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2);
		string ApplicableStandard { get; }
		bool IsExemption { get; }
	}
}
