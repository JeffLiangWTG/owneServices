using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.MAFeBACCa;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;

	public class MAFeBACCaDeclarationPlugInController : BaseMAFeBACCaDeclarationPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.MAFeBACCaDeclarationPlugin; }
		}

		protected override ZPlugIn GetPlugIn(JobDeclaration declaration)
		{
			return new MAFeBACCaPlugIn(new MAFPlugInSupportDeclarationWrapper(declaration));
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
