using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class SupporterTest<TInterfacedObject, TInterface, TSupporter> : TestCaseWithFactory
			where TInterfacedObject : TInterface
	{
		public abstract void TestHasDangerousGoods();

		public void TestGetterImplementations()
		{
			RunAssertions();
			Assert("Empty test", atLeastOnePairMatched);
			Assert(string.Format("Implementations for below properties in {0} failed:\r\n", typeof(TSupporter)) + new ZStringBuilder(fails).ToStringWithDelimiterBetweenAppends("\r\n\r\n"), fails.Count == 0);
		}

		public virtual void TestMeetsCondition()
		{
			TInterfacedObject interfacedObject = GetInterfacedObject();

			var supporter = GetSupporter(interfacedObject) as RateLineConditionsSupporter;

			AssertEquals(supporter.HasDangerousGoods, supporter.MeetsCondition(RateLineConditions.DangerousGoods, null, null, false, false));
			Assert(supporter.MeetsCondition(RateLineConditions.UserDefined, "test", (c, s) => { return !c.IsEmpty && s != null; }, false, false));
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;

			if (supporter.SendingAgent != null)
			{
				Assert(!supporter.MeetsCondition(RateLineConditions.OwnGateway, null, null, false, false));
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = supporter.SendingAgent.PK;
				Assert(supporter.MeetsCondition(RateLineConditions.OwnGateway, null, null, false, false));
			}

			if (supporter.ControllingAgent != null)
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				Assert(!supporter.MeetsCondition(RateLineConditions.OwnControllingAgent, null, null, false, false));
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = supporter.ControllingAgent.PK;
				Assert(supporter.MeetsCondition(RateLineConditions.OwnControllingAgent, null, null, false, false));
			}

			if (supporter.ExportBroker != null && supporter.ImportBroker != null)
			{
				var supporterNeedClearCache = supporter.ExportBroker.PK == GlbCompany.CurrentCompany.GC_OH_OrgProxy
											|| supporter.ExportBroker.PK == GlbBranch.CurrentBranch.GB_OH_OrgProxy
											|| supporter.ImportBroker.PK == GlbCompany.CurrentCompany.GC_OH_OrgProxy
											|| supporter.ImportBroker.PK == GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

				if (supporterNeedClearCache)
				{
					supporter.ObjectToWrap.Factory.InvalidateCachedProperties();
				}

				Assert(supporter.MeetsCondition(RateLineConditions.HandOver, null, null, true, false));
				Assert(supporter.MeetsCondition(RateLineConditions.HandOver, null, null, false, true));

				Assert(!supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, false));
				Assert(!supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, false, true));

				if (supporter.ExportBroker != null && supporter.ImportBroker != null)
				{
					if (supporter.ExportBroker.PK != supporter.ImportBroker.PK)
					{
						GlbBranch.CurrentBranch.GB_OH_OrgProxy = supporter.ExportBroker.PK;

						Assert(supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, false));
						Assert(!supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, false, true));

						GlbBranch.CurrentBranch.GB_OH_OrgProxy = supporter.ImportBroker.PK;

						Assert(!supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, false));
						Assert(supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, false, true));
					}
					else
					{
						GlbBranch.CurrentBranch.GB_OH_OrgProxy = supporter.ExportBroker.PK;
						Assert(supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, false));
						Assert(supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, false, true));
					}
				}
			}

			if (supporter.SendingAgent != null && supporter.ReceivingAgent != null && supporter.ExportBroker != null && supporter.ImportBroker != null)
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;

				Assert(!supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, true, false));
				Assert(!supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, false, true));

				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = GlbCompany.CurrentCompany.PK;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = supporter.ExportBroker.PK;
				branch.GB_OH_OrgProxy = supporter.SendingAgent.PK;

				Assert(supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, true, false));
				Assert(!supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, false, true));

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = supporter.ImportBroker.PK;
				branch.GB_OH_OrgProxy = supporter.ReceivingAgent.PK;

				Assert(!supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, true, false));
				Assert(supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, false, true));
			}

			if (supporter.DepartureCFS != null && supporter.ArrivalCFS != null)
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;

				Assert(!supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, true, false));
				Assert(!supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, false, true));

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = supporter.DepartureCFS.PK;

				Assert(supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, true, false));
				Assert(!supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, false, true));

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = supporter.ArrivalCFS.PK;

				Assert(!supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, true, false));
				Assert(supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, false, true));
			}
		}

		protected void RunSetterGetterAssertion(string propertyName, Func<TInterfacedObject, object> setter, Func<TSupporter, object> getter)
		{
			TInterfacedObject interfacedObject = GetInterfacedObject();

			var supporter = GetSupporter(interfacedObject);
			var expected = setter(interfacedObject);
			var actual = getter(supporter);

			if (expected != actual)
			{
				fails.Add(string.Format("Property name: {0}\r\nExpected: {1}\r\nActual: {2}", propertyName, expected, actual));
			}
			else
			{
				atLeastOnePairMatched = true;
			}
		}

		protected abstract TInterfacedObject GetInterfacedObject();

		bool atLeastOnePairMatched;
		readonly List<string> fails = new List<string>();
		protected abstract void RunAssertions();

		protected abstract TSupporter GetSupporter(TInterface supportable);

		protected static void Assert(bool? condition)
		{
			Assert(null, condition);
		}

		protected static void Assert(string message, bool? condition)
		{
			Assertion.Assert(message, condition.HasValue && condition.Value);
		}
	}
}
