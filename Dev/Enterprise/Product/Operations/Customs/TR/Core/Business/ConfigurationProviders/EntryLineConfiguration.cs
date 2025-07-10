using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class EntryLineConfiguration : EU.Business.EntryLineConfiguration
	{
		protected override ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => true;
	}
}
