using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalRequestInfoTemplateCollection : ActiveBusinessObjectCollection<ExternalRequestInfoTemplate>
	{
		public ExternalRequestInfoTemplateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ExternalRequestInfoTemplateCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}
	}
}
