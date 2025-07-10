using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	public class CarrierContractsFilterGrid : ZDisplayGrid
	{
		public CarrierContractsFilterGrid()
		{
			InitializeColumns();
		}

		void InitializeColumns()
		{
			var quantityCNColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var quantityTEUColumnStyleInfo = new ZCalcEditColumnStyleInfo();

			var capacityWithVarianceCNColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var capacityWithVarianceTEUColumnStyleInfo = new ZCalcEditColumnStyleInfo();

			var outstandingCommittedCNColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var outstandingCommittedTEUColumnStyleInfo = new ZCalcEditColumnStyleInfo();

			var outstandingWithVarianceCNColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var outstandingWithVarianceTEUColumnStyleInfo = new ZCalcEditColumnStyleInfo();

			var utilisationCNColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			var utilisationTEUColumnStyleInfo = new ZCalcEditColumnStyleInfo();

			var allowHazardousCommoditiesColumnStyleInfo = new ZCheckBoxColumnStyleInfo();

			// quantity
			quantityCNColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("8ad2bda2-9a33-2e98-490b-5024db6d79fa", "Quantity (CN)");
			quantityCNColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			quantityCNColumnStyleInfo.IsReadOnly = true;
			quantityCNColumnStyleInfo.ColumnName = "CarrierContractQuantities.ContainerValue";
			quantityCNColumnStyleInfo.IsVisible = false;

			quantityTEUColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("069c606c-7c74-1081-46e8-6d983077db44", "Quantity (TEU)");
			quantityTEUColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			quantityTEUColumnStyleInfo.IsReadOnly = true;
			quantityTEUColumnStyleInfo.ColumnName = "CarrierContractQuantities.TEUValue";
			quantityTEUColumnStyleInfo.IsVisible = false;

			// capacity with variance
			capacityWithVarianceCNColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("ee885c92-14e5-ffa0-47d3-7ee30815fcc8", "Capacity with Variance (CN)");
			capacityWithVarianceCNColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			capacityWithVarianceCNColumnStyleInfo.IsReadOnly = true;
			capacityWithVarianceCNColumnStyleInfo.ColumnName = "CarrierContractCapacityWithVariance.ContainerValue";
			capacityWithVarianceCNColumnStyleInfo.IsVisible = false;

			capacityWithVarianceTEUColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("8798e390-75c9-ce92-436b-e00fa748779b", "Capacity with Variance (TEU)");
			capacityWithVarianceTEUColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			capacityWithVarianceTEUColumnStyleInfo.IsReadOnly = true;
			capacityWithVarianceTEUColumnStyleInfo.ColumnName = "CarrierContractCapacityWithVariance.TEUValue";
			capacityWithVarianceTEUColumnStyleInfo.IsVisible = false;

			// outstanding committed
			outstandingCommittedCNColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("06ce555c-fa29-278e-4b1b-67d1583a80d8", "Outstanding Committed (CN)");
			outstandingCommittedCNColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			outstandingCommittedCNColumnStyleInfo.IsReadOnly = true;
			outstandingCommittedCNColumnStyleInfo.ColumnName = "CurrentContractOutstandingCommitted.ContainerValue";
			outstandingCommittedCNColumnStyleInfo.IsVisible = false;

			outstandingCommittedTEUColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("26307db9-0125-7cb5-4b26-63fd80ec018f", "Outstanding Committed (TEU)");
			outstandingCommittedTEUColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			outstandingCommittedTEUColumnStyleInfo.IsReadOnly = true;
			outstandingCommittedTEUColumnStyleInfo.ColumnName = "CurrentContractOutstandingCommitted.TEUValue";
			outstandingCommittedTEUColumnStyleInfo.IsVisible = false;

			// outstanding with variance
			outstandingWithVarianceCNColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("a20be833-5c6b-f1b3-400c-6deefd6841e1", "Outstanding with Variance (CN)");
			outstandingWithVarianceCNColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			outstandingWithVarianceCNColumnStyleInfo.IsReadOnly = true;
			outstandingWithVarianceCNColumnStyleInfo.ColumnName = "CurrentContractOutstandingWithVariance.ContainerValue";
			outstandingWithVarianceCNColumnStyleInfo.IsVisible = false;

			outstandingWithVarianceTEUColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("d42ce8e7-a4e1-c28f-41e1-d95600a1afb7", "Outstanding with Variance (TEU)");
			outstandingWithVarianceTEUColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			outstandingWithVarianceTEUColumnStyleInfo.IsReadOnly = true;
			outstandingWithVarianceTEUColumnStyleInfo.ColumnName = "CurrentContractOutstandingWithVariance.TEUValue";
			outstandingWithVarianceTEUColumnStyleInfo.IsVisible = false;

			// utilisation
			utilisationCNColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("3814c421-b8ca-1c9c-4620-7a5ab4ab2960", "Utilization (CN)");
			utilisationCNColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			utilisationCNColumnStyleInfo.IsReadOnly = true;
			utilisationCNColumnStyleInfo.ColumnName = "CurrentContractUtilisation.ContainerValue";
			utilisationCNColumnStyleInfo.IsVisible = false;

			utilisationTEUColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("c5e81289-8392-baad-4adb-a0f456f4ad00", "Utilization (TEU)");
			utilisationTEUColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			utilisationTEUColumnStyleInfo.IsReadOnly = true;
			utilisationTEUColumnStyleInfo.ColumnName = "CurrentContractUtilisation.TEUValue";
			utilisationTEUColumnStyleInfo.IsVisible = false;

			// allow hazardous commodities
			allowHazardousCommoditiesColumnStyleInfo.CaptionResourceString = Enterprise.ContractManagement.GUI.Res.GetData("3a909126-9be8-dd99-4efd-2fcf10d13647", "Allow Hazardous Commodities");
			allowHazardousCommoditiesColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			allowHazardousCommoditiesColumnStyleInfo.IsReadOnly = true;
			allowHazardousCommoditiesColumnStyleInfo.ColumnName = "RCT_AllowHazardousCommodities";
			allowHazardousCommoditiesColumnStyleInfo.IsVisible = false;

			ColumnStyles.Add(quantityCNColumnStyleInfo);
			ColumnStyles.Add(quantityTEUColumnStyleInfo);
			ColumnStyles.Add(capacityWithVarianceCNColumnStyleInfo);
			ColumnStyles.Add(capacityWithVarianceTEUColumnStyleInfo);
			ColumnStyles.Add(outstandingCommittedCNColumnStyleInfo);
			ColumnStyles.Add(outstandingCommittedTEUColumnStyleInfo);
			ColumnStyles.Add(outstandingWithVarianceCNColumnStyleInfo);
			ColumnStyles.Add(outstandingWithVarianceTEUColumnStyleInfo);
			ColumnStyles.Add(utilisationCNColumnStyleInfo);
			ColumnStyles.Add(utilisationTEUColumnStyleInfo);
			ColumnStyles.Add(allowHazardousCommoditiesColumnStyleInfo);
		}
	}
}
