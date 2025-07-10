using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsOrderAndReceiveDataObjectWriter<T> : WhsDocketDataObjectWriter<T>
		where T : WhsDocket, IJobWithTransportCompany
	{
		protected WhsOrderAndReceiveDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region Export Job

		protected override void PopulateDataObject(T whsDocketBO, UniversalShipment shipmentDataObject)
		{
			base.PopulateDataObject(whsDocketBO, shipmentDataObject);

			var orderDataObject = shipmentDataObject.Order;
			orderDataObject.ClientReference = whsDocketBO.WD_CustomerReference;
			orderDataObject.DropMode = ListHelper.GetWithDescription<DropMode>(whsDocketBO.WD_DropMode, whsDocketBO.Lookups.DropModes);
			orderDataObject.OrderNumberSplit = whsDocketBO.WD_ExternalReferenceSplit;
			orderDataObject.PalletsSent = GetPallets(whsDocketBO);
			orderDataObject.TotalLineVolume = whsDocketBO.WD_TotalCubic;
			orderDataObject.TotalLineWeight = whsDocketBO.WD_TotalWeight;
			orderDataObject.TotalUnits = whsDocketBO.WD_TotalUnits;
			orderDataObject.TransportReference = whsDocketBO.WD_TransportReference;
			orderDataObject.Type = ListHelper.GetWithDescription<CodeDescriptionPair>(whsDocketBO.WD_DocketSubType, whsDocketBO.Lookups.SubTypes);

			shipmentDataObject.GoodsDescription = whsDocketBO.WD_GoodsDescription;
			shipmentDataObject.GoodsValueCurrency = ListHelper.GetWithDescription<Currency>(whsDocketBO.WD_RX_NKTotalOrderCurrency, whsDocketBO.Lookups.TotalOrderCurrencies);
			shipmentDataObject.GoodsValue = whsDocketBO.WD_TotalOrderValue;
			PopulateOuterPacksAndPackageType(whsDocketBO, shipmentDataObject);
			shipmentDataObject.CarrierServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(whsDocketBO.WD_PL_NKCarrierServiceLevel, whsDocketBO.Lookups.CarrierServiceLevels);
			shipmentDataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(whsDocketBO.WD_RS_NKServiceLevel, whsDocketBO.Lookups.ServiceLevels);
			shipmentDataObject.TotalVolume = whsDocketBO.WD_CubicSent != 0m ? whsDocketBO.WD_CubicSent : whsDocketBO.WD_TotalCubic;
			shipmentDataObject.TotalWeight = whsDocketBO.WD_WeightSent != 0m ? whsDocketBO.WD_WeightSent : whsDocketBO.WD_TotalWeight;
			shipmentDataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(whsDocketBO.WD_TotalCubicUnit, whsDocketBO.TotalCubicUnits);
			shipmentDataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(whsDocketBO.WD_TotalWeightUnit, whsDocketBO.TotalWeightUnits);
			shipmentDataObject.WayBillNumber = whsDocketBO.WD_BOLNo;
			shipmentDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());
			shipmentDataObject.ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(whsDocketBO.WD_ScreeningStatus, whsDocketBO.Lookups.ScreeningStatusesList);

			PopulateCarrierServiceLevel(whsDocketBO, shipmentDataObject);
			ExportRelatedObjects(whsDocketBO, shipmentDataObject);
		}

		#region PopulateCarrierServiceLevel

		void PopulateCarrierServiceLevel(T whsDocketBO, UniversalShipment shipmentDataObject)
		{
			var carrierServiceLevel = GetCarrierServiceLevel(whsDocketBO);
			if (carrierServiceLevel != null)
			{
				var writer = new CarrierServiceLevelDataObjectWriter(writeManager);
				shipmentDataObject.CarrierServiceLevel = writer.GetDataObject(carrierServiceLevel);
				shipmentDataObject.IsSignatureRequired = carrierServiceLevel.PL_IsSignatureRequired;
			}
		}

		#endregion

		#region GetCarrierServiceLevel

		OrgCarrierServiceLevel GetCarrierServiceLevel(T whsDocketBO)
		{
			OrgCarrierServiceLevel serviceLevel = null;

			var transportCo = whsDocketBO.GetTransportCo();
			if (transportCo != null)
			{
				var query = new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, whsDocketBO.WD_PL_NKCarrierServiceLevel);
				query.AddToFilter(OrgCarrierServiceLevelSchema.PL_OM, transportCo.MiscServ.PK);
				serviceLevel = whsDocketBO.Factory.LoadTop1<OrgCarrierServiceLevel>(query);
			}

			return serviceLevel;
		}

		#endregion

		void PopulateOuterPacksAndPackageType(T whsDocketBO, UniversalShipment shipmentDataObject)
		{
			if (PopulateOuterPacksQty(whsDocketBO))
			{
				shipmentDataObject.OuterPacks = GetOuterPacks(whsDocketBO);
			}

			SetOuterPacksPackageType(whsDocketBO, shipmentDataObject);
		}

		protected abstract bool PopulateOuterPacksQty(T whsDocketBO);

		#region GetOuterPacks

		ZInt? GetOuterPacks(T whsDocketBO)
		{
			return new[]
			{
				GetPallets(whsDocketBO),
				whsDocketBO.WD_PackagesSent,
				ToZIntSafely(whsDocketBO.WD_TotalUnits, nameof(whsDocketBO.WD_TotalUnits)),
				ToZIntSafely(whsDocketBO.Lines.Sum(l => l.WE_TransactionQuantity), WhsDocketLineSchema.Constants.WE_TransactionQuantity)
			}.FirstOrDefault(d => d > 0);
		}

		protected abstract ZShort GetPallets(T whsDocketBO);

		#endregion

		#region SetOuterPacksPackageType

		void SetOuterPacksPackageType(T whsDocketBO, UniversalShipment shipmentDataObject)
		{
			if (shipmentDataObject.Order.PalletsSent > 0) // If Total Pallets entered, populate package type as PLT 
			{
				shipmentDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Pallet, Description = Constants.PkgUnit.GetDescription(Constants.PkgUnit.Pallet) };
			}
			else if (shipmentDataObject.OuterPacks.GetValueOrDefault() == 0 || whsDocketBO.WD_F3_NKTotalPackType.IsEmpty) // Else if Total Packages haven't been entered populate package type as PCE
			{
				shipmentDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Piece, Description = Constants.PkgUnit.GetDescription(Constants.PkgUnit.Piece) };
			}
			else
			{
				shipmentDataObject.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(whsDocketBO.WD_F3_NKTotalPackType, whsDocketBO.Lookups.TotalPackTypes);
			}
		}

		#endregion

		void ExportRelatedObjects(T whsDocketBO, UniversalShipment shipmentDataObject)
		{
			var jobHeader = whsDocketBO.JobHeader;
			if (jobHeader != null)
			{
				shipmentDataObject.AddOrgAddress(writeManager, jobHeader.LocalChargesAddr, AddressTypes.SendersLocalClient);
			}

			var additionalServices = ProcessCollection(whsDocketBO.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Complete);
			if (additionalServices != null)
			{
				shipmentDataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
				shipmentDataObject.LocalProcessing.SetAdditionalServiceCollection(() => additionalServices);
			}

			var jobDocAddressWriter = new JobDocAddressDataObjectWriter(writeManager)
			{
				PopulateIsResidential = PopulateJobDocAddressIsResidential
			};
			var addresses = ProcessCollection(whsDocketBO.DocAddresses, jobDocAddressWriter);
			if (addresses != null)
			{
				if (shipmentDataObject.OrganizationAddressCollection == null)
				{
					shipmentDataObject.SetOrganizationAddressCollection(() => addresses);
				}
				else
				{
					shipmentDataObject.OrganizationAddressCollection.AddRange(addresses);
				}
			}

			shipmentDataObject.SetAdditionalReferenceCollection(() => ProcessCollection(whsDocketBO.References, new WhsDocketReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		protected virtual bool PopulateJobDocAddressIsResidential => false;

		#endregion
	}
}
