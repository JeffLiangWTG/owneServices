using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class DeclarationJobDatesProvider : JobDatesProvider<BaseJobDeclaration>
	{
		public DeclarationJobDatesProvider(BaseJobDeclaration declaration)
			: base(declaration) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.JE_DateOfArrival;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.JE_ExportDate;
		}
	}
}
