using System;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationHasSalesRelationFilterValidation : HasSalesRelationFilterValidation
	{
		public OrganisationHasSalesRelationFilterValidation(OrganisationHasSalesRelationFilter parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			ValidateTypeProperty();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}
	}
}
