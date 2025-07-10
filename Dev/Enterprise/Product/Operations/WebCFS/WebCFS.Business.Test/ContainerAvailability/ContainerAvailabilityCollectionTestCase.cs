using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business.Testing
{
	[TestedType(typeof(ContainerAvailabilityCollection))]
	public class ContainerAvailabilityCollectionTestCase : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected ContainerAvailabilityCollection TestCollection
		{
			get { return (ContainerAvailabilityCollection)Collection; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ContainerAvailability));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ContainerAvailabilityCollection(WebFactory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		#endregion Implementation
	}
}
