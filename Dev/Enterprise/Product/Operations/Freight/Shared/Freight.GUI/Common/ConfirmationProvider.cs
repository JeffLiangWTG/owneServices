namespace Enterprise.Freight.GUI
{
	using System;
	using System.Windows.Forms;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;

	public class ConfirmationProvider : IConfirmationProvider
	{
		public ConfirmationResult GetConfirmation(string message, string caption, ConfirmationOption option)
		{
			if (option == ConfirmationOption.YesNo)
			{
				var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, DialogResult.Yes);
				return (result == DialogResult.Yes) ? ConfirmationResult.Yes : ConfirmationResult.No;
			}
			else
			{
				throw new InvalidOperationException("Incorrect confirmation option");
			}
		}

		public static void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.SetValue<IConfirmationProvider, ConfirmationProvider>();
			}
		}

		public static void Register(ZForm form)
		{
			if (form != null && form.BusinessEntity != null)
			{
				Register(form.BusinessEntity.Factory);
			}
		}
	}
}
