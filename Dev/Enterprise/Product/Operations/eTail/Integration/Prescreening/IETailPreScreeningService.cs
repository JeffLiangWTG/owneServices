using System;

namespace Enterprise.eTail.Integration
{
	public interface IETailPreScreeningService
	{
		IETailPreScreeningResponse PreScreenHVLVConsignment(Guid consignmentPK);
		IETailPreScreeningResponse PreScreenHVLVConsignmentCollection(string entityTableCode, Guid entityPK);
	}
}
