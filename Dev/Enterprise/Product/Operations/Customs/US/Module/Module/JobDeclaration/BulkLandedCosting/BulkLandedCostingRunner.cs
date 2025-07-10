using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	class BulkLandedCostingRunner
	{
		public void RunLandedCosting(BusinessObject[] declarations)
		{
			var creator = ObjectFactory.Get<ILandedCostBulkCreator>();

			using (var form = new ProgressForm())
			{
				form.ShowProgressBar = true;
				form.ShowCancelButton = false;
				form.Status = "Running Landed Costing...";
				form.Show();

				creator.OnLCProgressChanged += new LCProgressEventHandler((int percentage) =>
				{
					form.PercentComplete = percentage;
					form.Refresh();
				});

				var declarationsNotProcessed = creator.CreateAndRunLC(new TypedEnumerable<MasterFiles.Business.ILandedCostHeader>(declarations));

				if (declarationsNotProcessed.Any())
				{
					var information = @"
Landed Costing finished running.

Landed Costing was not run on the following selected Customs Declarations.
Landed Costing is only applicable where Shipment Type is IMP(Import) or IMX(Import by External Broker).

";

					information += new ZStringBuilder(declarationsNotProcessed).ToStringWithDelimiterBetweenAppends(", ");

					Globals.Message.ShowInformation(information);
				}
			}
		}
	}
}
