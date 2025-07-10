using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Common
{
	public class DeduplicationExclusionManager<TBizo>
		where TBizo : BusinessObject, IDeduplicatable
	{
		readonly DeduplicationExclusionBuilder<TBizo> builder;

		public DeduplicationExclusionManager()
		{
			builder = new DeduplicationExclusionBuilder<TBizo>();
		}

		public DeduplicationExclusionBuilder<TBizo> Builder => builder;

		public void BuildDisplay()
		{
			ItemsCount = builder.Exclusion.Count;
			DisplayInfo = builder.Exclusion.BuildDisplay();
		}

		public int ItemsCount { get; set; }

		public string DisplayInfo { get; set; }
	}
}
