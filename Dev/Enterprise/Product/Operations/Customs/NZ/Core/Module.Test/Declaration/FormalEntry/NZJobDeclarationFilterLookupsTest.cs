using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	sealed class NZJobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertEquals("MessageTypeList of correct type", typeof(Common.NZ.NZJobMessageTypeList), lookups.MessageTypeList.GetType());
		}

		public void TestMessageSubTypeList()
		{
			AssertEquals("MessageSubTypeList of correct type", typeof(JobMessageSubTypeList), lookups.MessageSubTypeList().GetType());
			AssertEquals("When TSW Import is active, IPI should be in the message sub type list", true, lookups.MessageSubTypeList().ContainsCode(MessageSubTypeCombinedList.Codes.IPI));
		}

		public void TestEntryStatusList()
		{
			AssertEquals("EntryStatusList of correct type", typeof(ZArchitecture.Core.CodeDescriptionPairList), lookups.EntryStatusList().GetType());
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Assert("EntryStatus List contains Consolidation status ReadyForConsolidation when ConsolidatedEntries is enabled", lookups.EntryStatusList().ContainsCode("RFC"));
				Assert("EntryStatus List contains Consolidation status AppliedToConsolidation when ConsolidatedEntries is enabled", lookups.EntryStatusList().ContainsCode("ATC"));
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Assert("EntryStatus List doesn't contain Consolidation status ReadyForConsolidation when ConsolidatedEntries is not enabled", !lookups.EntryStatusList().ContainsCode("RFC"));
				Assert("EntryStatus List doesn't contain Consolidation status AppliedToConsolidation when ConsolidatedEntries is not enabled", !lookups.EntryStatusList().ContainsCode("ATC"));
			}
		}

		public void TestContainerModeList()
		{
			AssertEquals("ContainerModeList of correct type", typeof(ContainerModeCustomsList), lookups.ContainerModeList.GetType());
			Assert("Contains air", lookups.ContainerModeList.ContainsCode("AIR"));
			Assert("Contains containerised", lookups.ContainerModeList.ContainsCode("CNT"));
			Assert("Contains non-containerised", lookups.ContainerModeList.ContainsCode("NCT"));
			Assert("Contains bulk", lookups.ContainerModeList.ContainsCode("BLK"));
			Assert("Contains liquid", lookups.ContainerModeList.ContainsCode("LQD"));
			Assert("Contains buyer's consolidation", lookups.ContainerModeList.ContainsCode("BCN"));
			Assert("Contains roll on/roll off", lookups.ContainerModeList.ContainsCode("ROR"));
			Assert("Does not contain full container load", !lookups.ContainerModeList.ContainsCode("FCL"));
			Assert("Does not contain less than container load", !lookups.ContainerModeList.ContainsCode("LCL"));
			Assert("Does not contain empty", !lookups.ContainerModeList.ContainsCode("EMP"));
		}

		public void TestTransportTypeList()
		{
			AssertEquals("TransportTypeList of correct type", typeof(JobTransportModeList), lookups.TransportTypeList.GetType());
		}

		public void TestApplicationCodeList()
		{
			var applicationCodeList = lookups.ApplicationCodeList();
			AssertEquals("ApplicationCodeList of correct type.", typeof(JobApplicationCodeList), applicationCodeList.GetType());
			AssertEquals("ApplicationCodeList should have 2 values.", 2, applicationCodeList.Count);
			AssertSame(applicationCodeList, filterBizObj.Factory.GetCachedValue<JobApplicationCodeList>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new NZJobDeclarationFilterBusinessObject();
			lookups = filterBizObj.Lookups;
		}

		NZJobDeclarationFilterBusinessObject filterBizObj;
		NZJobDeclarationFilterLookups lookups;
	}
}
