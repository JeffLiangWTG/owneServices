using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public static class OpportunityFilterValidationMessage
	{
		public static void Show(object sender, EventArgs e)
		{
			Globals.Message.ShowError(Res.GetString("30c209b0-1a9a-4422-9de2-ebdd5d5be4b2", "There are errors. Please correct these before searching."), "Errors...");
		}
	}
}
