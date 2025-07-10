using System;
using WTG.RTUS.Interface;

namespace Enterprise.TransportCommon.Integration
{
	public interface IOrganisationRTUSOption
	{
		RTUSCBA RTUSCBA { get; }
		Uri WrappedUrl { get; }
	}
}
