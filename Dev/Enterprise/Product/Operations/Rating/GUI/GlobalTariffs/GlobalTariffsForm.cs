using System.Linq;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public sealed partial class GlobalTariffsForm : RatingForm
	{
		public GlobalTariffsForm(CompanyTariff globalTariff)
			: base(globalTariff)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl1);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			if (globalTariff.IsLevelOneTariff())
			{
				GlobalTariffsHeaderUserControl.CompanyTariffDiscountVisible = false;
			}

			GlobalTariffsTabControl.WorkflowTabPage.Initialize(globalTariff);
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return GlobalTariffsTabControl.TopLevelTabControl; }
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			this.SetReadOnlyIncludingChildren(new[] { rateEntryFilterStripControl.Name }.ToList());
		}

		#region Data Transfer

		protected override IValueObjectDataAdapter ValueObjectDataAdapter
		{
			get { return new CompanyTariffValueObjectDataAdapter(); }
		}

		#endregion

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

