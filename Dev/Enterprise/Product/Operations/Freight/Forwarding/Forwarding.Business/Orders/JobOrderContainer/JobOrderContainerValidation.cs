using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderContainerValidation : AutoJobOrderContainerValidation
	{
		public JobOrderContainerValidation(AutoJobOrderContainer parent)
			: base(parent)
		{
		}

		#region J1_ContainerNumber

		protected override void CheckJ1_ContainerNumber()
		{
			base.CheckJ1_ContainerNumber();
			ContainerNumberValidation.WarnIfInvalid(Parent.J1_ContainerNumberInfo);
		}

		#endregion

		#region J1_RC

		protected override void CheckJ1_RC()
		{
			base.CheckJ1_RC();
			MandatoryValidation.CheckEntered(Parent.J1_RCInfo);
		}

		#endregion

		#region J1_ContainerCount

		protected override void CheckJ1_ContainerCount()
		{
			base.CheckJ1_ContainerCount();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.J1_ContainerCountInfo, 0);
		}

		#endregion
	}
}
