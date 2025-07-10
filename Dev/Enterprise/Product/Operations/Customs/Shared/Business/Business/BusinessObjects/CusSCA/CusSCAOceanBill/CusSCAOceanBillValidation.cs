//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSCAOceanBillValidation
//
//    This class should be used for overriding validation in AutoCusSCAOceanBillValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusSCAOceanBillValidation : AutoCusSCAOceanBillValidation
	{
		public CusSCAOceanBillValidation(AutoCusSCAOceanBill parent) : base(parent)
		{
		}

		public new BaseCusSCAOceanBill Parent
		{
			get { return (BaseCusSCAOceanBill)base.Parent; }
		}

		protected override void CheckCB_GB()
		{
			base.CheckCB_GB();
			if (Parent.Branch == null)
			{
				Parent.CB_GBInfo.AddError(Res.GetString("830458b5-3cb9-4a5a-91aa-115fa4f2cc44", "Valid branch is required."));
			}
			else if (!Parent.IsCrossCompanyRecord && (Parent.Branch.Company == null || Parent.Branch.Company.PK != GlbCompany.CurrentCompany.PK))
			{
				Parent.CB_GBInfo.AddError(Res.GetString("5f70c18c-b14f-41a3-9db7-17f106ca6c78", "Please select a branch of {0}.", GlbCompany.CurrentCompany.GC_Name));
			}
		}
	}
}
