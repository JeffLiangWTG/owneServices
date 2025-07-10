using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPackageJobInfoForRelease
	{
		Guid PK { get; }
		string JobId { get; }
		string ParentJobNo { get; }
		string ClientCode { get; }
		DateTime RequiredDate { get; }
		string JobStatus { get; }
	}
}
