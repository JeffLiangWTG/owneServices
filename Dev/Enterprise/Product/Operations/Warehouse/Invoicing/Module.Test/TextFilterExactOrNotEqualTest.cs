using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Module.Test
{
	[TestedType(typeof(TextFilterExactOrNotEqual))]
	public class TextFilterExactOrNotEqualTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TextFilterExactOrNotEqual(JobStorageSchema.Constants.ET_OffBandProcessingStatus,
				JobStorageSchema.ET_OffBandProcessingStatus,
				new DummyPeriodicInvoicingFilterBusinessObject().StorageOffBandProcessingStatuses);
		}
	}

	class DummyPeriodicInvoicingFilterBusinessObject : PeriodicInvoicingFilterBusinessObject
	{
		public DummyPeriodicInvoicingFilterBusinessObject() : base(WarehouseCollectionType.CYDWarehouse)
		{
		}

		protected override SecurityCheckpoint CheckPointForJobManagement => Env.Security.CYDPeriodicInvoicingJobInvoicing;
	}
}
