namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbRoutePlannerDetailsUserControl : DtbRoutePlannerDetailsUserControlBase
	{
		public DtbRoutePlannerDetailsUserControl()
			: base()
		{
			InitializeComponent();
		}

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
