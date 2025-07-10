using System;
using CargoWise.Types;

namespace Enterprise.Customs.PL.GUI;

public partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
{
	public EntryInstructionDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

	protected override Type GetAuthorisationsUserControlType() => typeof(EntryInstructionAuthorisationsUserControl);

	protected override ZBool SealsTabPageVisibleCore(EU.Business.Declaration.JobDeclaration jobDeclaration) => jobDeclaration.JE_ContainerMode != Core.Constants.ContainerModes.FCL;
}
