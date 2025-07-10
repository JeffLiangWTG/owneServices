namespace Enterprise.Rating.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;

	public class RatingEmailDefWithGroup : RatingEmailDef
	{
		internal RatingEmailDefWithGroup(GuidRegistryItem groupRegistry)
		{
			this.groupRegistry = groupRegistry;
			IsActive = true;
		}

		protected override bool SendCore(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				Env.OutgoingMailManager.CreateAndSave(this, groupRegistry.Value, GroupSourceLocator.GetFromRegistryItem(groupRegistry));
			}
			else
			{
				Env.OutgoingMailManager.Create(factory, this, groupRegistry.Value, GroupSourceLocator.GetFromRegistryItem(groupRegistry));
			}

			return true;
		}

		readonly GuidRegistryItem groupRegistry;
	}
}
