
namespace Enterprise.Customs.SG.V4.Business
{
	public class SendsMessagesToCustomsShutterUpperer : Customs.Business.SendsMessagesToCustomsShutterUpperer, ISendsMessagesToCustoms
	{
		public SendsMessagesToCustomsShutterUpperer()
			: base()
		{
		}

		public SendsMessagesToCustomsShutterUpperer(bool throwExceptionOnError)
			: base(throwExceptionOnError)
		{
		}
	}

	public interface ISendsMessagesToCustoms : Customs.Business.ISendsMessagesToCustoms
	{
	}
}
