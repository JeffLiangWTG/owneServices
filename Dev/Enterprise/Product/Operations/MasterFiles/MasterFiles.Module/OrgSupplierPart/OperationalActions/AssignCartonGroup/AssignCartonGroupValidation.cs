using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module
{
	public class AssignCartonGroupValidation : ZValidation
	{
		public AssignCartonGroupValidation(BusinessObject parent)
			: base(parent)
		{
		}

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(AssignCartonGroupValidation); }
		}

		#endregion

		#region ValidateCartonGroupPK

		public void ValidateCartonGroupPK()
		{
			ValidateCalculatedProperty(Applicator.CartonGroupPKInfo);
		}

		protected void CheckCartonGroupPK()
		{
			ListValidation.ErrorIfInvalidPK(Applicator.CartonGroupPKInfo);
		}

		#endregion

		#region ValidateOrganisationPK

		public void ValidateOrganisationPK()
		{
			ValidateCalculatedProperty(Applicator.OrganisationPKInfo);
		}

		protected void CheckOrganisationPK()
		{
			MandatoryValidation.CheckEntered(Applicator.OrganisationPKInfo);
			ListValidation.ErrorIfInvalidPK(Applicator.OrganisationPKInfo);
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateCartonGroupPK();
			ValidateOrganisationPK();
		}

		#endregion

		#region Parent

		AssignCartonGroupMethodApplicator Applicator
		{
			get { return (AssignCartonGroupMethodApplicator)base.ParentFilter; }
		}

		#endregion
	}
}
