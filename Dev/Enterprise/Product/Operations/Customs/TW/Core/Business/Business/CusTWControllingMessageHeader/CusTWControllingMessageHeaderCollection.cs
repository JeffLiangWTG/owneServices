using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWControllingMessageHeaderCollection : DependentBusinessObjectCollection<CusTWControllingMessageHeader, CusEntryInstruction>
	{
		public CusTWControllingMessageHeaderCollection(CusEntryInstruction master) : base(master)
		{
		}
	}
}
