//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFSISLotAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSFSISLotAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USFSISLotAddInfoValidation : AutoUSFSISLotAddInfoValidation
	{
		public USFSISLotAddInfoValidation(AutoUSFSISLotAddInfo parent)
			: base(parent)
		{
		}

		new USFSISLotAddInfo Parent
		{
			get { return (USFSISLotAddInfo)base.Parent; }
		}

		USInvoiceLineFSISLine Line
		{
			get { return Lot.Parent; }
		}

		USFSISLot Lot
		{
			get { return Parent.Parent; }
		}

		bool IsElectronicallyCertificated
		{
			get { return Line.IsElectronicallyCertificated; }
		}

		protected override void CheckUS_SourceEstNo()
		{
			base.CheckUS_SourceEstNo();
			USFSISLineAddInfoValidationHelper.CheckEstablishNumbers(Parent.US_SourceEstNoInfo, Parent.US_SourceEstNo);
		}

		protected override void CheckUS_UQ1()
		{
			base.CheckUS_UQ1();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ1Info);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ1Info, Parent.Lookups.PackingTypes);
			}
		}

		protected override void CheckUS_UQ2()
		{
			base.CheckUS_UQ2();
			if (!IsElectronicallyCertificated)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ2Info, Parent.Lookups.PackingTypes);
				if (!Parent.US_NoOfUnit2.IsEmpty && Parent.US_UQ2.IsEmpty)
				{
					Parent.US_UQ2Info.AddMessageError(string.Format(InnermostUQIsMissing, "UQ", "Innermost Package"));
				}
			}
		}
		internal const string InnermostUQIsMissing = "{0} is required when {1} has a value.";

		protected override void CheckUS_NoOfUnit2()
		{
			base.CheckUS_NoOfUnit2();
			if (!IsElectronicallyCertificated)
			{
				if (!Parent.US_UQ2.IsEmpty && Parent.US_NoOfUnit2.IsEmpty)
				{
					Parent.US_NoOfUnit2Info.AddMessageError(string.Format(InnermostUQIsMissing, "Innermost Package", "UQ"));
				}
			}
		}

		protected override void CheckUS_WeightUQ()
		{
			base.CheckUS_WeightUQ();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_WeightUQInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_WeightUQInfo, Parent.Lookups.WeightUQList);
			}
		}

		protected override void CheckUS_LotNumber()
		{
			base.CheckUS_LotNumber();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LotNumberInfo);
				var isLotNoRepeated = Line.Lots.Cast<USFSISLot>().Any(x => x.US_LotNumber == Lot.US_LotNumber && x.PK != Lot.PK);
				if (isLotNoRepeated)
				{
					Parent.US_LotNumberInfo.AddMessageError(RepeatedLotNoMessage);
				}
				if (!Lot.US_LotNumber.Trim().IsNumbersOnlyOrEmpty || Lot.US_LotNumber.Length > 3)
				{
					Parent.US_LotNumberInfo.AddMessageError(OnlyNumberAndThreeDigsLotNoMessage);
				}
			}
		}

		internal const string RepeatedLotNoMessage = "You have already entered this lot number.";
		internal const string OnlyNumberAndThreeDigsLotNoMessage = "Lot number may only contain up to three digits";

		protected override void CheckUS_NoOfUnit1()
		{
			base.CheckUS_NoOfUnit1();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NoOfUnit1Info);
			}
		}

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightInfo);
			}
		}

		protected override void CheckUS_ShippingMarks()
		{
			base.CheckUS_ShippingMarks();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ShippingMarksInfo);
			}
		}

		protected override void CheckUS_StartDate()
		{
			base.CheckUS_StartDate();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_StartDateInfo);
			}
		}

		protected override void CheckUS_EndDate()
		{
			base.CheckUS_EndDate();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EndDateInfo);
				if (Parent.US_EndDate < Parent.US_StartDate)
				{
					Parent.US_EndDateInfo.AddMessageError(EndDateBeforeStartDate);
				}
			}
		}
		internal const string EndDateBeforeStartDate = "Production End Date must be after Production Start Date.";

		protected override void CheckUS_Species()
		{
			base.CheckUS_Species();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SpeciesInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_SpeciesInfo, Parent.Lookups.ProductSpeciesNames);
			}
		}

		protected override void CheckUS_ProductQualifierCode()
		{
			base.CheckUS_ProductQualifierCode();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductQualifierCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductQualifierCodeInfo, Parent.Lookups.ProductQualifierCodes);
			}
		}

		protected override void CheckUS_ProductCharacteristicQualifier()
		{
			base.CheckUS_ProductCharacteristicQualifier();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductCharacteristicQualifierInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductCharacteristicQualifierInfo, Parent.Lookups.ProductCharacteristics);
			}
		}

		protected override void CheckUS_ProducingEstNo()
		{
			base.CheckUS_ProducingEstNo();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProducingEstNoInfo);
			}
			USFSISLineAddInfoValidationHelper.CheckEstablishNumbers(Parent.US_ProducingEstNoInfo, Parent.US_ProducingEstNo);
		}

		protected override void CheckUS_SourceCountry()
		{
			base.CheckUS_SourceCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SourceCountryInfo, Parent.Lookups.USCountryList);
		}
	}
}
