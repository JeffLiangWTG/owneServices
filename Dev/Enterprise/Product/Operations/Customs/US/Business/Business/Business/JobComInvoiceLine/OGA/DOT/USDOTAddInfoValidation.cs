//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDOTAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDOTAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USDOTAddInfoValidation : AutoUSDOTAddInfoValidation
	{
		public USDOTAddInfoValidation(AutoUSDOTAddInfo parent)
			: base(parent)
		{
			if (!(parent is DOTAddInfo))
			{
				throw new ArgumentException("Parent should be DOTAddInfo");
			}
		}

		DOT DOT
		{
			get { return ((DOTAddInfo)Parent).Parent; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return DOT.InvoiceLine; }
		}

		bool IsACECargoCertificationMode
		{
			get { return InvoiceLine != null && InvoiceLine.IsACECargoCertificationMode; }
		}

		void ValidateDOTVINs()
		{
			foreach (DOTVIN dotvin in DOT.DOTVINs)
			{
				dotvin.AddInfoValidation.ValidateAll();
			}
		}

		bool FromInvoiceLine
		{
			get { return DOT != null && DOT.InvoiceLine != null; }
		}

		protected override void CheckUS_DOTCommercialDesc()
		{
			base.CheckUS_DOTCommercialDesc();

			if (Parent.US_DOTCommercialDesc.IsEmpty && FromInvoiceLine && !IsACECargoCertificationMode)
			{
				Parent.US_DOTCommercialDescInfo.AddMessageError("Commercial Description is mandatory.");
			}
		}

		protected override void CheckUS_DOTBoxNo()
		{
			base.CheckUS_DOTBoxNo();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DOTBoxNoInfo, Parent.Lookups.BoxNumbers);

			if (Parent.US_DOTBoxNo.IsEmpty && FromInvoiceLine && !IsACECargoCertificationMode)
			{
				Parent.US_DOTBoxNoInfo.AddMessageError("Box Number is required.");
			}

			ValidateAll();
			ValidateDOTVINs();
		}

		protected override void CheckUS_DOTPassport()
		{
			base.CheckUS_DOTPassport();
			if (Parent.US_DOTPassport.IsEmpty && FromInvoiceLine && !IsACECargoCertificationMode)
			{
				if (Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._05)
				{
					Parent.US_DOTPassportInfo.AddMessageError("Passport number is required for box 05.");
				}
			}
		}

		protected override void CheckUS_DOTCountryOfOrigin()
		{
			base.CheckUS_DOTCountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DOTCountryOfOriginInfo, Parent.Lookups.USCountries);

			if (!IsACECargoCertificationMode)
			{
				if (Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._05 || Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._06 || Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._12)
				{
					if (Parent.US_DOTCountryOfOrigin.IsEmpty && FromInvoiceLine)
					{
						Parent.US_DOTCountryOfOriginInfo.AddMessageError("Country of Origin is required for box 05, 06, 12.");
					}
				}
				else
				{
					if (!Parent.US_DOTCountryOfOrigin.IsEmpty)
					{
						Parent.US_DOTCountryOfOriginInfo.AddMessageError("Country of Origin is not required when Box Number is other than 05, 06, 12.");
					}
				}
			}
		}

		protected override void CheckUS_DOTBondSuretyCode()
		{
			base.CheckUS_DOTBondSuretyCode();

			if (!IsACECargoCertificationMode)
			{
				if (Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._03)
				{
					if (Parent.US_DOTBondSuretyCode.IsEmpty && FromInvoiceLine)
					{
						Parent.US_DOTBondSuretyCodeInfo.AddMessageError("Surety Code is required when Box Number is 03.");
					}
					else
					{
						ZString messageError = SuretyCodeValidator.Validate(Parent.US_DOTBondSuretyCode);
						if (!messageError.IsEmpty)
						{
							Parent.US_DOTBondSuretyCodeInfo.AddMessageError(messageError);
						}
					}
				}
				else
				{
					if (!Parent.US_DOTBondSuretyCode.IsEmpty)
					{
						Parent.US_DOTBondSuretyCodeInfo.AddMessageError("Surety Code is not required when Box Number is other than 03.");
					}
				}
			}
		}

		protected override void CheckUS_DOTPriorApproval()
		{
			base.CheckUS_DOTPriorApproval();

			if (!IsACECargoCertificationMode)
			{
				if (Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._2B
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._06
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._07
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._12)
				{
					if (!Parent.US_DOTPriorApproval && FromInvoiceLine)
					{
						Parent.US_DOTPriorApprovalInfo.AddMessageError("Prior approval is required for box 2B, 06, 07, 12.");
					}
				}
				else
				{
					if (Parent.US_DOTPriorApproval)
					{
						Parent.US_DOTPriorApprovalInfo.AddMessageError("Prior approval is NOT required for the selected box number.");
					}
				}
			}
		}

		protected override void CheckUS_DOTImpSubstStatement()
		{
			base.CheckUS_DOTImpSubstStatement();

			if (!IsACECargoCertificationMode)
			{
				if (Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._2B
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._03
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._07
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._08
					|| Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._09)
				{
					if (!Parent.US_DOTImpSubstStatement && FromInvoiceLine)
					{
						Parent.US_DOTImpSubstStatementInfo.AddMessageError("A substantiating statement, copy of contract or manufacturers confirmation letter is required when box number is 2B, 03, 07, 08 or 09");
					}
				}
				else
				{
					if (Parent.US_DOTImpSubstStatement)
					{
						Parent.US_DOTImpSubstStatementInfo.AddMessageError("A substantiating statement, copy of contract or manufacturers confirmation letter is NOT required for the selected box number.");
					}
				}
			}
		}

		protected override void CheckUS_DOTClarCode()
		{
			base.CheckUS_DOTClarCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DOTClarCodeInfo, Parent.Lookups.ClarificationCodes);

			if (Parent.US_DOTClarCode != ClarificationCodeList.Codes.Vehicle && Parent.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._12 && !IsACECargoCertificationMode)
			{
				Parent.US_DOTClarCodeInfo.AddMessageError("If Box Number is 12, then Clarification Code should be 'V(ehicle'");
			}

			ValidateDOTVINs();
		}

		protected override void CheckUS_DOTTireID()
		{
			base.CheckUS_DOTTireID();

			if (!IsACECargoCertificationMode)
			{
				if (Parent.US_DOTTireID.IsEmpty)
				{
					if (Parent.US_DOTClarCode == ClarificationCodeList.Codes.Tire && FromInvoiceLine)
					{
						Parent.US_DOTTireIDInfo.AddWarning("Manufacture ID of three alpha-numeric characters is required when clarification code of T is selected.");
					}
				}
				else
				{
					if (Parent.US_DOTClarCode == ClarificationCodeList.Codes.Tire)
					{
						if (!Regex.IsMatch(Parent.US_DOTTireID, "^[A-Z0-9]{3}$"))
						{
							Parent.US_DOTTireIDInfo.AddMessageError("Tire ID should be three alpha-numeric characters.");
						}
					}
					else
					{
						Parent.US_DOTTireIDInfo.AddWarning("Tire ID should not be specified unless a clarification code of T is selected.");
					}
				}
			}
		}

		protected override void CheckUS_DOTTireBrandName()
		{
			base.CheckUS_DOTTireBrandName();

			if (!IsACECargoCertificationMode)
			{
				if (Parent.US_DOTTireBrandName.IsEmpty)
				{
					if (!Parent.US_DOTTireID.IsEmpty && FromInvoiceLine)
					{
						Parent.US_DOTTireBrandNameInfo.AddWarning("Tire Manufacturer Brand Name (containing only letters and numbers) is required when Tire ID is entered.");
					}
				}
				else
				{
					if (Parent.US_DOTTireID.IsEmpty)
					{
						Parent.US_DOTTireBrandNameInfo.AddWarning("Tire Manufacturer Brand Name is not required unless Tire ID is entered.");
					}
					else
					{
						if (!Regex.IsMatch(Parent.US_DOTTireBrandName, @"^[\w ]+$"))
						{
							Parent.US_DOTTireBrandNameInfo.AddMessageError("Tire Manufacturer Brand Name should contain only letters and numbers.");
						}
					}
				}
			}
		}
	}
}
