using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class OneStopContainerEventRequestLineTest : TestCaseWithFactory
	{
		public void TestToCsvLineForAU()
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = false;

			ToCsvLineTest("AU");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ToCsvLineTest("AU");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ToCsvLineTest("AU");
		}

		public void TestToCsvLineForNZ()
		{
			FreightDataRegistry.HasOneStopAU = false;
			FreightDataRegistry.HasOneStopNZ = true;

			ToCsvLineTest("NZ");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ToCsvLineTest("NZ");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ToCsvLineTest("NZ");
		}

		public void TestToCsvLineForBoth()
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			ToCsvLineTest("ANY");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ToCsvLineTest("AU");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ToCsvLineTest("AU");
			FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ToCsvLineTest("NZ");
		}

		public void TestToCsvLineForNone()
		{
			FreightDataRegistry.HasOneStopAU = false;
			FreightDataRegistry.HasOneStopNZ = false;

			ToCsvLineTest("AU");
		}

		void ToCsvLineTest(string subscriptionCountry)
		{
			AssertCsvLine("GATEIN", false, subscriptionCountry);
			AssertCsvLine("GATEOUT", false, subscriptionCountry);
			AssertCsvLine("LOAD", false, subscriptionCountry);
			AssertCsvLine("IMPORT PREADVICE", false, subscriptionCountry);
			AssertCsvLine("EXPORT PREADVICE", false, subscriptionCountry);
			AssertCsvLine("DISCHARGE", false, subscriptionCountry);
		}

		void AssertCsvLine(string eventType, bool expectedPersistentFlag, string subscriptionCountry)
		{
			Env.Registry.MailboxEmailAddress = "clinton@edi.com.au";
			Container.JC_ContainerNum = "ContainerNum";
			SetETA(new ZDateTime(2005, 1, 1));

			ZString oldABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			try
			{
				GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "41 065-894 724";
				AssertEquals(
					("'41065894724','EDalerts','eda1ert5','clinton@edi.com.au','CONTAINER','" + eventType + "','ANY'," + (expectedPersistentFlag ? "'Y'" : "'N'") + ",'','','CONTAINERNUM','" + Container.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'").Replace("'", "\""),
					RequestLine.ToCsvLine(eventType, subscriptionCountry).ToString());
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = oldABN;
			}
		}

		protected abstract CommonContainer NewContainer();
		protected abstract void SetETD(ZDateTime etd);
		protected abstract void SetETA(ZDateTime eta);

		#region IsEventRequestRequired

		public void TestIsEventRequestRequired_WhenContainerCreated()
		{
			Container.JC_ContainerNum = "ContainerNum";
			AssertEquals(true, RequestLine.IsEventRequestRequired());
		}

		public void TestIsEventRequestRequired_WhenContainerNumberRenamed()
		{
			Container.JC_ContainerNum = "InitialContainer";
			Factory.Save();

			Container.JC_ContainerNum = "RenamedContainer";
			AssertEquals(true, RequestLine.IsEventRequestRequired());
		}

		public void TestIsEventRequestRequired_WhenUnchanged()
		{
			Container.JC_ContainerNum = "ContainerNum";
			SetETA(new ZDateTime(2005, 1, 1));
			Factory.Save();

			AssertEquals(false, RequestLine.IsEventRequestRequired());
		}

		public void TestIsEventRequestRequired_WhenContainerNumberEmpty()
		{
			Container.JC_ContainerNum = "";
			Factory.Save();

			SetETA(new ZDateTime(2005, 1, 1));
			AssertEquals(false, RequestLine.IsEventRequestRequired());
		}

		public void TestIsEventRequestRequired_OnlyForSeaContainers()
		{
			SetETA(new ZDateTime(2005, 1, 1));
			Container.JC_ContainerNum = "ContainerNum";

			Container.JC_ContainerMode = Core.Constants.ContainerModes.AIR;
			AssertEquals("Request not required for Air", false, RequestLine.IsEventRequestRequired());

			Container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Request required for Sea", true, RequestLine.IsEventRequestRequired());
		}

		#endregion
		#region Implementation

		ZString oldMailboxEmailAddress;

		OneStopContainerEventRequest RequestLine
		{
			get
			{
				if (requestLine == null)
				{
					requestLine = OneStopContainerEventRequest.New(Container);
				}
				return requestLine;
			}
		}
		OneStopContainerEventRequest requestLine;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = NewContainer();
				}
				return container;
			}
		}
		CommonContainer container;

		protected override void SetUp()
		{
			base.SetUp();
			oldMailboxEmailAddress = Env.Registry.MailboxEmailAddress;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Env.Registry.MailboxEmailAddress = oldMailboxEmailAddress;
		}

		#endregion
	}
}
