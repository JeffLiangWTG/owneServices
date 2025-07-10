using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// The base control for all rating calculator controls.
	/// </summary>
	public partial class RateCalculatorUserControl : ZUserControl, IBindTo
	{
		public RateCalculatorUserControl()
		{
			InitializeComponent();
		}

		public RateLine.CalculatorWrapper ViewCalculatorForBinding { get; set; }

		protected ZString IncludeGST
		{
			get { return Res.GetString("f8705251-655a-4d72-9ab9-54b27ef723cd", "Include {0}", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription); }
		}

		protected int DecimalPlaces
		{
			get
			{
				return ViewCalculatorForBinding.Calculator.DecimalPlaces;
			}
		}

		#region Binding

		public string BindTo
		{
			get { return bindTo; }
			set
			{
				if (ViewCalculatorForBinding.Calculator == null)
				{
					var nameOfCalculator = nameof(ViewCalculatorForBinding) + "." + nameof(ViewCalculatorForBinding.Calculator);
					throw new ArgumentNullException(nameOfCalculator);
				}

				bindTo = value;
				SetBindings();
			}
		}
		string bindTo;

		protected virtual void SetBindings()
		{
			ZGrid itemsGrid = GetRateLineItemsGrid();
			if (itemsGrid != null)
			{
				if (ViewCalculatorForBinding.IsWiseRatesView)
				{
					itemsGrid.BindTo = BindTo + "+IRateLineItems";
				}
				else
				{
					itemsGrid.BindTo = BindTo + "+RateLineItems";
				}
			}
		}

		protected void SetBinding(IBindTo control, string propertyName)
		{
			if (!ViewCalculatorForBinding.Calculator.ContainsMapToProperty(propertyName))
			{
				ReportIncorrectMapperPropertyAccessed(propertyName);
			}

			control.BindTo = BindTo + '+' + propertyName;
		}

		protected void SetListBinding(IBindToList control, string propertyName)
		{
			control.BindToList = BindTo + '+' + propertyName;
		}

		/// <summary>
		/// This binding method is called to set the collection view that will
		/// be used to populate the grid. Eg the RateLineItems grid on the CMB
		/// calculator that is not the checkboxes (which themselves are also
		/// RateLineItems)
		/// </summary>
		protected void SetGridBinding(ZGrid control, string propertyName)
		{
			control.BindTo = BindTo + '+' + propertyName;
		}

		protected Type DataSourceTypeForBinding => ViewCalculatorForBinding.IsWiseRatesView ? typeof(IRateLineItemsView) : typeof(RateLineItemsView);

		void ReportIncorrectMapperPropertyAccessed(ZString propertyName)
		{
			ErrorReporter.ReportOnce("IncorrectRateLineItemMapper" + propertyName, "There is no mapping for property [" + propertyName + "] in calculator " + this.GetType().FullName + ".");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			ZGrid itemsGrid = GetRateLineItemsGrid();

			base.SetDataBinding(dataSource, dataMember);

			if (CurrentDataItem != null && itemsGrid != null)
			{
				MenuItem[] menuItems = new MenuItem[itemsGrid.ContextMenu.MenuItems.Count];
				itemsGrid.ContextMenu.MenuItems.CopyTo(menuItems, 0);
				ContextMenu contextMenu = new ContextMenu(menuItems);

				itemsGrid.ContextMenu = contextMenu;
			}
		}

		internal virtual ZGrid GetRateLineItemsGrid()
		{
			return Controls["RateLineItemsGrid"] as ZGrid;
		}

		#endregion

		#region Grid Remove Action

		public virtual RemoveAction RemoveAction
		{
			get { return fRemoveAction; }
			set
			{
				fRemoveAction = value;

				ZGrid itemsGrid = GetRateLineItemsGrid();
				if (itemsGrid != null)
				{
					itemsGrid.RemoveAction = value;
				}
			}
		}

		RemoveAction fRemoveAction = RemoveAction.RemoveAndDelete;

		#endregion

		#region OnSwitched

		internal virtual void OnSwitched(Calculator viewCalculator)
		{
		}

		internal void ShowAndBringToFront()
		{
			this.Show();
			this.BringToFront();
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

