using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefCountryFilterBusinessObjectForTest : RefCountryFilterBusinessObject
	{
		public override CargoWise.EntityFramework.ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(RefCountrySchema.RN_Desc, CountryDescForTest);
				return result;
			}
		}

		internal static string CountryDescForTest = "F73FD2E174114735AA29447ADA766351";
	}
}
