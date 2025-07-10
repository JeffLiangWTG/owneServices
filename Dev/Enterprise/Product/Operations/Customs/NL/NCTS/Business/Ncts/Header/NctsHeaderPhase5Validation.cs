using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsHeaderPhase5Validation : EU.NCTS.Business.NctsHeaderPhase5Validation
{
	public NctsHeaderPhase5Validation(NctsHeader parent) : base(parent)
	{
	}

	new NctsHeader Parent => (NctsHeader)base.Parent;

	public void ValidateCALCalculationMethod()
	{
		ValidateCalculatedProperty(Parent.CALCalculationMethodInfo);
	}

	protected void CheckCALCalculationMethod()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CALCalculationMethodInfo, Parent.Lookups.CalculationMethodList);

		if (Parent.CALCalculationMethod == CalculationMethodList.Codes.WGT
			&& Parent.MovementHeader.ExportTransportModeFallbackOnInlandTransportMode == ModeOfTransportList.Codes._1_SeaTransport
			&& !Parent.DepartureHeaderContainers.Any(x => x.IsContainerised))
		{
			Parent.CALCalculationMethodInfo.AddMessageError(Res.GetString("1CC007FF-6ED9-4EC3-A1A4-01CD88B873E1", "When calculation is WGT and Transport mode is SEA, a container is mandatory."));
		}
	}
}
