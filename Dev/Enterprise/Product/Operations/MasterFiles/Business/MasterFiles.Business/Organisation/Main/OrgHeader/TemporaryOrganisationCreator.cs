using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class TemporaryOrganisationCreator
	{
		/// <summary>
		/// Makes a new instance of the TemporaryOrganisation type
		/// </summary>
		/// <param name="factory">This should usually be a new factory to prevent saving the temp org causing the whole form to save</param>
		/// <param name="orgCollection">The organisation collection that the temporary org will reside in.</param>
		public static IOrgHeader GetNewTemporaryOrganisation(BusinessObjectFactory factory, IOrgHeaderCollection orgCollection)
		{
			var newOrg = factory.New<OrgHeader>();
			newOrg.SetDefaultValuesForTemporaryOrganisation();

			orgCollection.SetDefaultsForNewChild(newOrg);

			var validator = orgCollection as IValidateForController;
			if (validator != null)
			{
				newOrg.SetValidationFromController(validator);
			}

			return newOrg;
		}
	}
}
