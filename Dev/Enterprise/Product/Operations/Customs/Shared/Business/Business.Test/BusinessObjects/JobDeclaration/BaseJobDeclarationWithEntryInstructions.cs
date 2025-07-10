using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationWithEntryInstructions : BaseJobDeclaration
	{
		public BaseJobDeclarationWithEntryInstructions(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool SupportMultipleWarehouseEntryCoreForTesting;
		protected internal override bool SupportMultipleWarehouseEntryCore => SupportMultipleWarehouseEntryCoreForTesting;

		public bool SupportContainerEntryInstructionPivotCoreForTesting;
		protected internal override bool SupportContainerEntryInstructionPivot => SupportContainerEntryInstructionPivotCoreForTesting;

		protected override EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}
	}
}
