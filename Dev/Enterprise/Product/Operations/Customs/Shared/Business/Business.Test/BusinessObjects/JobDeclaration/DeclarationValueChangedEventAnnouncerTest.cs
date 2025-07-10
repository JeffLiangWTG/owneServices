using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationValueChangedEventAnnouncerTest : TestCaseWithFactory
	{
		public void TestAnnounceValueChangedEvent()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			int changedEventCalled = 0;

			using (DeclarationValueChangedAnnouncer announcer = new DeclarationValueChangedAnnouncer(declaration))
			{
				announcer.OnValueChanged += new EventHandler(delegate
				{ changedEventCalled++; });

				declaration.JE_MessageType = "CHA";
				AssertEquals("should have been incremented", 1, changedEventCalled);

				declaration.JE_MessageSubType = "CHA";
				AssertEquals("should have been incremented", 2, changedEventCalled);

				declaration.JE_MergeBy = "CHA";//not hooked
				AssertEquals("should not have been incremented", 2, changedEventCalled);

				declaration.JE_TransportMode = "CHA";
				AssertEquals("should have been incremented", 3, changedEventCalled);

				declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
				AssertEquals("should have been incremented", 4, changedEventCalled);

				declaration.JE_DateOfFirstArrival = ZDateTime.BrettsBirthday;
				AssertEquals("should have been incremented", 5, changedEventCalled);

				declaration.JE_ContainerMode = "CHA";
				AssertEquals("should have been incremented", 6, changedEventCalled);

				declaration.JE_ApplicationCode = "CHA";
				AssertEquals("should have been incremented", 7, changedEventCalled);
			}

			declaration.JE_MessageType = "CH2";
			AssertEquals("Announcer is disposed and counter should stay same", 7, changedEventCalled);
		}
	}
}
