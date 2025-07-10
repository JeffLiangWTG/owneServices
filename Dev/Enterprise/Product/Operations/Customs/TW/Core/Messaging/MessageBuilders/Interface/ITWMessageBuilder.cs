using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public interface ITWMessageBuilder
	{
		ZString SerializeToMessageString(object input, string functionCode = null);
	}
}
