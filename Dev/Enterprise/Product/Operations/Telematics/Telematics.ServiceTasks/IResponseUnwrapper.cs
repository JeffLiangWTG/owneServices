using System.Collections.Generic;

namespace Enterprise.Telematics.ServiceTasks
{
	public interface IResponseUnpacker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<int, decimal> Unpack(string responseJson);
	}
}
