using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class TariffColumnStyleTest : TestCase
	{
		public void TestConstructor()
		{
			using (var tariffColumnStyle = new TariffColumnStyle(new TariffColumnStyleInfo()))
			{
				AssertType<TariffGridFindBox>(tariffColumnStyle.EditControl);
			}
		}
	}
}
