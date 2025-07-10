using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Recruiter.Business.GlbAccreditationAttempt;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttempt))]
	sealed class GlbAccreditationAttemptTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChainOfAttemptsSpecialisation()
		{
			SetupMappings();

			GlbPerson person = CreatePerson();
			Factory.Save();

			var cco = CreateAccreditation("CCO", "CCO", false, null);
			cco.HAC_ValidityMonths = 12;

			var oc = CreateAccreditation("OC", "OC", false, null);
			var fwl = CreateAccreditation("FWL", "FWL", false, null);

			var attemptOC = CreateAttempt(person, oc, new ZDate(2016, 1, 1), new ZDate(2016, 5, 5), ZDate.Invalid);
			attemptOC.HAA_CompletionDueDate = new ZDate(2016, 5, 5);

			var attemptCCO = CreateAttempt(person, cco, new ZDate(2015, 1, 1), new ZDate(2015, 5, 5), ZDate.Invalid);
			attemptCCO.HAA_CompletionDueDate = new ZDate(2015, 5, 5);

			var attemptFWL = CreateAttempt(person, fwl, new ZDate(2016, 3, 3), new ZDate(2016, 4, 4), ZDate.Invalid);
			attemptFWL.HAA_CompletionDueDate = new ZDate(2016, 5, 5);

			Factory.Save();

			AssertEquals(2, attemptOC.ChainOfPreRequesiteAttempts.Count());
			AssertEquals(attemptCCO.PK, attemptOC.ChainOfPreRequesiteAttempts.OrderBy(a => a.HAA_CommencementDate).FirstOrDefault().HAA_PK);
			AssertEquals(1, attemptFWL.ChainOfPreRequesiteAttempts.Count());
		}

		[TestDate(2020, 3, 3)]
		public void TestSpecialisationResync()
		{
			SetupMappings();

			GlbPerson person = CreatePerson();
			Factory.Save();

			// create cco, rco, ccs, rcs
			var cco = CreateAccreditation("CCO", "CCO", false, null);
			cco.HAC_ValidityMonths = 12;
			var rco = CreateAccreditation("RCO", "CCO", true, cco);
			rco.HAC_ValidityMonths = 12;
			var ccs = CreateAccreditation("CCS", "CCS", false, cco);
			ccs.HAC_ValidityMonths = 12;
			var rcs = CreateAccreditation("RCS", "CCS", true, ccs);
			rcs.HAC_ValidityMonths = 12;

			// create oc, ot, s1
			var oc = CreateAccreditation("OC", "OC", false, null);
			var ot = CreateAccreditation("OT", "OT", false, null);
			var fwl = CreateAccreditation("FWL", "FWL", false, null);

			// create attempts
			var attemptOC = CreateAttempt(person, oc, new ZDate(2016, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptOC.HAA_CompletionDueDate = new ZDate(2016, 3, 3);
			var attemptOT = CreateAttempt(person, ot, new ZDate(2016, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptOT.HAA_CompletionDueDate = new ZDate(2016, 2, 2);

			var attemptCCO = CreateAttempt(person, cco, new ZDate(2015, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptCCO.HAA_CompletionDueDate = new ZDate(2015, 3, 3);

			var attemptFWL = CreateAttempt(person, fwl, new ZDate(2016, 3, 3), ZDate.Invalid, ZDate.Invalid);
			attemptFWL.HAA_CompletionDueDate = new ZDate(2016, 5, 5);
			var attemptCCS = CreateAttempt(person, ccs, new ZDate(2016, 3, 3), ZDate.Invalid, ZDate.Invalid);
			attemptCCS.HAA_CompletionDueDate = new ZDate(2017, 3, 3);

			Factory.Save();

			var attemptRCO = CreateAttempt(person, rco, new ZDate(2015, 5, 5), ZDate.Invalid, ZDate.Invalid);
			attemptRCO.HAA_CompletionDueDate = new ZDate(2016, 3, 3);
			var attemptRCS = CreateAttempt(person, rcs, new ZDate(2016, 8, 8), ZDate.Invalid, ZDate.Invalid);
			attemptRCS.HAA_CompletionDueDate = new ZDate(2016, 3, 3);

			Factory.Save();

			attemptCCO.HAA_CompletionDate = new ZDate(2015, 1, 1);

			Factory.Save();

			AssertEquals(2, attemptCCO.Updater.RunParams.Count);
			AssertUpdater(oc, attemptCCO.HAA_CommencementDate, attemptCCO.HAA_ExpiryDate, attemptCCO);
			AssertUpdater(ot, attemptCCO.HAA_CommencementDate, attemptCCO.HAA_ExpiryDate, attemptCCO);
			Factory.Save();

			attemptRCO.HAA_CompletionDate = new ZDate(2016, 1, 1);
			Factory.Save();

			AssertEquals(2, attemptRCO.Updater.RunParams.Count);
			AssertUpdater(oc, attemptCCO.HAA_CommencementDate, attemptRCO.HAA_ExpiryDate, attemptRCO);
			AssertUpdater(ot, attemptCCO.HAA_CommencementDate, attemptRCO.HAA_ExpiryDate, attemptRCO);
			Factory.Save();

			attemptCCS.HAA_CommencementDate = new ZDate(2020, 1, 1); // <-- gap
			attemptCCS.HAA_CompletionDate = new ZDate(2020, 1, 1);
			Factory.Save();
			AssertEquals(0, attemptCCS.Updater.RunParams.Count);
			Factory.Save();

			attemptCCS.ResetEarliestCommencementDate();
			attemptCCS.HAA_CommencementDate = new ZDate(2017, 1, 1); // <-- no gap
			attemptCCS.HAA_CompletionDate = new ZDate(2017, 1, 1);
			Factory.Save();

			AssertEquals(1, attemptCCS.Updater.RunParams.Count);
			AssertUpdater(fwl, attemptCCO.HAA_CommencementDate, attemptCCS.HAA_ExpiryDate, attemptCCS);
			Factory.Save();

			attemptRCS.HAA_CompletionDate = new ZDate(2018, 1, 1);
			Factory.Save();
			AssertEquals(1, attemptRCS.Updater.RunParams.Count);
			AssertUpdater(fwl, attemptCCO.HAA_CommencementDate, attemptRCS.HAA_ExpiryDate, attemptRCS);
		}

		static void SetupMappings()
		{
			var mappings = new CertificationCodeMappingCollection();
			mappings.AddPair("CCO", "OC");
			mappings.AddPair("CCO", "OT");
			mappings.AddPair("CCO", "OW");
			mappings.AddPair("CCO", "OL");
			mappings.AddPair("CCO", "OF");

			mappings.AddPair("CCS", "FWT");
			mappings.AddPair("CCS", "FWL");
			mappings.AddPair("CCS", "FTL");
			mappings.AddPair("CCS", "FCW");
			mappings.AddPair("CCS", "FCT");
			mappings.AddPair("CCS", "FCL");
			mappings.AddPair("CCS", "CWT");
			mappings.AddPair("CCS", "CTL");

			RecruiterDataRegistry.Instance.CertificateCodeSpecialialisationMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("CCO", (NoResString)"CCO");
			certCodes.Add("CCS", (NoResString)"CCS");
			certCodes.Add("OC", (NoResString)"OC");
			certCodes.Add("OT", (NoResString)"OT");
			certCodes.Add("OW", (NoResString)"OW");
			certCodes.Add("OL", (NoResString)"OL");
			certCodes.Add("OF", (NoResString)"OF");
			certCodes.Add("FWT", (NoResString)"FWT");
			certCodes.Add("FWL", (NoResString)"FWL");
			certCodes.Add("FTL", (NoResString)"FTL");
			certCodes.Add("FCW", (NoResString)"FCW");
			certCodes.Add("FCT", (NoResString)"FCT");
			certCodes.Add("FCL", (NoResString)"FCL");
			certCodes.Add("CWT", (NoResString)"CWL");
			certCodes.Add("CTL", (NoResString)"CTL");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);
		}

		static void AssertUpdater(GlbAccreditation accred, ZDate commence, ZDate expiry, GlbAccreditationAttemptForTest attempt)
		{
			RunParams runParams = null;
			foreach (var rp in attempt.Updater.RunParams)
			{
				if (rp.AccredCode == accred.HAC_Code)
				{
					runParams = rp;
					break;
				}
			}

			AssertNotNull(runParams);

			AssertEquals(accred.PK, runParams.ParentAccreditationPK);
			AssertEquals("Commencement", commence, runParams.FromDate);
			AssertEquals("Expiry", expiry, runParams.ToDate);
			AssertEquals(10, runParams.AdditionalCompletionToleranceDays);
		}

		[TestDate(2020, 3, 3)]
		public void TestSpecialisation_Simple()
		{
			// set registry
			SetupMappings();

			GlbPerson person = CreatePerson();
			Factory.Save();

			// create cco, rco, ccs, rcs
			var cco = CreateAccreditation("CCO", "CCO", false, null);
			cco.HAC_ValidityMonths = 12;

			// create oc, ot, s1
			var oc = CreateAccreditation("OC", "OC", false, null);
			oc.HAC_ValidityMonths = 3;

			// create attempts
			var attemptOC = CreateAttempt(person, oc, new ZDate(2016, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptOC.HAA_CompletionDueDate = new ZDate(2016, 3, 3);
			var attemptCCO = CreateAttempt(person, cco, new ZDate(2015, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptCCO.HAA_CompletionDueDate = new ZDate(2015, 3, 3);
			Factory.Save();

			attemptCCO.HAA_CompletionDate = new ZDate(2015, 1, 1);

			Factory.Save();

			AssertEquals("OC", attemptCCO.Updater.RunParams[0].AccredCode);
			AssertEquals(oc.PK, attemptCCO.Updater.RunParams[0].ParentAccreditationPK);
			AssertEquals(attemptCCO.HAA_CommencementDate, attemptCCO.Updater.RunParams[0].FromDate);
			AssertEquals(attemptCCO.HAA_ExpiryDate, attemptCCO.Updater.RunParams[0].ToDate);
			AssertEquals(10, attemptCCO.Updater.RunParams[0].AdditionalCompletionToleranceDays);
		}

		[TestDate(2020, 3, 3)]
		public void TestSpecialisation_Simple_WithResyncParams()
		{
			// set registry
			SetupMappings();

			GlbPerson person = CreatePerson();
			Factory.Save();

			// create cco, rco, ccs, rcs
			var cco = CreateAccreditation("CCO", "CCO", false, null);
			cco.HAC_ValidityMonths = 12;

			// create oc, ot, s1
			var oc = CreateAccreditation("OC", "OC", false, null);
			oc.HAC_ValidityMonths = 3;

			// create attempts
			var attemptOC = CreateAttempt(person, oc, new ZDate(2016, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptOC.HAA_CompletionDueDate = new ZDate(2016, 3, 3);
			var attemptCCO = CreateAttempt(person, cco, new ZDate(2015, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptCCO.HAA_CompletionDueDate = new ZDate(2015, 3, 3);
			Factory.Save();

			attemptCCO.SetTempResyncParams(true, new AccreditationUpdaterParameters(true, 13, 13, new ZDate(2015, 7, 10)));

			attemptCCO.HAA_CompletionDate = new ZDate(2015, 1, 1);

			Factory.Save();

			AssertEquals("OC", attemptCCO.Updater.RunParams[0].AccredCode);
			AssertEquals(oc.PK, attemptCCO.Updater.RunParams[0].ParentAccreditationPK);
			AssertEquals(attemptCCO.HAA_CommencementDate, attemptCCO.Updater.RunParams[0].FromDate);
			AssertEquals(new ZDate(2015, 7, 10), attemptCCO.Updater.RunParams[0].ToDate);
			AssertEquals(13, attemptCCO.Updater.RunParams[0].AdditionalCompletionToleranceDays);
		}

		[TestDate(2020, 3, 3)]
		public void TestSpecialisation_Simple_SyncRelatedOnCompletion()
		{
			// set registry
			SetupMappings();

			GlbPerson person = CreatePerson();
			Factory.Save();

			// create cco, rco, ccs, rcs
			var cco = CreateAccreditation("CCO", "CCO", false, null);
			cco.HAC_ValidityMonths = 12;

			// create oc, ot, s1
			var oc = CreateAccreditation("OC", "OC", false, null);
			oc.HAC_ValidityMonths = 3;

			// create attempts
			var attemptOC = CreateAttempt(person, oc, new ZDate(2016, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptOC.HAA_CompletionDueDate = new ZDate(2016, 3, 3);
			var attemptCCO = CreateAttempt(person, cco, new ZDate(2015, 1, 1), ZDate.Invalid, ZDate.Invalid);
			attemptCCO.HAA_CompletionDueDate = new ZDate(2015, 3, 3);
			Factory.Save();

			attemptCCO.HAA_CompletionDate = new ZDate(2015, 1, 1);
			AssertNull(attemptCCO.Updater);
		}

		class GlbAccreditationAttemptForTest : GlbAccreditationAttempt
		{
			public GlbAccreditationAttemptForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public AccreditationUpdaterForPersonForTest Updater;
			protected override AccreditationUpdaterForPerson GetAccreditationUpdater()
			{
				Updater = new AccreditationUpdaterForPersonForTest();
				return Updater;
			}

			public void ResetEarliestCommencementDate()
			{
				earliestAttemptCommencementDate = null;
			}
		}

		class AccreditationUpdaterForPersonForTest : AccreditationUpdaterForPerson
		{
			public AccreditationUpdaterForPersonForTest()
			{
			}

			public List<RunParams> RunParams = new List<RunParams>();

			public override void Run()
			{
				var runParams = new RunParams();
				runParams.AccredCode = ParentAccreditation.HAC_Code;
				runParams.ParentAccreditationPK = ParentAccreditation.PK;
				runParams.FromDate = FromDate;
				runParams.ToDate = ToDate;
				runParams.AdditionalCompletionToleranceDays = AdditionalCompletionToleranceDays;

				RunParams.Add(runParams);
			}
		}

		class RunParams
		{
			public string AccredCode;
			public ZGuid ParentAccreditationPK;
			public ZDate FromDate;
			public ZDate ToDate;
			public int AdditionalCompletionToleranceDays;
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Single()
		{
			GlbPerson person = CreatePerson();

			var accred = CreateAccreditation("AC1", "CUS", false, null);
			var attempt = CreateAttempt(person, accred, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Two()
		{
			GlbPerson person = CreatePerson();

			var accred = CreateAccreditation("AC1", "CUS", false, null);
			var attempt1 = CreateAttempt(person, accred, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 1, 1));
			var attempt2 = CreateAttempt(person, accred, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2021, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt2.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Single_NoOverlap()
		{
			GlbPerson person = CreatePerson();

			var accred1 = CreateAccreditation("AC1", "CUS", false, null);
			var accred2 = CreateAccreditation("AC2", "CUS", false, accred1);

			var attempt1 = CreateAttempt(person, accred1, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2019, 12, 12));
			var attempt2 = CreateAttempt(person, accred2, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2021, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2020, 1, 1), attempt2.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Single_Extend()
		{
			GlbPerson person = CreatePerson();

			var accred1 = CreateAccreditation("AC1", "CUS", false, null);
			var accred2 = CreateAccreditation("AC2", "CUS", false, accred1);

			var attempt1 = CreateAttempt(person, accred1, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 2, 2));
			var attempt2 = CreateAttempt(person, accred2, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2021, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt2.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Refresher_Extend()
		{
			GlbPerson person = CreatePerson();

			var accred1 = CreateAccreditation("AC1", "CUS", false, null);
			var accred2 = CreateAccreditation("AC2", "CUS", true, accred1);

			var attempt1 = CreateAttempt(person, accred1, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 2, 2));
			var attempt2 = CreateAttempt(person, accred2, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2021, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt2.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_RefresherAndNextMain_Extend()
		{
			GlbPerson person = CreatePerson();

			var main1 = CreateAccreditation("AC1", "CUS", false, null);
			var ref1 = CreateAccreditation("AC2", "CUS", true, main1);
			var main2 = CreateAccreditation("AC3", "ZZZ", false, main1);

			var attempt1 = CreateAttempt(person, main1, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 2, 2));
			var attempt2 = CreateAttempt(person, ref1, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2021, 1, 1));
			var attempt3 = CreateAttempt(person, main2, new ZDate(2020, 3, 3), new ZDate(2020, 3, 3), new ZDate(2021, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt2.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt3.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_RefresherAndNextMain_NoOverlap()
		{
			GlbPerson person = CreatePerson();

			var main1 = CreateAccreditation("AC1", "CUS", false, null);
			var ref1 = CreateAccreditation("AC2", "CUS", true, main1);
			var main2 = CreateAccreditation("AC3", "ZZZ", false, main1);

			var attempt1 = CreateAttempt(person, main1, new ZDate(2019, 1, 1), new ZDate(2019, 1, 1), new ZDate(2020, 2, 2));
			var attempt2 = CreateAttempt(person, ref1, new ZDate(2020, 1, 1), new ZDate(2020, 1, 1), new ZDate(2020, 2, 2));
			var attempt3 = CreateAttempt(person, main2, new ZDate(2020, 3, 3), new ZDate(2020, 3, 3), new ZDate(2021, 1, 1));
			Factory.Save();

			AssertEquals(new ZDate(2019, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2019, 1, 1), attempt2.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2020, 3, 3), attempt3.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Big_NoGaps()
		{
			GlbPerson person = CreatePerson();

			var cco = CreateAccreditation("CCO", "CCO", false, null);
			var rco = CreateAccreditation("RCO", "CCO", true, cco);
			var ccs = CreateAccreditation("CCS", "CCS", false, cco);
			var rcs = CreateAccreditation("RCS", "CCS", true, ccs);
			var ccp = CreateAccreditation("CCP", "CCP", false, ccs);
			var rcp = CreateAccreditation("RCP", "CCP", true, ccp);

			var attempt1 = CreateAttempt(person, cco, new ZDate(2015, 1, 1), new ZDate(2015, 1, 1), new ZDate(2015, 12, 1));
			var attempt2 = CreateAttempt(person, rco, new ZDate(2015, 5, 5), new ZDate(2015, 5, 5), new ZDate(2016, 4, 4));
			var attempt3 = CreateAttempt(person, ccs, new ZDate(2016, 1, 1), new ZDate(2016, 1, 1), new ZDate(2016, 12, 1));
			var attempt4 = CreateAttempt(person, rcs, new ZDate(2016, 5, 5), new ZDate(2016, 5, 5), new ZDate(2017, 4, 4));
			var attempt5 = CreateAttempt(person, ccp, new ZDate(2017, 1, 1), new ZDate(2017, 1, 1), new ZDate(2017, 12, 1));
			var attempt6 = CreateAttempt(person, rcp, new ZDate(2017, 5, 5), new ZDate(2017, 5, 5), new ZDate(2018, 4, 4));

			Factory.Save();

			AssertEquals(new ZDate(2015, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt2.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt3.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt4.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt5.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt6.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Big_NoGaps_ExtraOldAttempt()
		{
			GlbPerson person = CreatePerson();

			var cco = CreateAccreditation("CCO", "CCO", false, null);
			var rco = CreateAccreditation("RCO", "CCO", true, cco);
			var ccs = CreateAccreditation("CCS", "CCS", false, cco);
			var rcs = CreateAccreditation("RCS", "CCS", true, ccs);
			var ccp = CreateAccreditation("CCP", "CCP", false, ccs);
			var rcp = CreateAccreditation("RCP", "CCP", true, ccp);

			var attempt0 = CreateAttempt(person, cco, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1), new ZDate(2014, 12, 1));
			var attempt1 = CreateAttempt(person, cco, new ZDate(2015, 1, 1), new ZDate(2015, 1, 1), new ZDate(2015, 12, 1));
			var attempt2 = CreateAttempt(person, rco, new ZDate(2015, 5, 5), new ZDate(2015, 5, 5), new ZDate(2016, 4, 4));
			var attempt3 = CreateAttempt(person, ccs, new ZDate(2016, 1, 1), new ZDate(2016, 1, 1), new ZDate(2016, 12, 1));
			var attempt4 = CreateAttempt(person, rcs, new ZDate(2016, 5, 5), new ZDate(2016, 5, 5), new ZDate(2017, 4, 4));
			var attempt5 = CreateAttempt(person, ccp, new ZDate(2017, 1, 1), new ZDate(2017, 1, 1), new ZDate(2017, 12, 1));
			var attempt6 = CreateAttempt(person, rcp, new ZDate(2017, 5, 5), new ZDate(2017, 5, 5), new ZDate(2018, 4, 4));

			Factory.Save();

			AssertEquals(new ZDate(2014, 1, 1), attempt0.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt2.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt3.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt4.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt5.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt6.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Big_NoGaps_ExtraOldAttempt_WithRefresher()
		{
			GlbPerson person = CreatePerson();

			var cco = CreateAccreditation("CCO", "CCO", false, null);
			var rco = CreateAccreditation("RCO", "CCO", true, cco);
			var ccs = CreateAccreditation("CCS", "CCS", false, cco);
			var rcs = CreateAccreditation("RCS", "CCS", true, ccs);
			var ccp = CreateAccreditation("CCP", "CCP", false, ccs);
			var rcp = CreateAccreditation("RCP", "CCP", true, ccp);

			var attempt0 = CreateAttempt(person, cco, new ZDate(2014, 1, 1), new ZDate(2014, 1, 1), new ZDate(2014, 8, 1));
			var attempt0ref = CreateAttempt(person, rco, new ZDate(2014, 4, 4), new ZDate(2014, 1, 1), new ZDate(2014, 12, 1));

			var attempt1 = CreateAttempt(person, cco, new ZDate(2015, 1, 1), new ZDate(2015, 1, 1), new ZDate(2015, 12, 1));
			var attempt2 = CreateAttempt(person, rco, new ZDate(2015, 5, 5), new ZDate(2015, 5, 5), new ZDate(2016, 4, 4));
			var attempt3 = CreateAttempt(person, ccs, new ZDate(2016, 1, 1), new ZDate(2016, 1, 1), new ZDate(2016, 12, 1));
			var attempt4 = CreateAttempt(person, rcs, new ZDate(2016, 5, 5), new ZDate(2016, 5, 5), new ZDate(2017, 4, 4));
			var attempt5 = CreateAttempt(person, ccp, new ZDate(2017, 1, 1), new ZDate(2017, 1, 1), new ZDate(2017, 12, 1));
			var attempt6 = CreateAttempt(person, rcp, new ZDate(2017, 5, 5), new ZDate(2017, 5, 5), new ZDate(2018, 4, 4));

			Factory.Save();

			AssertEquals(new ZDate(2014, 1, 1), attempt0.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2014, 1, 1), attempt0ref.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt2.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt3.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt4.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt5.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt6.EarliestAttemptCommencementDate);
		}

		[TestDate(2020, 3, 3)]
		public void TestEarliestAttemptCommencementDate_Requirements_Big_MainGap()
		{
			GlbPerson person = CreatePerson();

			var cco = CreateAccreditation("CCO", "CCO", false, null);
			var rco = CreateAccreditation("RCO", "CCO", true, cco);
			var ccs = CreateAccreditation("CCS", "CCS", false, cco);
			var rcs = CreateAccreditation("RCS", "CCS", true, ccs);
			var ccp = CreateAccreditation("CCP", "CCP", false, ccs);
			var rcp = CreateAccreditation("RCP", "CCP", true, ccp);

			var attempt1 = CreateAttempt(person, cco, new ZDate(2015, 1, 1), new ZDate(2015, 1, 1), new ZDate(2015, 12, 1));
			var attempt2 = CreateAttempt(person, rco, new ZDate(2015, 5, 5), new ZDate(2015, 5, 5), new ZDate(2016, 4, 4));
			var attempt3 = CreateAttempt(person, ccs, new ZDate(2016, 5, 5), new ZDate(2016, 5, 5), new ZDate(2016, 12, 1)); // <-- gap
			var attempt4 = CreateAttempt(person, rcs, new ZDate(2016, 6, 6), new ZDate(2016, 6, 6), new ZDate(2017, 4, 4));
			var attempt5 = CreateAttempt(person, ccp, new ZDate(2017, 1, 1), new ZDate(2017, 1, 1), new ZDate(2017, 12, 1));
			var attempt6 = CreateAttempt(person, rcp, new ZDate(2017, 5, 5), new ZDate(2017, 5, 5), new ZDate(2018, 4, 4));

			Factory.Save();

			AssertEquals(new ZDate(2015, 1, 1), attempt1.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2015, 1, 1), attempt2.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2016, 5, 5), attempt3.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2016, 5, 5), attempt4.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2016, 5, 5), attempt5.EarliestAttemptCommencementDate);
			AssertEquals(new ZDate(2016, 5, 5), attempt6.EarliestAttemptCommencementDate);
		}

		GlbPerson CreatePerson()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "name";
			person.PER_EmailAddress = "per@so.n";
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_PER = person.PK;
			person.ApplicantCollection.Add(applicant);
			return person;
		}

		GlbAccreditation CreateAccreditation(string code, string certCode, bool isRefresher, GlbAccreditation parent)
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_Code = code;
			accred.HAC_CertificateCode = certCode;
			accred.HAC_Description = code + " desc";
			accred.HAC_IsRefresher = isRefresher;
			if (isRefresher)
			{
				accred.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			}

			if (parent != null)
			{
				var pivot = Factory.New<GlbAccreditationRequirementPivot>();
				pivot.HAR_HAC = accred.PK;
				pivot.HAR_HAC_Parent = parent.PK;
			}

			return accred;
		}

		GlbAccreditationAttemptForTest CreateAttempt(GlbPerson person, GlbAccreditation accreditation, ZDate commenced, ZDate completed, ZDate expiry)
		{
			var attempt = Factory.New<GlbAccreditationAttemptForTest>();
			attempt.HAA_HAC = accreditation.PK;
			attempt.HAA_PER = person.PK;
			attempt.HAA_CommencementDate = commenced;

			if (completed.IsValid)
			{
				attempt.HAA_CompletionDate = completed;
				attempt.HAA_CompletionDueDate = completed;
			}

			if (expiry.IsValid)
			{
				attempt.HAA_ExpiryDate = expiry;
			}

			return attempt;
		}

		public void TestGetEmailAddressFromTriggerParty()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = GlbPerson.CreateFromStaff(Factory, staff);
			person.PER_EmailAddress = "personal@email.com";
			attempt.HAA_PER = person.PK;

			Assert(attempt is IEmailAddressGetterForTrigger);
			AssertEquals("personal@email.com", attempt.GetEmailAddressesFromTriggerParty(MessageRecipientPartyTypeList.Codes.PersonalEmail).Single());

			staff.GS_EmailAddress = "work@email.com";
			AssertEquals("work@email.com", attempt.GetEmailAddressesFromTriggerParty(MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail).Single());

			person.PER_EmailAddress = "";
			AssertEquals("work@email.com", attempt.GetEmailAddressesFromTriggerParty(MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail).Single());
		}

		public void TestCheckIsExpired()
		{
			var now = ZDateTime.Now.Date;

			var attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_ExpiryDate = now.AddDays(5);
			AssertEquals(false, attempt.CheckIsExpired(now));

			attempt.HAA_CommencementDate = now.AddDays(-5);
			AssertEquals(false, attempt.CheckIsExpired(now));

			attempt.HAA_CompletionDueDate = now.AddDays(1);
			AssertEquals(false, attempt.CheckIsExpired(now));

			attempt.HAA_CompletionDueDate = now.AddDays(-1);
			attempt.HAA_ExpiryDate = now.AddDays(5);
			AssertEquals(true, attempt.CheckIsExpired(now));

			attempt.HAA_CompletionDate = now.AddDays(-2);
			AssertEquals(false, attempt.CheckIsExpired(now));

			attempt.HAA_CompletionDate = now.AddDays(2);
			AssertEquals(false, attempt.CheckIsExpired(now));

			attempt.HAA_ExpiryDate = now.AddDays(-1);
			AssertEquals(true, attempt.CheckIsExpired(now));

			attempt.HAA_ExpiryDate = now.AddDays(5);
			AssertEquals(false, attempt.CheckIsExpired(now));
		}

		public void TestIsStarted()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			AssertEquals(false, attempt.IsStarted);
			AssertEquals(false, attempt.IsCompleted);

			attempt.HAA_CommencementDate = ZDate.BrettsBirthday;
			AssertEquals(true, attempt.IsStarted);
			AssertEquals(false, attempt.IsCompleted);
		}

		public void TestIsCompleted()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			AssertEquals(false, attempt.IsStarted);
			AssertEquals(false, attempt.IsCompleted);

			attempt.HAA_CompletionDate = ZDate.BrettsBirthday;
			AssertEquals(false, attempt.IsStarted);
			AssertEquals(true, attempt.IsCompleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			Factory.Save();

			var attempt = base.GetNewBusinessObject() as GlbAccreditationAttempt;
			attempt.HAA_HAC = accreditation.PK;

			return attempt;
		}

		public void TestLogs()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = ZDate.Today;
			attempt.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt.HAA_CompletionDate = ZDate.Empty;
			attempt.Accreditation.HAC_Code = "AC1";
			attempt.Accreditation.HAC_Description = "Description(123)";
			attempt.Logs.AddNew(AutoEvents.AccreditationAttemptCommenced, "AC1 - Description(123)");
			attempt.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "AC1 - Description(123)");
			Factory.Save();

			var log = attempt.Logs.GetAllLogs().OfType<StmALog>().Single(x => x.SL_SE_NKEvent == AutoEvents.AccreditationAttemptCommencedCode);
			AssertEquals("AC1 - Description(123)", log.SL_Reference);
			AssertEquals(false, log.SL_IsEstimate);
			AssertEquals(true, log.SL_FireWorkflow);

			log = attempt.Logs.GetAllLogs().OfType<StmALog>().Single(x => x.SL_SE_NKEvent == AutoEvents.AccreditationAttemptCompletedCode);
			AssertEquals("AC1 - Description(123)", log.SL_Reference);
			AssertEquals(true, log.SL_IsEstimate);
			AssertEquals(true, log.SL_FireWorkflow);
			AssertEquals(ZDate.Today.AddDays(5), log.SL_EventTime.Date);

			attempt.HAA_CompletionDate = ZDate.Today.AddDays(2);
			Factory.Save();
			log = attempt.Logs.GetAllLogs().OfType<StmALog>().Single(x => x.SL_SE_NKEvent == AutoEvents.AccreditationAttemptCompletedCode && x.PK != log.PK);
			AssertEquals("AC1 - Description(123)", log.SL_Reference);
			AssertEquals(false, log.SL_IsEstimate);
			AssertEquals(true, log.SL_FireWorkflow);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = person1.WorkflowItems.WorkflowType;

			var milestone = workflowTemplate.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.P9_Description = "Test milestone - 001";
			Factory.Save();

			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			AssertEquals(false, person1.WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));
			AssertEquals(true, person2.WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));

			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = ZDate.Today;
			attempt.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt.HAA_CompletionDate = ZDate.Empty;
			attempt.Accreditation.HAC_Code = "AC1";
			attempt.Accreditation.HAC_Description = "Description(123)";
			attempt.HAA_PER = person1.PK;
			Factory.Save();

			AssertEquals(true, person1.WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));
			AssertEquals(true, person2.WorkflowItems.Milestones.OfType<ProcessTask>().Any(x => x.P9_Description == "Test milestone - 001"));
		}

		public void TestHAA_CompletionDueDate()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt.HAA_CompletionDate = ZDate.Empty;
			AssertEquals(false, attempt.HAA_CompletionDueDate_ReadOnly);

			attempt.HAA_CompletionDate = ZDate.Today;
			AssertEquals(true, attempt.HAA_CompletionDueDate_ReadOnly);

			attempt.HAA_CompletionDate = ZDate.Today.AddDays(-1);
			attempt.HAA_ExpiryDate = ZDate.Today.AddDays(3);
			attempt.HAA_CompletionDueDate = ZDate.Today;
			AssertEquals(ZDate.Today.AddDays(-1), attempt.HAA_CompletionDate);
			AssertEquals(ZDate.Today.AddDays(3), attempt.HAA_ExpiryDate);
			AssertEquals(ZDate.Today, attempt.HAA_CompletionDueDate);

			attempt.HAA_CompletionDate = ZDate.Empty;
			attempt.HAA_ExpiryDate = ZDate.Today.AddDays(3);
			attempt.HAA_CompletionDueDate = ZDate.Today;
			AssertEquals(ZDate.Empty, attempt.HAA_CompletionDate);
			AssertEquals(ZDate.Today, attempt.HAA_ExpiryDate);
			AssertEquals(ZDate.Today, attempt.HAA_CompletionDueDate);
		}

		public void TestHAA_ExpiryDate_ContinuingRefresher()
		{
			var today = ZDate.Today;
			var earlier = ZDate.Today.AddDays(-5);
			var later1 = ZDate.Today.AddDays(5);
			var later2 = ZDate.Today.AddDays(10);

			var accredMain = Factory.NewWithValidTestData<GlbAccreditation>();
			accredMain.HAC_ValidityMonths = 1;
			accredMain.HAC_CertificateCode = "CUS";

			var accredRef = Factory.NewWithValidTestData<GlbAccreditation>();
			accredRef.HAC_ValidityMonths = 1;
			accredRef.HAC_IsRefresher = true;
			accredRef.HAC_CertificateCode = "CUS";

			var attemptMain = Factory.New<GlbAccreditationAttempt>();
			attemptMain.HAA_HAC = accredMain.PK;
			attemptMain.HAA_CommencementDate = earlier;

			var attemptRef = Factory.New<GlbAccreditationAttempt>();
			attemptRef.HAA_HAC = accredRef.PK;
			attemptRef.HAA_CommencementDate = today;

			var attemptRef2 = Factory.New<GlbAccreditationAttempt>();
			attemptRef2.HAA_HAC = accredRef.PK;
			attemptRef2.HAA_CommencementDate = later1;

			var person = Factory.New<GlbPerson>();
			var applicant = person.ApplicantCollection.AddNew() as HRJobApplicant;

			attemptMain.HAA_PER = person.PK;
			attemptMain.HAA_CompletionDueDate = today;
			attemptMain.HAA_ExpiryDate = today;

			attemptRef.HAA_PER = person.PK;
			attemptRef.HAA_CompletionDueDate = later1;

			attemptRef2.HAA_PER = person.PK;
			attemptRef2.HAA_CompletionDueDate = later2;

			AssertEquals(today, attemptMain.HAA_ExpiryDate);
			attemptMain.HAA_CompletionDate = earlier;
			attemptRef.HAA_CompletionDate = later1;
			attemptRef2.HAA_CompletionDate = later2;

			AssertEquals(attemptMain.HAA_ExpiryDate.AddMonths(1).AddMonths(1), attemptRef2.HAA_ExpiryDate);
		}

		public void TestHAA_ExpiryDate()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			attempt.HAA_HAC = accred.PK;
			accred.HAC_ValidityMonths = 1;
			var today = ZDate.Today;
			var earlier = ZDate.Today.AddDays(-5);

			var person = Factory.New<GlbPerson>();
			var applicant = person.ApplicantCollection.AddNew() as HRJobApplicant;
			attempt.HAA_PER = person.PK;
			attempt.HAA_CompletionDueDate = today;

			AssertEquals(today, attempt.HAA_ExpiryDate);

			attempt.HAA_CompletionDate = earlier;

			AssertEquals(earlier.AddMonths(1), attempt.HAA_ExpiryDate);
		}

		public void TestHAA_ExpiryDate_ExistingCert()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			attempt.HAA_HAC = accred.PK;
			accred.HAC_CertificateCode = "XXX";

			var person = Factory.New<GlbPerson>();
			var applicant = person.ApplicantCollection.AddNew() as HRJobApplicant;
			attempt.HAA_PER = person.PK;

			accred.HAC_ValidityMonths = 1;
			var today = ZDate.Today;
			var earlier = ZDate.Today.AddDays(-5);

			attempt.HAA_CompletionDueDate = today;

			AssertEquals(today, attempt.HAA_ExpiryDate);

			var cert = applicant.Certificates.AddNew();
			cert.XZ_IssueDate = earlier;
			cert.XZ_Type = "XXX";

			attempt.HAA_CompletionDate = earlier;

			AssertEquals(earlier.AddMonths(1), attempt.HAA_ExpiryDate);
		}

		public void TestIWorkflowProvider()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			IWorkflowProvider workflowProvider = attempt;

			AssertNotNull(workflowProvider);
			AssertEquals(attempt.PK, workflowProvider.PK);
			AssertEquals(new GlbAccreditationAttemptWorkflowDescriptor().Code, workflowProvider.WorkflowType);
			AssertEquals(typeof(GlbAccreditationAttemptProcessTaskCollection), workflowProvider.WorkflowItems.GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestRelatedLocation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUBRN";
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_OH = org.PK;
			contact2.OC_OH = org.PK;
			contact3.OC_OH = org.PK;

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress2.OA_RL_NKRelatedPortCode = "NZAUK";
			contact1.OC_OA_OrgAddress = orgAddress1.PK;
			contact2.OC_OA_OrgAddress = orgAddress2.PK;

			var person1 = contact1.Person;
			contact2.OC_PER = person1.PK;
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var person3 = contact3.Person;
			person1.SetPrimaryRelationship(contact1);
			person3.SetPrimaryRelationship(contact3);

			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			var attempt3 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_PER = person1.PK;
			attempt2.HAA_PER = person2.PK;
			attempt3.HAA_PER = person3.PK;

			AssertEquals(orgAddress1.OA_RL_NKRelatedPortCode, attempt1.RelatedLocation);
			AssertEquals(ZString.Empty, attempt2.RelatedLocation);
			AssertEquals(org.OH_RL_NKClosestPort, attempt3.RelatedLocation);
		}

		[TestDate(2019, 1, 1)]
		public void TestRefresherCertExpirationType_EndOfLastCertPlusValidityPeriodDefault()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			accreditation1.HAC_ValidityMonths = 3;
			AssertEquals(true, accreditation1.HAC_IsRefresher_ReadOnly);
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			accreditation2.HAC_ValidityMonths = 3;

			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_PER = person.PK;
			Factory.Save();

			var attempt1 = person.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_CommencementDate = new ZDate(2018, 1, 1);
			attempt1.HAA_CompletionDueDate = new ZDate(2018, 2, 1);
			attempt1.HAA_CompletionDate = new ZDate(2018, 1, 3);
			Factory.Save();

			var attempt2 = person.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_CommencementDate = new ZDate(2018, 3, 20);
			attempt2.HAA_CompletionDueDate = new ZDate(2018, 4, 20);
			attempt2.HAA_CompletionDate = new ZDate(2018, 3, 22);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("C01", attempt1.CertificateCode);
				AssertEquals(new ZDate(2018, 1, 3), attempt1.CertificateIssueDate);
				AssertEquals(new ZDate(2018, 4, 3), attempt1.CertificateExpiryDate);
				AssertEquals(new ZDate(2018, 4, 3), attempt1.HAA_ExpiryDate);
				AssertEquals("AC1 Completed: 03-Jan-18, Expiry: 03-Apr-18", attempt1.Certificate.XZ_Comment);

				AssertEquals("C01", attempt2.CertificateCode);
				AssertEquals(new ZDate(2018, 3, 22), attempt2.CertificateIssueDate);
				AssertEquals(new ZDate(2018, 7, 3), attempt2.CertificateExpiryDate); // Last Cert. (2018,4,3) + 3 months
				AssertEquals(new ZDate(2018, 7, 3), attempt2.HAA_ExpiryDate); // Last Cert. (2018,4,3) + 3 months
				AssertEquals("AC2 Completed: 22-Mar-18, Expiry: 03-Jul-18", attempt2.Certificate.XZ_Comment);
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestRefresherCertExpirationType_EndOfLastCertPlusValidityPeriodDefault_ExistingCert()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			accreditation1.HAC_ValidityMonths = 3;
			AssertEquals(true, accreditation1.HAC_IsRefresher_ReadOnly);
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			accreditation2.HAC_ValidityMonths = 3;

			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_PER = person.PK;
			Factory.Save();

			var certificate = Factory.New<GenRegCertAccredMaintList>();

			certificate.XZ_RefNumber = ZString.Empty;
			certificate.XZ_IssueDate = new ZDate(2017, 1, 1);
			certificate.XZ_StateOrProvinceOfIssuance = ZString.Empty;
			certificate.XZ_ExpiryOrDueDate = new ZDate(2020, 1, 1);
			certificate.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			certificate.XZ_ParentID = applicant.PK;
			certificate.MasterParent = applicant;
			certificate.XZ_Type = "C01";
			certificate.XZ_RN_NKCountryOfIssuance = applicant.HA_RN_NKCountry;
			Factory.Save();

			var attempt1 = person.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_CommencementDate = new ZDate(2018, 1, 1);
			attempt1.HAA_CompletionDueDate = new ZDate(2018, 2, 1);
			attempt1.HAA_CompletionDate = new ZDate(2018, 1, 3);
			Factory.Save();

			var attempt2 = person.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_CommencementDate = new ZDate(2018, 3, 20);
			attempt2.HAA_CompletionDueDate = new ZDate(2018, 4, 20);
			attempt2.HAA_CompletionDate = new ZDate(2018, 3, 22);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("C01", attempt1.CertificateCode);
				AssertEquals(new ZDate(2018, 1, 3), attempt1.CertificateIssueDate);
				AssertEquals(new ZDate(2018, 4, 3), attempt1.CertificateExpiryDate);
				AssertEquals(new ZDate(2018, 4, 3), attempt1.HAA_ExpiryDate);
				AssertEquals("AC1 Completed: 03-Jan-18, Expiry: 03-Apr-18", attempt1.Certificate.XZ_Comment);

				AssertEquals("C01", attempt2.CertificateCode);
				AssertEquals(new ZDate(2018, 3, 22), attempt2.CertificateIssueDate);
				AssertEquals(new ZDate(2018, 7, 3), attempt2.CertificateExpiryDate); // Last Cert. (2018,4,3) + 3 months
				AssertEquals("AC2 Completed: 22-Mar-18, Expiry: 03-Jul-18", attempt2.Certificate.XZ_Comment);
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestRefresherCertExpirationType_RefresherCompletionDatePlusValidityPeriod()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			accreditation1.HAC_ValidityMonths = 3;
			AssertEquals(true, accreditation1.HAC_IsRefresher_ReadOnly);
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;
			accreditation2.HAC_ValidityMonths = 3;

			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_PER = person.PK;
			Factory.Save();

			var attempt1 = person.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_CommencementDate = new ZDate(2018, 1, 1);
			attempt1.HAA_CompletionDueDate = new ZDate(2018, 2, 1);
			attempt1.HAA_CompletionDate = new ZDate(2018, 1, 3);
			Factory.Save();

			var attempt2 = person.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_CommencementDate = new ZDate(2018, 3, 20);
			attempt2.HAA_CompletionDueDate = new ZDate(2018, 4, 20);
			attempt2.HAA_CompletionDate = new ZDate(2018, 3, 22);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("C01", attempt1.CertificateCode);
				AssertEquals(new ZDate(2018, 1, 3), attempt1.CertificateIssueDate);
				AssertEquals(new ZDate(2018, 4, 3), attempt1.CertificateExpiryDate);
				AssertEquals("AC1 Completed: 03-Jan-18, Expiry: 03-Apr-18", attempt1.Certificate.XZ_Comment);

				AssertEquals("C01", attempt2.CertificateCode);
				AssertEquals(new ZDate(2018, 3, 22), attempt2.CertificateIssueDate);
				AssertEquals(new ZDate(2018, 6, 22), attempt2.CertificateExpiryDate); // Refresher Completion Date (2018,3,22) + 3 months
				AssertEquals("AC2 Completed: 22-Mar-18, Expiry: 22-Jun-18", attempt2.Certificate.XZ_Comment);
			});
		}

		[TestDate(2019, 10, 17)]
		public void TestStatus()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			AssertEquals("", attempt.Status);

			attempt.HAA_CommencementDate = new ZDate(2019, 1, 1);
			AssertEquals(AttemptStatus.Started, attempt.Status);

			attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt.HAA_ExpiryDate = new ZDate(2019, 7, 7);
			attempt.HAA_CompletionDueDate = new ZDate(2019, 7, 7);
			AssertEquals(AttemptStatus.FailedToComplete, attempt.Status);

			attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attempt.HAA_ExpiryDate = new ZDate(2020, 7, 7);
			attempt.HAA_CompletionDueDate = new ZDate(2020, 7, 7);
			AssertEquals(AttemptStatus.Completed, attempt.Status);

			attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attempt.HAA_ExpiryDate = new ZDate(2019, 7, 7);
			attempt.HAA_CompletionDueDate = new ZDate(2019, 7, 7);
			AssertEquals(AttemptStatus.Expired, attempt.Status);
		}
	}
}
