using System;
using System.Collections.Generic;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public interface ITransportBookingDocumentOptionsProvider
	{
		IDisposable LastDocumentOptionsForTestStartRecording();
		ITransportBookingDocumentOptions LastDocumentOptionsForTest();
	}

	public interface ITransportBookingDocumentOptions
	{
		List<IDtbDocumentContainerOption> Containers { get; }
	}

	public interface IDtbDocumentContainerOption
	{
		string ContainerNumber { get; }
	}
}
