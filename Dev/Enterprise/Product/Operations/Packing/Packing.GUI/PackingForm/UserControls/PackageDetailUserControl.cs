using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public partial class PackageDetailUserControl : ZUserControl
	{
		#region Construction

		public PackageDetailUserControl()
		{
			InitializeComponent();
			ClearControlText();
			InitializeComponentOverlap();

			var captionRender = MarksAndNumbersTextBox.GetExtension<ILabelCaptionRenderer>();
			captionRender.Caption = " " + captionRender.Caption;
		}

		void InitializeComponentOverlap()
		{
			AdditionalInfoGroupBox.AllowOutsideOfParent();
			DetailsGroupBox.AllowOutsideOfParent();
			MarksAndNumbersGroupBox.AllowOutsideOfParent();
			PackageTemperaturesGroupBox.AllowOutsideOfParent();
			ReferencesGroupBox.AllowOutsideOfParent();
			PackageOrderReferenceGroupBox.AllowOutsideOfParent();
		}

		void ClearControlText()
		{
			foreach (Control control in Controls)
			{
				ClearControlText(control);
			}
		}

		void ClearControlText(Control control)
		{
			foreach (Control childControl in control.Controls)
			{
				ClearControlText(childControl);
			}

			var textBox = control as TextBoxBase;
			if (textBox != null)
			{
				textBox.ResetText();
			}
		}

		#endregion

		#region Bind

		public void Bind(PkgPackageJob packageJob, PkgPackage package)
		{
			var isScanEventsTabVisible = IsScanEventsTabVisible = packageJob?.ParentJob?.IsScanEventsVisible ?? false;
			IsPackageSealsTabVisible = WarehouseDataRegistry.Instance.EnablePackageSealNumbers.Value;

			if (packageJob != null)
			{
				IsContainerTabVisible = package != null && package.IsContainer;
				IsLabelPrinterVisible = package != null && package.CanPrintCarrierLabel;
				IsPackageOrderReferenceTabVisible = package != null && package.IsTransitPackage;
				IsAdditionalReferenceTabVisible = package != null && package.IsTransitPackage;
			}

			var previousPackage = GetBoundPackage();
			if (previousPackage == null || previousPackage != package) // no need to rebind if same node selected
			{
				Unbind();
				SetDataBinding(package, "");

				if (packageJob != null) // is null on closing the parent form
				{
					if (package == null) // there is nothing to Bind To, set manually otherwise it will be blank
					{
						PackageIdLabel.Text = Res.GetString("1e0a2986-e02f-44fb-af9a-f74974847ec2", "Package ID:");
						RemoveAddNewEventMenuItem();
					}
					else
					{
						package.KP_ClosedTimeUtcInfo.ValueChanged += KP_IsClosedInfo_ValueChanged;
						package.PackTypeChangingFromContainerCancelled += Package_ChangeFromContainerCancelled;
						package.AllowLoadingOverpackChildrenUndgs = true;

						if (isScanEventsTabVisible && addNewEventMenuItem == null)
						{
							RebuildAddNewEventMenuItem();
						}
					}
#if WINZOR
					Refresh();
#endif
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var package = dataSource as PkgPackage;
			var isCreatingCollection = (Collection == null && package != null);

			if (isCreatingCollection)
			{
				Collection = new NonActivePkgPackageCollection(package.Factory);
				temporaryCollection = null; // cleanup
			}

			if (Collection == null) // bind to a temporary collection to update readonly of the controls
			{
				base.SetDataBinding(TemporaryCollection, dataMember);
			}
			else
			{
				Collection.RemoveAll();
				if (package != null)
				{
					Collection.Add(package);
				}

				if (isCreatingCollection)
				{
					base.SetDataBinding(Collection, dataMember);
				}
			}
		}

		PkgPackage GetBoundPackage()
		{
			var collection = DataSource as NonActivePkgPackageCollection;
			return (collection != null && collection.Any()) ? collection[0] : null;
		}

		void Unbind()
		{
			var package = GetBoundPackage();
			if (package != null)
			{
				package.KP_ClosedTimeUtcInfo.ValueChanged -= KP_IsClosedInfo_ValueChanged;
				package.PackTypeChangingFromContainerCancelled -= Package_ChangeFromContainerCancelled;
			}
		}

		#region NonActivePkgPackageCollection

		NonActivePkgPackageCollection TemporaryCollection => temporaryCollection ?? (temporaryCollection = new NonActivePkgPackageCollection(new BusinessObjectFactory()));
		NonActivePkgPackageCollection Collection;
		NonActivePkgPackageCollection temporaryCollection;

		internal class NonActivePkgPackageCollection : BusinessObjectCollection<PkgPackage>
		{
			public NonActivePkgPackageCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion

		#endregion

		#region ViewMode

		public void SetViewMode(PackingViewMode viewMode)
		{
			EnableOrDisableControls(Controls, viewMode != PackingViewMode.NoEditing);
		}

		void EnableOrDisableControls(Control.ControlCollection controls, bool enabled)
		{
			foreach (Control control in controls)
			{
				if (!(control is TabControl) && !(control is GroupBox) && !(control is Label) && !(control is Panel))
				{
					control.Enabled = enabled;
				}

				EnableOrDisableControls(control.Controls, enabled);
			}
		}

		#endregion

		#region IsContainerTabVisible, IsScanEventsTabVisible, IsPackageOrderReferenceTabVisible, IsAdditionalReferenceTabVisible

		bool IsContainerTabVisible
		{
			set
			{
				// .NET BUG: Create the Handle otherwise TabPages.Insert() won't work 
				// See http://social.msdn.microsoft.com/Forums/en/winforms/thread/5d10fd0c-1aa6-4092-922e-1fd7af979663
				var handle = DetailsTabControl.Handle;
				DetailsTabControl.SelectedIndex = 0;

				if (value)
				{
					DetailsTabControl.TabPages.Remove(PackageTemperaturesTabPage);

					if (!DetailsTabControl.TabPages.Contains(ContainerTabPage))
					{
						DetailsTabControl.TabPages.Insert(1, ContainerTabPage);
					}
					if (!DetailsTabControl.TabPages.Contains(ContainerTemperaturesTabPage))
					{
						DetailsTabControl.TabPages.Insert(2, ContainerTemperaturesTabPage);
					}
				}
				else
				{
					DetailsTabControl.TabPages.Remove(ContainerTabPage);
					DetailsTabControl.TabPages.Remove(ContainerTemperaturesTabPage);

					if (!DetailsTabControl.TabPages.Contains(PackageTemperaturesTabPage))
					{
						DetailsTabControl.TabPages.Insert(1, PackageTemperaturesTabPage);
					}
				}
			}
		}

		bool IsScanEventsTabVisible
		{
			set => SetTabVisible(ScanEventsTabPage, value);
		}

		bool IsPackageSealsTabVisible
		{
			set => SetTabVisible(PackageSealsTabPage, value);
		}

		bool IsPackageOrderReferenceTabVisible
		{
			set => SetTabVisible(PackageOrderReferenceTabPage, value);
		}

		bool IsAdditionalReferenceTabVisible
		{
			set => SetTabVisible(AdditionalReferenceTabPage, value);
		}

		void SetTabVisible(ZTabPage tabPage, bool isVisible)
		{
			if (isVisible)
			{
				if (!DetailsTabControl.TabPages.Contains(tabPage))
				{
					DetailsTabControl.TabPages.Add(tabPage);
				}
			}
			else
			{
				DetailsTabControl.TabPages.Remove(tabPage);
			}
		}

		#endregion

		#region Add New Event Menu Item

		void RebuildAddNewEventMenuItem()
		{
			ScanEventsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			addNewEventMenuItem = new ZMenuItem(ResString.GetMultilingualString("56af3f0b-cc71-4ba5-b5cf-963e2feb6c89", "Add New Event"), AddNewEventMenuItem_Click);
			ScanEventsGrid.ContextMenu.MenuItems.Add(addNewEventMenuItem);
		}

		void RemoveAddNewEventMenuItem()
		{
			if (addNewEventMenuItem != null)
			{
				var indexOfSeperator = ScanEventsGrid.ContextMenu.MenuItems.IndexOf(addNewEventMenuItem) - 1;
				ScanEventsGrid.ContextMenu.MenuItems.RemoveAt(indexOfSeperator);
				ScanEventsGrid.ContextMenu.MenuItems.Remove(addNewEventMenuItem);
				addNewEventMenuItem = null;
			}
		}

		MenuItem addNewEventMenuItem;

		void AddNewEventMenuItem_Click(object sender, EventArgs e)
		{
			ShowAddEventForm();
		}

		void ShowAddEventForm()
		{
			var package = GetBoundPackage();
			if (package != null)
			{
				var view = new StmALogCollectionView(package);
				ZFormModaliser.ShowDialogAndDispose(new ZStmALogAddForm(view, package.HasChanges));
			}
		}

		#endregion

		#region FocusOnPackageQty / IsPackageIDBoxFocused

		public void FocusOnPackageQty()
		{
			PackQtyCalcEdit.Focus();
		}

		public bool IsPackageIDBoxFocused => PackageIDTextBox.Focused;

		#endregion

		#region PackageIsClosedChanged

		public event EventHandler PackageIsClosedChanged;

		void KP_IsClosedInfo_ValueChanged(object sender, EventArgs e)
		{
			if (PackageIsClosedChanged != null)
			{
				PackageIsClosedChanged(sender, e);
			}
		}

		#endregion

		#region Package_ChangeFromContainerCancelled

		// When changing from CNT to non-CNT (eg PLT) and then clicking "No/Cancel", the droplist retains the PLT.
		// Refreshing binding after leaving the control fixes this and sets the text back to CNT.

		void Package_ChangeFromContainerCancelled(object sender, EventArgs e)
		{
			PackTypeDropEdit.Leave += PackTypeDropEdit_Leave;
		}

		void PackTypeDropEdit_Leave(object sender, EventArgs e)
		{
			PackTypeDropEdit.Leave -= PackTypeDropEdit_Leave;
			GetBoundPackage().KP_F3_NKPackTypeInfo.RefreshBinding();
		}

		#endregion

		#region IsLabelPrinterVisible

		bool IsLabelPrinterVisible
		{
			set
			{
				var labelPrinterControl = PackageTabPage.Controls.Find(nameof(LabelPrinterDropEdit), true).Single();
				labelPrinterControl.Visible = value;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Unbind();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}

	public class ZGroupBoxWithoutCaption : ZGroupBox
	{
	}
}
