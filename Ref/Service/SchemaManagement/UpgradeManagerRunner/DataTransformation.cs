namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public abstract class DataTransformation
	{
		protected DataTransformation(int version)
		{
			Version = version;
		}

		public int Version
		{
			get;
			private set;
		}
	}
}
