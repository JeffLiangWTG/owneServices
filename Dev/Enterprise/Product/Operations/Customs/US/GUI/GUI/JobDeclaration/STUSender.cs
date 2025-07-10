using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.STU;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI
{
	class STUSender
	{
		public STUSender(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public void Send()
		{
			if (declaration != null && declaration.HasPSDChangedAndLiveEntry())
			{
				var notification = declaration.GetPSDChangedNotification();

				if (Globals.Message.Show(notification, "Statement Delete/Add", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.DialogResult.Yes) == System.Windows.Forms.DialogResult.Yes)
				{
					new StatementDateChangeRequest(declaration).BuildStatementDateChangeRequestUsingCurrentPSD();
				}
				else
				{
					declaration.LogNoSTUSent();
				}
			}
		}
	}
}
