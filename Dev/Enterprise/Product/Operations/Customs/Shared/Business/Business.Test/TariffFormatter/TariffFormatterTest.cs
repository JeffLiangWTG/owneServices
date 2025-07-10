using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class TariffFormatterTest : TestCaseWithFactory
	{
		public virtual void TestFormat()
		{
			AssertEquals("123. 45 .67.890", Formatter.Format("123. 45 .67.890"));
		}

		public virtual void TestDisplayFormat()
		{
			AssertEquals("1234.56.78 90", Formatter.DisplayFormat("123. 45 .67.890"));
		}

		protected TariffFormatter Formatter
		{
			get { return GetNewTariffFormatter(); }
		}

		protected virtual TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}
	}
}
