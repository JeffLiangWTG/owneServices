using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public interface IOrderLineDictionaryProvider
	{
		Dictionary<ZGuid, ZInt> GetOrderLineDictionary();
	}
}
