namespace Enterprise.Customs.NZ.Module.MAFeBACCa
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
	using Enterprise.Customs.NZ.GUI.MAFeBACCa;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.PlugIn;

	public class MAFeBACCaConsolPlugInController : BaseMAFeBACCaPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.MAFeBACCaConsolPlugIn; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new MAFeBACCaPlugIn(new MAFPlugInSupportConsolWrapper((ForwardingConsol)businessEntity));
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
