using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ForwardingConsolActionSupporter))]
	internal class ForwardingConsolActionSupporterTest : OperationalActionSupporterTest<ForwardingConsolActionSupporter>
	{
		OperationalActionSupporter GetForwardingConsolSupporter()
		{
			using (ZFilterGridModule module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				return ((IOperationalActionSupportable)module).OperationalActionSupporter;
			}
		}

		public void TestPopulateMethods()
		{
			var supporter = GetForwardingConsolSupporter();
			var allIds = supporter.Methods.GetAllIds();
			AssertCollectionContains(ActionMethodProviderIDs.AWB, allIds);
			AssertCollectionContains(ActionMethodProviderIDs.Accounting, allIds);
			AssertCollectionContains(ActionMethodProviderIDs.SendAdvancedAirCargoReport, allIds);
			AssertCollectionContains(ActionMethodProviderIDs.DtbBookingParent, allIds);
		}

		public void TestInvoicingCheckpoint()
		{
			AssertEquals(Env.Security.MaintainConsolJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}
		public void TestBizObjectsInSameDataRowAndLogsCreatedFromForwardingConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var moduleConsol = factory.Load<ForwardingModuleConsol>(consol.PK);
			moduleConsol.JK_AgentsReference = "AAA";

			factory.Save();

			var bizObjsForARow = factory.GetBizOsForDataRow(((INeedRow)moduleConsol).Row);

			var typesOfBizObjsForARow = string.Join(", ", bizObjsForARow.Select(b => b.GetType().Name));

			AssertMultilineASCIIEquals("ForwardingModuleConsol, ForwardingConsol", typesOfBizObjsForARow);

			foreach (var biz in bizObjsForARow)
			{
				if (biz is IStmALogParent logParent)
				{
					logParent.Logs.AddNew(Events.CustomisableEvent00);
				}
			}

			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingModuleConsol>(consol.PK);

			var logs = consol2.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).ToArray();

			AssertEquals("Logs", 2, logs.Length);
		}

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobConsol; }
		}

		#endregion
	}
}
