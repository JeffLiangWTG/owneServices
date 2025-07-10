using System;

namespace Enterprise.Customs.US.AMS.Business
{
	[Flags]
	public enum ValidationModes
	{
		None = 0,
		InventoryRecord = 1,
		UseParentValidateMode = 2,
		PermitToTransfer = 4,
		SubsequentInBond = 8,
		ChangeEstDateOfArrival = 16,
		VesselDeparture = 32,
		VesselArrival = 64,
		InBondArrival = 128,
		InBondExportation = 256,
		InBondTOL = 512,
		InBondDiversion = 1024,
		InventoryRecordAmendment = 2048
	}

	public interface IValidationModesSupporter
	{
		ValidationModes ValidationModes { get; set; }
	}

	public static class ValidationModesCalculator
	{
		public static bool IsThisValidationOn(ValidationModes currentValidationMode, ValidationModes modeToCheckAgainst)
		{
			return modeToCheckAgainst == (currentValidationMode & modeToCheckAgainst);
		}
	}
}

