using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	interface IContainerLinker
	{
		CommonContainer[] GetLogParent(IXmlEventValueObject xmlEvent);
	}
}
