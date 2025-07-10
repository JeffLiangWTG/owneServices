using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.DocumentEngine.Service;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Services.ServiceHost
{
	public class DeliveryRequest
	{
		public Guid DocumentCommandPk { get; set; }
		public string TablePrefix { get; set; }
		public Guid BusinessObjectPk { get; set; }
		public DeliveryInstructionsBase DeliveryInstructions { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Enterprise requires this to be an array.")]
		public DocumentDetail[] Documents { get; set; }
	}
}