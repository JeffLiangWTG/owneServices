using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingJobDeclarationFilterBusinessObjectFactory : JobDeclarationFilterBusinessObjectFactory
	{
		public TrackingJobDeclarationFilterBusinessObjectFactory()
			: base()
		{ }

		public JobDeclarationFilterBusinessObject GetJobDeclarationFilterBusinessObject(ZString countryCode, OrgHeader loggedInOrganisation = null)
		{
			var filterStripBizo = (JobDeclarationFilterBusinessObject)System.Activator.CreateInstance(GetCountrySpecificType(countryCode));
			filterStripBizo.CountryCode = countryCode;
			filterStripBizo.LoggedInWebUsersOrg = loggedInOrganisation;

			return filterStripBizo;
		}
	}
}
