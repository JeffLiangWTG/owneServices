using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class BaseRateLinesAndItemsControl : ZUserControl, IBindTo
	{
		public BaseRateLinesAndItemsControl()
		{
			InitializeComponent();
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (RateLinesListManager != null)
			{
				UnhookEvents();
			}
			base.SetDataBinding(dataSource, dataMember);
			if (RateLinesListManager != null)
			{
				HookEvents();
				OnCurrentChanged(RateLinesListManager, EventArgs.Empty);
			}
		}

		protected CurrencyManager RateLinesListManager
		{
			get
			{
				if (RateLinesGrid.IsDisposed)
				{
					return null;
				}

				return (CurrencyManager)GetBindingManager(RateLinesGrid.GetBindingMember());
			}
		}

		protected RatingHeader Header
		{
			get { return (RatingHeader)CurrentDataItem; }
		}

		#region Events

		protected virtual void HookEvents()
		{
			if (RateLinesGrid != null && RateLinesListManager != null)
			{
				RateLinesListManager.CurrentChanged += OnCurrentChanged;
				RateLinesListManager.PositionChanged += OnCurrentChanged;
				RateLinesGrid.GridColourSchemeManagerDeciding += RateLinesBoundGrid_GridColourSchemeManagerDeciding;
			}
		}

		protected virtual void UnhookEvents()
		{
			if (RateLinesGrid != null && RateLinesListManager != null)
			{
				RateLinesListManager.CurrentChanged -= OnCurrentChanged;
				RateLinesListManager.PositionChanged -= OnCurrentChanged;
				RateLinesGrid.GridColourSchemeManagerDeciding -= RateLinesBoundGrid_GridColourSchemeManagerDeciding;
			}
		}

		protected virtual void OnCurrentChanged(object sender, EventArgs e)
		{
			if (RateLinesListManager.List != null)
			{
				ZGuid selectedChargeCode = ZGuid.Empty;
				CurrencyManager manager = (CurrencyManager)sender;
				if (manager.Position != -1)
				{
					selectedChargeCode = ((IRateLine)manager.GetCurrent()).TL_AC;
				}

				if (MasterEntry != null && MasterEntry.SelectedLineChargeCode != selectedChargeCode)
				{
					MasterEntry.SelectedLineChargeCode = selectedChargeCode;
				}
			}
		}

		#endregion

		#region BindTo

		[Browsable(true)]
		public virtual string BindTo
		{
			get { return fBindTo; }
			set
			{
				fBindTo = value;
				RateLinesGrid.SetBindingMember(fBindTo);
				calculatorPanel1.BindingMember = fBindTo + ".ViewCalculator";
			}
		}

		string fBindTo;

		void RateLinesBoundGrid_GridColourSchemeManagerDeciding(object sender, GridColourSchemeManagerDecidingEventArgs e)
		{
			e.GridColourSchemeManager = new RateLinesGridColorSchemaManager(RateLinesGrid);
		}

		#endregion

		#region RemoveAction

		public RemoveAction RemoveAction
		{
			get { return RateLinesGrid.RemoveAction; }
			set
			{
				RateLinesGrid.RemoveAction = value;
				calculatorPanel1.RemoveAction = value;
			}
		}

		#endregion

		#endregion

		#region Accept Related Line

		protected virtual void RateLinesGrid_DragEnter(object sender, DragEventArgs e)
		{
			if (!(this is CostingRateLineAndItemsControl))
			{
				if (e.Data.GetDataPresent(typeof(ArrayList)))
				{
					ArrayList list = (ArrayList)e.Data.GetData(typeof(ArrayList));
					foreach (BusinessObject @object in list)
					{
						if (@object is RelatedRateLine)
						{
							e.Effect = DragDropEffects.Copy;
							break;
						}
					}
				}
			}
		}

		protected virtual void RateLinesGrid_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ArrayList)))
			{
				if (MasterEntry != null)
				{
					var selectedLines = (IEnumerable)e.Data.GetData(typeof(ArrayList));

					foreach (RelatedRateLine line in selectedLines)
					{
						InsertRelatedRateLine(line, MasterEntry);
					}
				}
			}
		}

		protected IRateLine InsertRelatedRateLine(RelatedRateLine line, RateEntry entry)
		{
			var replace = false;

			if (entry.ContainsLineWithChargeCode(line.ChargeCode.AC_Code))
			{
				replace = Globals.Message.Show(Res.GetString("6e95d6e8-01ec-460f-a655-c3be820e3cd6", "Replace existing {0} charge?", line.ChargeCode.AC_Code), Res.GetString("8b9caf2f-14d4-4533-859d-173e729b4804", "Charge Code Exists"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
			}

			var insertedLine = entry.InsertRelatedRateLine(line, replace);

			if (insertedLine != null)
			{
				if ((line.IsTariff || line.IsCosting)
				&& insertedLine.Calculator is CompanyTariffOrCostBasedCalculator cstCalculator
				&& (insertedLine.TL_CompanyTariffLevel <= 1))
				{
					// the applytoline should only be set on non-overridden charges.
					// when it is overridden, the CompanyTariffOrCostLineCloneHelper will find the
					// level 1 charge
					cstCalculator.ApplyToLine = line.PK.ToString();
				}
			}

			return insertedLine;
		}

		internal protected IRateLinesWithParentEntry RateLinesCollection
			=> RateLinesListManager?.List as IRateLinesWithParentEntry;

		internal protected RelatedRateLinesCollection RelatedRateLines
			=> RateLinesListManager?.List as RelatedRateLinesCollection;

		protected RateEntry MasterEntry
			=> RateLinesCollection?.Master;

		internal string Category
		{
			get { return MasterEntry != null ? MasterEntry.TI_RateCategory : category; }
			set { category = value; }
		}
		ZString category;

		#endregion

		#region Additional Properties

		public bool CalculatorPanelAgentRatesCheckBoxVisible
		{
			get => calculatorPanel1.AgentRatesCheckBoxVisible;
			set => calculatorPanel1.AgentRatesCheckBoxVisible = value;
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				try
				{
					UnhookEvents();
				}
				catch (SqlException)
				{
					//don't interrupt dispose process because of errors like 'Invalid column name 'WW_DGThresholdPercentage'. (WI00370640)
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
