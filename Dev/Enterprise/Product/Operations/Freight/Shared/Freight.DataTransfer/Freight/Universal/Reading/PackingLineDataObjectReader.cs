using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class PackingLineDataObjectReader<T, U> : DataObjectReader<PackingLine, T>
		where T : PackLine
		where U : CommonShipment
	{
		public PackingLineDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, U parentShipment, Func<PackingLine, T> packLineBizObjFinder = null, Func<T> packLineBizObjCreator = null)
			: base(packingLineDataObject, logger, factory)
		{
			this.parentShipment = Argument.NotNull(parentShipment, "parentShipment");
			this.packLineBizObjFinder = packLineBizObjFinder;
			this.packLineBizObjCreator = packLineBizObjCreator;
		}

		public PackingLineDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, BusinessObjectFactory factory, U parentShipment, Func<PackingLine, T> packLineBizObjFinder = null)
			: this(packingLineDataObject, logger, new UniversalObjectFactory(), parentShipment, packLineBizObjFinder)
		{
			factoryOverride = factory;
			packLineBizObjCreator = () => factoryOverride.New<T>();
		}

		protected readonly BusinessObjectFactory factoryOverride;
		protected readonly U parentShipment;
		readonly Func<PackingLine, T> packLineBizObjFinder;
		readonly Func<T> packLineBizObjCreator;

		#region Implementation

		protected override T GetExistingBusinessObject()
		{
			return packLineBizObjFinder != null ? packLineBizObjFinder(dataObject) : null;
		}

		protected override T GetNewBusinessObject()
		{
			T result = null;

			if (packLineBizObjCreator != null)
			{
				result = packLineBizObjCreator();
			}

			return result ?? base.GetNewBusinessObject();
		}

		protected override void PopulateBusinessObject(T packingLineBO)
		{
			ISupportDataImporting supportDataImporting = null;
			if (packingLineBO is ISupportDataImporting)
			{
				supportDataImporting.IsImportingData = true;
			}

			try
			{
				if (!IsUNDGMatchedForExistingPackingLineBO(packingLineBO, dataObject))
				{
					packingLineBO.UNDGs.DeleteAll();
				}

				packingLineBO.JL_FreightMode = Enterprise.Freight.Business.FreightConstants.OuterPackType;
				packingLineBO.JL_JS = parentShipment.PK;

				SetValue(packingLineBO, JobPackLinesSchema.JL_RH_NKCommodityCode, dataObject.Commodity);
				SetValue(packingLineBO, JobPackLinesSchema.JL_ContainerPackingOrder, dataObject.ContainerPackingOrder);
				SetValue(packingLineBO, JobPackLinesSchema.JL_HarmonisedCode, dataObject.HarmonisedCode);
				SetValue(packingLineBO, JobPackLinesSchema.JL_ItemNo, dataObject.ItemNo);
				SetValue(packingLineBO, JobPackLinesSchema.JL_MarksAndNumbers, dataObject.MarksAndNos);
				SetValue(packingLineBO, JobPackLinesSchema.JL_RN_NKOrigin, dataObject.CountryOfOrigin);
				SetValue(packingLineBO, JobPackLinesSchema.JL_Outturn, dataObject.OutturnQty);
				SetValue(packingLineBO, JobPackLinesSchema.JL_Damaged, dataObject.OutturnDamagedQty);
				SetValue(packingLineBO, JobPackLinesSchema.JL_Pillaged, dataObject.OutturnPillagedQty);
				SetValue(packingLineBO, JobPackLinesSchema.JL_OutturnComment, dataObject.OutturnComment);
				SetValue(packingLineBO, JobPackLinesSchema.JL_F3_NKPackType, dataObject.PackType);
				SetValue(packingLineBO, JobPackLinesSchema.JL_RefNumber, dataObject.ReferenceNumber);
				SetValue(packingLineBO, JobPackLinesSchema.JL_ExportRefNumber, dataObject.ExportReferenceNumber);
				SetValue(packingLineBO, JobPackLinesSchema.JL_ImportRefNumber, dataObject.ImportReferenceNumber);
				SetValue(packingLineBO, JobPackLinesSchema.JL_Description, dataObject.GetCleanSingleLineGoodsDescription());
				SetValue(packingLineBO, JobPackLinesSchema.JL_LoadingMeters, dataObject.LoadingMeters);
				SetValue(packingLineBO, JobPackLinesSchema.JL_EndItemNo, dataObject.EndItemNo);
				SetValue(packingLineBO, JobPackLinesSchema.JL_LinePrice, dataObject.LinePrice);
				SetValue(packingLineBO, JobPackLinesSchema.JL_DetailedDescription, dataObject.DetailedDescription);
				SetValue(packingLineBO, JobPackLinesSchema.JL_LastKnownTransitWarehouseStatusDateTime, dataObject.LastKnownCFSStatusDate);

				SetLastKnownTransitWarehouseStatus(packingLineBO);

				SetOrgAddresses(packingLineBO);

				PopulateBusinessObjectFromVehicle(packingLineBO);

				SetValue(packingLineBO, JobPackLinesSchema.JL_PackageCount, dataObject.PackQty);

				SetValue(packingLineBO, JobPackLinesSchema.JL_UnitOfDimension, dataObject.LengthUnit);

				SetValue(packingLineBO, JobPackLinesSchema.JL_Length, dataObject.Length);
				SetValue(packingLineBO, JobPackLinesSchema.JL_Width, dataObject.Width);
				SetValue(packingLineBO, JobPackLinesSchema.JL_Height, dataObject.Height);

				SetValue(packingLineBO, JobPackLinesSchema.JL_OutturnedLength, dataObject.OutturnedLength);
				SetValue(packingLineBO, JobPackLinesSchema.JL_OutturnedWidth, dataObject.OutturnedWidth);
				SetValue(packingLineBO, JobPackLinesSchema.JL_OutturnedHeight, dataObject.OutturnedHeight);

				SetValue(packingLineBO, JobPackLinesSchema.JL_ActualVolumeUQ, dataObject.VolumeUnit);
				SetValue(packingLineBO, JobPackLinesSchema.JL_ActualVolume, dataObject.Volume);

				SetValue(packingLineBO, JobPackLinesSchema.JL_ActualWeightUQ, dataObject.WeightUnit);
				SetValue(packingLineBO, JobPackLinesSchema.JL_ActualWeight, dataObject.Weight);

				SetValue(packingLineBO, JobPackLinesSchema.JL_OutturnedVolume, dataObject.OutturnedVolume);
				SetValue(packingLineBO, JobPackLinesSchema.JL_OutturnedWeight, dataObject.OutturnedWeight);

				if (dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0)
				{
					var reader = new CustomFieldsDataObjectReader<PackLine>(logger, packingLineBO, new PackLineCustomFieldsDescriptor());
					reader.ReadCustomFields(dataObject.CustomizedFieldCollection);
				}

				if (ShouldPopulateUNDGCollection)
				{
					PopulateUNDGCollection(packingLineBO);
				}

				PopulateClassificationCollection(packingLineBO);

				SetValue(packingLineBO, JobPackLinesSchema.JL_ActualVolume, packingLineBO.JL_ActualVolume);

				ZBool requiresTemperatureControl = dataObject.RequiresTemperatureControl.GetValueOrDefault();
				SetValue(packingLineBO, JobPackLinesSchema.JL_RequiresTemperatureControl, dataObject.RequiresTemperatureControl);
				if (requiresTemperatureControl)
				{
					SetValue(packingLineBO, JobPackLinesSchema.JL_RequiredTemperatureMinimum, dataObject.RequiredTemperatureMinimum);
					SetValue(packingLineBO, JobPackLinesSchema.JL_RequiredTemperatureMaximum, dataObject.RequiredTemperatureMaximum);
					SetValue(packingLineBO, JobPackLinesSchema.JL_RequiredTemperatureUnit, dataObject.RequiredTemperatureUnit?.Code);

					var result = TemperatureHelper.CheckRequiredTemperatures(packingLineBO);
					if (!result.IsEmpty)
					{
						throw new DataObjectReadFailureException(result);
					}
				}

				SetValue(packingLineBO, JobPackLinesSchema.JL_IsHighRisk, dataObject.IsHighRisk);
				ImportAviationSecurityAdditionalInspectionTypeIfNeeded(packingLineBO);
				ImportAviationSecurityInspectionTypeIfNeeded(packingLineBO);
			}
			finally
			{
				if (supportDataImporting != null)
				{
					supportDataImporting.IsImportingData = false;
				}
			}
		}

		protected virtual void ImportAviationSecurityAdditionalInspectionTypeIfNeeded(T packingLineBO)
		{
			if (dataObject.AviationSecurityAdditionalInspectionType != null)
			{
				new AviationSecurityAdditionalInspectionTypeDataObjectReader(dataObject.AviationSecurityAdditionalInspectionType, logger, factory, packingLineBO, packingLineBO.Shipment.Logs, packingLineBO.IsInDatabase, (NoResString)"Packline Additional Inspection Type Changed by Data Import").ReadIntoBusinessObject();
			}
		}

		protected virtual void ImportAviationSecurityInspectionTypeIfNeeded(T packingLineBO)
		{
			if (dataObject.AviationSecurityInspectionType != null)
			{
				new AviationSecurityInspectionTypeDataObjectReader(dataObject.AviationSecurityInspectionType, logger, factory, packingLineBO, packingLineBO.Shipment.Logs, packingLineBO.IsInDatabase, (NoResString)"Packline Inspection Type Changed by Data Import").ReadIntoBusinessObject();
			}
		}

		void SetLastKnownTransitWarehouseStatus(T packingLineBO)
		{
			var lastKnownCFSStatus = dataObject.LastKnownCFSStatus?.Code ?? ZString.Empty;

			if (lastKnownCFSStatus.IsEmpty
				|| packingLineBO.JL_LastKnownTransitWarehouseStatus_List.ContainsCode(lastKnownCFSStatus))
			{
				SetValue(packingLineBO, JobPackLinesSchema.JL_LastKnownTransitWarehouseStatus, dataObject.LastKnownCFSStatus);
			}
			else
			{
				throw new DataObjectReadFailureException(Res.GetString("bd3ccd71-f776-41a4-8558-d0af0691a613", "Last known TW status should be RCV, DSP, or empty"));
			}
		}

		void SetOrgAddresses(T packingLineBO)
		{
			var lastKnownTransitWarehouseAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(AddressTypes.LastKnownCFSFacility);
			if (lastKnownTransitWarehouseAddress != null)
			{
				var addressBO = new OrganisationDataObjectReader(lastKnownTransitWarehouseAddress, logger, factory).GetMatched();
				if (addressBO != null)
				{
					var matchedAddressPK = MatchLastKnownTransitWarehouseAddress(packingLineBO, addressBO);

					if (matchedAddressPK.IsEmpty)
					{
						throw new DataObjectReadFailureException(Res.GetString("d040269c-a0dc-43df-82d6-2837efdd26f1", "Could not find matched Last Known Transit Warehouse Address ({0}) ({1})",
							lastKnownTransitWarehouseAddress.OrganizationCode,
							lastKnownTransitWarehouseAddress.AddressShortCode));
					}

					SetValue(packingLineBO, JobPackLinesSchema.JL_OA_LastKnownTransitWarehouseAddress, matchedAddressPK);
				}
			}
		}

		static ZGuid MatchLastKnownTransitWarehouseAddress(T packingLineBO, OrgAddress addressBO)
		{
			return packingLineBO.GetDefaultLastKnownTransitWarehouseAddress(addressBO.Header, addressBO, packingLineBO?.Shipment?.OuterPackLines?.CurrentConsol);
		}

		protected virtual bool ShouldPopulateUNDGCollection
		{
			get { return true; }
		}

		protected virtual UNDGDataObjectReader GetUNDGDataObjectReader(UNDG undgDataObject, Func<UNDGDataItem> undgDataItemBizObjProvider)
		{
			return factoryOverride == null ? new UNDGDataObjectReader(undgDataObject, logger, factory, undgDataItemBizObjProvider) : new UNDGDataObjectReader(undgDataObject, logger, factoryOverride, undgDataItemBizObjProvider);
		}

		protected void PopulateUNDGCollection(T packingLineBO)
		{
			if (dataObject.UNDGCollection != null)
			{
				if (IsUNDGMatchedForExistingPackingLineBO(packingLineBO, dataObject))
				{
					var reader = GetUNDGDataObjectReader(dataObject.UNDGCollection[0], () => packingLineBO.UNDGs[0]);

					reader.ReadIntoBusinessObject();
				}
				else
				{
					foreach (var source in dataObject.UNDGCollection)
					{
						var reader = GetUNDGDataObjectReader(source, null);

						packingLineBO.UNDGs.Add(reader.ReadIntoBusinessObject());
					}
				}
			}
		}

		protected void PopulateClassificationCollection(T packingLineBO)
		{
			if (dataObject.ClassificationCollection != null)
			{
				var reader = new ClassificationCollectionReader<JobPackLineHarmonisedCode, JobPackLineHarmonisedCodeCollection>(dataObject.ClassificationCollection, logger, factory, packingLineBO);
				reader.ReadIntoCollection();
			}
		}

		protected void PopulateBusinessObjectFromVehicle(T packingLineBO)
		{
			if (dataObject.Vehicle != null)
			{
				var vehicle = dataObject.Vehicle;
				SetValue(packingLineBO, JobPackLinesSchema.JL_VehicleColor, vehicle.Color);
				SetValue(packingLineBO, JobPackLinesSchema.JL_VehicleMake, vehicle.Make);
				SetValue(packingLineBO, JobPackLinesSchema.JL_VehicleModel, vehicle.Model);
				SetValue(packingLineBO, JobPackLinesSchema.JL_VehicleNumberOfDoors, vehicle.NumberOfDoors);
				SetValue(packingLineBO, JobPackLinesSchema.JL_VehicleTransmission, vehicle.Transmission?.Code ?? ZString.Empty);
				SetValue(packingLineBO, JobPackLinesSchema.JL_VehicleYear, vehicle.Year);
			}
		}

		#endregion

		bool IsUNDGMatchedForExistingPackingLineBO(PackLine packingLineBO, PackingLine packingLineDO)
		{
			if (packingLineBO.IsInDatabase
				&& packingLineBO.UNDGs.Count == 1
				&& packingLineDO.UNDGCollection != null
				&& packingLineDO.UNDGCollection.Count == 1)
			{
				return CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(packingLineDO.UNDGCollection[0], packingLineBO.UNDGs[0]);
			}

			return false;
		}
	}
}
