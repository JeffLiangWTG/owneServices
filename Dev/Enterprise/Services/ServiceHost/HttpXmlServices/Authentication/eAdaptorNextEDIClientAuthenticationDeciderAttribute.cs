using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	public sealed class eAdaptorNextEDIClientAuthenticationDeciderAttribute : EDIClientAuthenticationDeciderAttribute
	{
		public eAdaptorNextEDIClientAuthenticationDeciderAttribute()
			: base(eAdaptorNextApplicationDescriptor.ApplicationCode) { }
	}
}
