using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.MAFeBACCa;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa
{
	public class MAFeBACCaContainerPlugInController : BaseMAFeBACCaDeclarationPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.MAFeBACCaContainerPlugin; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.NZ.Module.Res.GetData("PlugInTabPage|MAFeBACCaContainerPlugin", "eBACCa/IPI", "Additional Fields required for an MPI eBACCa/IPI message."); } }

		protected override ZPlugIn GetPlugIn(JobDeclaration declaration)
		{
			return new MAFeBACCaContainerPlugIn(declaration);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
