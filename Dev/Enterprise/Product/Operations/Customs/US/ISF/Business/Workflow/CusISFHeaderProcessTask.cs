using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderProcessTask : ProcessTask, Integration.Customs.US.ISF.ICusISFHeaderProcessTask
	{
		public CusISFHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusISFHeader); }
		}

		public new CusISFHeader Parent
		{
			get { return (CusISFHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.ImporterSecurityFiling; }
		}

		#region ProcessTask Property Overrides

		#region Discharge/Destination Port

		public override ZString DischargeOrDestinationPort
		{
			get { return Parent.BF_RL_NKPlaceOfDelivery; }
		}

		#endregion

		#endregion
	}
}

