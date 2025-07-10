using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class EntryLineFilterControl : ZFilterStripControl
	{
		readonly System.ComponentModel.Container components;

		public EntryLineFilterControl()
		{
			InitializeComponent();
		}

		public EntryLineFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
