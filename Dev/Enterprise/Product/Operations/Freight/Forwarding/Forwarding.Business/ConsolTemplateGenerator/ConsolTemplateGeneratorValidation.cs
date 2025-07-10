using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolTemplateGeneratorValidation : AutoConsolTemplateGeneratorValidation
	{
		public ConsolTemplateGeneratorValidation(AutoConsolTemplateGenerator parent)
			: base(parent)
		{
		}

		protected override void CheckServiceLevel()
		{
			base.CheckServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.ServiceLevelInfo, Parent.NeutralAirWaybillServiceLevelList);
		}

		#region Implementation

		public new ConsolTemplateGenerator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ConsolTemplateGenerator)base.Parent; }
		}

		#endregion
	}
}
