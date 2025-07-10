using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccGLHeaderFilterControl : ZFilterStripControl
	{
		public AccGLHeaderFilterControl(IBusinessObjectCollection gridCollection, AccGLHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip() => new ZFilterStrip();

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				if (!AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
				{
					grid.RemoveFromAvailableColumns("AlternateAccounts");
				}
			}
		}
	}
}
