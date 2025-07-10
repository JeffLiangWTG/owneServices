using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IJobComInvLineRefsTypeSupporter : IAdditionalBusinessObjectFetchStrategyProvider
	{
		ZGuid PK { get; }
		BusinessObjectFactory Factory { get; }
		bool IsInDatabase { get; }
		IDictionary<ZString, Type> GetJobComInvLineRefsTypes();
	}
}
