using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IUniversalXmlContentFilterDataObjectWriterFactory
	{
		IDataObjectWriterStrategy Load(BusinessObjectFactory factory, ZString purposeCode, EDIMessageContentFilterSchemaType type);
		IDataObjectWriterStrategy Load(BusinessObjectFactory factory, ZString purposeCode, ZString actionType);
		IDataObjectWriterStrategy Load(IMessageProfile messageProfile);
	}
}
