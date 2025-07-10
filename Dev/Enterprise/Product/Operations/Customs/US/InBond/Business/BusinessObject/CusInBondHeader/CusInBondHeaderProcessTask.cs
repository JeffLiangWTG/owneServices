using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderProcessTask : ProcessTask, Integration.Customs.US.InBond.ICusInBondHeaderProcessTask
	{
		public CusInBondHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.US.InBond; }
		}

		public new CusInBondHeader Parent
		{
			get { return (CusInBondHeader)base.Parent; }
		}

		protected override Type ParentType
		{
			get { return typeof(CusInBondHeader); }
		}

		public override ZDateTime ETA
		{
			get { return Parent.BH_ETA; }
		}

		public override ZDateTime ETD
		{
			get { return Parent.BH_SailingDate; }
		}

		public override ZString LoadOrOriginPort
		{
			get { return Parent.BH_Calc_ImportLoadPortUNLOCO; }
		}

		public override ZString DischargeOrDestinationPort
		{
			get { return Parent.BH_Calc_PortUnladingUNLOCO; }
		}
	}
}
