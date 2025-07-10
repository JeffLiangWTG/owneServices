using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class ContainerResult
	{
		public ContainerResult(string containerTypeCode, ZString containerNumber, ZDecimal result)
		{
			ContainerTypeCode = containerTypeCode;
			ContainerNumber = containerNumber;
			Result = result;
		}

		public string ContainerTypeCode { get; }
		public ZString ContainerNumber { get; }
		public ZDecimal Result { get; }
	}
}
