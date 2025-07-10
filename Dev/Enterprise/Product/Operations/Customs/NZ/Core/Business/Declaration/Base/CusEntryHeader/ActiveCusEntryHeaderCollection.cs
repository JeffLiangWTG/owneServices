using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		public ActiveCusEntryHeaderCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return base.IsThisPartOfTheCollection(element) && Declaration != null && Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings.IsAssignableFrom(element.GetType());
		}
	}
}
