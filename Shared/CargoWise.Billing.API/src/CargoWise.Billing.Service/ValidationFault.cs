using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CargoWise.Billing.Service
{
	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/CargoWise.eServices.Billing.WcfService")]
	public sealed class ValidationFault
	{
		public ValidationFault(List<string> errors)
		{
			Errors = errors;
		}

		[DataMember]
		public readonly List<string> Errors;
	}
}
