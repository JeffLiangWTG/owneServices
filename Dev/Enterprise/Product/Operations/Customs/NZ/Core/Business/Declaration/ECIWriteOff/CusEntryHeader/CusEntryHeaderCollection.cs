using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class CusEntryHeaderCollection : Declaration.CusEntryHeaderCollection
	{
		public CusEntryHeaderCollection(JobDeclaration parentDeclaration, BusinessObjectFactory factory)
			: base(parentDeclaration, factory)
		{
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)Elements[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}
	}
}
