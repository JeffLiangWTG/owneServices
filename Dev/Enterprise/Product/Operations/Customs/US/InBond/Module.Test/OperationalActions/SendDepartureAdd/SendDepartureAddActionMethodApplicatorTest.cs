using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendDepartureAddActionMethodApplicator))]
	sealed class SendDepartureAddActionMethodApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestValidationMode()
		{
			AssertEquals(((SendDepartureAddActionMethodApplicator)Applicator).ValidationMode, US.Business.ValidationModes.None);
		}

		public void TestSecurityRight()
		{
			Env.Security.USInBondMessaging.ClearOverriddenSecurityValue();

			var job = Factory.New<CusInBondHeader>();
			_ = job.MovementHeader;
			Factory.Save();
			var log = SimulateRun(new BusinessObject[] { job }, false);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			var user = Factory.NewWithValidTestData<GlbStaff>();
			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
			{
				log = SimulateRun(new BusinessObject[] { job }, false);
				AssertEquals("Access Denied: Messaging", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(Env.Security.USInBondMessaging.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendDepartureMessageForUSInBondMoveHeader()
		{
			var inBond1 = Factory.New<CusInBondHeader>();
			var movement1 = inBond1.MovementHeader;
			movement1.InBondNumber = "12345678";
			movement1.BM_InBondEntryType = "61";
			var inBond2 = Factory.New<CusInBondHeader>();
			var movement2 = inBond2.MovementHeader;
			movement2.InBondNumber = "98765432";
			movement2.BM_InBondEntryType = "61";
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var applicator = (SendDepartureAddActionMethodApplicator)Applicator;
			applicator.SendWithMessageErrors = true;
			SimulateRun(new BusinessObject[] { moveHeader1, moveHeader2 }, false);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, movement1.BM_CustomsStatus);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, movement2.BM_CustomsStatus);
		}
	}
}
