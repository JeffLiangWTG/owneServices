using CargoWise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business;

public interface IValidationToolActionSourceListProvider
{
	ICodeDescriptionPairList GetActionSourceList(ControllerID controllerId, string countryCode);
}
