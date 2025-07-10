using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderProcessTask : ProcessTask, Integration.Customs.US.USAMS.ICusInBondHeaderProcessTask
	{
		public CusInBondHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusInBondHeader); }
		}

		public new CusInBondHeader Parent
		{
			get { return (CusInBondHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.US.AMS; }
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
			get { return Parent.BH_RL_NKPortOfLoading; }
		}

		public override ZString DischargeOrDestinationPort
		{
			get { return Parent.BH_RL_NKPortUnlading; }
		}
	}
}
