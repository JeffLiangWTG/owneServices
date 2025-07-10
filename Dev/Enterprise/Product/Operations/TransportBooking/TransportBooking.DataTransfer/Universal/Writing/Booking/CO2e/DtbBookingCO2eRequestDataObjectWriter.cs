using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingCO2eRequestDataObjectWriter : CO2eRequestDataObjectWriter<DtbBooking>
	{
		public DtbBookingCO2eRequestDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		public DtbBookingCO2eRequestDataObjectWriter(IDataWritingManager manager, ICO2eCalculationSupporter hostSupporter) : base(manager, hostSupporter)
		{
		}

		protected override void PopulateDataObject(DtbBooking bookingBO, UniversalShipment bookingDataObject)
		{
			bookingDataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.KM_TransportMode, bookingBO.Lookups.BookingTransportModes);
			base.PopulateDataObject(bookingBO, bookingDataObject);
			PopulateInstructions(bookingBO, bookingDataObject);
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportBooking;
		}

		void PopulateInstructions(DtbBooking dataTbBO, UniversalShipment shipmentData)
		{
			shipmentData.SetInstructionCollection(() =>
			{
				var result = new DataObjectList<Instruction>();
				var matcher = new CO2eInstructionMatcher(dataTbBO);
				var matchedInstructions = matcher.FindAllMatches().Actions;

				foreach (var matchedInstruction in matchedInstructions.GroupBy(x => x.GetDivot().Instruction).OrderBy(x => x.Key.KN_Sequence))
				{
					var instruction = new Instruction(writeManager.WriterStrategy);

					instruction.Sequence = matchedInstruction.Key.KN_Sequence;
					PopulateInstructionAddress(instruction, matchedInstruction.Key);
					PopulateInstructionWeight(instruction, matchedInstruction);

					result.Add(instruction);
				}

				return result;
			});
		}

		void PopulateInstructionAddress(Instruction instructionDO, DtbBookingInstruction instructionBO)
		{
			if (instructionBO.Address.E2_AddressOverride)
			{
				instructionDO.Address = instructionBO.Address.ToUXmlOrganizationAddress(writeManager.WriterStrategy, instructionBO.Factory);
			}
			else
			{
				instructionDO.Address = instructionBO.Address.Address.ToUXmlOrganizationAddress(writeManager.WriterStrategy, instructionBO.Factory);
			}
		}

		void PopulateInstructionWeight(Instruction instructionDO, IEnumerable<ICO2eMatchAction> instructionActions)
		{
			instructionDO.SetWeightCollection(() =>
			{
				var result = new DataObjectList<WeightData>();
				foreach (var action in instructionActions)
				{
					var divot = action.GetDivot();
					var weightData = new WeightData
					{
						Direction = action.IsPickup ? DirectionType.Load : DirectionType.Unload,
						TotalWeight = action.Weight,
						TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, divot.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
					};

					if (divot.IsContainerised)
					{
						var container = divot.Package.Container;
						var numOfTEU = container.ContainerType.RC_TEU * action.Quantity;
						var tonnesPerTEU = numOfTEU > 0 && !action.IsEmptyContainer ? Constants.Weight.ConvertSafe(divot.Package.GoodsWeight, divot.WeightUQ, Constants.Weight.Tonnes) / numOfTEU : 0;
						var emptyWeightPerTEU = container.ContainerType.RC_TareWeight / (numOfTEU > 0 ? numOfTEU : 1);

						weightData.ContainerJobID = divot.PackageID;
						weightData.TEU = new TEU
						{
							NumberOfTEU = numOfTEU,
							TonnesPerTEU = Utilities.Round(tonnesPerTEU, 6),
							ContainerEmptyWeightPerTEU = Utilities.Round(emptyWeightPerTEU, 6),
							ContainerEmptyWeightPerTEUUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, divot.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
						};
					}
					result.Add(weightData);
				}
				return result;
			});
		}
	}
}
