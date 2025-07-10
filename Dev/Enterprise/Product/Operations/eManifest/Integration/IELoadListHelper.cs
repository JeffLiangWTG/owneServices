
namespace Enterprise.eManifest.Integration
{
	using System.Collections.Generic;
	using Enterprise.Integration;

	public interface IELoadListHelper
	{
		Forwarding.IForwardingConsol CreateConsol(IELoadList eLoadList);

		void AttachELoadLists(Forwarding.IForwardingConsol consol, IEnumerable<IELoadList> eLoadLists);
	}
}
