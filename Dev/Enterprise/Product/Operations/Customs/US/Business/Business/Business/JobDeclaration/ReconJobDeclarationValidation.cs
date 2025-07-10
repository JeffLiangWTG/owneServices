using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconJobDeclarationValidation : JobDeclarationValidation
	{
		public ReconJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public override bool IsMasterBillMandatory
		{
			get { return false; }
		}

		protected override bool JE_MergeByRequired
		{
			get { return false; }
		}

		protected override bool ShouldValidatePackagesActualPackageCount
		{
			get { return false; }
		}

		protected override void CheckJE_TotalWeight()
		{
		}

		protected override void CheckJE_TransportMode()
		{
		}

		protected override void CheckJE_MessageType()
		{
		}

		protected override void CheckJE_PrimaryITNumber()
		{
		}

		protected override void CheckJE_VoyageFlightNo()
		{
		}

		protected override void CheckJE_ContainerMode()
		{
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();

			if (Parent.JE_ApplicationCode != JobApplicationCodeList.Codes.ACE)
			{
				Parent.JE_ApplicationCodeInfo.AddMessageError(ApplicationCodeMessageText);
			}
		}

		public const string ApplicationCodeMessageText = "Message Mode must be ACE.";

		bool HasBeenLodgedInCustoms
		{
			get { return ReconciliationDeclaration.CanSendWithdrawal; }
		}

		ReconDeclaration ReconciliationDeclaration
		{
			get { return Parent.ReconDeclaration; }
		}

		protected internal override void CheckIOROrgPK(ZPropertyInfo info)
		{
			base.CheckIOROrgPK(info);

			if (Parent.IOROrgPK.IsEmpty)
			{
				info.AddMessageError("Importer Of Record is required.");
			}
			else
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(info, OrgMatchedCustomsRegNoType.EIN, string.Format(CultureInfo.InvariantCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Importer Of Record"), false, true);

				if (ReconciliationDeclaration.ImporterOfRecord != null)
				{
					var eIN = ReconciliationDeclaration.ImporterOfRecord.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber);
					var importerIDLodged = ReconciliationDeclaration.US_R_ImporterIDLodged;
					if (HasBeenLodgedInCustoms && eIN != importerIDLodged)
					{
						info.AddMessageError(string.Format(CultureInfo.InvariantCulture, R10DataLodged, importerIDLodged, "Importer ID"));
					}
				}

				new AuthorityToActValidator().Validate(Parent, Parent.IOR, info, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "", PowerOfAttorneyValidator.ExtraMatchingConditionForImportDirectionAndPortOfEntry(ZString.Empty), PowerOfAttorneyValidator.DirectionOrPortOfEntryNotMatchForReconAndDrawback);
			}
		}

		public const string R10DataLodged = "A reconciliation entry has already been added as '{0}' for a field, {1}. This field cannot be changed in the replacement message.";
	}
}
