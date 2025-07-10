using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderProcessTaskCollection : ProcessTaskCollection<CusISFHeaderProcessTask, CusISFHeader>
	{
		public CusISFHeaderProcessTaskCollection(CusISFHeader declaration)
			: base(declaration)
		{
		}

		#region IsConditionMet

		public override bool IsCondition1Met(ZString conditionCode)
		{
			return true;
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			return true;
		}

		#endregion
	}
}
