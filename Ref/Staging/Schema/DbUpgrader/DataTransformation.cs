namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public abstract class DataTransformation
	{
		public int Version
		{
			get;
		}

		protected DataTransformation(int version)
		{
			Version = version;
		}
	}
}
