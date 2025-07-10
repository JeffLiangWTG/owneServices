using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestQueryBizObjCollection : NonPersistentBusinessObjectCollection<CargoManifestQueryBizObj>
	{
		public CargoManifestQueryBizObjCollection(CargoManifestQueryHeader header)
		{
			this.header = header;
		}

		readonly CargoManifestQueryHeader header;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CargoManifestQueryBizObj(header);
		}
	}
}
