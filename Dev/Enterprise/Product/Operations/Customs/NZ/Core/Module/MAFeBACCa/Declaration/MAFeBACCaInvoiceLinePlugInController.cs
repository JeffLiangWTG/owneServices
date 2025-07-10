using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.MAFeBACCa;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa
{
	public class MAFeBACCaInvoiceLinePlugInController : BaseMAFeBACCaDeclarationPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.MAFeBACCaInvoiceLinePlugin; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.NZ.Module.Res.GetData("PlugInTabPage|MAFeBACCaInvoiceLinePlugin", "eBACCa/IPI"); } }

		protected override ZPlugIn GetPlugIn(JobDeclaration declaration)
		{
			return new MAFeBACCaInvoiceLinePlugIn(declaration);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
