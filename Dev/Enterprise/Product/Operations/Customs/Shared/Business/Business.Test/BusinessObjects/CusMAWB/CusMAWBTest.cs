using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMAWB))]
	public class CusMAWBTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestCM_MessageReferenceGetsFilledInOnSave()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("Before Save: mawb.CM_MessageReference", ZString.Empty, mawb.CM_MessageReference);
			Factory.Save();
			AssertEquals("After Save: mawb.CM_MessageReference", "X00001000", mawb.CM_MessageReference);
		}

		public virtual void TestGetNewCusMAWBProcessTaskCollection()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>", typeof(ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>), ((IWorkflowProvider)mawb).WorkflowItems);
		}

		public void TestResgisterAndUnregisterHouseBillsAsEditableChildren()
		{
			var mAWB = Factory.New<CusMAWB>();
			AssertEquals("Register Housebills as editable child is not done automatically", true, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));

			mAWB.UnregisterHouseBillsAsEditableChildren();
			AssertEquals("HouseBills are unregistered", false, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));

			mAWB.RegisterHouseBillsAsEditableChildren();
			AssertEquals("HouseBills are editable children", true, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));
		}

		public virtual void TestIsIDocumentSupportable()
		{
			IDocumentSupportable parent = Factory.New<CusMAWB>();
			CusMAWBDocumentSupporter documentSupporter = parent.DocumentSupporter as CusMAWBDocumentSupporter;
			AssertNotNull("IDocumentSupportable parent.DocumentSupporter as CusMAWBDocumentSupporter", documentSupporter);
		}

		public void TestChildBillsIsLoadedOnNewMasterBill()
		{
			var mawb = GetNewMAWB();
			Assert("Should default to false on a new MAWB.", !mawb.ChildBillsIsLoaded);
		}

		public void TestChildBillsCollectionAndIsLoaded()
		{
			CusHAWB cusHAWB = MAWB.ChildBills.AddNew();
			AssertEquals("cusHAWB.CS_CM", MAWB.PK, cusHAWB.CS_CM);
			AssertEquals("cusMAWB.ChildBillsIsLoaded", true, MAWB.ChildBillsIsLoaded);
		}

		public virtual void TestFilteredChildBills()
		{
			var mawb = Factory.New<CusMAWB>();

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_CustomsStatus = "ACS";
			hawb1.CS_MsgStatus = "AMS";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_CustomsStatus = "BCS";
			hawb2.CS_MsgStatus = "BMS";
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_CustomsStatus = "CCS";
			hawb3.CS_MsgStatus = "CMS";

			mawb.InvalidBillsOnlyFilter = true;
			AssertEquals("House bills don't have validation at this level", 0, mawb.FilteredChildBills.Count);

			mawb.CustomsCargoStatusFilter = "ACS";
			mawb.FilteredChildBills.Rebuild();
			AssertEquals("Cargo Status Filter", 1, mawb.FilteredChildBills.Count);

			mawb.CustomsMessageStatusFilter = "BMS";
			mawb.FilteredChildBills.Rebuild();
			AssertEquals("Cargo Status & Message Filter", 2, mawb.FilteredChildBills.Count);
		}

		public void TestCreateMutexForConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.Save();

				using (var consolMutex1 = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedStates))
				using (var consolMutex2 = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedStates))
				using (var consolMutex3 = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia))
				{
					consolMutex1.Lock();
					Assert("Cannot lock US mutex 2 when US mutex 1 is locked", !consolMutex2.Lock());
					Assert("Can lock AU mutex when US mutex 1 is locked", consolMutex3.Lock());
					consolMutex3.Unlock();
					consolMutex1.Unlock();
					Assert("Can lock US mutex 2 when US mutex 1 is unlocked", consolMutex2.Lock());
				}
			}
		}

		public void TestMAWBsWithMatchingActiveMAWBNoWithParent()
		{
			var mAWB1 = (CusMAWB)GetNewBusinessObject();
			var mAWB2 = (CusMAWB)GetNewBusinessObject();
			var mAWB3 = (CusMAWB)GetNewBusinessObject();
			mAWB1.CM_MAWB = "752";
			mAWB1.CM_MasterHouseBill = "81993";
			mAWB1.CM_IsActive = false;
			mAWB2.CM_MAWB = "752";
			mAWB3.CM_MAWB = "81993";
			Factory.Save();
			AssertEquals(1, mAWB2.CusMAWBLoader.ActiveMAWBsWithThisMatchingMAWBNo().Length);
			AssertEquals(1, mAWB3.CusMAWBLoader.ActiveMAWBsWithThisMatchingMAWBNo().Length);

			mAWB1.CM_IsActive = true;
			Factory.Save();
			AssertEquals(2, mAWB1.CusMAWBLoader.ActiveMAWBsWithThisMatchingMAWBNo().Length);
			AssertEquals(2, mAWB2.CusMAWBLoader.ActiveMAWBsWithThisMatchingMAWBNo().Length);
			AssertEquals(1, mAWB3.CusMAWBLoader.ActiveMAWBsWithThisMatchingMAWBNo().Length);
		}

		public void TestFindMatchingActiveMAWBsWithParent()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_ApplicationCode = "A";
			mAWB1.CM_MAWB = "~2100000000";
			mAWB1.CM_MasterHouseBill = "1111";
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB1.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB1.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "").Length);

			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_ApplicationCode = "A";
			mAWB2.CM_MAWB = "~2100000000";
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB2.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "").Length);

			mAWB2.CM_MasterHouseBill = "1111";
			mAWB2.CM_IsActive = false;
			AssertEquals("No match should be found as mAWB2 is inactive", 0, mAWB1.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 1, mAWB2.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);

			mAWB2.CM_MasterHouseBill = "2222";
			mAWB2.CM_IsActive = true;
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB1.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB2.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "2222").Length);

			CusMAWB mAWB3 = Factory.New<CusMAWB>();
			mAWB3.CM_ApplicationCode = "A";
			mAWB3.CM_MAWB = "~2100000000";
			mAWB3.CM_MasterHouseBill = "1111";
			mAWB2.CM_MasterHouseBill = "1111";
			mAWB2.CM_IsActive = false;
			AssertEquals("Should only match with mAWB3 as mAWB2 is inactive", 1, mAWB1.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 2, mAWB2.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Should only match with mAWB1 as mAWB2 is inactive", 1, mAWB3.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			mAWB3.CM_ApplicationCode = "B";
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB1.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 1, mAWB2.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB3.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);

			CusMAWB mAWB4 = Factory.New<CusMAWB>();
			mAWB4.CM_ApplicationCode = "A";
			mAWB4.CM_MAWB = "~2100000001";
			mAWB4.CM_MasterHouseBill = "1111";
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB4.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000001", "1111").Length);
			AssertEquals("Correct MAWB/CoLoad matches", 1, mAWB4.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", "1111").Length);

			AssertEquals("Correct MAWB/CoLoad matches", 1, mAWB4.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", true, "1111", true, true).Length);
			mAWB1.CM_ArrivalDate = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value - 1);
			AssertEquals("Correct MAWB/CoLoad matches", 0, mAWB4.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", true, "1111", true, true).Length);
			AssertEquals("Correct MAWB/CoLoad matches", 1, mAWB4.CusMAWBLoader.FindMatchingActiveMAWBsWithParent("~2100000000", true, "1111", true, false).Length);
		}

		public void TestFindMatchingMAWBsByMAWB()
		{
			CusMAWB mAWB1 = (CusMAWB)GetNewBusinessObject();
			mAWB1.CM_MAWB = "1";
			CusMAWB mAWB2 = (CusMAWB)GetNewBusinessObject();
			mAWB2.CM_MAWB = "1";
			CusMAWB mAWB3 = (CusMAWB)GetNewBusinessObject();
			mAWB3.CM_MAWB = "2";
			AssertEquals("FindMatchingMAWBs", 2, GetMAWBCount("1"));
			AssertEquals("FindMatchingMAWBs", 1, GetMAWBCount("2"));
			AssertEquals("FindFirstMatchingMAWB", mAWB3, GetFirstMatchingMAWB("2"));
		}

		public void TestFindMatchingMAWBsByMAWBCompanyAndFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var branch11 = company1.Branches.AddNew();
			var branch12 = company1.Branches.AddNew();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";
			var branch21 = company2.Branches.AddNew();

			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "1";
			mawb1.CM_GB = branch11.PK;
			mawb1.CM_FlightNo = "QF001";

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "1";
			mawb2.CM_GB = branch12.PK;
			mawb2.CM_FlightNo = "QF001";

			var mawb3 = Factory.New<CusMAWB>();
			mawb3.CM_MAWB = "2";
			mawb3.CM_GB = branch11.PK;
			mawb3.CM_FlightNo = "QF001";

			var mawb4 = Factory.New<CusMAWB>();
			mawb4.CM_MAWB = "1";
			mawb4.CM_GB = branch21.PK;
			mawb4.CM_FlightNo = "QF001";

			var mawb5 = Factory.New<CusMAWB>();
			mawb5.CM_MAWB = "1";
			mawb5.CM_GB = branch11.PK;
			mawb5.CM_FlightNo = "QF002";

			var loader = new CusMAWB.Loader(Factory);

			var mawbs = loader.FindMatchingMAWBs("1", "AAA", new ZQuery(CusMAWBSchema.CM_FlightNo, "QF001"));
			AssertContainsExactElementsInAnyOrder(new[] { mawb1, mawb2 }, mawbs);

			mawbs = loader.FindMatchingMAWBs("1", "YYY", new ZQuery(CusMAWBSchema.CM_FlightNo, "QF001"));
			AssertContainsExactElementsInAnyOrder(new[] { mawb1, mawb2, mawb4 }, mawbs);
		}

		public void TestFindMatchingMAWBsByConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CusMAWB mawb = (CusMAWB)GetNewBusinessObject();
			mawb.CM_JK = consol.PK;
			AssertEquals("FindMatchingMAWBs", 1, GetMAWBsByConsol(consol).Length);
			AssertEquals("FindMatchingMAWBs", mawb, GetMAWBsByConsol(consol)[0]);
		}

		public void TestFindFirstMatchingCTOMAWBsByMAWB()
		{
			CusMAWB mAWB1 = (CusMAWB)GetNewBusinessObject();
			mAWB1.CM_IsCTOMAWB = false;
			mAWB1.CM_MAWB = "1";
			CusMAWB mAWB2 = (CusMAWB)GetNewBusinessObject();
			mAWB2.CM_IsCTOMAWB = true;
			mAWB2.CM_MAWB = "1";
			CusMAWB mAWB3 = (CusMAWB)GetNewBusinessObject();
			mAWB3.CM_IsCTOMAWB = true;
			mAWB3.CM_MAWB = "2";
			AssertEquals("FindFirstMatchingCTOMAWB", mAWB2, GetFirstMatchingCTOMAWB("1"));
			AssertEquals("FindFirstMatchingCTOMAWB", mAWB3, GetFirstMatchingCTOMAWB("2"));
		}

		public void TestFindMatchingCTOMAWBs()
		{
			CusMAWB mAWB1 = (CusMAWB)GetNewBusinessObject();
			mAWB1.CM_IsCTOMAWB = false;
			mAWB1.CM_FlightNo = "QF1";
			mAWB1.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 12, 30, 31);
			mAWB1.CM_MAWB = "1";
			CusMAWB mAWB2 = (CusMAWB)GetNewBusinessObject();
			mAWB2.CM_IsCTOMAWB = true;
			mAWB2.CM_FlightNo = "QF1";
			mAWB2.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 12, 30, 31);
			mAWB2.CM_MAWB = "1";
			CusMAWB mAWB3 = (CusMAWB)GetNewBusinessObject();
			mAWB3.CM_IsCTOMAWB = true;
			mAWB3.CM_FlightNo = "QF1";
			mAWB3.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 13, 30, 31);
			mAWB3.CM_MAWB = "2";
			CusMAWB mAWB4 = (CusMAWB)GetNewBusinessObject();
			mAWB4.CM_IsCTOMAWB = true;
			mAWB4.CM_FlightNo = "QF2";
			mAWB4.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 12, 30, 31);
			mAWB4.CM_MAWB = "1";
			CusMAWB mAWB5 = (CusMAWB)GetNewBusinessObject();
			mAWB5.CM_IsCTOMAWB = true;
			mAWB5.CM_FlightNo = "QF1";
			mAWB5.CM_ArrivalDate = new ZDateTime(2008, 4, 2, 12, 30, 31);
			mAWB5.CM_MAWB = "1";
			List<CusMAWB> mAWBs = new List<CusMAWB>();
			foreach (CusMAWB mAWB in GetMatchingCTOMAWBs("QF1", new ZDateTime(2008, 4, 1, 14, 30, 31)))
			{
				mAWBs.Add(mAWB);
			}
			AssertEquals("FindMatchingCTOMAWBs", 2, mAWBs.Count);
			Assert("FindMatchingCTOMAWBs", mAWBs.Contains(mAWB2));
			Assert("FindMatchingCTOMAWBs", mAWBs.Contains(mAWB3));
		}

		public void TestFindMatchingForwarderMAWBs()
		{
			CusMAWB mAWB1 = (CusMAWB)GetNewBusinessObject();
			mAWB1.CM_IsCTOMAWB = true;
			mAWB1.CM_FlightNo = "QF1";
			mAWB1.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 12, 30, 31);
			mAWB1.CM_MAWB = "1";
			CusMAWB mAWB2 = (CusMAWB)GetNewBusinessObject();
			mAWB2.CM_IsCTOMAWB = false;
			mAWB2.CM_FlightNo = "QF1";
			mAWB2.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 12, 30, 31);
			mAWB2.CM_MAWB = "1";
			CusMAWB mAWB3 = (CusMAWB)GetNewBusinessObject();
			mAWB3.CM_IsCTOMAWB = false;
			mAWB3.CM_FlightNo = "QF1";
			mAWB3.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 13, 30, 31);
			mAWB3.CM_MAWB = "2";
			CusMAWB mAWB4 = (CusMAWB)GetNewBusinessObject();
			mAWB4.CM_IsCTOMAWB = false;
			mAWB4.CM_FlightNo = "QF2";
			mAWB4.CM_ArrivalDate = new ZDateTime(2008, 4, 1, 12, 30, 31);
			mAWB4.CM_MAWB = "1";
			CusMAWB mAWB5 = (CusMAWB)GetNewBusinessObject();
			mAWB5.CM_IsCTOMAWB = false;
			mAWB5.CM_FlightNo = "QF1";
			mAWB5.CM_ArrivalDate = new ZDateTime(2008, 4, 2, 12, 30, 31);
			mAWB5.CM_MAWB = "1";
			List<CusMAWB> mAWBs = new List<CusMAWB>();
			foreach (CusMAWB mAWB in GetMatchingForwarderMAWBs("1", "QF1", new ZDateTime(2008, 4, 1, 14, 30, 31)))
			{
				mAWBs.Add(mAWB);
			}
			AssertEquals("FindMatchingForwarderMAWBs", 1, mAWBs.Count);
			Assert("FindMatchingForwarderMAWBs", mAWBs.Contains(mAWB2));

			Factory.Save();
			mAWBs = new List<CusMAWB>();
			foreach (CusMAWB mAWB in Factory.Load<CusMAWB>(GetMAWBDBOnlyQuery("QF1", new ZDateTime(2008, 4, 1, 14, 30, 31))))
			{
				mAWBs.Add(mAWB);
			}
			AssertEquals("FindMatchingForwarderMAWBs", 3, mAWBs.Count);
			Assert("FindMatchingForwarderMAWBs", mAWBs.Contains(mAWB1));
			Assert("FindMatchingForwarderMAWBs", mAWBs.Contains(mAWB3));
			Assert("FindMatchingForwarderMAWBs", mAWBs.Contains(mAWB3));
		}

		public virtual void TestEffectiveResponsibleParty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mawb = Factory.New<CusMAWB>();

			AssertNull("Responsible party is not specified", mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals("Responsible party is not specified", mawb.ReasonEffectiveResponsiblePartyIsUnavailable);

			mawb.CM_OH_ResponsibleParty = orgHeader.PK;
			AssertEquals("Responsible party", orgHeader, mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals(ZString.Empty, mawb.ReasonEffectiveResponsiblePartyIsUnavailable);
		}

		public void TestCFS()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = Factory.New<OrgAddress>();
			var mawb = Factory.New<CusMAWB>();

			AssertNull("CFS is not specified", mawb.UnpackDepotAddress);
			AssertEquals("CFS is not specified", mawb.ReasonCFSIsUnavailable);

			mawb.CM_OA_UnpackDepotAddress = orgAddress.PK;
			mawb.UnpackDepotAddress.OA_OH = orgHeader.PK;
			AssertEquals("CFS", orgHeader, mawb.UnpackDepotAddress.Header);
			AssertEquals(ZString.Empty, mawb.ReasonCFSIsUnavailable);
		}

		public void TestCancellation()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CusMAWB mawb = (CusMAWB)GetNewBusinessObject();
			mawb.CM_JK = consol.PK;
			mawb.IsCancelled = true;
			Assert(consol.IsCancelled);
			mawb.IsCancelled = false;
			Assert(!consol.IsCancelled);
		}

		public void TestOnSavedFailed()
		{
			MAWB.CM_MessageReference = "123";
			MAWB.OnSaved(false);
			AssertEquals("CM_MessageReference", string.Empty, MAWB.CM_MessageReference);
		}

		#region Implementation
		CusMAWB fMAWB;
		CusMAWB MAWB
		{
			get { return fMAWB ?? (fMAWB = GetNewMAWB()); }
		}

		CusMAWB GetNewMAWB()
		{
			return (CusMAWB)GetNewBusinessObject();
		}

		protected virtual int GetMAWBCount(string mAWBNo)
		{
			return new CusMAWB.Loader(Factory).FindMatchingMAWBs(mAWBNo).Length;
		}

		protected virtual BusinessObject[] GetMAWBsByConsol(ForwardingConsol consol)
		{
			return new CusMAWB.Loader(Factory).FindMatchingMAWBs(consol.PK, false);
		}

		protected virtual BusinessObject GetFirstMatchingMAWB(string mAWBNo)
		{
			return new CusMAWB.Loader(Factory).FindFirstMatchingMAWB(mAWBNo);
		}

		protected virtual BusinessObject GetFirstMatchingCTOMAWB(string mAWBNo)
		{
			return new CusMAWB.Loader(Factory).FindFirstMatchingCTOMAWB(mAWBNo);
		}

		protected virtual BusinessObject[] GetMatchingCTOMAWBs(ZString flightNumber, ZDateTime arrivalDate)
		{
			return new CusMAWB.Loader(Factory).FindMatchingCTOMAWBs(flightNumber, arrivalDate);
		}

		protected virtual BusinessObject[] GetMatchingForwarderMAWBs(ZString mAWBNo, ZString flightNumber, ZDateTime arrivalDate)
		{
			return new CusMAWB.Loader(Factory).FindMatchingForwarderMAWBs(mAWBNo, flightNumber, arrivalDate);
		}

		protected virtual ZDBOnlyQuery GetMAWBDBOnlyQuery(ZString flightNumber, ZDateTime arrivalDate)
		{
			return new CusMAWB.Loader(Factory).GetMAWBQuery(flightNumber, arrivalDate);
		}
		#endregion
	}
}
