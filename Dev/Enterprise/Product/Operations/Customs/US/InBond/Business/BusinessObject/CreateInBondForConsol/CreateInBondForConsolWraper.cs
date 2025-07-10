using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CreateInBondForConsolWraper : NonPersistentBusinessObject
	{
		public CreateInBondForConsolWraper(ICusInBondParent parent)
		{
			if (parent is ForwardingConsol consol)
			{
				this.consol = consol;
			}
			else
			{
				throw new ArgumentException("CreateInBondForConsolWraper is for ForwardingConsol only but was " + parent.GetType());
			}
		}
		readonly ForwardingConsol consol;

		public ZString ConsolNumber => consol.JobNumber;

		public CusInBondShipmentWrapperCollection ShipmentsWithoutInBond => shipmentsWithoutInBond ?? (shipmentsWithoutInBond = new CusInBondShipmentWrapperCollection(consol));
		CusInBondShipmentWrapperCollection shipmentsWithoutInBond;

		public MovementHeaderWrapperCollection NewMovementHeaders => newMovementHeaders ?? (newMovementHeaders = new MovementHeaderWrapperCollection(consol.Factory, ShipmentsWithoutInBond));
		MovementHeaderWrapperCollection newMovementHeaders;

		#region Initialize CusInBond

		CusInBondHeaderConsolDataCalculator consolDataCalculator;
		CusInBondHeader inBond;

		public void Initialize(CusInBondHeader inBond)
		{
			this.inBond = Argument.NotNull(inBond, "inBond");
			this.consolDataCalculator = new CusInBondHeaderConsolDataCalculator(consol, inBond);

			InitInBondHeader();
			InitMovementHeader();
			this.inBond.ReloadCachedCollections();
		}

		void InitInBondHeader()
		{
			var firstShipment = NewMovementHeaders.Cast<MovementHeaderWrapper>().FirstOrDefault()?.AllocatedShipmentsForMovement.Cast<CusInBondShipmentWrapper>().FirstOrDefault();
			inBond.BH_OA_Importer = firstShipment?.ConsigneeOrganizationAddress ?? ZGuid.Empty;
			inBond.BH_OH_Supplier = firstShipment?.ConsignorPK ?? ZGuid.Empty;
			inBond.BH_ImportTransportMode = consolDataCalculator.GetInBondModeFromTransportAndPacking();
			inBond.BH_CarrierSCAC = consolDataCalculator.GetCarrierSCAC();
			inBond.BH_ImportConveyanceName = consolDataCalculator.GetConveyanceName();
			inBond.BH_VoyageNumber = consolDataCalculator.GetVoyageFlight();
			inBond.BH_ImportLoadPortKCode = consolDataCalculator.GetImportLoadingPort();
			inBond.BH_SailingDate = consolDataCalculator.GetSailingDate();
			inBond.BH_ETA = consolDataCalculator.GetETADate();
			inBond.BH_RN_NKFirstExportCountry = consolDataCalculator.GetExportCountry();
			inBond.BH_PortUnladingDCode = consolDataCalculator.GetPortUnladingDCode();
			inBond.BH_FIRMS = consolDataCalculator.GetFIRMSCode();
		}

		void InitMovementHeader()
		{
			foreach (var moveHeaderWrapper in NewMovementHeaders.Cast<MovementHeaderWrapper>())
			{
				var movementHeader = inBond.MovementHeaders.AddNew();
				var moveHeaderDataCalculator = new CusInBondMoveHeaderConsolDataCalculator(consol, moveHeaderWrapper, movementHeader);
				movementHeader.BM_InBondEntryType = moveHeaderWrapper.EntryType;
				movementHeader.BM_DestinationPortCode = moveHeaderWrapper.Destination;
				movementHeader.BM_OA_InBondCarrier = moveHeaderWrapper.InBondCarrierAddress;
				movementHeader.BM_ForeignDestPortKCode = moveHeaderDataCalculator.GetForeignDestPort();

				var goodsValueSum = ZDecimal.Zero;
				var shouldSumUpGoodsValue = true;
				var allocatedShipmentsForMovement = moveHeaderWrapper.AllocatedShipmentsForMovement.Cast<CusInBondShipmentWrapper>();
				foreach (var shipmentWrapper in allocatedShipmentsForMovement)
				{
					if (shouldSumUpGoodsValue)
					{
						if (shipmentWrapper.GoodsValueCurrency == Core.Constants.CurrencyCodes.UnitedStates)
						{
							goodsValueSum += shipmentWrapper.GoodsValue;
						}
						else
						{
							shouldSumUpGoodsValue = false;
							goodsValueSum = ZDecimal.Zero;
						}
					}
				}
				movementHeader.BM_MonetaryValue = goodsValueSum;

				InitBillAndCalculateTotalValue(moveHeaderWrapper, movementHeader);
			}
		}

		void InitBillAndCalculateTotalValue(MovementHeaderWrapper moveHeaderWrapper, CusInBondMoveHeader movementHeader)
		{
			foreach (var shipmentWrapper in moveHeaderWrapper.AllocatedShipmentsForMovement.Cast<CusInBondShipmentWrapper>())
			{
				var shipment = shipmentWrapper.Shipment;
				var declarationPK = ((ICusInBondParent)shipment).GetDeclarationPK(GlbCompany.CurrentCompany.PK);
				var declaration = shipment.Factory.Load<JobDeclaration>(declarationPK);
				var bill = inBond.Bills.AddNew();
				var billDataCalculator = new CusInBondBillDataCalculator(shipment);
				bill.B0_IssuerCode = consolDataCalculator.MasterBillIssuerCode;
				bill.B0_MasterBillNumber = consolDataCalculator.MasterBill;
				bill.B0_HouseBillNumber = shipmentWrapper.HouseBillNumber;
				bill.B0_HouseBillIssuerCode = billDataCalculator.GetHouseBillIssuerCode();
				bill.B0_Weight = shipmentWrapper.ActualWeight;
				bill.B0_WeightUQ = shipmentWrapper.WeightUnit;
				bill.B0_Volume = shipmentWrapper.ActualVolume;
				bill.B0_VolumeUQ = shipmentWrapper.VolumeUnit;
				bill.B0_ManifestQty = declaration?.JE_TotalNoOfPacks ?? ZInt.Zero;
				bill.B0_ManifestUQ = declaration?.JE_TotalNoOfPacksPackType ?? ZString.Empty;

				InitMoveDetailAndContainer(bill, movementHeader);
			}
		}

		void InitMoveDetailAndContainer(CusInBondBill bill, CusInBondMoveHeader movementHeader)
		{
			var movementDetail = movementHeader.MovementDetails.AddNew();
			movementDetail.B9_B0 = bill.PK;

			_ = movementDetail.CBP7512Lines.AddNew();
		}

		#endregion
	}
}
