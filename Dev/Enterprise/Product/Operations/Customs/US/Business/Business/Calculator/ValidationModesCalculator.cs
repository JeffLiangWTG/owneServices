using System;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Distinct validation modes for each of which has a separate message sent to US Customs.
	/// This validation mode will be set at Declaration for most of the cases, but
	/// with Declaration validation mode being set to None if users choose to send messages for a container or bill.
	/// </summary>
	[Flags]
	public enum ValidationModes
	{
		None = 0, //CusContainer Arrival validation can set Declaration's modes to None
		EntrySummary = 1,
		CargoRelease = 2,
		StandAlonePriorNotice = 512,
		FTZAdmissionValidationMode = 1024,
		FTZArrivalValidationMode = 2048,
		FTZEventsValidationMode = 4096,
		FTZPTTValidationMode = 8192
	}

	public class ValidationModesCalculator
	{
		public ValidationModesCalculator(JobDeclaration declaration)
		{
			if (declaration == null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public bool IsThisValidationOn(ValidationModes currentValidationMode, ValidationModes modeToCheckAgainst)
		{
			return declaration.IsImport && modeToCheckAgainst == (currentValidationMode & modeToCheckAgainst);
		}

		public void RecalculateValidationModesOnDeclaration(bool suspendMarkingAsNeedingValidation)
		{
			ValidationModes result = ValidationModes.None;

			if (declaration.IsImport)
			{
				if (declaration.US_EnableENS)
				{
					result = ValidationModes.EntrySummary;
				}

				if (declaration.US_EnableCRL || declaration.US_CertifyCargoRelease)
				{
					result |= ValidationModes.CargoRelease;
				}

				if (declaration.IsFTZAdmission)
				{
					result = ValidationModes.FTZAdmissionValidationMode;
				}

				if (declaration.US_EnableSPN)
				{
					result |= ValidationModes.StandAlonePriorNotice;
				}
			}

			IDisposable decSuspender = suspendMarkingAsNeedingValidation ? declaration.SuspendMarkingAsNeedingValidation() : null;

			try
			{
				declaration.ValidationModes = result;
			}
			finally
			{
				if (decSuspender != null)
				{
					decSuspender.Dispose();
				}
			}

			foreach (CusContainer container in declaration.CusContainers)
			{
				IDisposable suspender = suspendMarkingAsNeedingValidation ? container.SuspendMarkingAsNeedingValidation() : null;
				try
				{
					container.ValidationModes = ValidationModes.None;
				}
				finally
				{
					if (suspender != null)
					{
						suspender.Dispose();
					}
				}
			}

			foreach (Bill bill in declaration.Bills)
			{
				IDisposable suspender = suspendMarkingAsNeedingValidation ? bill.SuspendMarkingAsNeedingValidation() : null;

				try
				{
					bill.ValidationModes = ValidationModes.None;
				}
				finally
				{
					if (suspender != null)
					{
						suspender.Dispose();
					}
				}
			}
		}

		internal void UpdateValidationModes(ValidationModes modeToCheckAgainst, bool isSpecificModeEnabled)
		{
			if (declaration.IsImport)
			{
				if (isSpecificModeEnabled)
				{
					declaration.ValidationModes |= modeToCheckAgainst;
				}
				else if ((declaration.ValidationModes & modeToCheckAgainst) == modeToCheckAgainst)//cannot use IsThisValidationOn as this is after a value is set
				{
					int result = declaration.ValidationModes - modeToCheckAgainst;
					declaration.ValidationModes = (ValidationModes)result;
				}
			}
		}
	}
}
