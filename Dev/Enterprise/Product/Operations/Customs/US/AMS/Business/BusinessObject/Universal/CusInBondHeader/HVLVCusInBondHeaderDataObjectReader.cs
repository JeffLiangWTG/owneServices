using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class HVLVCusInBondHeaderDataObjectReader : CusInBondHeaderDataObjectReader
	{
		public HVLVCusInBondHeaderDataObjectReader(Shipment consolDataObject, Shipment shipmentDataObject, ForwardingShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(consolDataObject, logger, factory, null)
		{
			this.consolDataObject = Argument.NotNull(consolDataObject, nameof(consolDataObject));
			this.shipmentDataObject = Argument.NotNull(shipmentDataObject, nameof(shipmentDataObject));
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
		}

		readonly Shipment consolDataObject;
		readonly Shipment shipmentDataObject;
		readonly ForwardingShipment shipment;

		public override DataContextType DataContextType => DataContextType.USAMS;

		protected override IMatchingBusinessEntityFinder<CusInBondHeader> GetCombinedReferenceMatcher() => null;

		protected override CusInBondHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var result = default(CusInBondHeader);
			var latestAMSLog = shipment.Logs.Find(log =>
					!log.SL_IsCancelled
					&& log.SL_SE_NKEvent == AutoEvents.TransferredCode
					&& log.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type)
					&& type == "AMS"
					&& log.Parameters.ContainsKey(EventReferenceParameters.Codes.ReferenceNumber)).MaxBySafe(log => log.SL_EventTime);

			if (latestAMSLog != null)
			{
				var amsJobNumber = latestAMSLog.Parameters[EventReferenceParameters.Codes.ReferenceNumber];
				var amsQuery = new ZQuery(CusInBondHeaderSchema.BH_JobReference, amsJobNumber);
				amsQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentID, shipment.PK);
				amsQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				result = factory.LoadTop1<CusInBondHeader>(amsQuery);
			}

			return result;
		}

		protected override void PopulateBusinessObject(CusInBondHeader headerBO)
		{
			SetValue(headerBO, CusInBondHeaderSchema.BH_TransitDirection, DirectionTypeList.Codes.NVOCC);

			SetValue(headerBO, CusInBondHeaderSchema.BH_ParentID, shipment.PK);
			SetValue(headerBO, CusInBondHeaderSchema.BH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			SetValue(headerBO, CusInBondHeaderSchema.BH_GB, GlbBranch.CurrentBranch.PK);
			SetValue(headerBO, CusInBondHeaderSchema.BH_ImportConveyanceCountry, shipment.Origin?.Country.Code ?? ZString.Empty);
			SetValue(headerBO, CusInBondHeaderSchema.BH_ImportTransportMode, GetImportTransportMode());
			SetValue(headerBO, CusInBondHeaderSchema.BH_CarrierSCAC, headerBO.SCACInCarrier);

			var consol = shipment.ArrivalConsol;
			if (consol != null)
			{
				var transport = GetMostInterestingTransportForBinding(consol);
				if (transport != null)
				{
					SetValue(headerBO, CusInBondHeaderSchema.BH_RL_NKImportLoadPort, transport.JW_RL_NKLoadPort);
					SetValue(headerBO, CusInBondHeaderSchema.BH_FirstExportDate, transport.JW_ETD);
				}
			}

			var consolDataCalculator = new ConsolDataCalculator(consol, headerBO);
			SetValue(headerBO, CusInBondHeaderSchema.BH_RL_NKPortUnlading, GetPortUnlading(consolDataCalculator));
			SetValue(headerBO, CusInBondHeaderSchema.BH_ETA, consolDataCalculator.FirstCountryDischargeDate);
			SetValue(headerBO, CusInBondHeaderSchema.BH_ImportConveyanceName, GetImportConveyanceName(consolDataCalculator));
			SetValue(headerBO, CusInBondHeaderSchema.BH_VoyageNumber, GetVoyageNumber(consolDataCalculator));

			PopulateBills(headerBO, consol, shipment, consolDataCalculator);
		}

		void PopulateBills(CusInBondHeader header, ForwardingConsol consol, ForwardingShipment shipment, ConsolDataCalculator consolDataCalculator)
		{
			PopulateCusInBondHeaderOceanBill(header, consol);
			PopulateBillsFromSubShipmentsOfShipmentDataObject(header, shipment, consolDataCalculator);
		}

		void PopulateCusInBondHeaderOceanBill(CusInBondHeader cusInBondHeader, ForwardingConsol consol)
		{
			var oceanBill = cusInBondHeader.OceanBill;
			oceanBill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.MasterBill;

			var billNumber = consol.JK_MasterBillNum.KeepValidBillNumberCharacters();
			var validSCACs = consol.GetValidSCACIssuerCodes(consol.TransportMode);

			oceanBill.B0_IssuerCode = billNumber.GetSCAC(validSCACs);
			oceanBill.B0_MasterBillNumber = billNumber.ShouldTrimSCACFromBills(validSCACs) ? billNumber.GetBillNumberTrimSCAC() : billNumber;
		}

		void PopulateBillsFromSubShipmentsOfShipmentDataObject(CusInBondHeader header, ForwardingShipment shipment, ConsolDataCalculator consolDataCalculator)
		{
			if (shipmentDataObject.SubShipmentCollection != null)
			{
				foreach (var subShipmentDataObject in shipmentDataObject.SubShipmentCollection)
				{
					if (IsConsignmentDataObject(subShipmentDataObject))
					{
						new HVLVCusInBondBillDataObjectReader(subShipmentDataObject, consolDataObject, header, shipment, consolDataCalculator, logger, factory).ReadIntoBusinessObject();
					}
				}
			}
		}

		bool IsConsignmentDataObject(Shipment dataObject)
		{
			return dataObject.DataContext != null
				&& dataObject.DataContext.DataSourceCollection != null
				&& dataObject.DataContext.DataSourceCollection.Any(dataSource => dataSource.Type.GetValueOrDefault() == nameof(DataContextType.HVLVConsignment));
		}

		ZString GetImportTransportMode()
		{
			var result = ZString.Empty;
			var consolTransportMode = consolDataObject.TransportMode?.Code;
			if (consolTransportMode.Equals(TransportModes.Sea))
			{
				result = consolDataObject.ContainerCollection == null || consolDataObject.ContainerCollection.All(container => container.FCL_LCL_AIR?.Code.Equals(ContainerModes.BreakBulk) ?? false)
					? TransportTypeList.Codes.VesselNonContainer
					: TransportTypeList.Codes.VesselContainer;
			}

			return result;
		}

		Transport GetMostInterestingTransportForBinding(ForwardingConsol consol)
		{
			var orderHelper = new TransportOrderHelper(consol.Transports);
			var matchingLeg = orderHelper.LastLegMatching(transport =>
			{
				var matchTransportMode = transport.JW_TransportMode == consol.JK_TransportMode;
				var foreignToUS = transport.JW_RL_NKLoadPort.Left(2) != CountryCodes.UnitedStates && transport.JW_RL_NKDiscPort.Left(2) == CountryCodes.UnitedStates;
				return matchTransportMode && foreignToUS;
			});

			return matchingLeg ?? orderHelper.LastLeg;
		}

		ZString GetPortUnlading(ConsolDataCalculator consolDataCalculator)
		{
			var port = consolDataCalculator.FirstCountryPortOfDischarge;
			return port?.RL_Code.ToUpper() ?? ZString.Empty;
		}

		ZString GetImportConveyanceName(ConsolDataCalculator consolDataCalculator)
		{
			var leg = consolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			return leg?.JW_Vessel.ToUpper() ?? ZString.Empty;
		}

		ZString GetVoyageNumber(ConsolDataCalculator consolDataCalculator)
		{
			var leg = consolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			return leg?.JW_VoyageFlight.ToUpper() ?? ZString.Empty;
		}
	}
}
