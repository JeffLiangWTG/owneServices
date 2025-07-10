using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class DrawbackInvoiceLineClaimedAmountsUserControl : ZUserControl
	{
		public DrawbackInvoiceLineClaimedAmountsUserControl()
		{
			InitializeComponent();
			UsedManufQuantityCalcDropEdit.SetReadOnly(true);
		}

		internal void ShowOrHideACERelatedUserControls(ZBool isACEDrawback)
		{
			ACSClaimedPanel.Visible = !isACEDrawback;
			ACEClaimedPanel.Visible = isACEDrawback;
		}

		internal void ShowOrHide7552RelatedUserControls(ZBool isACEDrawback, ZBool is7552)
		{
			if (isACEDrawback)
			{
				_99ACEClaimedLabel.Visible = !is7552;
				_99ACEClaimedHMFCalcEdit.Visible = !is7552;
				_99ACEClaimedMPFCalcEdit.Visible = !is7552;
				_99ACEClaimedTaxCalcEdit.Visible = !is7552;
				_99ACEClaimedDutyCalcEdit.Visible = !is7552;
				ACEClaimedHMFCalcEdit.Visible = is7552;
				ACEClaimedMPFCalcEdit.Visible = is7552;
				ACEMPFEligibleLabel.Visible = !is7552;
				ACEHMFEligibleLabel.Visible = !is7552;

				if (is7552)
				{
					ACEOtherFeesGrid.RemoveFromAvailableColumns(DrawbackOtherFee.Schema._99ClaimedAmount);
				}
				else
				{
					ACEOtherFeesGrid.AddToAvailableColumns(DrawbackOtherFee.Schema._99ClaimedAmount);
				}

				ACEOtherFeesGrid.ReOrderColumns(ACEDrawbackOtherFeeColumnNamesInSortOrder);
			}
			else
			{
				_99ClaimedLabel.Visible = !is7552;
				_99ClaimedHMFCalcEdit.Visible = !is7552;
				_99ClaimedMPFCalcEdit.Visible = !is7552;
				_99ClaimedTaxCalcEdit.Visible = !is7552;
				_99ClaimedOtherFeesCaclEdit.Visible = !is7552;
				_99ClaimedDutyCalcEdit.Visible = !is7552;
				ClaimedHMFCalcEdit.Visible = is7552;
				ClaimedMPFCalcEdit.Visible = is7552;
				MPFEligibleLabel.Visible = !is7552;
				HMFEligibleLabel.Visible = !is7552;
			}
		}

		internal void ShowOrHideManufacturerRelatedUserControls(ZBool isForManufSection)
		{
			UsedManufQuantityCalcDropEdit.Visible = isForManufSection;
		}

		string[] ACEDrawbackOtherFeeColumnNamesInSortOrder
		{
			get
			{
				if (fACEDrawbackOtherFeeColumnNamesInSortOrder == null)
				{
					var columnNamesInSortOrderList = new List<string>();
					columnNamesInSortOrderList.AddRange(new[]
					{
						DrawbackOtherFee.Schema.US_FeeType,
						DrawbackOtherFee.Schema.FeeDescription,
						DrawbackOtherFee.Schema.DeclaredAmount,
						DrawbackOtherFee.Schema.FeeAmountPerUnit,
						DrawbackOtherFee.Schema.ClaimedAmount,
						DrawbackOtherFee.Schema._99ClaimedAmount,
						DrawbackOtherFee.Schema.CalculatedAmount
					});

					fACEDrawbackOtherFeeColumnNamesInSortOrder = columnNamesInSortOrderList.ToArray();
				}

				return fACEDrawbackOtherFeeColumnNamesInSortOrder;
			}
		}
		string[] fACEDrawbackOtherFeeColumnNamesInSortOrder;
	}
}
