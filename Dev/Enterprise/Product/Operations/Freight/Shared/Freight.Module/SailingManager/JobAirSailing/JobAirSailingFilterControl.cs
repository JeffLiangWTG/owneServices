using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Module
{
	public partial class JobAirSailingFilterControl : JobSailingFilterControl
	{
		/// <summary>
		/// Only for VS designer.
		/// </summary>
		public JobAirSailingFilterControl()
		{
		}

		public JobAirSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_NKVessel).IsUnavailable = true;
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_VoyageFlight).Caption = Res.GetString("JobSailingFilterControl|FlightNo", "Flight No.");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotCutOff).Caption = Res.GetString("JobSailingFilterControl|LooseCutOff", "Loose Cut Off");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotReceivalCommences).Caption = Res.GetString("JobSailingFilterControl|LooseRecStart", "Loose Rec. Start");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotAvailabilityDate).Caption = Res.GetString("JobSailingFilterControl|LooseAvail", "Loose Avail.");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotStorageDate).Caption = Res.GetString("JobSailingFilterControl|LooseStor", "Loose Stor.");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_CTOCutOff).Caption = Res.GetString("JobSailingFilterControl|ULDCutOff", "ULD Cut Off");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_CTOReceivalCommences).Caption = Res.GetString("JobSailingFilterControl|ULDRecStart", "ULD Rec. Start");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_CTOAvailabilityDate).Caption = Res.GetString("JobSailingFilterControl|ULDAvail", "ULD Avail.");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_CTOStorageDate).Caption = Res.GetString("JobSailingFilterControl|ULDStor", "ULD Stor.");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_ReservedMasterBill).Caption = Res.GetString("JobSailingFilterControl|RsrvdMaster", "Rsrvd. Master");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_E_DEP).CaptionResourceString = Res.GetData("JobAirSailingFilterControl|059ce742-d07a-4223-876e-191c56d33d4e", "Load Port ETD");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_E_ARV).CaptionResourceString = Res.GetData("JobAirSailingFilterControl|7055f071-824d-4540-b892-966f8e5cc879", "Disch. Port ETA");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_A_DEP).CaptionResourceString = Res.GetData("JobAirSailingFilterControl|0f94d5ff-f54b-4923-b177-cf820bdc6aaa", "ATD");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_A_ARV).CaptionResourceString = Res.GetData("JobAirSailingFilterControl|e68c8ebf-0fdf-4f33-b3a1-d7e4451d2ded", "ATA");
			base.OnLoad(e);
		}

		#region BuildConsols

		protected override void BuildConsols()
		{
			var selectedSailings = GetSelectedSailings();

			var factory = new BusinessObjectFactory();
			var helper = new BuildConsolHelper();

			var errorMessage = new StringBuilder();
			var sailings = new JobSailingCollection(factory);
			foreach (BusinessObject element in selectedSailings)
			{
				var sailing = factory.Load<JobSailing>(element.PK);
				if (helper.CheckCanBuildConsolFromSailing(sailing))
				{
					sailings.Add(sailing);
				}
				else
				{
					errorMessage.AppendLine("- " + Res.GetString("a68fd30d-2537-4525-a184-a20c5a6cfbb6", "{0} already has bookings packed for it. Please use Pack Containers.", sailing.HumanReadableName));
				}
			}

			if (sailings.Count > 0)
			{
				var multiDaysSelection = ObjectFactory.Get<IMultiDaysSelection>("IMultiDaysSelection", sailings, factory);
				var result = ShowBulkConsolCreationForm(multiDaysSelection);
				if (result == DialogResult.Yes)
				{
					multiDaysSelection.Generate(helper.BuildConsolFromSailing);

					ShowSchedulesConsolsForm(multiDaysSelection);
				}
			}

			if (errorMessage.Length > 0)
			{
				errorMessage.Insert(0, Res.GetString("a7dc2043-bf00-4dec-a236-7473cceae398", "Can not build consols for all selected sailings:") + System.Environment.NewLine);
				Globals.Message.Show(errorMessage.ToString().Trim(), Res.GetString("d59def4e-aa3c-4bc3-b01b-7e35133cfec2", "Build Consol"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		DialogResult ShowBulkConsolCreationForm(IMultiDaysSelection multiDaysSelection)
		{
			using (var form = (ZChildForm)ObjectFactory.Get<IBulkConsolCreationForm>("IBulkConsolCreationForm", multiDaysSelection))
			{
				return form.ShowDialog();
			}
		}

		void ShowSchedulesConsolsForm(IMultiDaysSelection multiDaysSelection)
		{
			ZFormModaliser.ShowDialogAndDispose((ZChildForm)ObjectFactory.Get<ISchedulesConsolsForm>("ISchedulesConsolsForm", multiDaysSelection));
		}

		#endregion
	}
}
