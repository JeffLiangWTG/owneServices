using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business
{
	public interface IMessageInformationProvider
	{
		ZString MessageType { get; }

		ZString MessageSubType { get; }

		ZString MessageText { get; }

		BusinessObject Parent { get; }

		ZString ApplicationReference { get; }

		IMessageNumberStrategy MessageNumberStrategy { get; }

		ZString ApplicationCode { get; }
	}
}
