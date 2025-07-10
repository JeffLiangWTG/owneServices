using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using AsycudaContainer = Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer;

namespace Enterprise.Customs.NZ.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(AsycudaManifestHeader header)
		{
			var mpiQuarantineDeclarationGroup = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("199FE51D-C695-4E75-B652-13AD2DE78C1F", "MPI Quarantine Declaration");
			var stuffingLocationGroup = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("7086F5DA-21C9-4A52-B6EA-6BB94DAB81C1", "Stuffing Location");
			var deliveryDestinationGroup = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("9638DF2B-F880-4098-8463-F129BF0A98B6", "Delivery Destination");

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.SendMCDInformation,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.HasMPIQD,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.IsContainerClean,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(77),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.IsPackingContaminated,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(159),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.IsWoodPackingUsed,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.IsWoodPackingTreated,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.HasWoodPackingTreatmentCert,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(184),
				GroupName = mpiQuarantineDeclarationGroup
			};

			yield return new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.PackLocationOrgPK,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
				GroupName = stuffingLocationGroup
			};

			yield return new ZGuidDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = AsycudaContainer.Schema.ACN_OA_PackLocation,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146),
				GroupName = stuffingLocationGroup
			};

			yield return new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.DeliveryDestinationOrgPK,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				GroupName = deliveryDestinationGroup
			};

			yield return new ZGuidDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = AsycudaContainer.Schema.DeliveryDestination,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(162),
				GroupName = deliveryDestinationGroup
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var exportOrderNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			exportOrderNumberTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.CustomsEntryNumber;
			exportOrderNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return exportOrderNumberTextBoxColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(AsycudaManifestHeader header)
		{
			return new Dictionary<string, bool>()
			{
				{ AsycudaBill.Schema.CustomsEntryNumber, header.AMA_ManifestType == Business.NZManifestTypes.Codes.OCR }
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[]
					{
						AsycudaBill.Schema.ABL_UCRNumber,
						AsycudaBill.Schema.CustomsJobNumber,
						AsycudaBill.Schema.ABL_CustomsValue,
						AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
						AsycudaBill.Schema.DiscountValue,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_TransportValue,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_InsuranceValue,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.OtherChargesValue,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.ABL_PrepaidCollect,
						AsycudaBill.Schema.ABL_CargoStatus,
						AsycudaBill.Schema.ABL_CarrierReference,
						AsycudaBill.Schema.ABL_BolType
					}
				}
			};
		}

		protected override ContainerCountrySpecificUserControl GetContainerCountrySpecificUserControlCore() => new NZContainerSpecificUserControl();

		protected override IPanelLayoutProvider GetManifestLayoutCore()
		{
			return new NZManifestLayouts();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(AsycudaManifestHeader header)
		{
			yield return new NZManifestSpecificUserControl();
		}

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new NZBillLayouts();
		}
	}
}
