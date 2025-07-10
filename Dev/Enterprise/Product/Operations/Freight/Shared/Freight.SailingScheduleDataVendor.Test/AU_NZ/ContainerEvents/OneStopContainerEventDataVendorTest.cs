using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest("AU")]
	abstract class OneStopContainerEventDataVendorTest : TestCaseWithFactory
	{
		public void TestSubscribeContainerEvents_OnlyWhenEnabled()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddDays(-6);
			FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CommonContainer container = NewContainer();
			container.JC_ContainerNum = "ContainerNum";
			SetContainerNumber(container, "container");
			Factory.Save();

			EDIMessageCollection collection = new EDIMessageCollection(container);

			collection.Load();
			AssertEquals("1-Stop event subscription messages not created until enabled and 1-Stop vessel tables up to date", 0, collection.Count);

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddDays(-8);
			FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetContainerNumber(container, "");
			Factory.Save();
			SetContainerNumber(container, "container");
			Factory.Save();

			collection.Load();
			AssertEquals("1-Stop event subscription messages not created until 1-Stop vessel tables up to date", 0, collection.Count);

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddDays(-6);
			SetContainerNumber(container, "");
			Factory.Save();
			SetContainerNumber(container, "container");
			Factory.Save();

			collection.Load();
			AssertEquals("1-Stop event subscription messages created when configured", 9, collection.Count);
		}

		public void TestSubscribeContainerEvents_WhenContainerCreated()
		{
			CommonContainer container = NewContainer();
			container.JC_ContainerNum = "ContainerNum";
			SetETA(ZDateTime.Now);

			Factory.Save();

			EDIMessageCollection collection = new EDIMessageCollection(container);
			collection.Load();
			AssertEquals("Subscription messages be created", 9, collection.Count);
		}

		public void TestSubscribeContainerEvents_WhenContainerNumberRenamed()
		{
			CommonContainer container = NewContainer();
			EDIMessageCollection collection = new EDIMessageCollection(container);
			SetETA(ZDateTime.Now);

			container.JC_ContainerNum = "InitialContainer";
			Factory.Save();
			container.JC_ContainerNum = "RenamedContainer";
			Factory.Save();

			collection.Load();
			AssertEquals("Subscription messages should be created when the container number is renamed", 18, collection.Count);
		}

		public void TestSubscribeContainerEvents_NOTWhenContainerETDPopulated()
		{
			CommonContainer container = NewContainer();
			container.JC_ContainerNum = "ContainerNum";
			EDIMessageCollection collection = new EDIMessageCollection(container);

			Factory.Save();
			collection.Load();

			AssertEquals("Initial subscription messages", 9, collection.Count);

			SetETD(ZDateTime.Now);
			Factory.Save();
			collection.Load();

			AssertEquals("No new subscription messages created once ETD is set", 9, collection.Count);
		}

		public void TestSubscribeContainerEvents_NOTWhenContainerETAPopulated()
		{
			CommonContainer container = NewContainer();
			container.JC_ContainerNum = "ContainerNum";
			EDIMessageCollection collection = new EDIMessageCollection(container);

			Factory.Save();
			collection.Load();

			AssertEquals("Initial subscription message", 9, collection.Count);

			SetETA(ZDateTime.Now);
			Factory.Save();
			collection.Load();

			AssertEquals("No new subscription messages created once ETA is set", 9, collection.Count);
		}

		public void TestDontSubscribeContainerEventsIfFactorySaveFails()
		{
			CommonContainer container = NewContainer();
			container.JC_ContainerNum = "ContainerNum";
			SetETA(ZDateTime.Now);

			AssertEquals(0, container.Messages.Count);

			PerformFailingFactorySave();
			AssertEquals("Event subscription request message not created after Factory.Save() fails", 0, container.ComTracMessages.Count);

			Factory.Save();
			AssertEquals("Event subscription message should be saved on a successful Factory.Save()", 9, container.ComTracMessages.Count);
		}

		protected abstract CommonContainer NewContainer();
		protected abstract void SetContainerNumber(CommonContainer container, ZString containerNum);
		protected abstract void SetETD(ZDateTime date);
		protected abstract void SetETA(ZDateTime date);

		#region TestSubscribeContainerEvents_WhenContainersAddedOrRenamed

		public void TestSubscribeContainerEvents_WhenContainersAddedOrRenamedWithETDPopulated()
		{
			SetETD(new ZDateTime(2005, 1, 1));
			TestSubscribeContainerEvents_WhenContainersAddedOrRenamed("ANY");
		}

		public void TestSubscribeContainerEvents_WhenContainersAddedOrRenamedWithETAPopulated()
		{
			SetETA(new ZDateTime(2005, 1, 1));
			TestSubscribeContainerEvents_WhenContainersAddedOrRenamed("ANY");
		}

		public void TestSubscribeContainerEvents_ForAUEnabled()
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = false;

			SetETA(new ZDateTime(2005, 1, 1));
			TestSubscribeContainerEvents_WhenContainersAddedOrRenamed("AU");
		}

		public void TestSubscribeContainerEvents_ForNZEnabled()
		{
			FreightDataRegistry.HasOneStopAU = false;
			FreightDataRegistry.HasOneStopNZ = true;

			SetETA(new ZDateTime(2005, 1, 1));
			TestSubscribeContainerEvents_WhenContainersAddedOrRenamed("NZ");
		}

		public void TestSubscribeContainerEvents_WhenNotEnabled()
		{
			FreightDataRegistry.HasOneStopAU = false;
			FreightDataRegistry.HasOneStopNZ = false;

			SetETA(new ZDateTime(2005, 1, 1));
			TestSubscribeContainerEvents_WhenContainersAddedOrRenamed("");
		}

		void TestSubscribeContainerEvents_WhenContainersAddedOrRenamed(string subscriptionCountry)
		{
			CommonContainer savedContainer = NewContainer();
			SetContainerNumber(savedContainer, "ContainerNum");
			CommonContainer savedContainerWithDifferentContainerNumber = NewContainer();
			SetContainerNumber(savedContainerWithDifferentContainerNumber, "OrgContNum");
			Factory.Save();

			CommonContainer newContainer = NewContainer();
			SetContainerNumber(newContainer, "NewContainer");
			SetContainerNumber(savedContainerWithDifferentContainerNumber, "UpdContNum");
			Factory.Save();

			if (subscriptionCountry.Length > 0)
			{
				if (subscriptionCountry == "AU")
				{
					ContainerEventRequestListTest.AssertCreatedEDIMessageBodies(
						new[]
							{
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n"
							}, Factory);
				}
				else if (subscriptionCountry == "NZ")
				{
					ContainerEventRequestListTest.AssertCreatedEDIMessageBodies(
						new[]
							{
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','" + subscriptionCountry + "'\r\n"
							}, Factory);
				}
				else if (subscriptionCountry == "ANY")
				{
					ContainerEventRequestListTest.AssertCreatedEDIMessageBodies(
						new[]
							{
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','CONTAINERNUM','" + savedContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','ORGCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','NEWCONTAINER','" + newContainer.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','AU'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n",
								"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','UPDCONTNUM','" + savedContainerWithDifferentContainerNumber.PK.ToString().Replace("-", "") + "','NZ'\r\n"
							}, Factory);
				}
			}
			else
			{
				ContainerEventRequestListTest.AssertCreatedEDIMessageBodies(Array.Empty<string>(), Factory);
			}
		}

		#endregion

		#region Implementation

		void PerformFailingFactorySave()
		{
			Factory.Saving += OnFactorySavingThrowException;
			try
			{
				Factory.Save();
				Fail("Expected a ZSaveException for the test");
			}
			catch (ZSaveException)
			{
			}
			finally
			{
				Factory.Saving -= OnFactorySavingThrowException;
			}
		}

		void OnFactorySavingThrowException(BusinessObjectFactory factory)
		{
			throw new ZSaveException(new ZDataException(new Exception("Factory.Save() should fail for the test"), null, Db.Connection), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();

			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
		}

		#endregion
	}
}
