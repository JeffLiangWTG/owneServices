using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	public class NewManifestCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NewManifestCreator(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory)
		{
			this.companyPkToFilterOn = companyPkToFilterOn;
		}

		readonly ZGuid companyPkToFilterOn;

		public PotentialManifestCollection PotentialManifests => potentialManifests ?? (potentialManifests = new PotentialManifestCollection(Factory, companyPkToFilterOn));
		PotentialManifestCollection potentialManifests;

		public CusEntryHeader CreateManifestFrom(PotentialManifest manifest)
		{
			CusEntryHeader manifestEntryHeader = null;
			var manifestCreationFactory = new BusinessObjectFactory();
			var declarationsForManifest = manifest.GetDeclarationsForManifest(manifestCreationFactory, companyPkToFilterOn);
			declarationsForManifest.Sort(JobDeclaration.Schema.JE_DeclarationReference, System.ComponentModel.ListSortDirection.Ascending);
			if (declarationsForManifest.Count >= 1)
			{
				manifestEntryHeader = manifestCreationFactory.New<CusEntryHeader>();
				foreach (JobDeclaration declaration in declarationsForManifest)
				{
					manifestEntryHeader.Declarations.AddFromDatabase(declaration.PK);
				}
				manifestCreationFactory.Save();
			}
			return manifestEntryHeader;
		}
	}
}
