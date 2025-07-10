using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	internal class JobMawbTest : BaseFreightTest
	{
		#region TestContactOrg

		public void TestContactOrg()
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			JobMawb mawb = Factory.New<JobMawb>();
			job.JH_ParentID = mawb.PK;
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Bob's Company";
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;

			IDocumentDeliveryContact contact = mawb.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY);
			AssertEquals("ContactType is LocalClient", "Bob's Company", contact.OrgHeader.FullName);
			contact = mawb.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Receivables, DocumentDirection.ANY);
			AssertEquals("ContactType is Receivables", "Bob's Company", contact.OrgHeader.FullName);
			contact = mawb.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Miscellaneous, DocumentDirection.ANY);
			AssertEquals("Other ContactType", null, contact.OrgHeader);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			AssertEquals("MAWB", mawb.HumanReadableName);

			mawb.JM_Airline3DigitPrefix = "001";
			AssertEquals("MAWB", mawb.HumanReadableName);

			mawb.JM_MAWB = "00000001";
			AssertEquals("MAWB 001-00000001", mawb.HumanReadableName);
		}

		#endregion

		#region Service Level

		public void TestCarrierServiceLevels_NeutralAirWaybillServiceLevelList()
		{
			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "191").PK;
			OrgCarrierServiceLevel lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "XXX";
			lvl.PL_CarrierServiceLevelDescription = "XXX";
			lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "202").PK;
			lvl = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "ZUB";
			lvl.PL_CarrierServiceLevelDescription = "ZUBIN";

			Factory.Save();

			AssertEquals("STD and ALL", 2, mawb.NeutralAirWaybillServiceLevels.Count);

			mawb.JM_Airline3DigitPrefix = "191";
			AssertEquals("Two defined service levels + STD + ALL", 4, mawb.NeutralAirWaybillServiceLevels.Count);

			mawb.JM_Airline3DigitPrefix = "202";
			AssertEquals("One defined service level + STD + ALL", 3, mawb.NeutralAirWaybillServiceLevels.Count);

			mawb.JM_Airline3DigitPrefix = "";
			AssertEquals("STD + ALL", 2, mawb.NeutralAirWaybillServiceLevels.Count);
		}

		public void TestCarrierServiceLevels_NeutralAirWaybillServiceLevelList_ControllingBranchIsUsedToMatchCarrier()
		{
			var companyAUS = Factory.NewWithValidTestData<GlbCompany>();
			companyAUS.GC_Code = "AUS";
			var branchSYD = companyAUS.Branches.AddNew();
			branchSYD.GB_Code = "SY";
			var branchMEL = companyAUS.Branches.AddNew();
			branchMEL.GB_Code = "MEL";

			var companyUSA = Factory.NewWithValidTestData<GlbCompany>();
			companyUSA.GC_Code = "US";
			var branchLAX = companyUSA.Branches.AddNew();
			branchLAX.GB_Code = "LAX";
			var branchNYC = companyUSA.Branches.AddNew();
			branchNYC.GB_Code = "NYC";

			var companyNZ = Factory.NewWithValidTestData<GlbCompany>();
			companyNZ.GC_Code = "NZ";
			var branchAKL = companyNZ.Branches.AddNew();
			branchAKL.GB_Code = "AKL";

			CreateCarrier("001", branchLAX.PK, "A");
			CreateCarrier("001", branchSYD.PK, "B");
			CreateCarrier("001", branchMEL.PK, "C");

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_GB = branchSYD.PK;
				mawb.JM_Airline3DigitPrefix = "001";
				AssertContainsExactElementsInAnyOrder(new[] { "B", "STD", "ALL" }, mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().Select(x => x.PL_Code));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchLAX.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_GB = branchLAX.PK;
				mawb.JM_Airline3DigitPrefix = "001";
				AssertContainsExactElementsInAnyOrder(new[] { "A", "STD", "ALL" }, mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().Select(x => x.PL_Code));

				mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_GB = branchNYC.PK;
				mawb.JM_Airline3DigitPrefix = "001";
				AssertContainsExactElementsInAnyOrder(new[] { "A", "STD", "ALL" }, mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().Select(x => x.PL_Code));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchMEL.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_GB = branchSYD.PK;
				mawb.JM_Airline3DigitPrefix = "001";
				AssertContainsExactElementsInAnyOrder(new[] { "B", "STD", "ALL" }, mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().Select(x => x.PL_Code));

				mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_GB = branchMEL.PK;
				mawb.JM_Airline3DigitPrefix = "001";
				AssertContainsExactElementsInAnyOrder(new[] { "C", "STD", "ALL" }, mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().Select(x => x.PL_Code));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchAKL.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_GB = branchAKL.PK;
				mawb.JM_Airline3DigitPrefix = "001";
				var serviceLevelsCode = mawb.NeutralAirWaybillServiceLevels.Cast<OrgCarrierServiceLevel>().Select(x => x.PL_Code).ToArray();
				AssertEquals(3, serviceLevelsCode.Length);
				Assert(serviceLevelsCode.Contains("STD"));
				Assert(serviceLevelsCode.Contains("ALL"));
			}
		}

		void CreateCarrier(ZString airlineCode, ZGuid branchCode, params ZString[] serviceLevels)
		{
			var airline = RefAirline.LoadFromAirlinePrefix(Factory, "001");
			if (airline == null)
			{
				airline = Factory.New<RefAirline>();
				airline.RM_EagleAddedAirlinePrefixOrAccountingCode = airlineCode;
			}

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.CompanyData.OB_GB_ControllingBranch = branchCode;

			foreach (var serviceLevel in serviceLevels)
			{
				var lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
				lvl.PL_Code = serviceLevel;
				lvl.PL_CarrierServiceLevelDescription = serviceLevel + " description";
			}
		}

		#endregion

		#region Test HomePortText

		public void TestJM_Calc_HomePortText()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			AssertEquals("Expected the home port to be the formatted main unloco of the MAWB's branch", "AUBNE - Brisbane", mawb.JM_Calc_HomePortText);
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public void TestImplementsIJobInvoicingPlugIn()
		{
			JobMawb mawb = Factory.New<JobMawb>();

			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_MAWB = "11111111";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			AssertEquals("JobNumber", "M08111111111", ((IJobInvoicingPlugIn)mawb).JobNumber);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<JobMawb>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<JobMawb>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region TestJM_Calc_IsNeutral

		public void TestJM_Calc_IsNeutral()
		{
			Mawb.JM_Calc_IsNeutral = false;
			AssertEquals(true, Mawb.JM_IsPaper);
			AssertEquals(false, Mawb.JM_Calc_IsNeutral);

			Mawb.JM_Calc_IsNeutral = true;
			AssertEquals(false, Mawb.JM_IsPaper);
			AssertEquals(true, Mawb.JM_Calc_IsNeutral);
		}

		#endregion

		#region JM_IsPrinted

		public void TestMAWB_IsPrintedSet_ShouldBeAbleToReturnUnallocatedPrintedMawbToAvailableStock()
		{
			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");

			var deallocateCalled = 0;
			mawb.UncheckPrintedNeutralMAWB += (sender, e) => { deallocateCalled++; e.Cancel = false; };

			Env.Security.JobMAWBResetPrintedFlag.IsAllowed = false;
			mawb.MAWB_IsPrinted = false;
			AssertEquals(true, mawb.JM_IsPrinted);
			AssertEquals("Deallocate should not be called", 0, 0);

			Env.Security.JobMAWBResetPrintedFlag.IsAllowed = true;
			mawb.MAWB_IsPrinted = false;
			AssertEquals(false, mawb.JM_IsPrinted);
			AssertEquals("Deallocate should be called once", 1, deallocateCalled);

			mawb.MAWB_IsPrinted = false;
			AssertEquals("IsPrinted should not be reset", false, mawb.JM_IsPrinted);
			AssertEquals("Deallocate should not be called again", 1, deallocateCalled);

			Factory.Save();

			AssertEquals(false, mawb.JM_IsPrinted);
			Assert("Is Printed checkbox is readonly after save", mawb.MAWB_IsPrintedInfo.ReadOnly);
		}

		public void TestMAWB_IsPrintedSet_ShouldThrowException_WhenTheValueIsTrue()
		{
			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");
			AssertExceptionThrown<InvalidOperationException>(() => mawb.MAWB_IsPrinted = true);
		}

		JobMawb CreateMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;
			mawb.JM_ParentID = ZGuid.Empty;
			mawb.JM_IsPrinted = true;

			return mawb;
		}

		#endregion

		#region JM_OA_From

		public void TestSetJM_OAFromOrganization_ShouldDefaultAddressToMainAddress()
		{
			var borrower = Factory.New<OrgHeader>();
			borrower.OH_RL_NKClosestPort = "NZAKL";
			borrower.OH_IsGlobalAccount = true;
			borrower.OH_Code = "BORR";

			var addressMel = borrower.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");

			mawb.JM_OA_From_ZAddress.OrgPK = ZGuid.Empty;
			mawb.JM_OA_From_ZAddress.OrgPK = borrower.PK;

			AssertEquals("Address will fallback to Main Office Address", borrower.MainAddress.PK, mawb.JM_OA_From_ZAddress.AddressFK);

			addressMel.OA_IsActive = true;
			borrower.MainAddress.OA_IsActive = false;

			mawb.JM_OA_From_ZAddress.OrgPK = ZGuid.Empty;
			mawb.JM_OA_From_ZAddress.OrgPK = borrower.PK;

			AssertEquals("Address will fallback to First Active Address", addressMel.PK, mawb.JM_OA_From_ZAddress.AddressFK);

			addressMel.OA_IsActive = false;
			borrower.MainAddress.OA_IsActive = false;

			mawb.JM_OA_From_ZAddress.OrgPK = ZGuid.Empty;
			mawb.JM_OA_From_ZAddress.OrgPK = borrower.PK;

			AssertEquals("Address will fallback to Main Address even when it is not Active", borrower.MainAddress.PK, mawb.JM_OA_From_ZAddress.AddressFK);
		}

		#endregion

		#region Logging

		public void TestLogMessage()
		{
			RefAirline airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "ZN"));
			ZString prefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = "99991111";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			CommonConsol consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			consol.JK_RL_NKDischargePort = "SGSIN";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ZN111";

			Factory.Save();
			AssertEquals("Should have allocated mawb.", prefix + "99991111", consol.JK_MasterBillNum);

			Assert("Log for attaching Mawb to console", ContainsALogWithThisReference(mawb.Logs, string.Format("Attached to job {0}", mawb.JM_Calc_ParentJobNumber)));

			mawb.JM_IsPrinted = true;
			Factory.Save();
			Assert("Log for printing Mawb to console", ContainsALogWithThisReference(mawb.Logs, string.Format("Printed for job {0}", mawb.JM_Calc_ParentJobNumber)));

			consol.JK_IsNeutralMaster = false;
			ZString parentJobNumber = mawb.GetParentJobNumber((ZGuid)mawb.JM_ParentIDInfo.OriginalValue, (ZString)mawb.JM_ParentTableCodeInfo.OriginalValue);
			Factory.Save();
			Assert("Log for detaching Mawb from console", ContainsALogWithThisReference(mawb.Logs, string.Format("Detached printed MAWB from job {0}", parentJobNumber)));
		}

		bool ContainsALogWithThisReference(Logs logs, string reference)
		{
			return logs?.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.EqualsIgnoringCase(reference)) ?? false;
		}

		#endregion

		#region TestUpdateIsCompletedAWBReturned_And_IsBorrowedAWBInvoicedFields

		public void TestUpdateIsCompletedAWBReturned_And_IsBorrowedAWBInvoicedFields()
		{
			ZQuery filter = new ZQuery();
			var mawb = Factory.LoadTop1<JobMawb>(filter);

			//If there is nothing in the related table
			if (mawb == null)
			{
				mawb = Factory.New<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "000";
				mawb.JM_MAWB = "00000000";
				Factory.Save();
				mawb = Factory.LoadTop1<JobMawb>(filter);
			}

			//Setting exactly inversed of whatever already existed
			bool lTempIsCompleted = mawb.JM_IsCompletedAWBReturned, lTempIsBorrowed = mawb.JM_IsBorrowedAWBInvoiced;
			mawb.JM_IsCompletedAWBReturned = !lTempIsCompleted;
			mawb.JM_IsBorrowedAWBInvoiced = !lTempIsBorrowed;
			Factory.Save();

			//Check the actual values in the DB
			mawb.Reload();

			AssertEquals("JM_IsCompletedAWBReturned can not be updated properly", !lTempIsCompleted, mawb.JM_IsCompletedAWBReturned);
			AssertEquals("JM_IsBorrowedAWBInvoiced can not be updated properly", !lTempIsBorrowed, mawb.JM_IsBorrowedAWBInvoiced);
		}

		public void TestCreateAccountingJobOnIJobInvoicingPlugin()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			IJobInvoicingPlugIn plugIn = mawb;

			AssertEquals("IJobInvoicingPlugIn.CreateAccountingJobOnSavingOfOperationsJob",
						 false, plugIn.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);

			AssertNull("IJobInvoicingPlugIn.OperationsBranch", plugIn.InvoicingSupporter.OperationsBranch);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.JobMAWBAuditBilling, Mawb.InvoicingSupporter.AuditSecurity);
		}

		#endregion

		#region NewProperties

		public void TestJM_Calc_ParentJobNumber()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "111";
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_ParentID = consol.PK;
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("Expecting a particular ConsolNumber", "111", mawb.JM_Calc_ParentJobNumber);
			mawb.JM_ParentID = ZGuid.Empty;
			AssertEquals("No consol is attached so empty ConsolNumber would be expected.", true, mawb.JM_Calc_ParentJobNumber.IsEmpty);

			IQuotedBooking quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.UniqueConsignRef = "222";

			mawb.JM_ParentID = ((BusinessObject)quotedBooking).PK;
			mawb.JM_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Expecting a particular BookingShipment number", "222", mawb.JM_Calc_ParentJobNumber);
			mawb.JM_ParentID = ZGuid.Empty;
			AssertEquals("No CommonShipment is attached so empty job number would be expected.", true, mawb.JM_Calc_ParentJobNumber.IsEmpty);
		}

		public void TestJM_Calc_UsageIndicator()
		{
			CommonConsol consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			IMAWBParent quotedBooking = (IMAWBParent)ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_ParentID = consol.PK;
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("Usage Indicator Text", "Used by:", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentID = ZGuid.NewZGuid();
			AssertEquals("Usage Indicator Text", "Allocated to a non-existing job.", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentID = ZGuid.Empty;
			AssertEquals("Usage Indicator Text", "Unavailable.", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentTableCode = "";
			AssertEquals("Usage Indicator Text", "Available to allocate.", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentID = quotedBooking.PK;
			mawb.JM_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Usage Indicator Text", "Used by:", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentID = ZGuid.NewZGuid();
			AssertEquals("Usage Indicator Text", "Allocated to a non-existing job.", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentID = ZGuid.Empty;
			AssertEquals("Usage Indicator Text", "Unavailable.", mawb.JM_Calc_UsageIndicator);

			mawb.JM_ParentTableCode = "";
			AssertEquals("Usage Indicator Text", "Available to allocate.", mawb.JM_Calc_UsageIndicator);

			mawb.JM_IsPrinted = true;
			AssertEquals("Usage Indicator Text", "Previously printed and un-allocated.", mawb.JM_Calc_UsageIndicator);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestParent()
		{
			CommonConsol consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			IMAWBParent quotedBooking = (IMAWBParent)ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_ParentID = consol.PK;
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("Parent of Mawb", consol.PK, mawb.Parent.PK);
			mawb.JM_ParentID = quotedBooking.PK;
			mawb.JM_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Parent of Mawb", quotedBooking.PK, mawb.Parent.PK);
			mawb.JM_ParentTableCode = JobDeclarationSchema.Constants.Prefix; //Invalid prefix would raise exception
			object parent = mawb.Parent; //invoking the property
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent jobMawb = Factory.New<JobMawb>();
			Assert(jobMawb.AllowInvoiceDeletion);
		}

		#endregion

		#region ICanDelete Members

		public void TestJobMawb_CanDeleteAndReasons()
		{
			JobMawb mawbAllocatedForAConsol = Factory.New<JobMawb>();
			AssertEquals("Mawb should be deletable", true, mawbAllocatedForAConsol.CanDelete);

			IMAWBParent consol = (IMAWBParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			mawbAllocatedForAConsol.JM_ParentID = consol.PK;
			mawbAllocatedForAConsol.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("Mawb should not be deletable when allocated to a consol", false, mawbAllocatedForAConsol.CanDelete);
			AssertEquals("Mawb message when allocated to a consol",
				$"{mawbAllocatedForAConsol.HumanReadableName} cannot be deleted as it is allocated to {(consol as BusinessObject)?.HumanReadableName}.",
				mawbAllocatedForAConsol.ReasonForNotAbleToDelete);

			JobMawb mawbAllocatedForABooking = Factory.New<JobMawb>();
			IMAWBParent quotedBooking = (IMAWBParent)ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			mawbAllocatedForABooking.JM_ParentID = quotedBooking.PK;
			mawbAllocatedForABooking.JM_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Mawb should not be deletable when allocated to a booking", false, mawbAllocatedForABooking.CanDelete);
			AssertEquals("Mawb message when allocated to a booking",
				$"{mawbAllocatedForABooking.HumanReadableName} cannot be deleted as it is allocated to {(quotedBooking as BusinessObject)?.HumanReadableName}.",
				mawbAllocatedForABooking.ReasonForNotAbleToDelete);

			JobMawb mawbHasBeenPrinted = Factory.New<JobMawb>();
			mawbAllocatedForABooking.JM_IsPrinted = true;
			AssertEquals("Mawb should not be deletable when printed", false, mawbAllocatedForABooking.CanDelete);
			AssertEquals("Mawb message when printed",
				$"{mawbAllocatedForABooking.HumanReadableName} cannot be deleted as it has been printed.",
				mawbAllocatedForABooking.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDelete

		public void TestCannotDelete_WhenAllocated()
		{
			var consol = (IMAWBParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_ParentID = consol.PK;
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			AssertNotNull(mawb.Parent);
			AssertExceptionThrown<CannotDeleteException>(mawb.Delete);
		}

		#endregion

		#region GetNeutralAirWaybillServiceLevelsFrom2LetterCode

		public void TestGetNeutralAirWaybillServiceLevelsFrom2LetterCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "MU").PK;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XXX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "XXX Description";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "EXP";
			serviceLevel2.PL_CarrierServiceLevelDescription = "EXP Description";

			Factory.Save();

			var serviceLevelCollection = JobMawb.GetNeutralAirWaybillServiceLevelsFrom2LetterCode(Factory, GlbBranch.CurrentBranch, "MU");
			serviceLevelCollection.Load();
			AssertEquals(3, serviceLevelCollection.Count);
			var serviceLevels = serviceLevelCollection.Cast<OrgCarrierServiceLevel>().ToArray();
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == "XXX"));
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == "EXP"));
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == OrgCarrierServiceLevel.StandardCode));

			serviceLevelCollection = JobMawb.GetNeutralAirWaybillServiceLevelsFrom2LetterCode(Factory, GlbBranch.CurrentBranch, "SQ");
			serviceLevelCollection.Load();
			AssertEquals(1, serviceLevelCollection.Count);
			serviceLevels = serviceLevelCollection.Cast<OrgCarrierServiceLevel>().ToArray();
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == OrgCarrierServiceLevel.StandardCode));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Mawb = Factory.New(typeof(JobMawb)) as JobMawb;
		}

		JobMawb Mawb;

		#endregion
	}
}
