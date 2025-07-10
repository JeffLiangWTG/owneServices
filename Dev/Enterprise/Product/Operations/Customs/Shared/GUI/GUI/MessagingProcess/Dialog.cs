using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public class Dialog : IDialog
	{
		public Dialog(IBusiness businessEntity, Type typeOfForm)
		{
			BusinessEntity = Argument.NotNull(businessEntity, nameof(businessEntity));
			TypeOfForm = Argument.NotNull(typeOfForm, nameof(typeOfForm));
		}
		protected readonly IBusiness BusinessEntity;
		protected readonly Type TypeOfForm;

		IBusiness IDialog.DataSource => BusinessEntity;

		Type IDialog.TypeOfForm => TypeOfForm;
	}
}
