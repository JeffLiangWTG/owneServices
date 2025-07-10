using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Rating.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class CalculatorPanel : ZUserControl
	{
		public CalculatorPanel()
		{
			InitializeComponent();

			CalculatorDropEdit.TextChanged += new EventHandler(CalculatorDropEdit_CodeBoxTextChanged);
			CalculatorDropEdit.Enter += new EventHandler(CalculatorDropEdit_Enter);
			CalculatorDropEdit.Leave += new EventHandler(CalculatorDropEdit_Leave);
		}

		#region Form Basher Test Fix

		void CalculatorDropEdit_Enter(object sender, EventArgs e)
		{
			enteredCalculatorDropEdit = true;
		}

		void CalculatorDropEdit_Leave(object sender, EventArgs e)
		{
			enteredCalculatorDropEdit = false;
			if (changedCalculatorDropEdit)
			{
				Binding binding = DataBindings[nameof(ViewCalculatorForBinding)];
				if (binding != null)
				{
					binding.ReadValue();
				}
				changedCalculatorDropEdit = false;
			}
		}

		void CalculatorDropEdit_CodeBoxTextChanged(object sender, EventArgs e)
		{
			if (enteredCalculatorDropEdit)
			{
				Calculator calc = ViewCalculatorForBinding.Calculator;
				if (calc != null && CalculatorDropEdit.Text != calc.RateLineItems.Parent.TL_RateCalculator)
				{
					changedCalculatorDropEdit = true;
				}
			}
		}

		bool enteredCalculatorDropEdit;
		private ZCheckBox AgentRatesCheckBox;
		bool changedCalculatorDropEdit;

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SetDataSourceBinding("ViewCalculatorForBinding", BindingMember.Replace("ViewCalculator", "ViewCalculatorForBinding"));
				SetDataSourceBinding("ViewResultsVisible", BindingMember.Replace("ViewCalculator", "ViewResultsVisible"));
				SetDataSourceBinding("IsBreakWeightVolumeAvailable", BindingMember.Replace("ViewCalculator", "IsBreakWeightVolumeAvailable"));
			}
		}

		[Browsable(true)]
		public string BindingMember
		{
			get { return bindingMember; }
			set
			{
				bindingMember = value;
				if (bindingMember != null)
				{
					// we bind directly, bypassing the mapper
					CalculatorDropEdit.BindTo = BindingMember.Replace("ViewCalculator", "TL_RateCalculator");
					ResultsCheckBox.BindTo = BindingMember.Replace("ViewCalculator", "ViewResults");
					AgentRatesCheckBox.BindTo = BindingMember.Replace("ViewCalculator", "ViewAgentRates");

					foreach (Control ctrl in Controls)
					{
						if (ctrl is RateCalculatorUserControl rateCalculatorUserControl)
						{
							rateCalculatorUserControl.BindTo = bindingMember;
						}
					}
				}
			}
		}
		string bindingMember;

		public RateLine.CalculatorWrapper ViewCalculatorForBinding
		{
			get { return viewCalculatorForBinding; }
			set
			{
				bool changed = viewCalculatorForBinding.Calculator != value.Calculator;
				viewCalculatorForBinding = value;
				if (changed)
				{
					RateCalculatorUserControl newControl = GetActiveControl(value.IsRelatedRateLine);
					if (newControl != null)
					{
						newControl.OnSwitched(ViewCalculatorForBinding.Calculator);
						newControl.BeginInvoke(new MethodInvoker(newControl.ShowAndBringToFront));
					}
					HideAllControls(newControl);
				}
			}
		}

		RateLine.CalculatorWrapper viewCalculatorForBinding;

		RateCalculatorUserControl GetActiveControl(bool isRelatedRateLine = false)
		{
			if (ViewCalculatorForBinding.Calculator != null)
			{
				Type calcControlType = new ControlToCalculatorMap().GetControlType(ViewCalculatorForBinding.Calculator.GetType());
				if (calcControlType != null)
				{
					return GetCalculatorControl(calcControlType, isRelatedRateLine);
				}
			}

			return null;
		}

		public bool ViewResultsVisible
		{
			get { return fViewResultsVisible; }
			set
			{
				fViewResultsVisible = value;

				if (ResultsCheckBox.Visible != ViewResultsVisible)
				{
					ResultsCheckBox.Visible = ViewResultsVisible;
				}
			}
		}

		bool fViewResultsVisible;

		public bool IsBreakWeightVolumeAvailable
		{
			get { return fIsBreakWeightVolumeAvailable.GetValueOrDefault(true); }
			set
			{
				bool changed = !fIsBreakWeightVolumeAvailable.HasValue || fIsBreakWeightVolumeAvailable != value;
				fIsBreakWeightVolumeAvailable = value;

				if (changed)
				{
					RateCalculatorUserControl calcCtrl = GetActiveControl(ViewCalculatorForBinding.IsRelatedRateLine);
					if (calcCtrl != null)
					{
						ZGrid itemsGrid = calcCtrl.GetRateLineItemsGrid();
						if (itemsGrid != null)
						{
							for (int i = 0; i < itemsGrid.Columns.Count; i++)
							{
								if (itemsGrid.Columns[i].ColumnStyle.MappingName == RateLineItemsSchema.TM_BreakWeightVolume.Name)
								{
									if (itemsGrid.Columns[i].IsVisible != IsBreakWeightVolumeAvailable)
									{
										itemsGrid.Columns[i].IsVisible = IsBreakWeightVolumeAvailable;
										itemsGrid.RefreshTableStyles();
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		bool? fIsBreakWeightVolumeAvailable;

		#endregion

		#region Calculator Controls

		internal RateCalculatorUserControl GetCalculatorControl(Type calculatorControlType, bool isRelatedRateLine = false)
		{
			if (CalculatorControls == null)
			{
				lock (CalculatorControlsLock)
				{
					if (CalculatorControls == null)
					{
						CalculatorControls = new Dictionary<string, RateCalculatorUserControl>();
					}
				}
			}

			RateCalculatorUserControl calculatorControl;

			var calculatorControlKey = isRelatedRateLine
				? $"{calculatorControlType.Name}-{nameof(RelatedRateLine)}" // Hard-coded key constant
				: calculatorControlType.Name;
			if (!CalculatorControls.TryGetValue(calculatorControlKey, out calculatorControl))
			{
				lock (CalculatorControlsLock)
				{
					if (!CalculatorControls.TryGetValue(calculatorControlKey, out calculatorControl))
					{
						calculatorControl = (RateCalculatorUserControl)Activator.CreateInstance(calculatorControlType);
						calculatorControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
						calculatorControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24);
						calculatorControl.Name = calculatorControlKey;
						calculatorControl.RemoveAction = RemoveAction;
						calculatorControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(
							CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(Width),
							CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height) -
							CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(calculatorControl.Top));
						Controls.Add(calculatorControl);
						calculatorControl.ViewCalculatorForBinding = ViewCalculatorForBinding;
						calculatorControl.BindTo = BindingMember;
						if (CurrentDataItem != null)
						{
							calculatorControl.SetDataBinding(CurrentDataItem, "");
						}
						CalculatorControls.Add(calculatorControlKey, calculatorControl);
					}
				}
			}

			return calculatorControl;
		}

		Dictionary<string, RateCalculatorUserControl> CalculatorControls;
		readonly object CalculatorControlsLock = new object();

		void HideAllControls(RateCalculatorUserControl newControl)
		{
			if (CalculatorControls != null)
			{
				foreach (RateCalculatorUserControl ctrl in CalculatorControls.Values)
				{
					if (ctrl != newControl)
					{
						ctrl.BeginInvoke(new MethodInvoker(ctrl.Hide));
					}
				}
			}
		}

		#endregion

		#region Remove Action

		public RemoveAction RemoveAction
		{
			get { return fRemoveAction; }
			set
			{
				fRemoveAction = value;
				foreach (Control ctrl in Controls)
				{
					if (ctrl is RateCalculatorUserControl rateCalculatorUserControl)
					{
						rateCalculatorUserControl.RemoveAction = fRemoveAction;
					}
				}
			}
		}

		RemoveAction fRemoveAction = RemoveAction.RemoveAndDelete;

		#endregion

		#region Cost and Company Tariff Based Calculator

		void ResultsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!IsNotificationsEmpty(ResultsCheckBox))
			{
				CalculatorDropEdit.Focus();
			}
		}

		void ResultsCheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (ResultsCheckBox.ReadOnly)
			{
				ResultsCheckBox.ReadOnly = false;
			}
		}

		static bool IsNotificationsEmpty(IExtendedControl control)
		{
			INotificationExtension extension = control.Extensions.Get<INotificationExtension>();

			if (extension == null)
			{
				return true;
			}

			return !extension.Notifications.Any();
		}

		#endregion

		#region Visible Properties

		public bool CalculatorDropEditVisible
		{
			get { return CalculatorDropEdit.Visible; }
			set { CalculatorDropEdit.Visible = value; }
		}

		public bool AgentRatesCheckBoxVisible
		{
			get { return AgentRatesCheckBox.Visible; }
			set { AgentRatesCheckBox.Visible = value; }
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

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<CalculatorPanel>()
.Property("IsBreakWeightVolumeAvailable", true, false)
.Property("ViewCalculatorForBinding", new RateLine.CalculatorWrapper(), false)
.Property("ViewResultsVisible", false, false) //Whether the Result Check Box is hidden or not depends on the value of ViewResultsVisible in RateLine.
.Result;
		}

		#endregion

#if DEBUG

		internal void CreateHandle_ForTest()
			=> CreateHandle();

#endif

		#region Component Designer generated code

		private ZDropEdit CalculatorDropEdit;
		private ZCheckBox ResultsCheckBox;
		private readonly Container components;

		private void InitializeComponent()
		{
			this.CalculatorDropEdit = new ZDropEdit();
			this.ResultsCheckBox = new ZCheckBox();
			this.AgentRatesCheckBox = new ZCheckBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CalculatorDropEdit
			// 
			this.CalculatorDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.CalculatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 0, true);
			this.CalculatorDropEdit.Name = "CalculatorDropEdit";
			this.CalculatorDropEdit.PreBoundMaxLength = 3;
			this.CalculatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CalculatorDropEdit.TabIndex = 31;
			// 
			// ResultsCheckBox
			// 
			this.ResultsCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.ResultsCheckBox.AutoSize = true;
			this.ResultsCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CalculatorPanel|60b710a4-f569-4af5-b088-18c5217cd4ec", "Results");
			this.ResultsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ResultsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 2, true);
			this.ResultsCheckBox.Name = "ResultsCheckBox";
			this.ResultsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ResultsCheckBox.TabIndex = 33;
			this.ResultsCheckBox.Visible = false;
			this.ResultsCheckBox.ReadOnlyChanged += new EventHandler(this.ResultsCheckBox_ReadOnlyChanged);
			this.ResultsCheckBox.CheckedChanged += new EventHandler(this.ResultsCheckBox_CheckedChanged);
			// 
			// AgentRatesCheckBox
			// 
			this.AgentRatesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.AgentRatesCheckBox.AutoSize = true;
			this.AgentRatesCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CalculatorPanel|d7368890-7678-4939-82ad-648e609fabcc", "Ag. Rates", "Agent Rates", "");
			this.AgentRatesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AgentRatesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 2, true);
			this.AgentRatesCheckBox.Name = "AgentRatesCheckBox";
			this.AgentRatesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AgentRatesCheckBox.TabIndex = 32;
			this.AgentRatesCheckBox.UseVisualStyleBackColor = true;
			// 
			// CalculatorPanel
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AgentRatesCheckBox);
			this.Controls.Add(this.ResultsCheckBox);
			this.Controls.Add(this.CalculatorDropEdit);
			this.Name = "CalculatorPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 160, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
