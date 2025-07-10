using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NL.Module;

public class EntryHeaderModule : EU.Module.EntryHeaderModule
{
	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new EntryHeaderController();

	public override ZBool HasActions => true;

	public override bool AllowNew => false;

	public override bool AllowDelete => false;

	protected override FilterBusinessObject GetNewFilterBusinessObject()
		=> new EntryHeaderFilterBusinessObject();
}
