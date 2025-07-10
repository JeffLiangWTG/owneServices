using System;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	[Serializable]
	public class YardUnitNotFoundException : Exception
	{
		public YardUnitNotFoundException(string containerNumber, string reference)
			: base(Res.GetString("6dcecd05-7e3d-43df-8444-1635ebae5463", "Yard Unit ({0}) is not found or Instruction Advice ({1}) has expired.", containerNumber, reference))
		{
		}

#if NETFRAMEWORK
		protected YardUnitNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
