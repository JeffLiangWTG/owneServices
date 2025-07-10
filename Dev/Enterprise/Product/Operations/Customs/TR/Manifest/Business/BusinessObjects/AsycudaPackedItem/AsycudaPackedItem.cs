using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;
		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);
	}
}
