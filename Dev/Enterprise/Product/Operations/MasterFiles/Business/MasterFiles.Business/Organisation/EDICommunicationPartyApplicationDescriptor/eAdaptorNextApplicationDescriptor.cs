using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public class eAdaptorNextApplicationDescriptor : IEDIClientApplicationDescriptor
	{
		readonly string description = "eAdaptorNext";
		HashSet<AccessRequirement> accessTypes;
		public const string ApplicationCode = "EAN";
		public string Code => ApplicationCode;
		public string Description => description;
		public HashSet<AccessRequirement> AccessTypes
		{
			get
			{
				accessTypes = new HashSet<AccessRequirement>
				{
					AccessRequirement.SupportsInbound,
					AccessRequirement.SupportsOutbound,
					AccessRequirement.SupportsInboundOAuth,
					AccessRequirement.SupportsInboundBasicAuth,
					AccessRequirement.RequiresBranch,
					AccessRequirement.RequiresDepartment,
					AccessRequirement.SupportsOutboundOAuth,
					AccessRequirement.SupportsOutboundBasicAuth,
					AccessRequirement.SupportsOutboundNoAuth
				};
				return accessTypes;
			}
		}
	}
}
