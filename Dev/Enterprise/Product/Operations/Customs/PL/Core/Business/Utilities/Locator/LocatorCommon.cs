using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business;

[Immutable]
public sealed class LocatorCommon : LocatorBase
{
	public override ZString ApplicationCodes => Constants.AllSupportedApplications;

	protected override BusinessObject FindBusinessObjectByLRN(BusinessObjectFactory factory, ZString lrn) => null;
}
