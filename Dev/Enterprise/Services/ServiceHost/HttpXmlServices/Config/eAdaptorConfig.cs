using Enterprise.Integration;
using Enterprise.ZArchitecture.Web.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	[Immutable]
	public sealed class eAdaptorConfig : eAdaptorConfigBase<eAdaptorConfig>
	{
		public eAdaptorConfig()
		{
			ContextSetter = new eAdaptorContextSetter();
			ResponseWriter = new eAdaptorHttpResponseWriter();
			WebConfig = new WebAppEnvironment.Config(true, false);
		}

		public override string Name => "eAdaptor"; // Name of Adaptor

		public override IeAdaptorContextSetter ContextSetter { get; }

		public override IeAdaptorHttpResponseWriter ResponseWriter { get; }

		public override IWebConfig WebConfig { get; }

		public override bool ThrowOnParsingError => false;

		public override bool IsActive => true;

		public override bool ThrowIfNotActive => false;
	}
}
