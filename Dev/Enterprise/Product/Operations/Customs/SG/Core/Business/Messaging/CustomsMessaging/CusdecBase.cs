namespace Enterprise.Customs.SG.Business.CustomsMessaging
{
	using Enterprise.Customs.SG.V4.Business;
	using Enterprise.Edifact.Auto;

	public abstract class CusdecBase<T> : MessageBuilderBase<T> where T : SegmentGroup, new()
	{
		public CusdecBase(ISGCUSDEC sgCusdec)
			: base(sgCusdec)
		{
		}

		public T CusdecMessage { get { return EdifactMessage; } }

		#region Implementation

		protected sealed override string UnhControllingAgency
		{
			get { return UnhControllingAgencyConst; }
		}

		protected sealed override string UnhMessageVersionNumber
		{
			get { return UnhMessageVersionNumberConst; }
		}

		#region CusdecConstants

		const string UnhControllingAgencyConst = "UN";
		const string UnhMessageVersionNumberConst = "D";
		protected const string DmsInvoiceDetails = "INVOICE DETAILS";

		#endregion

		#region SGWeightConversion

		/// <summary>
		/// SG Customs requires declarations to be submitted in only 2 weight measurements:
		///     TNE when transport by sea
		///     KGM when transport by air
		/// If the sourceWeightUnit is not valid, sourceWeight will be returned without conversion.
		/// </summary>
		protected (decimal, string) ConvertToSingaporeCustomsRequiredWeightUnit(decimal sourceWeight, string sourceWeightUnit, string targetWeightUnit)
		{
			var resultWeight = sourceWeight;
			var resultWeightUnit = sourceWeightUnit;

			if (Core.Constants.Weight.ContainsCode(sourceWeightUnit))
			{
				if (targetWeightUnit == Core.Constants.Weight.Kilograms || targetWeightUnit == SGConstants.Weight.Kilograms)
				{
					resultWeight = Core.Constants.Weight.Convert(sourceWeight, sourceWeightUnit, Core.Constants.Weight.Kilograms);
					resultWeightUnit = targetWeightUnit;
				}
				else if (targetWeightUnit == Core.Constants.Weight.Tonnes || targetWeightUnit == SGConstants.Weight.Tonnes)
				{
					resultWeight = Core.Constants.Weight.Convert(sourceWeight, sourceWeightUnit, Core.Constants.Weight.Tonnes);
					resultWeightUnit = targetWeightUnit;
				}
			}

			return (resultWeight, resultWeightUnit);
		}

		protected string ConvertContainerWeightTo3CharString(decimal containerWeightInTonnes)
		{
			decimal roundedContainerWeightInTonnes = ZArchitecture.Core.Utilities.Round(containerWeightInTonnes, 0);
			if (roundedContainerWeightInTonnes > 999)
			{
				roundedContainerWeightInTonnes = 999;
			}
			else if (roundedContainerWeightInTonnes < 1)
			{
				roundedContainerWeightInTonnes = 1;
			}

			if (UnhMessageReleaseNumber == "09B")
			{
				return roundedContainerWeightInTonnes.ToString();
			}
			else
			{
				return roundedContainerWeightInTonnes.ToString().PadLeft(3, "0".ToCharArray()[0]);
			}
		}

		#endregion

		#endregion
	}

	static class CusdecConstants
	{
		public const string UnhMessageTypeIdentifier = "CUSDEC";
	}
}
