using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVPrescreeningDataProvider : IBusiness
	{
		string ETailerOrgCode { get; }

		string TableCode { get; }

		BusinessObject Entity { get; }

		IEnumerable<IHVLVConsignment> Consignments { get; }

		Logs Logs { get; }
	}
}
