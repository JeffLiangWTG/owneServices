using CargoWise.RefDbRepo.UniversalXmlProcessor;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test
{
	public abstract class BaseUnitTestFixture
	{
		protected UnityContainer Container => container;

		UnityContainer container;

		[SetUp]
		public void Setup()
		{
			Application.ConfigEnvironment("test.txt");
			container = Application.UnityContainer;

			OnSetup();
		}

		protected virtual void OnSetup() { }
	}
}
