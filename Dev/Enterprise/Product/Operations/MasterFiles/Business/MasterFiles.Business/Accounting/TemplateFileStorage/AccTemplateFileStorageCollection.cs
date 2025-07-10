using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTemplateFileStorageCollection : DependentBusinessObjectCollection<AccTemplateFileStorage, GlbCompany>
	{
		public AccTemplateFileStorageCollection(GlbCompany master) : base(master)
		{
		}

		protected override bool AllowNewCore => false;
	}
}
