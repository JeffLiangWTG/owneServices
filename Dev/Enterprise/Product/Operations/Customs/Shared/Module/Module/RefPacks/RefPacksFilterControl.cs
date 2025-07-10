using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.Shared.Module
{
	/// <summary>
	/// Filter control for RefPacks.
	/// </summary>
	public partial class RefPacksFilterControl : ZFilterStripControl
	{
		private readonly System.ComponentModel.Container components;

		public RefPacksFilterControl()
		{
			InitializeComponent();
		}

		public RefPacksFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
