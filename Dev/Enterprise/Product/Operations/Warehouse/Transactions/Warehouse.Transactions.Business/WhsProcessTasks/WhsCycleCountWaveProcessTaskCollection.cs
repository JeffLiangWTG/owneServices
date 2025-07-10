using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountWaveProcessTaskCollection : ProcessTaskCollection
	{
		public WhsCycleCountWaveProcessTaskCollection(WhsCycleCountWave wave) : base(wave)
		{
		}

		public new WhsCycleCountWaveProcessTask this[int index] => (WhsCycleCountWaveProcessTask)Elements[index];

		public new WhsCycleCountWaveProcessTask AddNew() => (WhsCycleCountWaveProcessTask)base.AddNew();
	}
}
