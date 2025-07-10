using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryHeaderCollection : Customs.Business.CusEntryHeaderCollection<CusEntryHeader>
	{
		public CusEntryHeaderCollection(JobDeclaration parentBO, BusinessObjectFactory factory)
			: base(parentBO, factory)
		{
		}

		public override void ClearPackages()
		{
			foreach (CusEntryHeader entryHeader in this)
			{
				entryHeader.CH_Packages = 0;
			}
		}
	}
}
