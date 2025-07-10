using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class USDeclarationOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			 {
				 new SendEntrySummaryOperationalActionMethod(),
				 new SendAESTIROperationalActionMethod(),
				 new SendCargoManifestEntryStatusQueryActionMethod(),
				 new SendEntrySummaryQueryOperationalActionMethod()
			 };
		}
	}
}
