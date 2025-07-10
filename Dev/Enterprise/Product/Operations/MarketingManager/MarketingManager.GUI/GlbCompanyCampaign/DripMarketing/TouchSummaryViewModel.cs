using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class TouchSummaryViewModel : ViewModelBase
	{
		public void SetDataSource(GlbCompanyCampaign masterCampaign)
		{
			if (MasterCampaign != null)
			{
				MasterCampaign.TransitionProgressChanged -= TransitionProgressChanged;
				MasterCampaign.EndTransitionProgress -= EndTransitionProgress;
			}
			MasterCampaign = masterCampaign;
			MasterCampaign.TransitionProgressChanged += OnTransitionProgressChanged;
			MasterCampaign.EndTransitionProgress += OnEndTransitionProgress;
			CollectionForDefaults = new GlbCompanyCampaignCollectionStrongyTyped(masterCampaign, ZQuery.NoResultQuery);

			horizontals = new ObservableCollection<CampaignHorizontal>(masterCampaign.Horizontals);

			OnPropertyChanged(nameof(CampaignName));
			OnPropertyChanged(nameof(Horizontals));
		}

		public bool HasDataContext
		{
			get
			{
				return MasterCampaign != null;
			}
		}

		public GlbCompanyCampaign MasterCampaign { get; private set; }

		public bool MasterCampaignHasChanges => MasterCampaign.HasChanges;

		GlbCompanyCampaignCollectionStrongyTyped CollectionForDefaults { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		public string CampaignName
		{
			get { return MasterCampaign?.G0_CampaignName; }
		}

		ObservableCollection<CampaignHorizontal> horizontals;

		public ObservableCollection<CampaignHorizontal> Horizontals
		{
			get { return horizontals; }
		}

		public CampaignHorizontal CurrentHorizontal { get; set; }

		public bool HorizontalIsSelected
		{
			get { return CurrentHorizontal != null; }
		}

		public void SetCurrentHorizontal(ZByte id)
		{
			CurrentHorizontal = Horizontals.FirstOrDefault(h => h.Id == id);
			OnPropertyChanged(nameof(HorizontalIsSelected));
		}

		internal static void OpenCampaign(GlbCompanyCampaign campaign, string statusCode = "")
		{
			var form = GetGlbCompanyCampaignForm(campaign);

			if (!string.IsNullOrEmpty(statusCode))
			{
				form.FocusOnCampaignItem<ZString>(FocusOnTrackingTabTypes.StatusDescription, statusCode);
			}
			else
			{
				form.SelectTab(form.TouchSetupTabPage);
			}
		}

		internal static void OpenCampaign(GlbCompanyCampaign campaign, bool? unsubscribed)
		{
			var form = GetGlbCompanyCampaignForm(campaign);

			if (unsubscribed.HasValue)
			{
				form.FocusOnCampaignItem<ZBool>(FocusOnTrackingTabTypes.UnsubscribeStatus, unsubscribed.Value);
			}
			else
			{
				form.SelectTab(form.TouchSetupTabPage);
			}
		}

		public static void OpenCampaignForTest(GlbCompanyCampaign campaign) => OpenCampaign(campaign);

		static GlbCompanyCampaignForm GetGlbCompanyCampaignForm(GlbCompanyCampaign campaign)
		{
			var form = ZControllerFactory.Create(campaign.IsHRCampaign ? ControllerIDs.HRGlbCompanyCampaign : ControllerIDs.GlbCompanyCampaign).ShowEditForm(campaign) as GlbCompanyCampaignForm;
			form.Closing += delegate
			{
				campaign.StatModel.ReloadLinksAndClicks();
			};

			return form;
		}

		public void AddHorizontal()
		{
			if (MasterCampaign.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("6261C557-C8CE-4D71-AD03-D785CC86BBAF", "Please save before adding horizontals"), Res.GetString("9B94C16E-FC70-47CD-9263-9C0BA322A9A2", "Cannot add a Horizontal"));
				return;
			}

			ZByte horizontalId;
			if (Horizontals.All(h => h.Campaigns.Count != 0))
			{
				var horizontal = MasterCampaign.AddHorizontal();
				horizontals.Add(horizontal);
				horizontalId = horizontal.Id;
			}
			else
			{
				horizontalId = Horizontals.Where(h => h.Campaigns.Count == 0).Max(h => h.Id);
			}

			AddVertical(horizontalId);
		}

		public void AddVertical(ZByte horizontalId)
		{
			if (MasterCampaign.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("AC5C9582-8F44-48C6-8974-DA7CDF8CA2B4", "Please save before adding verticals"), Res.GetString("75E4EB40-AB87-4A67-813A-87C32008081D", "Cannot add a Vertical"));
				return;
			}

			CollectionForDefaults.HorizontalId = horizontalId;

			var controller = MasterCampaign.IsHRCampaign ? ZControllerFactory.Create(ControllerIDs.HRGlbCompanyCampaign) : ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaign);
			controller.SetCollectionForDefaultsAndValidation(CollectionForDefaults);
			var form = controller.ShowNewForm() as ZForm;

			if (form != null)
			{
				form.BringToFront();
				form.Shown += delegate
				{ form.BringToFront(); };
			}

			var newCampaign = form.BusinessEntityForPersistingForm as GlbCompanyCampaign;
			if (newCampaign != null)
			{
				newCampaign.Saved += NewCampaignOnSaved;
			}

