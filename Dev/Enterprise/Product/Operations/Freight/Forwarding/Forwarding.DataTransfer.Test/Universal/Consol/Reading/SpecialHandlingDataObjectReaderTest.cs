using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class SpecialHandlingDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestReadIntoCollectionForSpecialHandlingIsNonSecurityJobConsolAWBSpecialHandling()
		{
			var specialHandlingDOList = new List<CodeDescriptionPair>();
			var specialHandlingDO = new CodeDescriptionPair();
			specialHandlingDO.Code = "ACT";
			specialHandlingDOList.Add(specialHandlingDO);

			var consol = Factory.New<ForwardingConsol>();
			var securitySpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
			securitySpecialHandling.JKH_JK_Consol = consol.PK;

			var reader = new SpecialHandlingCollectionDataObjectReader(specialHandlingDOList.ToArray(), Factory, consol);
			reader.ReadIntoCollection();
			Assert(consol.AWBSpecialHandlingItems.FirstOrDefault() is NonSecurityJobConsolAWBSpecialHandling);
		}

		public void TestReadIntoCollectionRemovesExistingSpecialHandlingCodesIfNotMatching()
		{
			var consol = Factory.New<ForwardingConsol>();

			var sh1 = consol.AWBSpecialHandlingItems.AddNew();
			sh1.JKH_Code = "ACT";
			var sh2 = consol.AWBSpecialHandlingItems.AddNew();
			sh2.JKH_Code = "PEB";

			var specialHandlingDOList = new List<CodeDescriptionPair>();
			var specialHandlingDO = new CodeDescriptionPair();
			specialHandlingDO.Code = "ACT";
			specialHandlingDOList.Add(specialHandlingDO);

			var reader = new SpecialHandlingCollectionDataObjectReader(specialHandlingDOList.ToArray(), Factory, consol);
			reader.ReadIntoCollection();
			AssertEquals(1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("ACT", consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		public void TestReadIntoCollectionRemovesExistingSpecialHandlingIfNotMatching()
		{
			var consol = Factory.New<ForwardingConsol>();
			var securitySpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
			securitySpecialHandling.JKH_JK_Consol = consol.PK;
			securitySpecialHandling.JKH_Code = SecurityJobConsolAWBSpecialHandling.NotSecured;
			var sh1 = consol.AWBSpecialHandlingItems.AddNew();
			sh1.JKH_Code = "CAO";
			var sh2 = consol.AWBSpecialHandlingItems.AddNew();
			sh2.JKH_Code = "PEB";
			Factory.SaveForTesting();

			var specialHandlingDOList = new List<CodeDescriptionPair>();
			var specialHandlingDO = new CodeDescriptionPair();
			specialHandlingDO.Code = "ACT";
			specialHandlingDOList.Add(specialHandlingDO);

			var reader = new SpecialHandlingCollectionDataObjectReader(specialHandlingDOList.ToArray(), Factory, consol);
			reader.ReadIntoCollection();
			AssertEquals(1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("ACT", consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		// Security items
		const string NSC = "NSC";
		const string SCO = "SCO";

		// Non-security items
		const string CRT = "CRT";
		const string EAP = "EAP";
		const string QRT = "QRT";

		const string MasterBillNumber = "MAWB002";

		public void TestSpecialHandlingItems_NoItems_NoChange()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems();

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "");
		}

		public void TestSpecialHandlingItems_NoItems_AddNonSecurityItems()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems(CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "", CRT, EAP);
		}

		public void TestSpecialHandlingItems_NoItems_ReadNoItems()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems();

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "");
		}

		public void TestSpecialHandlingItems_NoItems_SetSecurityItem()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems(NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC);
		}

		public void TestSpecialHandlingItems_NoItems_SetSecurityItemWithDuplicate()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems(NSC, NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC);
		}

		public void TestSpecialHandlingItems_NoItems_SetSecurityItemWithConfict()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var conflictDetected = false;
			try
			{
				ReadSpecialItems(NSC, SCO);
			}
			catch (MessageProcessingBusinessFailureException e)
			{
				conflictDetected = true;
				AssertEquals("MessageProcessingBusinessFailureException message", "Conflicting security special handling codes set", e.Message);
				AssertEquals("MessageProcessingBusinessFailureException shouldRetry", false, e.ShouldRetry);
			}
			Assert("Security special handling item conflict not detected", conflictDetected);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol, "");
		}

		public void TestSpecialHandlingItems_NoItems_SetNonSecurityItemsWithDuplicate()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems(CRT, EAP, CRT);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "", CRT, EAP);
		}

		public void TestSpecialHandlingItems_NoItems_SetSpecialHandlingItems()
		{
			var consol = InitConsolWithSpecialHandlingItems("");

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "");

			var consol2 = ReadSpecialItems(CRT, EAP, NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSecurityItem_NoChange()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems(SCO);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, SCO);
		}

		public void TestSpecialHandlingItems_WithSecurityItem_ClearSecurityItem()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems();

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "");
		}

		public void TestSpecialHandlingItems_WithSecurityItem_ModifySecurityItem()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems(NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC);
		}

		public void TestSpecialHandlingItems_WithSecurityItem_ModifySecurityItemWithDuplicate()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems(NSC, NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC);
		}

		public void TestSpecialHandlingItems_WithSecurityItem_SetNonSecurityItems()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems(CRT, EAP, SCO);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, SCO, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSecurityItem_SetNonSecurityItemsWithDuplicate()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems(CRT, EAP, SCO, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, SCO, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSecurityItem_ModifySpecialHandlingItems()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO);

			var consol2 = ReadSpecialItems(CRT, EAP, NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithNonSecurityItems_NoChange()
		{
			var consol = InitConsolWithSpecialHandlingItems("", CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "", CRT, EAP);

			var consol2 = ReadSpecialItems(EAP, CRT);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "", CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithNonSecurityItems_ReadNoItems()
		{
			var consol = InitConsolWithSpecialHandlingItems("", CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "", CRT, EAP);

			var consol2 = ReadSpecialItems();

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "");
		}

		public void TestSpecialHandlingItems_WithNonSecurityItems_SetSecurityItem()
		{
			var consol = InitConsolWithSpecialHandlingItems("", CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "", CRT, EAP);

			var consol2 = ReadSpecialItems(NSC, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithNonSecurityItems_SetSecurityItemWithDuplicates()
		{
			var consol = InitConsolWithSpecialHandlingItems("", CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "", CRT, EAP);

			var consol2 = ReadSpecialItems(NSC, CRT, EAP, NSC, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithNonSecurityItems_SetNonSecurityItems()
		{
			var consol = InitConsolWithSpecialHandlingItems("", CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "", CRT, EAP);

			var consol2 = ReadSpecialItems(QRT, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "", EAP, QRT);
		}

		public void TestSpecialHandlingItems_WithNonSecurityItems_SetSpecialHandlingItems()
		{
			var consol = InitConsolWithSpecialHandlingItems("", CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, "", CRT, EAP);

			var consol2 = ReadSpecialItems(CRT, QRT, SCO);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, SCO, CRT, QRT);
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_NoChange()
		{
			var consol = InitConsolWithSpecialHandlingItems(NSC, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, NSC, CRT, EAP);

			var consol2 = ReadSpecialItems(EAP, NSC, CRT);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_ReadNoItems()
		{
			var consol = InitConsolWithSpecialHandlingItems(NSC, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, NSC, CRT, EAP);

			var consol2 = ReadSpecialItems();

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "");
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_ClearSecurityItem()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO, CRT, EAP);

			var consol2 = ReadSpecialItems(CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, "", CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_SetSecurityItem()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO, CRT, EAP);

			var consol2 = ReadSpecialItems(NSC, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_SetSecurityItemWithDuplicates()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO, CRT, EAP);

			var consol2 = ReadSpecialItems(NSC, CRT, EAP, NSC, CRT);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, EAP);
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_SetNonSecurityItems()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO, CRT, EAP);

			var consol2 = ReadSpecialItems(QRT, EAP, SCO);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, SCO, EAP, QRT);
		}

		public void TestSpecialHandlingItems_WithSpecialHandlingItems_SetSpecialHandlingItems()
		{
			var consol = InitConsolWithSpecialHandlingItems(SCO, CRT, EAP);

			ValidateSpecialHandlingItemsAndReload("Constructed", consol, SCO, CRT, EAP);

			var consol2 = ReadSpecialItems(CRT, QRT, NSC);

			ValidateSpecialHandlingItemsAndReload("ReadSpecialItems", consol2, NSC, CRT, QRT);
		}

		ForwardingConsol InitConsolWithSpecialHandlingItems(ZString? securityItem, params ZString[] nonSecurityItems)
		{
			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = MasterBillNumber;
			consol.SecurityStatusCode = securityItem ?? string.Empty;

			foreach (var item in nonSecurityItems)
			{
				var sh = consol.AWBSpecialHandlingItems.AddNew();
				sh.JKH_Code = item;
			}

			consol.RunPreSaveValidation();
			consol.Factory.Save();

			return consol;
		}

		static ForwardingConsol ReloadConsol()
		{
			var newfactory = new BusinessObjectFactory();
			return newfactory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, MasterBillNumber));
		}

		ForwardingConsol ReadSpecialItems(params ZString[] items)
		{
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, MasterBillNumber));
			var specialHandlingItems = CreateSpecialHandlingItems(items);

			var reader = new SpecialHandlingCollectionDataObjectReader(specialHandlingItems, Factory, consol);
			reader.ReadIntoCollection();

			consol.RunPreSaveValidation();
			Factory.SaveForTesting();

			return consol;
		}

		CodeDescriptionPair[] CreateSpecialHandlingItems(params ZString[] items)
		{
			var specialHandlingItems = new CodeDescriptionPair[items.Length];
			for (var i = 0; i < items.Length; i++)
			{
				specialHandlingItems[i] = new CodeDescriptionPair { Code = items[i] };
			}

			return specialHandlingItems;
		}

		static ForwardingConsol ValidateSpecialHandlingItemsAndReload(string message, ForwardingConsol consol, ZString? expectedSecurityItem, params ZString[] expectedNonSecurityItems)
		{
			ValidateSpecialHandlingItems(message, consol, expectedSecurityItem, expectedNonSecurityItems);

			var consol2 = ReloadConsol();
			ValidateSpecialHandlingItems(message + "&Reload", consol2, expectedSecurityItem, expectedNonSecurityItems);
			return consol2;
		}

		static void ValidateSpecialHandlingItems(string message, ForwardingConsol consol, ZString? expectedSecurityItem, params ZString[] expectedNonSecurityItems)
		{
			var sorted = consol.AWBSpecialHandlingItems.Select(x => x.JKH_Code).OrderBy(x => x).ToArray();
			AssertArrayEqualsByElements(message + "/non security items", expectedNonSecurityItems, sorted);
			AssertEquals(message + "/security item", expectedSecurityItem, consol.SecurityStatusCode);
		}
	}
}
