using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement;

namespace Enterprise.Registry.Business
{
	public class ComplianceNumberSequenceCustomisationElementValidation
	{
		public ComplianceNumberSequenceCustomisationElementValidation(ComplianceNumberSequenceCustomisationElement parent)
		{
			Parent = parent;
		}

		readonly ComplianceNumberSequenceCustomisationElement Parent;

		public void ValidateOrder()
		{
			Parent.OrderInfo.ClearAllNotifications();
			if (Parent.Include)
			{
				if (Parent.Order == 0)
				{
					Parent.OrderInfo.AddError(Res.GetString("1f08eb41-3ca5-4c27-a3ef-6a9917735900", "Order must be greater than 0."));
				}
				else if (Parent.ParentCollection.Cast<ComplianceNumberSequenceCustomisationElement>().Any(x => x.PK != Parent.PK && x.Order == Parent.Order))
				{
					Parent.OrderInfo.AddError(Res.GetString("0b02a846-a9c4-428f-9829-4c5f8a037b98", "The Order already exists."));
				}
			}
		}

		public void ValidateInclude()
		{
			Parent.IncludeInfo.ClearAllNotifications();
			var currentFallbackLevel = Parent.ParentConfiguration?.CurrentFallbackLevel;
			if (Parent.Include && currentFallbackLevel != null)
			{
				if (Parent.IsTransactionRelatedOnly)
				{
					if (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.GetFallBackValueAtAllLevels(currentFallbackLevel.CompanyPK(false), currentFallbackLevel.BranchPK, currentFallbackLevel.DepartmentPK))
					{
						Parent.IncludeInfo.AddError(Res.GetString("f3879cee-06e6-456c-be6a-ec43a67742dc", "'Invoice Date' and 'Post Date' elements cannot be included when the Compliance Document Module is enabled."));
					}
				}
				else if (Parent.ElementName == ElementNames.TaxStatusCode && !DoseSupportTaxStausCode(currentFallbackLevel.CompanyPK(false)))
				{
					Parent.IncludeInfo.AddError(Res.GetString("78fabdb8-5587-44e4-8185-abd2d4480886", "This element is not applicable for your company."));
				}
			}
		}

		bool DoseSupportTaxStausCode(Guid companyPK)
		{
			var company = Parent.Factory.Load<GlbCompany>(companyPK);
			return CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(company.Country.Code)?.GetComplianceSubTypes()?.Any(x => !x.TaxStatusCode.IsEmpty) ?? false;
		}

		public void ValidateDigitCode()
		{
			Parent.DigitCodeInfo.ClearAllNotifications();
			if (Parent.Include)
			{
				if (Parent.IsYearElement)
				{
					var length = ZInt.ParseSafe(Parent.DigitCode, 0);
					if (length != 1 && length != 2 && length != 4)
					{
						Parent.DigitCodeInfo.AddError(Res.GetString("725678cb-597e-4964-9f8b-94c6b7aa6e0a", "The length of year should be either 1, 2 or 4."));
					}
				}
				else if (Parent.IsCodePairElement)
				{
					if ((Parent.ElementName == ElementNames.OriginalAmendmentStatus && Parent.AmendmentStatusCode.IsEmpty) ||
						(Parent.ElementName == ElementNames.TransactionType && Parent.AdjustmentNoteCode.IsEmpty))
					{
						Parent.DigitCodeInfo.AddError(Res.GetString("ac843b37-85b3-4e44-9f8f-7c00446d68ee", "Invalid code. Please enter exactly non-empty codes for {0} separated by '/'", Parent.ElementName));
					}
					else
					{
						if (Parent.ElementName == ElementNames.OriginalAmendmentStatus)
						{
							ValidateValueInCodePair(Parent.OriginalStatusCode);
							ValidateValueInCodePair(Parent.AmendmentStatusCode);
							CheckForSameValuesInCodePair();
						}
						else if (Parent.ElementName == ElementNames.TransactionType)
						{
							ValidateValueInCodePair(Parent.InvoiceCode);
							ValidateValueInCodePair(Parent.CreditNoteCode);
							ValidateValueInCodePair(Parent.AdjustmentNoteCode);
						}
					}
				}

				void ValidateValueInCodePair(ZString valueInCodePair)
				{
					if (!valueInCodePair.IsLettersAndNumbersOnlyOrEmpty)
					{
						Parent.DigitCodeInfo.AddError(Res.GetString("d2797546-8096-46c4-844e-49b838345506", "The one of the codes split by '/' is invalid. Please enter alphanumeric characters only."));
					}
				}

				void CheckForSameValuesInCodePair()
				{
					if (Parent.OriginalStatusCode.Equals(Parent.AmendmentStatusCode))
					{
						Parent.DigitCodeInfo.AddError(Res.GetString("edfeef7e-29a2-4d50-921f-f7eec20e6569", "The codes on either side of the '/' must be different values."));
					}
				}
			}
		}
	}
}
