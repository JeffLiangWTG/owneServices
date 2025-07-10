//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPOSConfigurationViewValidation
//
//    This class should be used for overriding validation in AutoAccPOSConfigurationViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class AccPOSConfigurationViewValidation : AutoAccPOSConfigurationViewValidation
	{
		public AccPOSConfigurationViewValidation(AutoAccPOSConfigurationView parent) : base(parent)
		{
		}

		new AccPOSConfiguration Parent => (AccPOSConfiguration)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePSC_ChargeType();
		}

		public void ValidatePSC_ChargeType() => ValidateCalculatedProperty(Parent.PSC_ChargeTypeInfo);

		protected void CheckPSC_ChargeType()
		{
			MandatoryValidation.CheckEntered(Parent.PSC_ChargeTypeInfo);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.PSC_ChargeTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PSC_ChargeTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_LedgerIsNotEmpty()
		{
			// Empty PSC_Ledger = "ALL" PSC_ChargeType
		}

		protected override void CheckPSC_ParentTableCodeIsNotEmpty()
		{
			// PSC_ParentTableCode may be empty.
		}

		protected override void CheckPSC_JobType()
		{
			base.CheckPSC_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_JobTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_IncoTerm()
		{
			base.CheckPSC_IncoTerm();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_IncoTermInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_IncoTermIsNotEmpty()
		{
			// PSC_IncoTerm may be empty
		}

		protected override void CheckPSC_ServiceDirection()
		{
			base.CheckPSC_ServiceDirection();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_ServiceDirectionInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_TransportMode()
		{
			base.CheckPSC_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_TransportModeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_TaxRegistrationType()
		{
			base.CheckPSC_TaxRegistrationType();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_TaxRegistrationTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_TaxRegistrationTypeIsNotEmpty()
		{
			// Tax Registration Type may be empty.
		}

		protected override void CheckPSC_NK_Branch()
		{
			base.CheckPSC_NK_Branch();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_NK_BranchInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_NK_BranchIsNotEmpty()
		{
			// Branch may be empty.
		}

		protected override void CheckPSC_SupplyType()
		{
			base.CheckPSC_SupplyType();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_SupplyTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckPSC_SupplyTypeIsNotEmpty()
		{
			// PSC_SupplyType may be empty
		}

		protected override void CheckPSC_PlaceOfSupplyRule()
		{
			base.CheckPSC_PlaceOfSupplyRule();
			ListValidation.ErrorIfInvalidCode(Parent.PSC_PlaceOfSupplyRuleInfo);
		}

		protected void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (AccPOSConfigurationCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccPOSConfigurationCollection);
			if (parentCollection == null)
			{
				return;
			}

			if (parentCollection.Cast<AccPOSConfiguration>().Any(c => Parent.IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		internal static string IsDuplicateErrorString => Res.GetString("17b984e3-b365-4398-9948-9e66fdad25c7", "Another record already sets POS Configuration for the same Job parameters.");
	}
}
