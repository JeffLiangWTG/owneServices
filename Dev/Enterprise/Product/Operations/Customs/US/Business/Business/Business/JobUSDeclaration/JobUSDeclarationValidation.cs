//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobUSDeclarationValidation
//
//    This class should be used for overriding validation in AutoJobUSDeclarationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class JobUSDeclarationValidation : AutoJobUSDeclarationValidation
	{
		public JobUSDeclarationValidation(AutoJobUSDeclaration parent)
			: base(parent)
		{
		}

		protected new JobUSDeclaration Parent => base.Parent as JobUSDeclaration;

		protected override void CheckUSD_RetailSalesSubstitutionIndicator()
		{
			base.CheckUSD_RetailSalesSubstitutionIndicator();
			var declaration = Parent.Declaration;
			if (Parent.USD_RetailSalesSubstitutionIndicator && declaration != null && declaration.US_EntryType != ACEDrawbackProvisionsList.Codes._56 && declaration.US_EntryType != ACEDrawbackProvisionsList.Codes._70)
			{
				Parent.USD_RetailSalesSubstitutionIndicatorInfo.AddMessageError(RetailSalesSubstitutionShouldTickOff);
			}
		}
		internal const string RetailSalesSubstitutionShouldTickOff = "Retail Sales Substitution should be ticked on only when Drawback Provision is 56 or 70.";

		protected override void CheckUSD_OH_ForeignPrincipalParty()
		{
			if (Parent.Declaration is JobDeclaration declaration && declaration.IsExport)
			{
				var fPPI = declaration.FPPI;
				var fPPIAddress = fPPI?.MainAddress;
				var routedInvoices = declaration.Invoices.Cast<JobComInvoiceHeader>().Where(header => header.US_IsRoutedTransaction).ToArray();
				if (routedInvoices.Length > 0)
				{
					var poaValidator = new PowerOfAttorneyValidator(POARequiredForRouted);
					if (fPPIAddress != null)
					{
						poaValidator.Validate(declaration, fPPI, declaration.USD_OH_ForeignPrincipalPartyInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding, PowerOfAttorneyValidator.ExtraMatchingConditionForExportDirection());
					}
					else
					{
						declaration.USD_OH_ForeignPrincipalPartyInfo.AddMessageError(FPPIRequired);
					}
				}
				else if (!declaration.USD_OH_ForeignPrincipalParty.IsEmpty)
				{
					declaration.USD_OH_ForeignPrincipalPartyInfo.AddMessageError(FPPINotRequired);
				}

				var fPPICountryCode = fPPIAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
				var supplierCountryCode = declaration.Supplier?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
				if (!fPPICountryCode.IsEmpty && fPPICountryCode == supplierCountryCode)
				{
					AESAddressValidator.ValidateUSAddressForUSRoutedTransaction(declaration.US_RoutedTransaction, declaration.USD_OH_ForeignPrincipalPartyInfo, fPPICountryCode);
				}
			}
		}

		internal const string POARequiredForRouted = "There is no Power of Attorney recorded for this Foreign Principal Party in Interest (FPPI).\r\nIf the transaction is marked as 'Routed', there must be a POA registered against the Foreign Principal Party in Interest (FPPI).\r\nPlease place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.";
		internal const string FPPIRequired = "There is transaction marked as 'Routed', FPPI is required to be entered.";
		internal const string FPPINotRequired = "There is no transaction marked as 'Routed', FPPI is not required to be entered.";
	}
}
