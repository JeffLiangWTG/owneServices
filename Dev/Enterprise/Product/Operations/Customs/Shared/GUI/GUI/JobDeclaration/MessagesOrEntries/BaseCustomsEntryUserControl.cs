using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsEntryUserControl : ZUserControl, IExtendedControl
	{
		public BaseCustomsEntryUserControl()
			: this(null)
		{
		}

		public BaseCustomsEntryUserControl(BaseJobDeclaration declaration)
		{
			this.JobDeclaration = declaration;
			InitializeComponent();
		}

		#region JobDeclaration

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual BaseJobDeclaration JobDeclaration
		{
			get { return fJobDeclaration; }
			set
			{
				if (fJobDeclaration != value)
				{
					if (fJobDeclaration != null)
					{
						UnHookControlVisibilityChangeEvents(fJobDeclaration);
					}
					fJobDeclaration = value;
					if (fJobDeclaration != null)
					{
						HookControlVisibilityChangeEvents(fJobDeclaration);
						HandleDeclarationControlVisibilityChanged();
					}
				}
			}
		}

		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		protected virtual void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			declarationValueChangedAnnouncer = ((IInvoicesProvider)declaration).GetValueChangedAnnouncer();
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged -= new EventHandler(JobDeclaration_ControlVisibilityChanged);
				declarationValueChangedAnnouncer.OnValueChanged += new EventHandler(JobDeclaration_ControlVisibilityChanged);
			}
		}

		protected virtual void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.Dispose();
			}
		}

		#endregion

		#region OnShown

#if DEBUG
		public bool OnShownCalled_ForTesting;
#endif

		public void OnShown()
		{
#if DEBUG
			OnShownCalled_ForTesting = true;
#endif
			if (JobDeclaration == null)
			{
				throw new InvalidOperationException("JobDeclaration is null");
			}
			if (JobDeclaration.MergeManager == null)
			{
				throw new InvalidOperationException("JobDeclaration.MergeManager is null");
			}

			if (JobDeclaration.MergeManager.SupportsAutoMerge)
			{
				if (JobDeclaration.MergeManager.RequiresMerge)
				{
					SendsMessagesToCustomsShutterUpperer messageCollector = new SendsMessagesToCustomsShutterUpperer(false);
					if (JobDeclaration.DoMerge(messageCollector))
					{
						HideRequiresMergeLabel();
					}
					else
					{
						ShowRequiresMergeLabel(messageCollector.InvalidOperationText);
					}
				}
				else
				{
					HideRequiresMergeLabel();
				}
			}
			RefreshControlVisibiltyOnShown();
		}

		protected virtual void RefreshControlVisibiltyOnShown()
		{
		}

		protected void ShowRequiresMergeLabel(string message)
		{
			RequiresMergeLabel.Text = message;
			RequiresMergeLabel.Dock = DockStyle.Fill;
			RequiresMergeLabel.BringToFront();
			RequiresMergeLabel.Visible = true;
		}
		public ZLabel RequiresMergeLabel;

		protected void HideRequiresMergeLabel()
		{
			RequiresMergeLabel.Visible = false;
		}

#if DEBUG
		public bool IsRequiresMergeLabelVisibleForTesting()
		{
			return RequiresMergeLabel != null && RequiresMergeLabel.Visible;
		}

		public ZString GetMergeErrorMessageForTesting()
		{
			return RequiresMergeLabel.Text;
		}

		public ZLabel GetRequiresMergeLabel()
		{
			return RequiresMergeLabel;
		}
#endif

		#endregion

		#region JobDeclaration_ControlVisibilityChanged

		protected void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
		{
			HandleDeclarationControlVisibilityChanged();
		}

		void HandleDeclarationControlVisibilityChanged()
		{
			if (!IsDisposed)
			{
				HandleDeclarationControlVisibilityChangedCore();
			}
		}

		protected virtual void HandleDeclarationControlVisibilityChangedCore()
		{
			ChangeGridColumnsVisibility();
			ChangeControlsVisibility();
		}

		protected virtual void ChangeGridColumnsVisibility()
		{
		}

		protected virtual void ChangeControlsVisibility()
		{
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			JobDeclaration = dataSource == null ? null : DeclarationFromDataSource((IBusiness)dataSource);
			if (dataSource != null)
			{
				HandleDeclarationControlVisibilityChanged();
			}
		}

		protected virtual BaseJobDeclaration DeclarationFromDataSource(IBusiness dataSource)
		{
			return dataSource as BaseJobDeclaration;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (JobDeclaration != null)
				{
					UnHookControlVisibilityChangeEvents(JobDeclaration);
				}

				extensions?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		public void InitializeGridLayout()
		{
			InitializeGridLayoutCore();
		}

		protected virtual void InitializeGridLayoutCore()
		{
		}

		#region Adding / Removing Tab Controls

		protected void AddTabPage(ZTabControl tabControl, ZTabPage tabPageToAdd)
		{
			if (tabPageToAdd != null)
			{
				bool foundTabPage = false;

				foreach (ZTabPage tabPage in tabControl.TabPages)
				{
					if (tabPage.Name == tabPageToAdd.Name)
					{
						foundTabPage = true;
						break;
					}
				}

				if (!foundTabPage)
				{
					tabControl.TabPages.Add(tabPageToAdd);
				}
			}
		}

		protected void RemoveTabPage(ZTabControl tabControl, ZTabPage tabPageToRemove)
		{
			if (tabPageToRemove != null)
			{
				foreach (ZTabPage tabPage in tabControl.TabPages)
				{
					if (tabPage.Name == tabPageToRemove.Name)
					{
						tabControl.TabPages.Remove(tabPage);
						break;
					}
				}
			}
		}

		#endregion

		#region Implementation

		BaseJobDeclaration fJobDeclaration;

		#endregion

		#region IExtendedControl

		Control IExtendedControl.Host => this;

		IControlExtensionCollection IExtendedControl.Extensions => extensions ?? (extensions = new ControlExtensionCollection(this));
		IControlExtensionCollection extensions;

		#endregion
	}
}
