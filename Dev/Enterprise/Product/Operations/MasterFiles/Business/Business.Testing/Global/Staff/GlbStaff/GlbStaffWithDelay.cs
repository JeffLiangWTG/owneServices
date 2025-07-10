using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffWithDelay : GlbStaff
	{
		Action action;
		public GlbStaffWithDelay(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetAction(Action action)
		{
			this.action = action;
		}

		protected override void AfterCodeGeneration()
		{
			action.Invoke();

			base.AfterCodeGeneration();
		}
	}
}
