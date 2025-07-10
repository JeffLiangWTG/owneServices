using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateOneOffContainersValidation : AutoRateOneOffContainersValidation
	{
		public RateOneOffContainersValidation(AutoRateOneOffContainers parent)
			: base(parent)
		{
		}

		#region TC_RC

		protected override void CheckTC_RC()
		{
			base.CheckTC_RC();
			MandatoryValidation.CheckEntered(Parent.TC_RCInfo);
		}

		#endregion

		#region TC_ContainerCount

		protected override void CheckTC_ContainerCount()
		{
			base.CheckTC_ContainerCount();
			MandatoryValidation.CheckEntered(Parent.TC_ContainerCountInfo);
		}

		#endregion
	}
}
