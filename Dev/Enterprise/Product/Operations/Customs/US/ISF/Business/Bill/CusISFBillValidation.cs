using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFBillValidation : AutoCusISFBillValidation
	{
		public CusISFBillValidation(AutoCusISFBill parent)
			: base(parent)
		{
		}

		protected new CusISFBill Parent
		{
			get { return (CusISFBill)base.Parent; }
		}

		protected override void CheckBB_BillType()
		{
			base.CheckBB_BillType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BB_BillTypeInfo);
			CheckTypeThereIsOnlyOneInstanceFor(Parent.BB_BillType);
			if (Parent.IsCarnetIssuingCountryCodeAndCarnetNumber && Parent.Header != null)
			{
				Parent.Header.Validation.ValidateBF_ShipmentType();
			}
			ValidateBB_BillNum();
		}

		protected override void CheckBB_BillNum()
		{
			base.CheckBB_BillNum();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BB_BillNumInfo);
			if (Parent.IsFullNameOfISFImporter)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.BB_BillNumInfo);
			}
			else
			{
				CharacterValidation.Validate(Parent.BB_BillNumInfo, CharacterValidation.Type.AlphaNumeric);
			}
			BillDuplicationValidator.ValidateBillIsNotDuplicated(Parent, Parent.BB_BillNumInfo);
			switch (Parent.BB_BillType)
			{
				case BillTypeList.Codes.USCBPEntryNumber:
					ValidateCBPEntryNumber();
					break;
				case BillTypeList.Codes.HouseBillOfLading:
				case BillTypeList.Codes.MasterBillOfLading:
				case BillTypeList.Codes.OceanBillOfLading:
					BillNumberFormatValidation.ValidateLength(Parent.BB_BillNumInfo, Parent.BB_BillType);
					break;
				case BillTypeList.Codes.SuretyCode:
					ValidateSuretyCode();
					break;
				case BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber:
					ValidateCarnetIssuingCountryCodeAndCarnetNumber();
					break;
			}
		}

		void ValidateCarnetIssuingCountryCodeAndCarnetNumber()
		{
			if (!Parent.BB_BillNum.IsEmpty)
			{
				if (Parent.BB_BillNum.Length < 3 || Parent.BB_BillNum.KeepAlphanumericCharacters() != Parent.BB_BillNum)
				{
					Parent.BB_BillNumInfo.AddMessageError(ValidationConstants.Bill.CarnetReferenceRightFormat);
				}
				else if (Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Parent.BB_BillNum.Left(2)) == null)
				{
					Parent.BB_BillNumInfo.AddMessageError(ValidationConstants.Bill.CarnetReferenceInvalidCountryCode);
				}
			}
		}

		void ValidateSuretyCode()
		{
			if (!Parent.BB_BillNum.IsEmpty)
			{
				if (Parent.BB_BillNum.Length != 3 || Parent.BB_BillNum.KeepNumericCharacters() != Parent.BB_BillNum)
				{
					Parent.BB_BillNumInfo.AddMessageError(ValidationConstants.Bill.SuretyCodeRightFormat);
				}
			}
		}

		void ValidateCBPEntryNumber()
		{
			if (!Regex.IsMatch(Parent.BB_BillNum, @"^[A-Z0-9]{3}[0-9]{8}$", RegexOptions.IgnoreCase))
			{
				Parent.BB_BillNumInfo.AddMessageError(ValidationConstants.Bill.CBPEntryNumberRightFormat);
			}
		}

		void CheckTypeThereIsOnlyOneInstanceFor(ZString billType)
		{
			ZString messageError = ZString.Empty;
			switch (billType)
			{
				case BillTypeList.Codes.SuretyCode:
					messageError = ValidationConstants.Bill.OnlyOneSuretyCode;
					break;
				case BillTypeList.Codes.BondReferenceNumber:
					messageError = ValidationConstants.Bill.OnlyOneBondReferenceNumber;
					break;
				case BillTypeList.Codes.FullNameOfISFImporter:
					messageError = ValidationConstants.Bill.OnlyOneFullNameOfISFImporter;
					break;
			}
			if (!messageError.IsEmpty)
			{
				CusISFHeader header = Parent.Header;
				if (header != null)
				{
					foreach (CusISFBill otherBill in header.ReferenceDatas)
					{
						if (otherBill != Parent && otherBill.BB_BillType == billType)
						{
							Parent.BB_BillTypeInfo.AddMessageError(messageError);
						}
					}
				}
			}
		}
	}
}
