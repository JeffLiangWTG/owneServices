using System;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class FailedMessage
	{
		public FailedMessage(string errorText)
		{
			this.ErrorText = errorText;
		}

		public string ErrorText { get; private set; }

		public string MessageNo { get; set; }
		public DateTime MessageDateTime { get; set; }
		public string ContainerNo { get; set; }
		public string ContainerType { get; set; }
		public string BillOfLading { get; set; }
		public string ShipmentNo { get; set; }
		public string Origin { get; set; }
		public string Destination { get; set; }
		public string Principal { get; set; }
	}
}
