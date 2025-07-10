using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.TransportConsignment.Module;

namespace Enterprise.TransportConsignment.GUI // this is not for the modules, it is an embedded filter control on a form
{
	public partial class DtbCarrierFilterControl : DtbChildFilterControl, IDtbCarrierFilterControl
	{
		public DtbCarrierFilterControl()
			: base(new DtbCarrierFilterBusinessObject())
		{
			InitializeComponent();
			PerformSearch += ChildFilterControl_PerformSearch;
		}

		public new DtbCarrierFilterBusinessObject FilterBusinessObject
		{
			get { return (DtbCarrierFilterBusinessObject)base.FilterBusinessObject; }
		}

		#region Perform Search

		void ChildFilterControl_PerformSearch(object sender, EventArgs e)
		{
			PerformSearchCore();
		}

		void PerformSearchCore()
		{
			Grid.SuspendLayout(); // do not refresh on each add to the collection

			try
			{
				((ActiveBusinessObjectCollection<OrgHeader>)GridCollection).AdditionalFilter = FilterBusinessObject.Filter;
			}
			finally
			{
				Grid.ResumeLayout();
			}
		}

		#endregion

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

			PerformSearch -= ChildFilterControl_PerformSearch;
			base.Dispose(disposing);
		}

		#endregion
	}
}
