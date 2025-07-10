using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ZA.Business.EventProcessors
{
	public interface IEventProcessor
	{
		void Process(JobDeclaration declaration, IXmlEventValueObject xmlEvent);
	}
}
