using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class EntryLineModule : Customs.Module.EntryLineModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.EntryLine;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryLineFilterBusinessObject(GridCollection);

		protected override IFilterControl GetNewFilterControl() => new EntryLineFilterControl(GridCollection, (EntryLineFilterBusinessObject)FilterBusinessObject);
	}
}
