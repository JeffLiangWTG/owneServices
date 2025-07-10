using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.WarehouseExtensions;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclaration_SupportModificationState : BaseJobDeclaration, IWarehouseIntegrationSupporter
	{
		public BaseJobDeclaration_SupportModificationState(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		bool IWarehouseIntegrationSupporter.IsInwardBondedWarehousingEnabled => true;

		bool IWarehouseIntegrationSupporter.SupportModificationState => true;

		protected override EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}
	}
}
