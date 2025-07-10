using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ContainerLinkerTest : TestCaseWithFactory
	{
		public void TestNoContainerReturnIfContainerNumberIsEmpty()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "";

			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new PackLineManyToManyCollection(container);
			collection.Add(packLine);

			Factory.Save();
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "" });

			var containerLinker = new ContainerLinker<CommonContainer, CommonConsol>(Factory, helper.Object);

			var containers = containerLinker.GetLogParent(eventValueObject.Object);

			AssertNull("No container return if container number is blank.", containers);
		}
	}
}
