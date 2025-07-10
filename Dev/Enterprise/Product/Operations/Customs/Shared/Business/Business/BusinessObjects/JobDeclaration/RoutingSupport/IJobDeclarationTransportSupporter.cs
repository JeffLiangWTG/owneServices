using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public interface IJobDeclarationTransportSupporter
	{
		void UpdateAllDeclarationTransportDataIfEmpty(Transport transport);
	}
}
