using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface IZACusCarContainer : ICusCarContainer
	{
		ZString GetLandedPurpose();
	}
}
