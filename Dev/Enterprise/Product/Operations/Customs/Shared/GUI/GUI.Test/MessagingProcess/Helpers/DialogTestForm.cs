using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.MessagingProcess.Testing
{
	sealed class ValidDialogTestForm : ZChildForm
	{
		public ValidDialogTestForm(IBusiness businessEntity) : base(businessEntity)
		{
		}
	}

	sealed class InvalidDialogTestForm : ZForm
	{
		public InvalidDialogTestForm(object dataSource) : base(dataSource)
		{
		}
	}
}
