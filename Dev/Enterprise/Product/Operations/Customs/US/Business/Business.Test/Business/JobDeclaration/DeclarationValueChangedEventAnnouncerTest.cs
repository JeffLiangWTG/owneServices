using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DeclarationValueChangedEventAnnouncerTest : TestCaseWithFactory
	{
		public void TestAnnounceValueChangedEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var changedEventCalled = 0;

			using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
			{
				announcer.OnValueChanged += new EventHandler(delegate
				{ changedEventCalled++; });

				declaration.JE_MessageType = "CHA";
				AssertEquals("should have been incremented", 2, changedEventCalled);

				declaration.JE_MessageSubType = "CHA";
				AssertEquals("should have been incremented", 3, changedEventCalled);

				declaration.JE_MergeBy = "CHA";//not hooked
				AssertEquals("should not have been incremented", 3, changedEventCalled);

				declaration.JE_TransportMode = "CHA";
				AssertEquals("should have been incremented", 4, changedEventCalled);

				declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
				AssertEquals("should have been incremented", 5, changedEventCalled);

				declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
				AssertEquals("should have been incremented", 6, changedEventCalled);

				declaration.JE_ContainerMode = "CHA";
				AssertEquals("should have been incremented", 7, changedEventCalled);

				declaration.US_EnableAII = true;
				AssertEquals("should have been incremented", 8, changedEventCalled);

				declaration.US_TariffType = "CHA";
				AssertEquals("should have been incremented", 9, changedEventCalled);

				declaration.US_InbondType = "C";
				AssertEquals("should have been incremented", 10, changedEventCalled);

				declaration.US_EntryType = "~";
				AssertEquals("should have been incremented", 11, changedEventCalled);

				declaration.US_IsInvoiceByRequest = true;
				AssertEquals("should have been incremented", 12, changedEventCalled);

				declaration.US_SchDLoading = "4909";
				AssertEquals("should have been incremented", 13, changedEventCalled);

				declaration.US_EnableENS = true;
				AssertEquals("should have been incremented", 14, changedEventCalled);

				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				AssertEquals("should have been incremented", 15, changedEventCalled);
			}

			declaration.JE_MessageType = "CH2";
			AssertEquals("Announcer is disposed and counter should stay same", 15, changedEventCalled);
		}
	}
}
