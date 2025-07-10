using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	//
	// !!! This class is used in web service, which means that Env.CurrentCompany can be null. !!!
	//
	public class AccWithholding : AutoAccWithholding
	{
		public AccWithholding(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		protected bool AW_Code_ReadOnly => !GlbStaff.CurrentUser.IsSupportUser;

		protected bool AW_Rate_ReadOnly => !GlbStaff.CurrentUser.IsSupportUser;

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					fCompanies = new GlbCompanyCollection(Factory);
				}
				return fCompanies;
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("F9D00ECB-AB21-4183-89A0-43074D240392", "Withholding Tax ID - {0}", CalculateShortcutName());

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return false;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("9d7359ec-7d58-4c74-9751-447c7153556f", "You are not allowed to delete a withholding tax code. However, you can set it as inactive");
			}
		}

		#endregion

		#region Implementation

		protected GlbCompanyCollection fCompanies;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AW_GC = GlbCompany.CurrentCompany.PK;
		}
	}
}
