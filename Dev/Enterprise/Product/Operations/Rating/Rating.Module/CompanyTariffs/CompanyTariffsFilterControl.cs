using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Module
{
	public partial class CompanyTariffsFilterControl : ZFilterStripControl
	{
		public CompanyTariffsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isDisposing);
		}

		#endregion
	}
}
