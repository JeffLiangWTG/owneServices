using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseDetailValidation : AutoReleaseDetailValidation
	{
		public ReleaseDetailValidation(AutoReleaseDetail parent)
			: base(parent) { }

		#region ReleaseCount

		protected override void CheckReleaseCount()
		{
			base.CheckReleaseCount();
			CompareValidation.CheckNumberNotNegative(Parent.ReleaseCountInfo);

			if (Parent.ReleaseCount > Parent.Container.JC_ContainerCount)
			{
				Parent.ReleaseCountInfo.AddError(Res.GetString("1e57b687-31e1-4d5f-af29-3758acfdf649", "You can't release more containers than are booked."));
			}
		}

		#endregion

		#region Implementation

		public new ReleaseDetail Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseDetail)base.Parent; }
		}

		#endregion
	}
}


