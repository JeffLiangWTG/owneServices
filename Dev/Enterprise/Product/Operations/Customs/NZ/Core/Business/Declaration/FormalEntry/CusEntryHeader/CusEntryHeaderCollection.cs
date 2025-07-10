
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
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

		public virtual new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}
	}
}
