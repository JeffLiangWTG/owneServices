//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCPSCAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSCPSCAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USCPSCAddInfoValidation : AutoUSCPSCAddInfoValidation
	{
		public USCPSCAddInfoValidation(AutoUSCPSCAddInfo parent) : base(parent)
		{
		}

		CPSCHeader Header
		{
			get { return (CPSCHeader)Parent.Parent; }
		}

		bool IsREF
		{
			get
			{
				var header = Header;
				return header != null && header.IsREF;
			}
		}

		bool IsNotREF
		{
			get
			{
				return !Header.US_ProcessingCode.IsEmpty && !IsREF;
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var header = Header;
				var invoiceLine = header == null ? null : header.InvoiceLine;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		protected override void CheckUS_ProcessingCode()
		{
			base.CheckUS_ProcessingCode();
			var header = Header;
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProcessingCodeInfo, header.AddInfoLookups.ProcessingCodeList);
			if (IsPGAValidation && header.InvoiceLine.US_CPSCDisclaimReason != PGADisclaimReasonList.Codes.A)
			{
				if (Parent.US_ProcessingCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProcessingCodeInfo);
				}
				else
				{
					if (header != null && IsNotREF && header.RuleAndLabs.Count == 0)
					{
						Parent.US_ProcessingCodeInfo.AddMessageError(AtLeastOneValidateThatThereAreRuleAndLabsShouldExist);
					}
				}
			}
		}
		internal const string AtLeastOneValidateThatThereAreRuleAndLabsShouldExist = "At least one Safety Test Labs and Rules is mandatory for every CPSC line.";

		protected override void CheckUS_IntendedUseDescription()
		{
			base.CheckUS_IntendedUseDescription();
			var header = Header;
			if (Parent.US_IntendedUseCode == header._980000)
			{
				var invoiceLine = header.InvoiceLine;
				if (invoiceLine != null && invoiceLine.US_CPSCDisclaimReason == PGADisclaimReasonList.Codes.A)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseDescriptionInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseDescriptionInfo);
				}
			}
		}

		protected override void CheckUS_ReferenceNumber()
		{
			base.CheckUS_ReferenceNumber();
			if (Header.IsREF && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ReferenceNumberInfo);
			}
		}

		protected override void CheckUS_ProductIDType()
		{
			base.CheckUS_ProductIDType();
			var header = Header;
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductIDTypeInfo, header.AddInfoLookups.ProductIDTypeCodeList);
			if (IsNotREF && IsPGAValidation && !header.HasItemIdentityNumber && Parent.US_SKUProductCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductIDTypeInfo);
			}
			ValidateUS_RegisteredNumber();
			ValidateUS_SerialNumber();
			ValidateUS_AltenateID();
			ValidateUS_ModelNumber();
			ValidateUS_SKUProductCode();
		}

		protected override void CheckUS_ProductID()
		{
			base.CheckUS_ProductID();
			if (IsNotREF && IsPGAValidation && !Parent.US_ProductIDType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductIDInfo);
			}
		}

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();
			var header = Header;
			ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, header.AddInfoLookups.IntendedUseCodeList);
			var invoiceLine = header.InvoiceLine;
			if (invoiceLine != null && invoiceLine.US_CPSCDisclaimReason == PGADisclaimReasonList.Codes.A)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseCodeInfo);
			}
		}

		protected override void CheckUS_ProductCode()
		{
			base.CheckUS_ProductCode();
			if (IsREF && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductCodeInfo);
			}
		}

		protected override void CheckUS_ProductCodeVersionNumber()
		{
			base.CheckUS_ProductCodeVersionNumber();
			if (IsREF && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductCodeVersionNumberInfo);
			}
		}

		protected override void CheckUS_SKUProductCode()
		{
			base.CheckUS_SKUProductCode();
			if (IsNotREF && IsPGAValidation && !Header.HasItemIdentityNumber && Parent.US_ProductIDType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SKUProductCodeInfo);
			}

			ValidateUS_RegisteredNumber();
			ValidateUS_SerialNumber();
			ValidateUS_AltenateID();
			ValidateUS_ModelNumber();
			ValidateUS_ProductIDType();
		}

		protected override void CheckUS_ProductName()
		{
			base.CheckUS_ProductName();
			if (IsNotREF && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductNameInfo);
			}
		}

		protected override void CheckUS_ModelNumber()
		{
			base.CheckUS_ModelNumber();
			CheckItemIdentityNumber(Parent.US_ModelNumberInfo);
			CheckMaxCountAndDuplicateValuesForNumbers(Parent.US_ModelNumberInfo, "Model Numbers");

			ValidateUS_RegisteredNumber();
			ValidateUS_SerialNumber();
			ValidateUS_AltenateID();
			ValidateUS_ProductIDType();
			ValidateUS_SKUProductCode();
		}

		protected override void CheckUS_RegisteredNumber()
		{
			base.CheckUS_RegisteredNumber();
			CheckItemIdentityNumber(Parent.US_RegisteredNumberInfo);
			CheckMaxCountAndDuplicateValuesForNumbers(Parent.US_RegisteredNumberInfo, "Registered Numbers");

			ValidateUS_ModelNumber();
			ValidateUS_SerialNumber();
			ValidateUS_AltenateID();
			ValidateUS_ProductIDType();
			ValidateUS_SKUProductCode();
		}

		protected override void CheckUS_SerialNumber()
		{
			base.CheckUS_SerialNumber();
			CheckItemIdentityNumber(Parent.US_SerialNumberInfo);
			CheckMaxCountAndDuplicateValuesForNumbers(Parent.US_SerialNumberInfo, "Serial Number");

			ValidateUS_RegisteredNumber();
			ValidateUS_ModelNumber();
			ValidateUS_AltenateID();
			ValidateUS_ProductIDType();
			ValidateUS_SKUProductCode();
		}

		protected override void CheckUS_AltenateID()
		{
			base.CheckUS_AltenateID();
			CheckItemIdentityNumber(Parent.US_AltenateIDInfo);
			CheckMaxCountAndDuplicateValuesForNumbers(Parent.US_AltenateIDInfo, "Alternate IDs");

			ValidateUS_RegisteredNumber();
			ValidateUS_ModelNumber();
			ValidateUS_SerialNumber();
			ValidateUS_ProductIDType();
			ValidateUS_SKUProductCode();
		}

		void CheckItemIdentityNumber(ZPropertyInfo info)
		{
			if (IsNotREF
				&& IsPGAValidation
				&& Parent.US_ModelNumber.IsEmpty
				&& Parent.US_SerialNumber.IsEmpty
				&& Parent.US_RegisteredNumber.IsEmpty
				&& Parent.US_AltenateID.IsEmpty
				&& Parent.US_ProductIDType.IsEmpty
				&& Parent.US_SKUProductCode.IsEmpty)
			{
				info.AddMessageError(AtLeastOneNumberRequired);
			}
		}

		void CheckMaxCountAndDuplicateValuesForNumbers(ZPropertyInfo info, ZString numberType)
		{
			var numbers = info.Value.ToString();
			if (!string.IsNullOrEmpty(numbers))
			{
				var numbersSplit = numbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (numbersSplit.Length > 5)
				{
					info.AddMessageError(ZString.Format(MaxCountForNumbers, numberType));
				}
				else
				{
					if (numbersSplit.Length != numbersSplit.Distinct().Count())
					{
						info.AddMessageError(ZString.Format(DuplicateValues, numberType));
					}
				}
			}
		}

		public const string AtLeastOneNumberRequired = "At least one 'Model Number','Serial Number','Registered Number' or 'Alternate ID' is required.";
		public const string MaxCountForNumbers = "You have entered more than 5 {0}. Up to five {0} are allowed.";
		public const string DuplicateValues = "You have entered duplicate {0}.";

		protected override void CheckUS_OA_ManufacturerAddress()
		{
			base.CheckUS_OA_ManufacturerAddress();
			if (IsNotREF && IsPGAValidation)
			{
				var header = Header;
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_ManufacturerAddressInfo);
				var manufacturerAddress = header?.ManufacturerAddress;
				if (manufacturerAddress != null)
				{
					OrganisationValidation.ValidatePGAContact(Parent.US_OA_ManufacturerAddressInfo, OrgHeaderWrapper.New(manufacturerAddress));
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ManufacturerAddressInfo, manufacturerAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_OA_ManufacturerAddressInfo, manufacturerAddress);
				}
			}
		}

		protected override void CheckUS_OA_CertifyingEntityAddress()
		{
			base.CheckUS_OA_CertifyingEntityAddress();
			if (IsNotREF && IsPGAValidation)
			{
				var header = Header;
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_CertifyingEntityAddressInfo);
				var certifyingEntityAddress = header?.CertifyingEntityAddress;
				if (certifyingEntityAddress != null)
				{
					OrganisationValidation.ValidatePGAContact(Parent.US_OA_CertifyingEntityAddressInfo, OrgHeaderWrapper.New(certifyingEntityAddress));
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_CertifyingEntityAddressInfo, certifyingEntityAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_OA_CertifyingEntityAddressInfo, certifyingEntityAddress);
				}
			}
		}

		protected override void CheckUS_OA_ContactPointAddress()
		{
			base.CheckUS_OA_ContactPointAddress();
			if (IsNotREF && IsPGAValidation)
			{
				var header = Header;
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_ContactPointAddressInfo);
				var contactPointAddress = header?.ContactPointAddress;
				if (contactPointAddress != null)
				{
					OrganisationValidation.ValidatePGAContact(Parent.US_OA_ContactPointAddressInfo, OrgHeaderWrapper.New(contactPointAddress));
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ContactPointAddressInfo, contactPointAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_OA_ContactPointAddressInfo, contactPointAddress);
				}
			}
		}

		protected override void CheckUS_CertificateExists()
		{
			base.CheckUS_CertificateExists();
			var header = Header;
			if (header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_CertificateExistsInfo, header.AddInfoLookups.YesNoList);
				if (IsNotREF && IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertificateExistsInfo);
				}
			}
		}

		protected override void CheckUS_ManufacturerMonthAndYear()
		{
			base.CheckUS_ManufacturerMonthAndYear();
			if (IsNotREF && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ManufacturerMonthAndYearInfo);

				if (!ZDateTime.TryParseExact(Parent.US_ManufacturerMonthAndYear, out var result, "MMyyyy"))
				{
					Parent.US_ManufacturerMonthAndYearInfo.AddMessageError(ManufacturerDateFormatIsIncorrect);
				}
			}
		}
		public const string ManufacturerDateFormatIsIncorrect = "Manufacturer Date format is incorrect. Format should be MMCCYY.";

		protected override void CheckUS_RuleCodes()
		{
			base.CheckUS_RuleCodes();
			if (IsNotREF && IsPGAValidation && Parent.US_NoLabTestingRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RuleCodesInfo);
				CheckMaxCountAndDuplicateValuesForNumbers(Parent.US_RuleCodesInfo, "Citation / Exemption Numbers");
			}
		}
	}
}
