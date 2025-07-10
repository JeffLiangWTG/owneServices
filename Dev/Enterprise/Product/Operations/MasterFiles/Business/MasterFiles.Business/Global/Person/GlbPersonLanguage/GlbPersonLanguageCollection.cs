using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonLanguageCollection : ActiveBusinessObjectCollection<GlbPersonLanguage>
	{
		public GlbPersonLanguageCollection(GlbPerson person)
			: base(person.Factory, person, new ZQuery(GlbPersonLanguageSchema.G7_PER_Person, person.PK), GlbPersonLanguageSchema.G7_PER_Person)
		{
		}

		public GlbPersonLanguageCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(GlbPersonLanguageSchema.G7_PER_Person, Guid.Empty))
		{
		}
	}
}
