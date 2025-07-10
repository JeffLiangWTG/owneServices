using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCarrierConsortiumValidation : AutoRefCarrierConsortiumValidation
	{
		public RefCarrierConsortiumValidation(AutoRefCarrierConsortium parent) : base(parent)
		{
		}

		#region RG_Code

		protected override void CheckRG_Code()
		{
			base.CheckRG_Code();
			MandatoryValidation.CheckEntered(Parent.RG_CodeInfo);
		}

		#endregion

		#region RG_OH

		protected override void CheckRG_OH()
		{
			base.CheckRG_OH();
			if (Parent.RG_OH.IsEmpty)
			{
				Parent.RG_OHInfo.AddWarning(Res.GetString("5463a34d-6e17-4bdf-8ecf-53f5bc64f5c2", "If you want to enter buy or sell rates for this Consortium, you must enter an organization proxy to represent this consortium."));
			}
			else if (OrgUsedOnAnotherConsortium)
			{
				Parent.RG_OHInfo.AddError(Res.GetString("49f4f615-9ef3-4f89-80f8-7102c56f5254", "This organization has already been used as an Organization Proxy on another Vessel Consortium. Please choose a different organization."));
			}
		}

		bool OrgUsedOnAnotherConsortium
		{
			get
			{
				ZQuery filter = new ZQuery(RefCarrierConsortiumSchema.RG_OH, Parent.RG_OH);
				filter.AddToFilter(RefCarrierConsortiumSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				return Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(RefCarrierConsortium)), filter);
			}
		}

		#endregion
	}
}
