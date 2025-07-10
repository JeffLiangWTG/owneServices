using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TextFilterExactOrNotEqual))]
	class TextFilterExactOrNotEqualTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TextFilterExactOrNotEqual(JobStorageSchema.Constants.ET_OffBandProcessingStatus,
				JobStorageSchema.ET_OffBandProcessingStatus,
				new InvoicingFilterBusinessObject().StorageOffBandProcessingStatuses);
		}
	}
}
