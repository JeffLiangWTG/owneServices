using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class OrgsEvaluatedForCreditControlValidation : JobConfigurationSelectorValidation
	{
		public OrgsEvaluatedForCreditControlValidation(OrgsEvaluatedForCreditControl parent) : base(parent)
		{
		}

		protected new OrgsEvaluatedForCreditControl Parent
		{
			get { return (OrgsEvaluatedForCreditControl)base.Parent; }
		}

		#region JobType

		public override void ValidateJobType()
		{
			base.ValidateJobType();

			if (!Parent.JobTypeInfo.HasErrors())
			{
				ValidateOrganizationsType();
				ValidateINCOTerm();
				ValidateFreightPaymentTerm();
			}
		}

		protected override bool IsDuplicateJobParameter(IJobConfigurationSelector item)
		{
			var orgsEvaluatedForCreditControlItem = (OrgsEvaluatedForCreditControl)item;

			return base.IsDuplicateJobParameter(item)
				&& (orgsEvaluatedForCreditControlItem.INCOTerm == INCOTermCodes.All || orgsEvaluatedForCreditControlItem.INCOTerm == Parent.INCOTerm)
				&& (orgsEvaluatedForCreditControlItem.FreightPaymentTerm == FreightPaymentTermCodes.All || orgsEvaluatedForCreditControlItem.FreightPaymentTerm == Parent.FreightPaymentTerm)
				&& (orgsEvaluatedForCreditControlItem.OrganizationType == OrganisationTypeCodes.All || orgsEvaluatedForCreditControlItem.OrganizationType == Parent.OrganizationType);
		}

		protected override string DuplicateJobParametersError
		{
			get
			{
				return Res.GetString("10e43cff-95be-41b2-b056-30b9ab1c124a", "At least one more record already sets organization evaluation behavior for the same Job parameters.");
			}
		}

		#endregion

		#region Organizations Type

		public void ValidateOrganizationsType()
		{
			if (!Parent.OrganizationTypeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.OrganizationTypeInfo, Res.GetString("ce6016bc-7e3a-493e-974f-da6a49de3d56", "Organizations Type"));
				ListValidation.ErrorIfInvalidCode(Parent.OrganizationTypeInfo, Parent.OrganizationTypeList);
			}
		}

		#endregion

		#region INCO Term

		public void ValidateINCOTerm()
		{
			if (!Parent.INCOTermInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.INCOTermInfo, Res.GetString("5308d668-887c-5b82-430d-bce03797bab4", "Incoterm"));
				ListValidation.ErrorIfInvalidCode(Parent.INCOTermInfo, Parent.INCOTermList);
			}
		}

		#endregion

		#region Freight Payment Term

		public void ValidateFreightPaymentTerm()
		{
			if (!Parent.FreightPaymentTermInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.FreightPaymentTermInfo, Res.GetString("156caddc-4295-4ff7-94ab-deb9d553d754", "Freight Payment Term"));
				ListValidation.ErrorIfInvalidCode(Parent.FreightPaymentTermInfo, Parent.FreightPaymentTermList);
			}
		}

		#endregion
	}
}