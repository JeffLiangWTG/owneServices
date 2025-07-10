using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.GUI;

public partial class Phase5ArrivalMovementForm : EU.NCTS.GUI.Phase5ArrivalMovementForm
{
	[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
	public Phase5ArrivalMovementForm()
	{
		InitializeComponent();
	}

	public Phase5ArrivalMovementForm(NctsHeader nctsMovement) : base(nctsMovement)
	{
		InitializeComponent();
	}

	protected override Type GetMessagesUserControlType() => typeof(MessagesTabUserControl);
}
