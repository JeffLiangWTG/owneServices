using System;
using System.Collections.Generic;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public interface ISegregationRulesManager
	{
		IEnumerable<DGPairInfo<T>> Check<T>(IEnumerable<string> standardsToCheckAgainst, IEnumerable<T> items, Func<T, string, (UNDGClassificationData, UNDGSubstance, string)> fetchEntity);
	}
}
