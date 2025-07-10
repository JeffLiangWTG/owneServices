using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class SynchronizeTestHelper : SynchronizeWithOrders
	{
		public SynchronizeTestHelper(BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString GetOutwardEntryNumber()
		{
			return "SJ5-ENT3431";
		}
	}
}
