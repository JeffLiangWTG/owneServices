using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusHAWB))]
	public class CusHAWBTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeleteAnyNewMessages_DoNotLoadMessagesDuringDelete()
		{
			var mawb = (CusMAWB)Factory.NewWithValidTestData(GetExpectedParentObjectType());
			var hawb = mawb.ChildBills.AddNew();
			var message = hawb.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV";
			Factory.Save();
			var expected = new Dictionary<string, int>
			{
				{ EDIMessage.Schema.TableName, 1 }
			};
			AssertEquals(1, hawb.Messages.Count);
			hawb.Messages.Reload(true);
			AssertDbHits(expected, Factory, true);

			var newFactory = new BusinessObjectFactory();
			var newHWAB = newFactory.Load<CusHAWB>(hawb.PK);
			newHWAB.DeleteAnyNewMessages();
			expected[EDIMessage.Schema.TableName] = 0;
			AssertDbHits(expected, newFactory, true);
		}

		public virtual void TestGetNewCusHAWBProcessTaskCollection()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>", typeof(ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>), ((IWorkflowProvider)hawb).WorkflowItems);
		}

		public void TestHouseIsRegisteredToThisCountry()
		{
			var hungarianCompany = Factory.New<GlbCompany>();
			hungarianCompany.GC_Code = "AGI";
			hungarianCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Hungary;
			var hungarianBranch = hungarianCompany.Branches.AddNew();
			hungarianBranch.GB_Code = "AGI";

			var eritreanCompany = Factory.New<GlbCompany>();
			eritreanCompany.GC_Code = "DAN";
			eritreanCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var eritreanBranch = eritreanCompany.Branches.AddNew();
			eritreanBranch.GB_Code = "DAN";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(eritreanBranch.PK.ToGuid()))
			{
				var eritreanHawb = Factory.New<CusHAWB>();
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Australia));
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Eritrea));
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Hungary));
				var mawb = Factory.New<CusMAWB>();
				eritreanHawb.CS_CM = mawb.PK;
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Australia));
				AssertEquals(true, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Eritrea));
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals(false, eritreanHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Hungary));
			}
			using (DisposableEnvironment.ForBranch(hungarianBranch.PK.ToGuid()))
			{
				var mawb2 = Factory.New<CusMAWB>();
				var magyarHawb = mawb2.ChildBills.AddNew();
				AssertEquals(false, magyarHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Australia));
				AssertEquals(false, magyarHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Eritrea));
				AssertEquals(false, magyarHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals(true, magyarHawb.IsRegisteredToThisCountry(Core.Constants.CountryCodes.Hungary));
			}
		}

		public void TestMayRequireAmendment()
		{
			CusMAWB mawb = (CusMAWB)Factory.NewWithValidTestData(GetExpectedParentObjectType());
			CusHAWB hawb = mawb.ChildBills.AddNew();
			EDIMessage message1 = hawb.Messages.AddNew();
			message1.EM_ReceiveTransmit = "RCV";
			Factory.Save();

			IMayRequireAmendment asInterface = hawb;
			AssertEquals("MayRequireAmendment", false, asInterface.MayRequireAmendment);

			mawb.CM_IsFinalManifest = true;
			AssertEquals("MayRequireAmendment", true, asInterface.MayRequireAmendment);
			Factory.Save();
			AssertEquals("MayRequireAmendment", false, asInterface.MayRequireAmendment);

			hawb.CS_Weight = 123;
			AssertEquals("MayRequireAmendment", true, asInterface.MayRequireAmendment);
			Factory.Save();
			AssertEquals("MayRequireAmendment", false, asInterface.MayRequireAmendment);
		}

		public void TestDeleteAnyNewMessages()
		{
			CusMAWB mawb = (CusMAWB)Factory.NewWithValidTestData(GetExpectedParentObjectType());
			CusHAWB hawb = mawb.ChildBills.AddNew();
			EDIMessage message1 = hawb.Messages.AddNew();
			message1.EM_ReceiveTransmit = "RCV";
			Factory.Save();
			EDIMessage message2 = hawb.Messages.AddNew();
			message2.EM_ReceiveTransmit = "RCV";
			AssertEquals(2, hawb.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(!message2.IsDeleted);
			hawb.DeleteAnyNewMessages();
			AssertEquals(1, hawb.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(message2.IsDeleted);
		}

		public void TestMAWBLoader()
		{
			CusMAWB mawb = (CusMAWB)Factory.NewWithValidTestData(GetExpectedParentObjectType());
			CusHAWB hawb = mawb.ChildBills.AddNew();
			Factory.Save();

			CusHAWB reHAWB = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			CusMAWB reMAWB = reHAWB.MAWB;

			reHAWB.CS_CM = ZGuid.Empty;
			AssertNull("reHAWB.MAWB", reHAWB.MAWB);

			reHAWB.CS_CM = reMAWB.PK;
			AssertNotNull("reHAWB.MAWB", reHAWB.MAWB);
		}

		public void TestCusHAWBItemsCollection()
		{
			var mawb = (CusMAWB)Factory.NewWithValidTestData(GetExpectedParentObjectType());
			var hawb = mawb.ChildBills.AddNew();
			hawb.CusHAWBItemsCollection.AddNew();
			Factory.Save();

			var reHAWB = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			var hawbItemsCollection = reHAWB.CusHAWBItemsCollection;

			AssertEquals(1, hawbItemsCollection.Count);
			Assert(reHAWB.IsRegisteredEditableChildObject(hawbItemsCollection));
		}

		#region Implementation
		protected virtual Type GetExpectedParentObjectType()
		{
			return typeof(CusMAWB);
		}
		#endregion
	}
}
