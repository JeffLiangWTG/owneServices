using System.Collections.Generic;

namespace Enterprise.Customs.TW.Business
{
	public interface IAddressCodeTypes
	{
		IEnumerable<string> IDCodeTypes { get; }

		IEnumerable<string> AEOCodeTypes { get; }

		IEnumerable<string> CBPCodeTypes { get; }

		IEnumerable<string> TPCCodeTypes { get; }

		IEnumerable<string> FRICodeTypes { get; }
	}
}
