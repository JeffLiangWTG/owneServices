using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public interface IContainerLinkManager<T> where T : CommonConsol
	{
		T Consol { get; }

		void AllocateContainerLink(CommonContainer container, Container containerDataObject);

		void CollectContainerLink(CommonContainer container, Container containerDataObject);

		void PackIntoContainer(PackLine packLine, PackingLine packingLine);

		void SetContainerLink(CommonContainer container, PackingLine packingLine);

		int GetContainerLink(CommonContainer container);
	}
}
