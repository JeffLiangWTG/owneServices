using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.Common.GUI.Testing;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class NZCClassColumnStyleTest : TariffColumnStyleTest
	{
		public override TariffColumnStyleInfo GetNewTariffColumnStyleInfo()
		{
			return new NZCClassColumnStyleInfo();
		}
	}
}
