using Enterprise.Integration;
using Enterprise.ZArchitecture.Web.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	[Immutable]
	public sealed class eAdaptorNextConfig : eAdaptorConfigBase<eAdaptorNextConfig>
	{
		public eAdaptorNextConfig()
		{
			ContextSetter = new eAdaptorNextContextSetter();
			ResponseWriter = new eAdaptorNextHttpResponseWriter();
			WebConfig = new WebAppEnvironment.Config(true, true);
		}

		public override string Name => "eAdaptorNext"; // Name of Adaptor

		public override IeAdaptorContextSetter ContextSetter { get; }

		public override IeAdaptorHttpResponseWriter ResponseWriter { get; }

		public override IWebConfig WebConfig { get; }

		public override bool ThrowOnParsingError => true;

		public override bool IsActive => true;

		public override bool ThrowIfNotActive => false;
	}
}
