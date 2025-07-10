namespace Enterprise.Customs.NL.NCTS.Module;

public class NctsMovementModule : EU.NCTS.Module.NctsMovementModule
{
	protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
	{
		return new NctsMovementFilterStripBusinessObject();
	}
}
