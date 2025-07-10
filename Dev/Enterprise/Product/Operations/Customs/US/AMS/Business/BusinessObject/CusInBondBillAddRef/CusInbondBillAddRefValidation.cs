using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInbondBillAddRefValidation : Customs.Business.CusInbondBillAddRefValidation
	{
		public CusInbondBillAddRefValidation(CusInbondBillAddRef parent)
			: base(parent)
		{
		}

		protected override void CheckBR_Qualifier()
		{
			base.CheckBR_Qualifier();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BR_QualifierInfo);
			}
			ValidateBR_ReferenceNum();
		}

		protected override void CheckBR_ReferenceNum()
		{
			base.CheckBR_ReferenceNum();
			if (IsInventoryRecordValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BR_ReferenceNumInfo);
				ACEOceanManifestIllegalCharacters.MessageErrorIfThereAreIllegalCharacters(Parent.BR_ReferenceNumInfo);
				var referenceNum = Parent.BR_ReferenceNum;
				if (!referenceNum.IsEmpty)
				{
					switch (Parent.BR_Qualifier)
					{
						case BillReferenceList.Codes.OB:
						case BillReferenceList.Codes.OL:
							if (referenceNum.Length < 5 || referenceNum.Length > 16)
							{
								Parent.BR_ReferenceNumInfo.AddMessageError(ValidationConstants.ShipmentReference.BillOfLadingFormat);
							}
							else
							{
								var scac = referenceNum.Left(4);
								var query = new ZQuery(USCarrierCombinedSchema.UI_Code, scac);
								var usCarrier = Parent.Factory.LoadTop1<USCarrierCombined>(query);
								if (usCarrier == null)
								{
									Parent.BR_ReferenceNumInfo.AddMessageError(ValidationConstants.ShipmentReference.SCACCode(scac));
								}
							}
							break;
						case BillReferenceList.Codes.CSK:
							ListValidation.MessageErrorIfInvalidCode(Parent.BR_ReferenceNumInfo, Parent.Lookups.ScheduleKList, ValidationConstants.ShipmentReference.CensusScheduleK);
							break;
						case BillReferenceList.Codes.ULC:
							ListValidation.MessageErrorIfInvalidCode(Parent.BR_ReferenceNumInfo, Parent.Lookups.UNLOCOCollection, ValidationConstants.ShipmentReference.UNLOCode);
							break;
						case BillReferenceList.Codes.FEN:
							if (!PedimentoNumberValidator.IsValidPedimentoNumber(referenceNum))
							{
								Parent.BR_ReferenceNumInfo.AddMessageError(PedimentoNumberValidator.PedimentoNumberRightFormat);
							}
							break;
					}
				}
			}
		}

		protected new CusInbondBillAddRef Parent
		{
			get { return (CusInbondBillAddRef)base.Parent; }
		}

		bool IsInventoryRecordValidationMode
		{
			get
			{
				var bill = Parent.Bill;
				return bill != null && bill.IsInventoryRecordValidationMode;
			}
		}
	}
}
