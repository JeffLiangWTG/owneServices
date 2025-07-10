using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class QueryTariffsForm : ZChildForm
	{
		public QueryTariffsForm()
		{
		}

		public QueryTariffsForm(QueryTariffOption option)
			: base(option)
		{
		}

		QueryTariffOption Option
		{
			get { return (QueryTariffOption)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "Query Tariffs"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				Option.RunPreSaveValidation();

				if (Option.TariffsToQuery.HasNotifications())
				{
					if (Globals.Message.Show(ThereIsANotification, "Send Messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) != DialogResult.OK)
					{
						result = ContinueWithSave.No;
					}
				}
			}

			return result;
		}
		public const string ThereIsANotification = "There are errors and/or warnings.\nAre you sure you wish to proceed?";

		void SendButton_Click(object sender, EventArgs e)
		{
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				try
				{
					Option.SendQuery();
					Option.Factory.Save();
				}
				catch (ZSaveException e1)
				{
					HandleSaveException(e1);
				}

				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
