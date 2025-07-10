using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.FrameworkExtensions.Functional;

namespace Enterprise.MasterFiles.Integration
{
	/// <summary>
	/// Variation of IGlbCompanyProvider which returns readonly ICompany objects.
	/// Prefer this type if you do not need to modify a company object.
	/// </summary>
	public interface ICompanyProvider
	{
		Option<ICompany> Get(ZGuid pk);

		/// <summary>
		/// Set an external Factory used to load ICompany objects.
		/// Must be set before Get() can be called.
		/// </summary>
		BusinessObjectFactory Factory { set; }
	}

	public static class ICompanyProviderExtensions
	{
		/// <summary>
		/// Set an external Factory used to load ICompany objects.
		/// Must be set before Get() can be called.
		/// </summary>
		public static ICompanyProvider WithFactory(this ICompanyProvider companyProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(companyProvider, nameof(companyProvider));

			companyProvider.Factory = factory;
			return companyProvider;
		}
	}
}
