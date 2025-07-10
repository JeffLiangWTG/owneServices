using CargoWise.Types;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.Business.MessageBuilders
{
	public interface IManifestMessageBuilder : IMessageBuilder
	{
		int ErrorCount { get; }
		string Errors { get; }
		ZString ManifestMessageTypeCode { get; }
	}
}
