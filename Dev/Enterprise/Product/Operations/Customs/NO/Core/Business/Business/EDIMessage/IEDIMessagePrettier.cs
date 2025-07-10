using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

public interface IEDIMessagePrettier
{
	ZString MakeHumanReadable();
}
