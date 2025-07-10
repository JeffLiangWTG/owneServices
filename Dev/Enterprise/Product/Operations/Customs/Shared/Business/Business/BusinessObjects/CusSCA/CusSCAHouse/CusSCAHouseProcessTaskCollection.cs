using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusSCAHouseProcessTaskCollection : ProcessTaskCollection
	{
		public CusSCAHouseProcessTaskCollection(BaseCusSCAHouse bizo)
			: base(bizo)
		{
		}

		public new CusSCAHouseProcessTask this[int index] => (CusSCAHouseProcessTask)Elements[index];
		public new CusSCAHouseProcessTask AddNew() => (CusSCAHouseProcessTask)base.AddNew();
		public new BaseCusSCAHouse Parent => (BaseCusSCAHouse)base.Parent;
	}
}
