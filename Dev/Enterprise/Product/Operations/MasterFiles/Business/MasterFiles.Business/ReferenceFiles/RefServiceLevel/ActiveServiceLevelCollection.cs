using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveServiceLevelCollection : RefServiceLevelCollection
	{
		public ActiveServiceLevelCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery()) { }

		public ActiveServiceLevelCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			this.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Active Status", "Property", (ZString)(NoResString)"Active"));
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = new ZQuery(RefServiceLevelSchema.RS_IsActive, true);
			result.AddToFilter(base.CreateRelationshipFilter());
			return result;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			RefServiceLevel serviceLevel = (RefServiceLevel)selectedBusinessObject;
			if (!serviceLevel.RS_IsActive)
			{
				errors.Add(Res.GetString("1cb5754c-f4fc-4084-88ef-9d939ad34e7e", "This service level is inactive"));
			}
		}
	}
}
