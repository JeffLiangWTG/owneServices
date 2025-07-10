using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FDALotValidation : USFDALotAddInfoValidation
	{
		public FDALotValidation(AutoUSFDALotAddInfo parent) : base(parent)
		{
		}

		Lot Lot => Parent?.Parent as Lot;

		protected override void CheckUS_TemperatureQualifier()
		{
			base.CheckUS_TemperatureQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TemperatureQualifierInfo, Parent.Lookups.TemperatureQualifierList);

			ValidateUS_Temperature();
			ValidateUS_DegreeType();
			ValidateUS_LocationOfTemp();
		}

		bool IsACECargoReleaseOrStandalonePNValidationMode
		{
			get
			{
				var fda = Lot != null ? Lot.FDA : null;
				return fda != null && fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice;
			}
		}

		protected override void CheckUS_DegreeType()
		{
			base.CheckUS_DegreeType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DegreeTypeInfo, Parent.Lookups.DegreeTypeList);

			if (Parent.US_DegreeType.IsEmpty && TemperatureQualifierList.AreDetailsMandatory(Parent.US_TemperatureQualifier) && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				Parent.US_DegreeTypeInfo.AddMessageError(TempDetailsRequired);
			}

			ValidateUS_Temperature();
		}
		internal const string TempDetailsRequired = "If Temperature Qualifier is F, R or D then Degree Type, Actual Temperature and Location of Temperature Recording are required.";

		protected override void CheckUS_LocationOfTemp()
		{
			base.CheckUS_LocationOfTemp();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_LocationOfTempInfo, Parent.Lookups.LocationOfTempList);

			if (Parent.US_LocationOfTemp.IsEmpty && TemperatureQualifierList.AreDetailsMandatory(Parent.US_TemperatureQualifier) && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				Parent.US_LocationOfTempInfo.AddMessageError(TempDetailsRequired);
			}
		}

		protected override void CheckUS_Temperature()
		{
			base.CheckUS_Temperature();
			if (Parent.US_Temperature.IsEmpty && !Parent.US_TemperatureQualifier.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				var tempUnitDescription = Lot != null && !Lot.US_DegreeType.IsEmpty ? " " + Lot.AddInfoLookups.DegreeTypeList.GetDescriptionFromCode(Lot.US_DegreeType) : string.Empty;
				Parent.US_TemperatureInfo.AddWarning(ZString.Format(ZeroTemperatureWillBeSent, tempUnitDescription));
			}
		}
		internal const string ZeroTemperatureWillBeSent = "Temperature of 0.00 {0} will be sent, please confirm that if correct before sending.";

		protected override void CheckUS_LotNumber()
		{
			base.CheckUS_LotNumber();

			var fda = Lot != null ? Lot.FDA : null;

			if (Parent.US_LotNumber.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				if (fda.US_ProgramCode == FDAProgramCodeList.Codes.FOO)
				{
					CheckLotNumberForFood(fda.US_ProductCode);
				}
				else if (fda.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804)
				{
					Parent.US_LotNumberInfo.AddMessageError(ImportationProgram);
				}
			}
		}
		internal const string ImportationProgram = "For the Section 804 Importation Program, Lot Number is mandatory.";

		void CheckLotNumberForFood(ZString productCode)
		{
			var industryCode = productCode.SubstringSafe(0, 2);
			var pic = productCode.SubstringSafe(4, 1);

			var isLACFOrAcidified = (ZInt.ParseSafe(industryCode, 0).IsInRange(2, 39) ||
				industryCode == "41" || industryCode == "71" || industryCode == "72") &&
				(pic == "I" || pic == "F" || pic == "E");

			var productClass = productCode.SubstringSafe(2, 1);

			var isInfantFormula = industryCode == "40" && (productClass == "C" || productClass == "N" ||
				productClass == "O" || productClass == "P" || productClass == "R");

			if (isLACFOrAcidified || isInfantFormula)
			{
				Parent.US_LotNumberInfo.AddMessageError(LotNumberMandatoryForFood);
			}
		}
		internal const string LotNumberMandatoryForFood = "Lot Number is required for Food, reporting Infant formula, Acidified Foods, and LACF products.";
	}
}
