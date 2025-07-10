using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DomesticValueSourceTest<T> : TestCaseWithFactory
			where T : DtbTransport
	{
		public void TestServiceLevel()
		{
			var transport = GetNewTransport();
			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new DomesticValueSource(transport));
			transport.KM_RS_NKServiceLevel = "D2D";

			AssertEquals("ServiceLevel", "D2D", valueProviders[Keys.ServiceLevel].GetValue(Generator, ""));
		}

		#region Implementation

		protected abstract T GetNewTransport();

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}

		NumberGenerator generator;

		#endregion
	}
}
