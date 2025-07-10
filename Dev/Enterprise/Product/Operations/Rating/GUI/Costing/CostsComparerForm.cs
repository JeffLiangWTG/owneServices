using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public partial class CostsComparerForm : ZChildForm
	{
		public CostsComparerForm()
			: this(new CostsComparer())
		{
		}

		public CostsComparerForm(CostsComparer costsComparer)
			: base(costsComparer)
		{
			InitializeComponent();
			SetButtons();
			Comparer.ResetSummaryColumns += Comparer_ResetSummaryColumns;
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		protected override void AddAdornments()
		{
			base.AddAdornments();
			ZFormMenuStrategy.AddAdornments(this);
			ZFormMenuStrategy.AddValidateMenuItem(this);
		}

		internal CostsComparer Comparer
		{
			get { return (CostsComparer)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		#region Navigation

		void MainTabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (MainTabControl.TabPages[MainTabControl.SelectedIndex] == CostsTabPage)
			{
				if (ValidateComparer())
				{
					Comparer.LoadCosts();

					int maxRowsToShow = RatingDataRegistry.Instance.CostComparisonLinesNumber.Value;

					if (Comparer.Costs.Count >= maxRowsToShow)
					{
						Globals.Message.Show(
							Res.GetString("7eb92ca1-e9b6-4f0b-b066-20574e8cc5d3", "Too many records to display. Only first {0} will be shown.", maxRowsToShow),
							Res.GetString("ad51cd79-aefa-4cf2-9a4f-abf83435fd8f", "Results"),
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
			}

			SetButtons();
		}

		void PreviousButton_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedIndex > 0)
			{
				MainTabControl.SelectedIndex--;
			}
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedIndex < MainTabControl.TabCount - 1)
			{
				MainTabControl.SelectedIndex++;
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SetButtons()
		{
			PreviousButton.Visible = MainTabControl.SelectedIndex != 0;
			NextButton.Visible = MainTabControl.SelectedIndex != 1;

			switch (MainTabControl.SelectedIndex)
			{
				case 0:
					PreviousButton.Text = Res.GetString("df15dab0-3c51-4a60-bc35-da9091e316bc", "Previous");
					NextButton.Text = Res.GetString("9a6abd56-25f2-44e4-925f-59239567d6f4", "Compare Costs");
					break;

				case 1:
					PreviousButton.Text = Res.GetString("3953b813-61cd-4787-8584-5126f86be5c7", "Filter");
					NextButton.Text = Res.GetString("43744398-8685-42e5-a4f4-06b88b453878", "Next");
					break;

				default:
					PreviousButton.Text = Res.GetString("df15dab0-3c51-4a60-bc35-da9091e316bc", "Previous");
					NextButton.Text = Res.GetString("43744398-8685-42e5-a4f4-06b88b453878", "Next");
					break;
			}
		}

		void CostsGridAfterBind(object sender, EventArgs eventArgs)
		{
			CostsGrid.ListManager.CurrentChanged += CostsGridOnCurrentChanged;
			CostsGridOnCurrentChanged(sender, eventArgs);
			SecurityMessageLabel.BindTo = CostsGrid.BindTo + (NoResString)".Entry.SecurityMessage";
		}

		void CostsGridOnCurrentChanged(object sender, EventArgs eventArgs)
		{
			if (CostsGrid.CurrentRowIndex >= 0)
			{
				var rateEntry = ((CostsComparerEntry)CostsGrid.List[CostsGrid.CurrentRowIndex]).Entry;

				if (rateEntry.IsInDatabase && rateEntry.RateLinesAccessDenied)
				{
					SecurityMessageLabel.Visible = true;
					RateLinesAndItemsControl.Visible = false;
				}
				else
				{
					SecurityMessageLabel.Visible = false;
					RateLinesAndItemsControl.Visible = true;
				}
			}
		}

		#endregion

		#region Validation

		bool ValidateComparer()
		{
			bool validationFailed = false;

			Comparer.RunPreSaveValidation();
			if (Comparer.HasErrors)
			{
				validationFailed = true;
				ShowErrorsDialog();
			}

			return !validationFailed;
		}

		#endregion

		#region ResetSummaryColumns

		internal const string SummaryColumnCaption = "SummaryColumn";

		void Comparer_ResetSummaryColumns(SortedList<ChargesSummaryItem, ChargesSummaryItem> summaryItems)
		{
			CostsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				for (int i = 0; i < CostsGrid.ColumnStyles.Count; i++)
				{
					if (((ZTextBoxColumnStyleInfo)CostsGrid.ColumnStyles[i]).ColumnName.StartsWith(SummaryColumnCaption))
					{
						CostsGrid.ColumnStyles.RemoveAt(i);
						i--;
					}
				}

				for (int i = 0; i < summaryItems.Count; i++)
				{
					var style = new ZTextBoxColumnStyleInfo();
					style.ColumnName = string.Format("{0}{1}", SummaryColumnCaption, i + 1);
					style.Caption = summaryItems.Keys[i].ColumnCaption;
					ControlDpiScalingHelper.SetWidth(ref style, 50, true);
					Type columnType = (summaryItems.Keys[i].FlatAmount.IsEmpty || summaryItems.Keys[i].Value.IsEmpty)
						? typeof(ZDecimal)
						: typeof(ZString);

					((IOverridablePropertyDescriptor)style).PropertyDescriptor = new CostsComparePropertyDescriptor(style.ColumnName, columnType);
					CostsGrid.ColumnStyles.Add(style);
				}

				this.BindingSource.SetBindingMember(this.CostsGrid, "");
				this.BindingSource.SetBindingMember(this.CostsGrid, "Costs");
			});
		}

		#endregion

		#region Dispose

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
