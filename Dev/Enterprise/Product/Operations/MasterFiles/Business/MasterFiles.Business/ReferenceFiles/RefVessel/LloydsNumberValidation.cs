using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class LloydsNumberValidation
	{
		public bool IsValid
		{
			get { return fErrorText.IsEmpty; }
		}

		public ZString ErrorText
		{
			get { return fErrorText; }
		}

		ZString fErrorText = ZString.Empty;

		public void Validate(ZString lloydsNumber, string humanReadableName = "Lloyds number")
		{
			fErrorText = ZString.Empty;
			if (!lloydsNumber.IsEmpty)
			{
				if (lloydsNumber.Length != CargoWise.Definitions.MasterFiles.RefVesselConstants.LloydsNumberLength)
				{
					fErrorText = Res.GetString("b4e24ac6-80a0-471d-8c92-d898f8503a11", "{0} must be 7 characters in length", humanReadableName);
				}
				else
				{
					if (lloydsNumber.IsNumbersOnlyOrEmpty)
					{
						int weightingSum = int.Parse(lloydsNumber.Substring(0, 1)) * 7 +
							int.Parse(lloydsNumber.Substring(1, 1)) * 6 +
							int.Parse(lloydsNumber.Substring(2, 1)) * 5 +
							int.Parse(lloydsNumber.Substring(3, 1)) * 4 +
							int.Parse(lloydsNumber.Substring(4, 1)) * 3 +
							int.Parse(lloydsNumber.Substring(5, 1)) * 2;
						if (weightingSum % 10 != Decimal.Parse(lloydsNumber.Substring(6, 1)))
						{
							fErrorText = Res.GetString("84feb138-2290-4f11-9a5e-26deaa6c20e9", "Invalid check-digit in {0}", humanReadableName);
						}
					}
					else
					{
						fErrorText = Res.GetString("ad6a97f5-2e07-417a-afcf-b514674a3c0a", "{0} must be completely numeric (nothing but numbers)", humanReadableName);
					}
				}
			}
		}
	}
}
