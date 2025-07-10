//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementLineLookups
//
//    This class should be used for overriding collections in AutoCusStatementLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineLookups : Customs.Business.CusStatementLineLookups
	{
		public CusStatementLineLookups(CusStatementLine parent)
			: base(parent)
		{
		}

		new CusStatementLine Parent => (CusStatementLine)base.Parent;

		public StatementLineStatusList StatementLineStatusList => Factory.GetCachedValue<StatementLineStatusList>();

		public StatementEntryStatus StatementEntryStatus => Factory.GetCachedValue<StatementEntryStatus>();

		public JobDeclarationCollection Declarations => new JobDeclarationCollection(Factory, Parent.StatementHeader.Company.PK);
	}
}
