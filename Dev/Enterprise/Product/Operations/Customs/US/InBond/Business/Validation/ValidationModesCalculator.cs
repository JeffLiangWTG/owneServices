using System;
using CargoWise.Common;
using static Enterprise.Customs.US.InBond.Business.InBondMenuItemMessageData;

namespace Enterprise.Customs.US.InBond.Business
{
	[Flags]
	public enum ValidationModes
	{
		None = 0,
		Departure = 1,
		InBondLevelArrival = 2,
		BillOfLadingLevelArrival = 4,
		ContainerLevelArrival = 8,
		InBondLevelTransferOfLiability = 16,
		InBondLevelExportation = 32,
		BillOfLadingLevelExportation = 64,
		ContainerLevelExportation = 128,
		AirInitiationAndDeletion = 256,
		AirEntireInBondArrival = 512,
		AirEntireInBondExportation = 1024,
		DepartureDelete = 2048,
		DiversionRequest = 4096,
		BillOfLadingDelete = 8192
	}

	public class ValidationModesCalculator
	{
		public ValidationModesCalculator(CusInBondHeader header)
		{
			Argument.NotNull(header, "header");
			this.header = header;
		}

		readonly CusInBondHeader header;

		public bool IsThisValidationOn(ValidationModes currentValidationMode, ValidationModes modeToCheckAgainst)
		{
			return modeToCheckAgainst == (currentValidationMode & modeToCheckAgainst);
		}

		public InBondMessageType ConvertMessageType(InBondMenuItemMessageTypes menuItemMessageType)
		{
			return menuItemMessageType == InBondMenuItemMessageTypes.Arrival ?
						(header.IsAir ? InBondMessageType.AirEntireInBondArrival : InBondMessageType.InBondLevelArrival) :
						(header.IsAir ? InBondMessageType.AirEntireInBondExportation : InBondMessageType.InBondLevelExportation);
		}

		public void RecalculateValidationModesOnHeader(InBondMessageType messageType)
		{
			header.ValidationModes = ValidationModes.Departure;
			switch (messageType)
			{
				case InBondMessageType.DepartureAdd:
					header.ValidationModes = ValidationModes.Departure;
					break;
				case InBondMessageType.DepartureBillDelete:
				case InBondMessageType.AirBillDelete:
					header.ValidationModes = ValidationModes.BillOfLadingDelete;
					break;
				case InBondMessageType.DepartureDelete:
					header.ValidationModes = ValidationModes.DepartureDelete;
					break;
				case InBondMessageType.InBondLevelArrival:
					header.ValidationModes = ValidationModes.InBondLevelArrival;
					break;
				case InBondMessageType.BillOfLadingLevelArrival:
					header.ValidationModes = ValidationModes.BillOfLadingLevelArrival;
					break;
				case InBondMessageType.ContainerLevelArrival:
					header.ValidationModes = ValidationModes.ContainerLevelArrival;
					break;
				case InBondMessageType.InBondLevelExportation:
					header.ValidationModes = ValidationModes.InBondLevelExportation;
					break;
				case InBondMessageType.BillOfLadingLevelExportation:
					header.ValidationModes = ValidationModes.BillOfLadingLevelExportation;
					break;
				case InBondMessageType.ContainerLevelExportation:
					header.ValidationModes = ValidationModes.ContainerLevelExportation;
					break;
				case InBondMessageType.InBondLevelTransferOfLiability:
					header.ValidationModes = ValidationModes.InBondLevelTransferOfLiability;
					break;
				case InBondMessageType.AirInBondAdd:
				case InBondMessageType.AirInBondDelete:
				case InBondMessageType.AirInBondAmend:
					header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
					break;
				case InBondMessageType.AirEntireInBondArrival:
					header.ValidationModes = ValidationModes.AirEntireInBondArrival;
					break;
				case InBondMessageType.AirEntireInBondExportation:
					header.ValidationModes = ValidationModes.AirEntireInBondExportation;
					break;
				case InBondMessageType.DiversionRequest:
					header.ValidationModes = ValidationModes.DiversionRequest;
					break;
			}
		}

		internal void UpdateValidationModes(ValidationModes modeToCheckAgainst, bool isSpecificModeEnabled)
		{
			if (isSpecificModeEnabled)
			{
				header.ValidationModes |= modeToCheckAgainst;
			}
			else if ((header.ValidationModes & modeToCheckAgainst) == modeToCheckAgainst)//cannot use IsThisValidationOn as this is after a value is set
			{
				int result = header.ValidationModes - modeToCheckAgainst;
				header.ValidationModes = (ValidationModes)result;
			}
		}
	}
}
