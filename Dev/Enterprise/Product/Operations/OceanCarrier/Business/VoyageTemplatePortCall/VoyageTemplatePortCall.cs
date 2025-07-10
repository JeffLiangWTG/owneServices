using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class VoyageTemplatePortCall : AutoVoyageTemplatePortCall
	{
		public VoyageTemplatePortCall(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
