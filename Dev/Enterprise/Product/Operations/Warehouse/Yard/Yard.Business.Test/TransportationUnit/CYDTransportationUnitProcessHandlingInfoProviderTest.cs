using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitProcessHandlingInfoProvider))]
	public class CYDTransportationUnitProcessHandlingInfoProviderTest : TestCaseWithFactory
	{
		public void TestPopulateCascadingTargets()
		{
			var tpu = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yardUnit = Factory.NewWithValidTestData<CYDYardUnitState>();
			tpu.ReceiveYardUnits.Add(yardUnit);

			var processTask = Factory.NewWithValidTestData<ProcessTask>();
			processTask.P9_ParentID = yardUnit.PK;
			processTask.TriggerConditions.TriggerEventCode = Events.GateInCode;
			processTask.P9_ParentTableCode = CYDYardUnitStateSchema.Constants.Prefix;
			processTask.P9_RespondToCascadedEvents = true;

			Factory.Save();

			var handlingInfo = new CYDTransportationUnitProcessHandlingInfoProvider(tpu);
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.GateInCode;
			}

			var cascadingLink = handlingInfo.GetCascadingTargets(eventLog).Single();
			AssertEquals(yardUnit, cascadingLink.Parent);
			AssertEquals(1, cascadingLink.Triggers.Length);
		}
	}
}
