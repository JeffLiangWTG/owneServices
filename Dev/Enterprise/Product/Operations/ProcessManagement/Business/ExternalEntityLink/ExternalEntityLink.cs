using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class ExternalEntityLink : AutoExternalEntityLink, IExternalEntityLink
	{
		public ExternalEntityLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
