using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageContainerDataObjectReader : BaseContainerDataObjectReader<PkgPackage>
	{
		public PkgPackageContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, PkgPackageCollection packageCollection, PkgPackage targetContainer = null)
			: base(containerDataObject, logger, factory)
		{
			this.PackageCollection = packageCollection;
			this.TargetContainer = targetContainer;
		}

		readonly PkgPackageCollection PackageCollection;
		readonly PkgPackage TargetContainer;

		#region GetExistingBusinessObject

		protected override PkgPackage GetExistingBusinessObject()
		{
			return TargetContainer;
		}

		#endregion

		#region GetNewBusinessObject

		protected override PkgPackage GetNewBusinessObject()
		{
			return PackageCollection.AddNew();
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(PkgPackage packageBO)
		{
			Argument.NotNull(packageBO, "package");
			((ISupportDataImporting)packageBO).IsImportingData = true;

			PopulatePackageTypeAndQuantity(packageBO);

			if (packageBO.IsContainer)
			{
				PopulateContainerProperties(packageBO);
			}

			PopulatePackageProperties(packageBO);
			PopulateUNDG(packageBO);
			ThrowImportFailureExceptionIfContainerTypeIsEmpty(packageBO);
		}

		#endregion

		#region PopulatePackageTypeAndQuantity

		void PopulatePackageTypeAndQuantity(PkgPackage packageBO)
		{
			SetValue(packageBO, PkgPackageSchema.KP_F3_NKPackType, GetPackageType(dataObject.FCL_LCL_AIR));
			SetValue(packageBO, PkgPackageSchema.KP_PackageQty, dataObject.ContainerCount);
		}

		#endregion

		#region PopulateContainerProperties

		void PopulateContainerProperties(PkgPackage packageBO)
		{
			var pkgContainerBO = packageBO.Container;
			Argument.NotNull(pkgContainerBO, "pkgContainerBO");

			// type / mode
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_RC_ContainerType, dataObject.ContainerType);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_ContainerMode, dataObject.FCL_LCL_AIR);

			// weight
			PopulateContainerTareWeight(packageBO);
			PopulateContainerDunnageWeight(packageBO);

			// seals & modes
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Seal1, dataObject.Seal);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Seal2, dataObject.SecondSeal);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Seal3, dataObject.ThirdSeal);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_IsSealOk, dataObject.IsSealOk);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Seal1PartyType, dataObject.SealPartyType);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Seal2PartyType, dataObject.SecondSealPartyType);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Seal3PartyType, dataObject.ThirdSealPartyType);

			// refrigeration
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_AirVentFlowRate, dataObject.AirVentFlow);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_AirVentFlowRateUnit, dataObject.AirVentFlowRateUnit);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_HumidityPercent, dataObject.HumidityPercent);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_IsControlledAtmosphere, dataObject.IsControlledAtmosphere);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_RefrigGeneratorID, dataObject.RefrigGeneratorID);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_SetPointTemp, dataObject.SetPointTemp);

			var tempUnit = dataObject.IsControlledAtmosphere.HasValue && dataObject.IsControlledAtmosphere.Value ? dataObject.SetPointTempUnit : ZString.Empty;
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_SetPointTempUnit, tempUnit);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_TempRecorderSerialNumber, dataObject.TempRecorderSerialNo);

			// other
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_IsDamaged, dataObject.IsDamaged);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_IsEmpty, dataObject.IsEmptyContainer);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_IsShipperOwned, dataObject.IsShipperOwned);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Quality, dataObject.ContainerQuality);
			SetValue(pkgContainerBO, PkgPackageContainerSchema.K0_Status, dataObject.ContainerStatus);

			CreatePackageExtension(pkgContainerBO);
		}

		void CreatePackageExtension(PkgPackageContainer pkgContainerBO)
		{
			if (pkgContainerBO.Package.PackageJob.ParentJob is IPackingParentSupportsPackageExtensions packageExtensionSupporter)
			{
				packageExtensionSupporter.CreateOrUpdatePackageExtension(pkgContainerBO.Package, logger);
			}
		}

		#endregion

		#region PopulateContainerTareWeight

		void PopulateContainerTareWeight(PkgPackage packageBO)
		{
			if (dataObject.TareWeight > 0)
			{
				SetValue(packageBO, PkgPackageSchema.KP_TareWeight, dataObject.TareWeight);
			}
			else if (dataObject.GrossWeight - dataObject.GoodsWeight < 0)
			{
				SetValue(packageBO, PkgPackageSchema.KP_TareWeight, 0);
			}
			else
			{
				SetValue(packageBO, PkgPackageSchema.KP_TareWeight, dataObject.GrossWeight - dataObject.GoodsWeight);
			}
		}

		#endregion

		void PopulateContainerDunnageWeight(PkgPackage packageBO)
		{
			if (dataObject.DunnageWeight < 0)
			{
				SetValue(packageBO, PkgPackageSchema.KP_DunnageWeight, 0);
			}
			else
			{
				SetValue(packageBO, PkgPackageSchema.KP_DunnageWeight, dataObject.DunnageWeight);
			}
		}

		#region PopulatePackageProperties

		void PopulatePackageProperties(PkgPackage packageBO)
		{
			// dimensions
			SetValue(packageBO, PkgPackageSchema.KP_Length, dataObject.TotalLength);
			SetValue(packageBO, PkgPackageSchema.KP_Height, dataObject.TotalHeight);
			SetValue(packageBO, PkgPackageSchema.KP_Width, dataObject.TotalWidth);
			SetValue(packageBO, PkgPackageSchema.KP_DimensionUQ, dataObject.LengthUnit);

			// weight
			SetValue(packageBO, PkgPackageSchema.KP_Weight, dataObject.GrossWeight);
			SetValue(packageBO, PkgPackageSchema.KP_WeightUQ, dataObject.WeightUnit);

			// volume
			var volume = dataObject.VolumeCapacity.GetValueOrDefault();
			SetValue(packageBO, PkgPackageSchema.KP_Volume, volume);
			if (volume >= 0m)
			{
				SetValue(packageBO, PkgPackageSchema.KP_VolumeUQ, dataObject.VolumeUnit);
			}
			// other

			// only create package id Row if Container ID is not empty
			PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(factory, logger, packageBO, dataObject.ContainerCount, dataObject.ContainerNumber);

			SetValue(packageBO, PkgPackageSchema.KP_TransportRef, dataObject.TransportReference);
			SetValue(packageBO, PkgPackageSchema.KP_GoodsDescription, dataObject.GoodsDescription);
			SetValue(packageBO, PkgPackageSchema.KP_HSCode, dataObject.HarmonisedCode);
			SetValue(packageBO, PkgPackageSchema.KP_RH_NKCommodityCode, dataObject.Commodity);

			SetValue(packageBO, PkgPackageSchema.KP_IsCheckedWeighedCubed, dataObject.IsCheckedWeighedCubed);
			SetValue(packageBO, PkgPackageSchema.KP_IsPillaged, dataObject.Pillaged);
			SetValue(packageBO, PkgPackageSchema.KP_IsFumigated, dataObject.Fumigated);
			SetValue(packageBO, PkgPackageSchema.KP_IsHeatTreated, dataObject.HeatTreated);
			SetValue(packageBO, PkgPackageSchema.KP_RequiresTemperatureControl, dataObject.RequiresTemperatureControl);
			SetValue(packageBO, PkgPackageSchema.KP_RequiredTemperatureMinimum, dataObject.RequiredTemperatureMinimum);
			SetValue(packageBO, PkgPackageSchema.KP_RequiredTemperatureMaximum, dataObject.RequiredTemperatureMaximum);
			SetValue(packageBO, PkgPackageSchema.KP_RequiredTemperatureUnit, dataObject.RequiredTemperatureUnit);
			SetValue(packageBO, PkgPackageSchema.KP_MarksAndNumbers, dataObject.MarksAndNos);

			PopulatePackageBookedDimensionsIfSupported(packageBO);

			PkgPackageDataObjectReader.ThrowImportFailuresOnInvalidData(packageBO);
		}

		void PopulatePackageBookedDimensionsIfSupported(PkgPackage packageBO)
		{
			if (packageBO.PackageJob?.ParentJob as IPackingParentSupportsImportingBookedDimensions != null)
			{
				// dimensions
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Length, dataObject.TotalLength);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Height, dataObject.TotalHeight);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Width, dataObject.TotalWidth);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_DimensionUQ, dataObject.LengthUnit);

				// weights
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Weight, dataObject.GrossWeight);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_WeightUQ, dataObject.WeightUnit);

				// volume
				var volume = dataObject.VolumeCapacity.GetValueOrDefault();
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Volume, volume);
				if (volume >= 0m)
				{
					SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_VolumeUQ, dataObject.VolumeUnit);
				}
			}
		}

		#endregion

		#region PopulateUNDG

		void PopulateUNDG(PkgPackage packageBO)
		{
			if (dataObject.UNDGCollection != null)
			{
				packageBO.UNDGs.DeleteAll();
				foreach (var source in dataObject.UNDGCollection)
				{
					var reader = new UNDGDataObjectReader(source, logger, factory);
					var undg = reader.ReadIntoBusinessObject();
					if (undg != null)
					{
						packageBO.UNDGs.Add(undg);
					}
				}
			}
		}

		#endregion

		#region GetPackageType

		ZString GetPackageType(ContainerMode containerMode)
		{
			switch (containerMode.GetCodeAsUpperCase())
			{
				case Constants.ContainerModes.Bulk:
				case Constants.ContainerModes.Liquid:
				case Constants.ContainerModes.RollOnRollOff:
					return Constants.PkgUnit.Unit;
				case Constants.ContainerModes.BreakBulk:
					return Constants.PkgUnit.BreakBulk;
				default:
					return Constants.PkgUnit.Container;
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfContainerTypeIsEmpty

		void ThrowImportFailureExceptionIfContainerTypeIsEmpty(PkgPackage packageBO)
		{
			var container = packageBO.Container;
			if (container != null && container.K0_RC_ContainerType.IsEmpty)
			{
				throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "Cannot import Container '{0} - {1}' without a Container Type.", packageBO.PackageIDWithFallback, dataObject.FCL_LCL_AIR.GetCodeAsUpperCase()));
			}
		}

		#endregion
	}
}
