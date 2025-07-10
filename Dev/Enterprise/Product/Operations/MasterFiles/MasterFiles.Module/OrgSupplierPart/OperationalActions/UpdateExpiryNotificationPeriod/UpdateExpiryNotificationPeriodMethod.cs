using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateExpiryNotificationPeriodMethod : OperationalActionMethod
	{
		public UpdateExpiryNotificationPeriodMethod()
			: base(new ZGuid("ca89b78c-f332-4627-a2bc-c0f7c2e174a1"))
		{
		}

		#region NewApplicator

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateExpiryNotificationPeriodMethodApplicator(Name, factory);
		}

		#endregion

		#region NewGuiControl

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new UpdateExpiryNotificationPeriodControl();
		}

		#endregion

		#region Name

		public override string Name
		{
			get { return Res.GetString("f9e8dc0c-a0c1-44e0-a40b-2c68c6d46183", "Update the Expiry Notification Period (Days)"); }
		}

		#endregion

		#region Description

		public override string Description
		{
			get { return Res.GetString("f9e8dc0c-a0c1-44e0-a40b-2c68c6d46183", "Update the Expiry Notification Period (Days)"); }
		}

		#endregion
	}
}
