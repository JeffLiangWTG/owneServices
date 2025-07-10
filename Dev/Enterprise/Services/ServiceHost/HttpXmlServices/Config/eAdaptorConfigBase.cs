using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	[Immutable]
	public abstract class eAdaptorConfigBase<T> : IeAdaptorConfig
		where T : eAdaptorConfigBase<T>, new()
	{
		static readonly object writerLock = new object();
		public static T Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}

				lock (writerLock)
				{
					if (_instance == null)
					{
						_instance = new T();
					}
				}
				return _instance;
			}
		}
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static T _instance;

		public abstract string Name { get; }

		public abstract IeAdaptorContextSetter ContextSetter { get; }

		public abstract IeAdaptorHttpResponseWriter ResponseWriter { get; }

		public abstract IWebConfig WebConfig { get; }

		public abstract bool ThrowOnParsingError { get; }

		public abstract bool IsActive { get; }

		public abstract bool ThrowIfNotActive { get; }
	}
}
