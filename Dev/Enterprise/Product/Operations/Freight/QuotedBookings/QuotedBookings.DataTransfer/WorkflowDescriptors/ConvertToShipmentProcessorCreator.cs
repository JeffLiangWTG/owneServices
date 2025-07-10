using CargoWise.EntityFramework;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	class ConvertToShipmentProcessorCreator : IConvertToShipmentCreator
	{
		public IProcessor CreateConvertToShipmentProcessor(IWorkflowProvider provider)
		{
			return provider is QuotedBooking booking
				? new ConvertToShipmentProcessor(booking)
				: null;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Testing
{
	using System;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.BufferManagement.Integration;
	using Enterprise.ZArchitecture.Business;

	#region Implementation

	class DummyIWorkflowProvider : IWorkflowProvider
	{
		public IProcessHeaderCollection Workflows => throw new NotImplementedException();

		public ProcessTaskCollection WorkflowItems => throw new NotImplementedException();

		public ZGuid PK => throw new NotImplementedException();

		public ZString WorkflowType => throw new NotImplementedException();

		public ZGuid Identifier => throw new NotImplementedException();

		public Logs Logs => throw new NotImplementedException();

		public BusinessObjectFactory LogsFactory => throw new NotImplementedException();

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			throw new NotImplementedException();
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}

#endif
#endregion
