using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPasswordValidation))]
	public abstract class GlbExternalPasswordValidationTest<T, TValidation> : BusinessObjectValidationTestCase
			where T : GlbExternalPassword
			where TValidation : GlbExternalPasswordValidation
	{
		#region Implementation

		#region GlbExternalPassword

		protected T GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = CreateNewGlbExternalPassword();
				}
				return glbExternalPassword;
			}
		}
		T glbExternalPassword;

		protected virtual T CreateNewGlbExternalPassword()
		{
			var result = Factory.New<T>();
			result.GP_GS = Staff.PK;
			return result;
		}

		#endregion

		#region GlbStaff

		protected GlbStaff Staff
		{
			get
			{
				if (glbStaff == null)
				{
					glbStaff = Factory.New<GlbStaff>();
					glbStaff.GS_Code = "ZAC";
				}
				return glbStaff;
			}
		}
		GlbStaff glbStaff;

		#endregion
		#endregion
	}
}
