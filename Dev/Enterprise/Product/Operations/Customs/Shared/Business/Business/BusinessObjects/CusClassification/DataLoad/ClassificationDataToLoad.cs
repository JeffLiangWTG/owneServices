using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class ClassificationDataToLoad
	{
		public ClassificationDataToLoad()
		{
		}

		public ZString Code { get; set; }
		public ZString Type { get; set; }
		public ZString Description { get; set; }
		public ZString Tariff { get; set; }
	}
}
