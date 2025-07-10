using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class EnvironmentTest : TestCaseWithFactory
	{
		public void TestBranch()
		{
			var env = new Environment();

			AssertNotNull("Branch.Name", GlbBranch.CurrentBranch.GB_BranchName);
			AssertEquals("Branch.Name", GlbBranch.CurrentBranch.GB_BranchName, env.Branch.Name);
		}

		public void TestDepartment()
		{
			var env = new Environment();

			AssertNotNull("Department.Code", GlbDepartment.CurrentDepartment.GE_Code);
			AssertEquals("Department.Code", GlbDepartment.CurrentDepartment.GE_Code, env.Department.Code);
		}

		public void TestCurrentUser()
		{
			var env = new Environment();

			AssertNotNull("GlbStaff.CurrentUser.GS_FullName", GlbStaff.CurrentUser.GS_FullName);
			AssertEquals("CurrentUser.Name", GlbStaff.CurrentUser.GS_FullName, env.CurrentUser.Name);
		}

		public void TestCurrentUserMap()
		{
			var env = new Environment();
			AssertNoExceptionThrown(() => env.ToMap().ToJSON());
		}

		public void TestLocalCurrency()
		{
			var env = new Environment();

			AssertNotNull("GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("LocalCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, env.LocalCurrency);
		}

		public void TestCompanies()
		{
			foreach (var company in Factory.Load<GlbCompany>(new ZQuery()))
			{
				company.GC_IsActive = false;
			}

			var comp1 = Factory.New<GlbCompany>();
			comp1.GC_Code = "AAA";
			comp1.GC_Name = "AAA Inc";
			comp1.GC_IsActive = true;

			var comp2 = Factory.New<GlbCompany>();
			comp2.GC_Code = "BBB";
			comp2.GC_Name = "BBB Inc";
			comp2.GC_IsActive = false;

			var comp3 = Factory.New<GlbCompany>();
			comp3.GC_Code = "CCC";
			comp3.GC_Name = "CCC Inc";
			comp3.GC_IsActive = true;

			Factory.Save();

			var env = new Environment();

			AssertContainsExactElementsInAnyOrder("Companies",
				new[]
				{
					"AAA",
					"CCC"
				},
				env.Companies.Select(comp => comp.Code));
		}

		#region Registry Items

		public void TestRegistryItems_NoExceptionInThrown()
		{
			var env = new Environment();

			foreach (var item in GetAccessibleRegistryItemNames())
			{
				AssertNoExceptionThrown(item, () => env.GetRegistryItem(item));
			}
		}

		public void TestRegistryItems_NoExtraDatabaseHitsAreDoneWhenAccessing()
		{
			_ = GlbCompany.CurrentCompany.Country; // This will usually be loaded on CW1 startup, but if we clear the UserContext environment between test runs it may no longer be cached, so cache it for consistency with the original test.

			var registryItemNames = GetAccessibleRegistryItemNames();
			var env = new Environment();

			CombineAssertions(() =>
			{
				using (AssertDbHitsForAllFactories($"no additional db hits are issued when accessing registry items",
					expectedHitCounts: new Dictionary<string, int>(),
					useOnlyNewFactories: false,
					tablesToIgnore: Array.Empty<string>()))
				{
					foreach (var registryItemName in registryItemNames)
					{
						env.GetRegistryItem(registryItemName);
					}
				}
			});
		}

		public void TestRegistryItems_TestAccessibleItemsCoverage()
		{
			var names = GetAccessibleRegistryItemNames();
			var paths = GetAccessibleRegistryItemPaths();

			AssertEquals("all registry items are cached along with their path", names.Length, paths.Length);

			var env = new Environment();

			for (var i = 0; i < names.Length; i++)
			{
				var itemByName = env.RegistryItems[names[i]];
				var itemByPath = env.RegistryItems[paths[i]];

				AssertEquals("registry item by name is also accessible via its path", itemByName, itemByPath);

				Assert("registry item by name is registered correctly",
					names[i].Equals(itemByName.Name, StringComparison.InvariantCultureIgnoreCase));

				var registryItemPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}",
					itemByPath.Category,
					itemByPath.Caption);

				AssertEquals("registry item by path is registered correctly", paths[i], registryItemPath);
			}
		}

		public void TestRegistryItems_AccessingIsCaseInsensitive()
		{
			var env = new Environment();

			using (RawDataRegistry.Instance.HouseBillOfLadingLogo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(1, 1)))
			{
				AssertNotNull(env.GetRegistryItem("HOUSEBILLOFLADINGLOGO"));
				AssertNotNull(env.GetRegistryItem("HouseBillOfLadingLogo"));
				AssertNotNull(env.GetRegistryItem("housebillofladinglogo"));
			}
		}

		public void TestRegistryItems_AccessUsingPath()
		{
			var env = new Environment();

			using (RawDataRegistry.Instance.HouseBillOfLadingLogo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(1, 1)))
			{
				AssertNotNull(env.GetRegistryItem("HOUSEBILLOFLADINGLOGO"));
				AssertNotNull(env.GetRegistryItem("Freight/House Bills/House Bill Of Lading Logo"));
			}
		}

		public void TestLegacyMergeFormAndDocumentsMenus()
		{
			var env = new Environment();
			Assert("Accessing legacy registry should return 'true'", (bool)env.GetRegistryItem("MERGEFORMANDDOCUMENTSMENUS"));
		}

		string[] GetAccessibleRegistryItemNames() =>
			new[]
			{
				"ENABLESUPPLYCHAINSECURITY_EU",
				"ENABLESUPPLYCHAINSECURITY_UK",
				"ENABLESUPPLYCHAINSECURITY_HK",
				"ENABLESUPPLYCHAINSECURITY_US",
				"HOUSEBILLOFLADINGLOGO",
				"BILL OF LADINGWEIGHTANDVOLUMEDISPLAY",
				"INCOTERMDEFINITION",
				"SHOWPACKLINEDETAILSONHOUSEBILLS",
				"SHIPPERLOADANDCOUNT",
				"ENABLEHOUSEBILLOFLADINGREGISTRYITEMS",
				"HOUSEBILLOFLADINGLOGOTYPES",
				"HOUSEBILLOFLADINGLOGOIMAGES",
				"HOUSEBILLOFLADINGTERMSANDCONDITIONSIMAGES",
				"BOLLUMPSUMDISPLAYCOUNTRIES",
				"ADDITIONALHBLTYPES",
				"BOLCLAUSE",
				"ENABLEGOODSVALUEFORSHIPPINGINSTRUCTION",
				"PRINTSIGNATUREFORHBLDOCUMENTS",
				"ENABLEMEXICANPORTINTEGRATIONFEATURES",
				"ENABLESPANISHPORTINTEGRATIONFEATURES",
				"ENABLEBOOKINGCONFIRMATION",
				"ENABLEDRAFTBILLOFLADINGFORM",
				"ENABLEFIATAHOUSEBILLSFEATURES",
				"ENABLECPTPPSUBMISSIONTOCAB",
				"ENABLENZCFTASUBMISSIONTOCAB",
				"ENABLERCEPSUBMISSIONTOCAB",
				"ENABLEAANZFTASUBMISSIONTOCAB",
				"ALLOWTOSENDEXPORTNOTIFICATION",
				"ENABLECOONZSUBMISSIONTOCAB",
				"ENABLECHAFTASUBMISSIONTOCAB",
				"ENABLEJAEPASUBMISSIONTOCAB",
				"ALLOWTOSENDEXPORTNOTIFICATIONTOCARGONAUT",
				"ENABLEAUKFTASUBMISSIONTOCAB",
				"ENABLETAFTASUBMISSIONTOCAB",
				"ENABLEIAECTASUBMISSIONTOCAB",
				"ENABLEKAFTASUBMISSIONTOCAB",
				"ENABLEPAFTASUBMISSIONTOCAB",
				"ENABLEIACEPASUBMISSIONTOCAB",
				"ENABLECERTOFORIGINAUSUBMISSIONTOCAB",
				"ENABLECOOUSSUBMISSIONTOCAB"
			};

		string[] GetAccessibleRegistryItemPaths() =>
			new[]
			{
				"Freight/Supply Chain Security/European Union/Enable Supply Chain Security for the European Union",
				"Freight/Supply Chain Security/United Kingdom/Enable Supply Chain Security for United Kingdom",
				"Freight/Supply Chain Security/Hong Kong/Enable Supply Chain Security for Hong Kong",
				"Freight/Supply Chain Security/United States/Enable Supply Chain Security for United States",
				"Freight/House Bills/House Bill Of Lading Logo",
				"Documents/Forwarding/Bill of Lading/Weight And Volume Display",
				"AutoRating/Charge Code Groups/Incoterm Charge Code Group Configuration",
				"Freight/House Bills/Show Pack Line Details On House Bills",
				"Documents/Forwarding/Shipment/Bill of Lading/Shipper Load and Count",
				"Freight/House Bills/House Bill of Lading Types/Enable House Bill of Lading Registry Items",
				"Freight/House Bills/House Bill of Lading Types/House Bill of Lading Settings",
				"Freight/House Bills/House Bill of Lading Types/Logos",
				"Freight/House Bills/House Bill of Lading Types/Terms & Conditions",
				"Documents/Forwarding/Shipment/Bill of Lading/Print Charges as Lump Sum",
				"Freight/House Bills/House Bill of Lading Types/Additional Types",
				"Freight/House Bills/Clauses/Standard",
				"Freight/Consolidations/Ocean Carrier Messaging/Shipping Instruction Goods Value",
				"Freight/House Bills/Print Signature",
				"Freight/Enables various Mexican port integration features",
				"Freight/Enables various Spanish sea port integration features",
				"Freight/Consolidations/Ocean Carrier Messaging/Enable Booking Confirmation form",
				"Freight/Consolidations/Ocean Carrier Messaging/Enable Draft Bill Of Lading form",
				"Freight/House Bills/House Bill of Lading Types/FIATA HBL Settings/Enable electronic FIATA Bills of Lading (eFBL)",
				"Documents/Digital Docs/Certification/CPTPP",
				"Documents/Digital Docs/Certification/NZCFTA",
				"Documents/Digital Docs/Certification/RCEP",
				"Documents/Digital Docs/Certification/AANZFTA",
				"Freight/Port Messaging/France/Allow to send Export Notification (755) to Cargo Information Network",
				"Documents/Digital Docs/Certification/Cert of Origin NZ",
				"Documents/Digital Docs/Certification/ChAFTA",
				"Documents/Digital Docs/Certification/JAEPA",
				"Freight/Port Messaging/Netherlands/Allow to send Export Notification (755) to Cargonaut",
				"Documents/Digital Docs/Certification/A-UKFTA",
				"Documents/Digital Docs/Certification/TAFTA",
				"Documents/Digital Docs/Certification/IA-ECTA",
				"Documents/Digital Docs/Certification/KAFTA",
				"Documents/Digital Docs/Certification/PAFTA",
				"Documents/Digital Docs/Certification/IA-CEPA",
				"Documents/Digital Docs/Certification/Cert of Origin AU",
				"Documents/Digital Docs/Certification/COOUS"
			};

		#endregion
	}
}
