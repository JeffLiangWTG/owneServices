using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateUpdaterValidation : AutoRateUpdaterValidation
	{
		public RateUpdaterValidation(AutoRateUpdater parent)
			: base(parent) { }

		protected override void CheckPrinterPK()
		{
			base.CheckPrinterPK();
			MandatoryValidation.CheckEntered(Parent.PrinterPKInfo);
		}

		#region Implementation

		public new RateUpdater Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RateUpdater)base.Parent; }
		}

		#endregion
	}
}

