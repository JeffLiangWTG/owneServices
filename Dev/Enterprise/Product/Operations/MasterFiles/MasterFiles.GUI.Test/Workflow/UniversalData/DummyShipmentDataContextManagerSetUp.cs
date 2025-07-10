using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Application.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DummyShipmentDataContextManagerSetUp : IDisposable
	{
		public DummyShipmentDataContextManagerSetUp()
		{
			var dataContextManager = new Mock<IShipmentDataContextManager>();
			var contextManagers = new Hashtable
			{
				{ nameof(DataContextType.DummyBusinessObject), new TestObjectHandle(dataContextManager.Object) }
			};

			universalDataContextManagersSubstitution = ObjectFactory.Substitute("UniversalDataContextManagers", contextManagers);
		}

		readonly IDisposable universalDataContextManagersSubstitution;

		void IDisposable.Dispose()
		{
			universalDataContextManagersSubstitution.Dispose();
		}
	}
}