#if DEBUG
			LastController = controller;
#endif
		}

		void NewCampaignOnSaved(object sender, EventArgs eventArgs)
		{
			var newCampaign = sender as GlbCompanyCampaign;

			if (newCampaign != null && newCampaign.IsInDatabase)
			{
				newCampaign.Saved -= NewCampaignOnSaved;
				MasterCampaign.AllTouches.RefreshFromDb();
				var newCampaignInCorrectFactory = MasterCampaign.AllTouches.FirstOrDefault(t => t.PK == newCampaign.PK);

				if (newCampaignInCorrectFactory != null)
				{
					var horizontal = horizontals.FirstOrDefault(h => h.Id == newCampaignInCorrectFactory.G0_HorizontalId);
					horizontal?.AddCampaign(newCampaignInCorrectFactory);

					newCampaignInCorrectFactory.RefreshStats();
				}

				OnNewCampaignSave(newCampaign, EventArgs.Empty);
			}
		}

#if DEBUG
		public ZController LastController;
#endif

		public void RemoveCampaign(GlbCompanyCampaign campaign)
		{
			var horizontalId = campaign.G0_HorizontalId;

			MasterCampaign.Horizontals.FirstOrDefault(h => h.Id == horizontalId)?.Campaigns.Remove(campaign);
			campaign.Delete();
			horizontals = new ObservableCollection<CampaignHorizontal>(MasterCampaign.Horizontals);
			OnPropertyChanged(nameof(Horizontals));
		}

		public bool ChangeTouchPosition(GlbCompanyCampaign campaign, GlbCompanyCampaign target)
		{
			if (MasterCampaign.ChangeTouchPosition(campaign, target))
			{
				horizontals = new ObservableCollection<CampaignHorizontal>(MasterCampaign.Horizontals);
				OnPropertyChanged(nameof(Horizontals));
				return true;
			}

			return false;
		}

		public string CheckTransitionsRules()
		{
			var transitionStringBuilder = new ZStringBuilder();

			foreach (var touch in MasterCampaign.AllTouches)
			{
				if (touch.TransitionRulesToThisCampaign.Count == 0 || touch.TransitionRulesToThisCampaign.All(r => r.IsDeleted))
				{
					transitionStringBuilder.AppendLine(touch.G0_CampaignNameMultilingual);
				}
			}

			if (transitionStringBuilder.Length > 0)
			{
				transitionStringBuilder.Prepend(Res.GetString("E3EE2D9A-CD69-4CC2-93FF-D8AD53C2813A", "The following Touch Point(s) have no transition capability. Please set up their data source prior to transitioning the Master List:"));
			}

			return transitionStringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public string Launch()
		{
			var transitionScheduleErrorList = new List<String>();
			MasterCampaign.TransitionAndSchedule(transitionScheduleErrorList);
			RefreshStats();

			if(transitionScheduleErrorList.Count > 0)
			{
				transitionScheduleErrorList = transitionScheduleErrorList.Distinct().Take(10).ToList();
			}

			return string.Join("\r\n", transitionScheduleErrorList);
		}

		public void RefreshStats()
		{
			MasterCampaign.RefreshStats();
		}

		#region Transition progress

		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> TransitionProgressChanged;
		public event EventHandler EndTransitionProgress;

		protected virtual void OnTransitionProgressChanged(object sender, GlbCompanyCampaign.TransitionProgressEventArgs eventArgs)
		{
			TransitionProgressChanged?.Invoke(sender, eventArgs);
		}

		protected virtual void OnEndTransitionProgress(object sender, EventArgs eventArgs)
		{
			EndTransitionProgress?.Invoke(sender, EventArgs.Empty);
		}

		#endregion

		#region New Campaign Save

		public event EventHandler NewCampaignSave;

		protected virtual void OnNewCampaignSave(object sender, EventArgs eventArgs)
		{
			NewCampaignSave?.Invoke(sender, EventArgs.Empty);
		}

		#endregion

	}
}
