namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitSealCollection : EU.ExitControl.Business.CusExitSealCollection
{
	public CusExitSealCollection(CusExitContainer master)
		: base(master)
	{
	}

	public new CusExitSeal this[int index] => (CusExitSeal)base[index];

	public new CusExitSeal AddNew() => (CusExitSeal)base.AddNew();
}
