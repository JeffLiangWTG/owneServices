using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.GUI;

public partial class Phase5DepartureMovementForm : EU.NCTS.GUI.Phase5DepartureMovementForm
{
	[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
	public Phase5DepartureMovementForm() : base()
	{
		InitializeComponent();
	}

	public Phase5DepartureMovementForm(NctsHeader nctsMovement) : base(nctsMovement)
	{
		InitializeComponent();
		SetTabAtIndex();
	}

	void SetTabAtIndex()
	{
		if (BusinessEntity.IsDepartureMovement)
		{
			this.MainTabControl.TabPages.Insert(this.Phase5ManifestsToOpenTabPage, 5);
			this.MainTabControl.TabPages.Insert(this.WarehouseToOpenTabPage, 6);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this.Phase5ManifestsToOpenTabPage.Dispose();
			this.WarehouseToOpenTabPage.Dispose();
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	public new NctsHeader BusinessEntity => (NctsHeader)base.BusinessEntity;
}
