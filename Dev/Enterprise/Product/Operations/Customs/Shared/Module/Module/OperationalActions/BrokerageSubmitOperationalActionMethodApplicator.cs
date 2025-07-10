using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class BrokerageSubmitOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public BrokerageSubmitOperationalActionMethodApplicator()
			: base(Res.GetString("d3ff54d0-1cdb-4495-b571-0f5051328ea1", "Submit operational action"))
		{
			actionRunner = new BrokerageSubmitRunner();
		}

		readonly BrokerageSubmitRunner actionRunner;

		protected override void ApplyCore(IOperationalActionSectionLog log, CargoWise.EntityFramework.BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);
			actionRunner.Log = log;
			foreach (BaseJobDeclaration declaration in targets)
			{
				actionRunner.Execute(declaration);
				log.BumpSectionProgress();
			}
		}
	}
}
