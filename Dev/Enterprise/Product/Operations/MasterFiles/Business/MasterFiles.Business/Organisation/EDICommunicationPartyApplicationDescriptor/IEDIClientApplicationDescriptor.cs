using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public enum AccessRequirement
	{
		SupportsInbound,
		SupportsOutbound,
		SupportsInboundOAuth,
		SupportsInboundBasicAuth,
		RequiresBranch,
		RequiresDepartment,
		SupportsOutboundOAuth,
		SupportsOutboundBasicAuth,
		SupportsOutboundNoAuth
	}
	public interface IEDIClientApplicationDescriptor
	{
		string Code { get; }
		string Description { get; }
		HashSet<AccessRequirement> AccessTypes { get; }
	}
}
