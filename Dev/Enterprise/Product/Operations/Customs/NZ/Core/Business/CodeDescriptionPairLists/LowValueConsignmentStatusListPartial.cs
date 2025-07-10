
namespace Enterprise.Customs.NZ.Business
{
	partial class LowValueConsignmentStatusList : Integration.Customs.NZ.ILowValueConsignmentStatusList
	{
		public static bool LastStatusIsImpediment(string code)
		{
			return (code == Codes.ConsignmentHeld || code == Codes.ConsignmentInError || code == Codes.FormalDeclarationRequired ||
					code == Codes.ConsolidationIcrRequired || code == Codes.ImportDeclarationRequired || code == Codes.MpiImportDecRequired ||
					code == Codes.RescindPreviousStatusNotification);
		}

		public static bool CanDelete(string code)
		{
			return code == Codes.NotSentToCustoms || code == Codes.ConsignmentCancelled;
		}
	}
}
