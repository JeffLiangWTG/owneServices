namespace Enterprise.Customs.US.Business
{
	public class AffirmationCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public AffirmationCodeValidation(AffirmationCode affirmCode)
			: base(affirmCode)
		{
		}

		new AffirmationCode Parent
		{
			get { return (AffirmationCode)base.Parent; }
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.CY_Data.IsEmpty)
			{
				USCAffirmationOfCompliance affirmationCode = Parent.USCAffirmationCode;
				if (affirmationCode != null && affirmationCode.UL_QualifierIndicator)
				{
					Parent.CY_DataInfo.AddMessageError(string.Format(AffirmationCodeRequiresValue, affirmationCode.UL_Code));
				}
			}
			else if (Parent.CY_Code == ProductCodeQualifiersList.Codes.PMNNumber)
			{
				if (!((Parent.CY_Data.StartsWith("K") && Parent.CY_Data.Length == 7 && Parent.CY_Data.SubstringSafe(1, Parent.CY_Data.Length - 1).IsNumbersOnlyOrEmpty)
					|| (Parent.CY_Data.StartsWith("DEN") && Parent.CY_Data.Length == 9 && Parent.CY_Data.SubstringSafe(3, Parent.CY_Data.Length - 3).IsNumbersOnlyOrEmpty)))
				{
					Parent.CY_DataInfo.AddMessageError(PMNCodeRequiresValue);
				}
			}
		}
		internal const string AffirmationCodeRequiresValue = "Affirmation code '{0}' requires a value to be entered.";
		internal const string PMNCodeRequiresValue = "Affirmation of Compliance code 'PMN' will either be 'K' followed by 6 digits or 'DEN' followed by 6 digits.";

		#region CheckCY_Code

		protected override void CheckCY_Code()
		{
			if (Parent.USCAffirmationCode == null)
			{
				Parent.CY_CodeInfo.AddMessageError(InvalidAffirmationCode);
			}
			else
			{
				FDA fda = Parent.Factory.Load<FDA>(Parent.CY_ParentID);
				if (fda != null)
				{
					foreach (AffirmationCode code in fda.AffirmationCodes)
					{
						if (code != this.Parent && code.CY_Code == Parent.CY_Code)
						{
							Parent.CY_CodeInfo.AddWarning(string.Format(DuplicateAffirmationCode, Parent.CY_Code));
							break;
						}
					}
				}

				if (IsExcludedAffirmationCode(Parent.CY_Code))
				{
					Parent.CY_CodeInfo.AddMessageError(AffirmationCodeExcluded);
				}
			}

			if (Parent.USCAffirmationCode != null && Parent.USCAffirmationCode.UL_IsExpired)
			{
				Parent.CY_CodeInfo.AddMessageError(ExpiredAffirmationCode);
			}
		}
		internal const string InvalidAffirmationCode = "Please enter a valid affirmation code.";
		internal const string DuplicateAffirmationCode = "You have already entered affirmation code, '{0}'.";
		internal const string AffirmationCodeExcluded = "This affirmation code should not be entered here, it is determined from other data in the declaration.";
		internal const string ExpiredAffirmationCode = "The Affirmation of Compliance Code is no longer available for use, using this code will result in a reject by Customs.";

		bool IsExcludedAffirmationCode(string code)
		{
			bool excludeCode = false;

			switch (code)
			{
				case "SLN":
				case "SFN":
				case "SPN":
				case "SFX":
				case "SEM":
				case "SCN":
				case "SA1":
				case "SA2":
				case "SAC":
				case "SAS":
				case "SCZ":
				case "SCC":
				case "SFT":
				case "APA":
				case "ADA":
				case "ATA":
				case "APC":
				case "VFT":
				case "CAN":
				case "CCN":
				case "BOL":
				case "NHB":
				case "AWB":
				case "AWH":
				case "CNO":
				case "RNO":
				case "PFR":
				case "PFT":
				case "FME":
				case "SFR":
				case "CSH":
				case "OFT":
				case "PNC":
				case "PND":
				case "PVL":
				case "PVS":
				case "PVP":
				case "PVC":
				case "EFC":
				case "ENT":
				case "ETP":
				case "INB":
				case "INT":
				case "MOT":
				case "FIR":
				case "FTZ":
				case "SCA":
				case "HTS":
				case "IMN":
				case "IMM":
				case "IMF":
				case "IMP":
				case "IMX":
				case "IME":
				case "IMC":
				case "IM1":
				case "IM2":
				case "IMU":
				case "IMS":
				case "IMZ":
				case "IMA":
				case "CON":
				case "COM":
				case "COF":
				case "COP":
				case "COX":
				case "COE":
				case "COC":
				case "CO1":
				case "CO2":
				case "COU":
				case "COV":
				case "COZ":
				case "COA":
				case "TEM":
					excludeCode = true;
					break;
			}

			return excludeCode;
		}

		#endregion
	}
}
