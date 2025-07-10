using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.GUI;

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
	}
}
