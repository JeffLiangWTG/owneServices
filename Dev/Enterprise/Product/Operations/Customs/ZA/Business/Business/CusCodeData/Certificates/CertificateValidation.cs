using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class CertificateValidation : CusCodeDataValidation
	{
		public CertificateValidation(CertificateCusCodeData parent)
			: base(parent)
		{
		}

		protected abstract ZString[] ExpectedPermitTypes { get; }

		protected new CertificateCusCodeData Parent
		{
			get { return (CertificateCusCodeData)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			MandatoryValidation.CheckEntered(Parent.CY_CodeInfo);

			var targetInfo = Parent.CY_CodeInfo;
			var pk = Parent.PK;
			var permitNumber = Parent.CY_Code;
			var permitHeader = Parent.PermitHeader;
			var entryInstruction = Parent.EntryInstruction;

			var matchingPermits = new CusPermitHeader.Loader(Parent.Factory).LoadByNumber(Core.Constants.CountryCodes.SouthAfrica, permitNumber).OfType<CusPermitHeader>();
			if (!matchingPermits.Any())
			{
				targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PermitNotFound(permitNumber));
			}
			else
			{
				var expectedPermitHolder = entryInstruction.JobDeclaration?.Importer;
				if (expectedPermitHolder != null)
				{
					matchingPermits = matchingPermits.Where(x => x.CPH_OH_PermitHolder == expectedPermitHolder.PK);
				}
				if (expectedPermitHolder == null || !matchingPermits.Any())
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PermitHolderInvalid(permitNumber, expectedPermitHolder?.OH_Code ?? ZString.Empty));
				}
				else
				{
					matchingPermits = matchingPermits.Where(x => ExpectedPermitTypes.Contains(x.CPH_Type));
					if (!matchingPermits.Any())
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PermitTypeInvalid(permitNumber, ExpectedPermitTypes));
					}
					else
					{
						var expectedQtyValIndicator = Customs.Business.PermitQtyValIndicatorList.Codes.VAL;
						matchingPermits = matchingPermits.Where(x => x.CPH_QtyValIndicator == expectedQtyValIndicator);
						if (!matchingPermits.Any())
						{
							targetInfo.AddMessageError(ValidationConstants.EntryInstruction.PermitQtyValIndicatorInvalid(permitNumber, expectedQtyValIndicator));
						}
						else
						{
							var assessmentDate = entryInstruction.AssessmentDate;
							matchingPermits = matchingPermits.Where(x => x.CPH_StartDate < assessmentDate || x.CPH_EndDate < assessmentDate);
							if (!matchingPermits.Any())
							{
								targetInfo.AddMessageError(ValidationConstants.EntryInstruction.PermitValidityPeriodNotContainingAssessmentdate(permitNumber, assessmentDate));
							}
						}
					}
				}
			}

			if (Parent.RemainingValueExcludingThisDeclaration <= ZDecimal.Zero)
			{
				targetInfo.AddMessageError(ValidationConstants.EntryInstruction.PermitMustHavePositiveValueBalance(permitNumber));
			}

			if (Parent.GetCertificateCollection(Parent.EntryInstruction)?.OfType<CertificateCusCodeData>().Any(x => x.PK != pk && x.CY_Code == permitNumber) ?? false)
			{
				targetInfo.AddWarning(CertificateShouldNotBeSpecifiedMultipleTimes(permitNumber));
			}
		}

		public static string CertificateShouldNotBeSpecifiedMultipleTimes(ZString certificateNumber)
		{
			return Res.GetString("048772E8-6781-459D-93F2-75F511F50028", "Certificate {0} should not be specified multiple times.", certificateNumber);
		}

		protected override void CheckCY_Order()
		{
			base.CheckCY_Order();

			if ((Parent.GetCertificateCollection(Parent.EntryInstruction)?.Cast<CertificateCusCodeData>()?.Count(x => x.CY_Order == Parent.CY_Order) ?? 0) > 1)
			{
				Parent.CY_OrderInfo.AddMessageError(NoDuplicateOrderAllowed);
			}
		}

		public static string NoDuplicateOrderAllowed => Res.GetString("DEB7CA13-5BAF-4E1F-B256-DFDC8BFEA506", "A duplicate order cannot be specified");
	}
}
