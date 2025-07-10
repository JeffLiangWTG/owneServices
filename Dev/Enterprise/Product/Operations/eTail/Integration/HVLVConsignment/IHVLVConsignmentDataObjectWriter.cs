using System.Collections.Generic;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentDataObjectWriter
	{
		void SetConsignmentsToMerge(IEnumerable<IHVLVConsignment> consignmentsToMerge);
	}
}
