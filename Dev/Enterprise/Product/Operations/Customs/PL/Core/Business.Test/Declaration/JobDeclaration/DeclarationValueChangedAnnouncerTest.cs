using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class DeclarationValueChangedAnnouncerTest : TestCaseWithFactory
{
	public void TestAnnounceValueChangedEvent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		int changedEventCalled = 0;

		using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
		{
			announcer.OnValueChanged += new EventHandler(delegate
				{ changedEventCalled++; });

			declaration.JE_LocationOfGoods = "ABC";
			AssertEquals("should have been incremented", 1, changedEventCalled);

			declaration.JE_LocationQualifier = "XYZ";
			AssertEquals("should have been incremented", 2, changedEventCalled);

			declaration.JE_OfficeOfEntryExit = "123";
			AssertEquals("should have been incremented", 3, changedEventCalled);
		}

		declaration.JE_MessageType = "CH2";
		AssertEquals("Announcer is disposed and counter should stay same", 3, changedEventCalled);
	}
}
