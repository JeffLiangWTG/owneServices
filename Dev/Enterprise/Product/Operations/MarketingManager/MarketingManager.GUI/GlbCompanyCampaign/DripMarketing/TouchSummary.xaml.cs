using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using CargoWise.Common;
using CargoWise.Main.Navigation.WPF;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Enterprise.MarketingManager.GUI
{
	/// <summary>
	/// Interaction logic for TouchSummary.xaml
	/// </summary>
	public partial class TouchSummary : UserControl, ITouchSummary
	{
		readonly TouchSummaryViewModel viewModel = new TouchSummaryViewModel();
		public TouchSummary()
		{
			InitializeComponent();
			viewModel.TransitionProgressChanged += OnLongRefreshProgress;
			viewModel.EndTransitionProgress += OnEndLongRefresh;
			viewModel.NewCampaignSave += OnNewCampaignSaved;
			DataContext = viewModel;

			Loaded += XamlTranslator.GetControlLoadedEvent<TouchSummary>();
			Loaded += delegate
			{
				var source = PresentationSource.FromVisual(this);
				var hwndTarget = source.CompositionTarget as HwndTarget;
				if (hwndTarget != null)
				{
					hwndTarget.RenderMode = RenderMode.SoftwareOnly;
				}
			};
		}

		public TouchSummaryViewModel ViewModel { get { return viewModel; } }

		public void SetDataContext(GlbCompanyCampaign campaign)
		{
			viewModel.SetDataSource(campaign);
			OnInitialCampaignSelected(campaign?.Horizontals?.FirstOrDefault()?.Campaigns?.FirstOrDefault());
		}

		void OpenCampaign_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var campaign = viewModel.CurrentHorizontal?.CurrentCampaign;
			if (campaign != null)
			{
				TouchSummaryViewModel.OpenCampaign(campaign);
			}
			else
			{
				Globals.Message.Show(
					Res.GetString("E8EF75F2-3830-4230-BC7A-D7E1D81C35E7", "Please select a Touch to edit"),
					Res.GetString("CFFE8942-B962-41A4-BC60-DEFE0410D7D4", "Edit Touch"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		void AddHorizontal_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			viewModel.AddHorizontal();
		}

		void AddVertical_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var horizontal = viewModel.CurrentHorizontal;
			if (horizontal != null)
			{
				viewModel.AddVertical(horizontal.Id);
			}
		}

		internal void RemoveCampaign_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string deleteCaption = Res.GetString("FEF284D2-44B7-46B9-99CA-758E5CED9794", "Delete Campaign");
			var campaign = viewModel.CurrentHorizontal?.CurrentCampaign;
			if (campaign != null)
			{
				var result = Globals.Message.Show(
					Res.GetString("EA470E3B-402B-4F53-BEB3-2ACA7CD0C82E", "Are you sure you want to delete {0}", campaign.TouchFullName),
					deleteCaption, MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					viewModel.RemoveCampaign(campaign);
					OnCampaignSelected(null);
					Refresh_Executed(null, e);
				}
			}
			else
			{
				Globals.Message.Show(
					Res.GetString("FE9405B1-5DF4-42D4-913C-5E84F3C817D6", "Please select a Touch to delete"),
					deleteCaption, MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		void Drop_OnHandler(object sender, DragEventArgs e)
		{
			var campaign = e.Data.GetData(DataFormats.Dif) as GlbCompanyCampaign;

			var lbi = sender as ListBoxItem;
			if (lbi == null)
			{
				return;
			}

			var target = lbi.DataContext as GlbCompanyCampaign;

			if (campaign != null && target != null)
			{
				if (viewModel.ChangeTouchPosition(campaign, target))
				{
					Refresh();
				}
			}
		}

		bool selectedRatioTextBox;
		Point dragStartPoint;

		void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control && sender is ListBoxItem listBoxItem && listBoxItem.IsSelected)
			{
				OnCampaignSelected((DataContext as TouchSummaryViewModel)?.MasterCampaign);
			}
			else
			{
				selectedRatioTextBox = FindVisualParent<TextBox>((DependencyObject)e.OriginalSource) != null;

				dragStartPoint = e.GetPosition(null);
				OnCampaignSelected((sender as ListBoxItem)?.DataContext as GlbCompanyCampaign);
			}
		}

		void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource.GetType() == typeof(Grid) || e.OriginalSource.GetType() == typeof(ScrollViewer))
			{
				OnCampaignSelected((DataContext as TouchSummaryViewModel)?.MasterCampaign);
			}
		}

		void PreviewMouseMove_OnHandler(object sender, MouseEventArgs e)
		{
			if (selectedRatioTextBox)
			{
				return;
			}

			var point = e.GetPosition(null);
			var diff = dragStartPoint - point;

			if (e.LeftButton == MouseButtonState.Pressed &&
				(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
			{
				var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
				if (lbi != null)
				{
					var dataObject = new DataObject();
					dataObject.SetData(DataFormats.Dif, lbi.DataContext);

					try
					{
						DragDrop.DoDragDrop(lbi, dataObject, DragDropEffects.Move);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						string key = this.GetType().Name + ".PreviewMouseMove_OnHandler." + ex.GetType().Name;
						ErrorReporter.ReportOnce(key, ex.Message, ex);
					}
				}
			}
		}

		T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
		{
			var parentObject = VisualTreeHelper.GetParent(child);
			if (parentObject == null)
			{
				return null;
			}

			T parent = parentObject as T;
			if (parent != null)
			{
				return parent;
			}

			return FindVisualParent<T>(parentObject);
		}

		void SummaryLabelClick_OnHandler(object sender, MouseButtonEventArgs e)
		{
			var item = sender as ListViewItem;
			var listView = item?.Parent as ListView;

			if (listView != null)
			{
				var campaign = listView.Tag as GlbCompanyCampaign;
				TouchSummaryViewModel.OpenCampaign(campaign, item.Tag.ToString());
			}
		}

		void UnsubscribeLabelClick_OnHandler(object sender, MouseButtonEventArgs e)
		{
			var item = sender as ListViewItem;
			var listView = item?.Parent as ListView;

			if (listView != null)
			{
				var campaign = listView.Tag as GlbCompanyCampaign;
				TouchSummaryViewModel.OpenCampaign(campaign, true);
			}
		}

#if DEBUG
		internal
#endif
				void Launch_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var caption = Res.GetString("86D4633C-4038-4002-BB9C-5CD8305A39FE", "Cannot Transition Master List");
			if (viewModel.MasterCampaignHasChanges)
			{
				Globals.Message.ShowError(Res.GetString("B1920141-BAF9-4D31-A5DC-E82B0DA40D20", "Please save this campaign before proceeding."), caption);
				return;
			}

			var transitionsRulesErrors = viewModel.CheckTransitionsRules();
			if (!string.IsNullOrEmpty(transitionsRulesErrors))
			{
				Globals.Message.ShowError(transitionsRulesErrors, caption);
				return;
			}

			try
			{
				OnBeginLongRefresh(ResString.GetMultilingualString("1318a399-089c-47b5-88ea-5d47317ee2c6", "The transition of Master List contacts is in progress..."));
				var errors = viewModel.Launch();
				if (!string.IsNullOrEmpty(errors))
				{
					Globals.Message.ShowError(errors);
				}
			}
			finally
			{
				OnEndLongRefresh(this, EventArgs.Empty);
			}
		}

		void Touch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			if (FindVisualParent<TextBox>((DependencyObject)e.OriginalSource) != null)
			{
				return;
			}

			var listBoxItem = sender as ListBoxItem;
			if (listBoxItem != null)
			{
				var campaign = listBoxItem.DataContext as GlbCompanyCampaign;
				TouchSummaryViewModel.OpenCampaign(campaign);
			}
		}

		void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Refresh();
		}

		void Refresh()
		{
			try
			{
				OnBeginLongRefresh(ResString.GetMultilingualString("19ab3776-d430-480e-817f-ae6362466b85", "Refreshing summary data..."));
				viewModel.RefreshStats();
			}
			finally
			{
				OnEndLongRefresh(this, EventArgs.Empty);
			}
		}

		void OnBeginLongRefresh(string statusMessage)
		{
			BeginLongRefresh?.Invoke(this, new GlbCompanyCampaign.TransitionProgressEventArgs(statusMessage));
		}

		void OnLongRefreshProgress(object sender, GlbCompanyCampaign.TransitionProgressEventArgs eventArgs)
		{
			LongRefreshProgress?.Invoke(sender ?? this, eventArgs);
		}

		void OnEndLongRefresh(object sender, EventArgs eventArgs)
		{
			EndLongRefresh?.Invoke(this, EventArgs.Empty);
		}

		void OnCampaignSelected(GlbCompanyCampaign glbCompanyCampaign)
		{
			CampaignSelected?.Invoke(this, new CampaignSelectedEventArgs(glbCompanyCampaign));

			if (glbCompanyCampaign != null)
			{
				viewModel?.SetCurrentHorizontal(glbCompanyCampaign.G0_HorizontalId);
			}
		}

		void OnInitialCampaignSelected(GlbCompanyCampaign glbCompanyCampaign)
		{
			InitialCampaignSelected?.Invoke(this, new CampaignSelectedEventArgs(glbCompanyCampaign));

			if (glbCompanyCampaign != null)
			{
				viewModel?.SetCurrentHorizontal(glbCompanyCampaign.G0_HorizontalId);
			}
		}

		void OnNewCampaignSaved(object sender, EventArgs eventArgs)
		{
			if (HorizontalsList.Items.Count == 1 && ((CampaignHorizontal)HorizontalsList.Items[0]).Campaigns.Count == 1)
			{
				var campaign = ((CampaignHorizontal)HorizontalsList.Items[0]).Campaigns[0];
				OnCampaignSelected(campaign);
			}
		}

		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> BeginLongRefresh;
		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> LongRefreshProgress;
		public event EventHandler EndLongRefresh;
		public event EventHandler<CampaignSelectedEventArgs> CampaignSelected;
		public event EventHandler<CampaignSelectedEventArgs> InitialCampaignSelected;

		void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		static bool IsTextAllowed(string text)
		{
			Regex regex = new Regex("[^0-9.-]+");
			return !regex.IsMatch(text);
		}

		void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				var text = (string)e.DataObject.GetData(typeof(string));
				if (!IsTextAllowed(text))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}
	}
}
