using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ImportExportHelperTest : TestCaseWithFactory
	{
		#region Local

		public void TestStaticIsLocal()
		{
			AssertEquals("Local", true, ImportExportHelper.IsBranchCountry(LocalPort));
			AssertEquals("Foreign", false, ImportExportHelper.IsBranchCountry(ForeignPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(m => m.IsLocalInRelationTo(ForeignPort)).Returns(true);

			AssertEquals("Foreign, but considered Local", true, ImportExportHelper.IsLocal(ForeignPort, homePortReference.Object));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
			AssertEquals("Foreign", false, ImportExportHelper.IsBranchCountry(ForeignPort));

			homePortReference.VerifyAll();
		}

		public void TestIsLocalToAnyBranch()
		{
			GlbBranch sydneyBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";

			TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);

			foreach (GlbBranch branch in Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)))
			{
				branch.Delete();
			}

			GlbBranch highpointBranch = sydneyBranch.Company.Branches.AddNew();
			highpointBranch.GB_Code = "XX1";
			highpointBranch.GB_RL_NKHomePort = "USHIT";

			GlbBranch pitalConDesvioBranch = sydneyBranch.Company.Branches.AddNew();
			pitalConDesvioBranch.GB_Code = "XX2";
			pitalConDesvioBranch.GB_RL_NKHomePort = "CRAPO";

			Factory.Save();

			AssertEquals("IsLocalToAnyBranch", false, ImportExportHelper.IsAnyBranchCountry(null, ZString.Empty));
			AssertEquals("IsLocalToAnyBranch", false, ImportExportHelper.IsAnyBranchCountry(Factory, ZString.Empty));
			AssertEquals("IsLocalToAnyBranch", false, ImportExportHelper.IsAnyBranchCountry(Factory, "SGSIN"));
			AssertEquals("IsLocalToAnyBranch", true, ImportExportHelper.IsAnyBranchCountry(Factory, "AUSYD"));
			AssertEquals("IsLocalToAnyBranch", true, ImportExportHelper.IsAnyBranchCountry(Factory, "AUMEL"));
			AssertEquals("IsLocalToAnyBranch", true, ImportExportHelper.IsAnyBranchCountry(Factory, "USHIT"));
			AssertEquals("IsLocalToAnyBranch", true, ImportExportHelper.IsAnyBranchCountry(Factory, "CRAPO"));
		}

		public void TestExcludeDemoCompany()
		{
			GlbCompany demoCompany = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"))[0];
			AssertNotEquals("prerequisite", demoCompany, GlbCompany.CurrentCompany);

			TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);

			foreach (GlbBranch branch in Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)))
			{
				branch.Delete();
			}

			GlbBranch sydneyBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";

			GlbBranch chicagoBranch = demoCompany.Branches.AddNew();
			chicagoBranch.GB_Code = "XXX";
			chicagoBranch.GB_RL_NKHomePort = "USCHI";

			Factory.Save();

			AssertEquals("should be true for AUSYD", true, ImportExportHelper.IsAnyBranchCountry(Factory, "AUSYD"));
			AssertEquals("should skip USCHI branch as it belongs to DEM company", false, ImportExportHelper.IsAnyBranchCountry(Factory, "USCHI"));

			GlbBranch miamiBranch = sydneyBranch.Company.Branches.AddNew();
			miamiBranch.GB_Code = "XX2";
			miamiBranch.GB_RL_NKHomePort = "USMIA";

			Factory.Save();

			AssertEquals("should be true now for USCHI as we have non-demo comp branch in US", true, ImportExportHelper.IsAnyBranchCountry(Factory, "USCHI"));
		}

		#endregion

		#region Import

		public void TestStaticIsImport()
		{
			AssertEquals("Import", true, ImportExportHelper.IsImport(ForeignPort, LocalPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(hp => hp.IsLocalInRelationTo(ForeignPort)).Returns(true);

			AssertEquals("Export", false, ImportExportHelper.IsImport(ForeignPort, LocalPort, homePortReference.Object));

			AssertEquals("Export", false, ImportExportHelper.IsImport(LocalPort, ForeignPort));
			AssertEquals("Trans-Shipment", false, ImportExportHelper.IsImport(ForeignPort, ForeignPort));
			AssertEquals("Domestic", false, ImportExportHelper.IsImport(LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", !IsDomestic, ImportExportHelper.IsImport("", LocalPort));
			AssertEquals("Export Missing-Origin", false, ImportExportHelper.IsImport("", ForeignPort));

			AssertEquals("Import Missing-Destination", true, ImportExportHelper.IsImport(ForeignPort, ""));
			AssertEquals("Export Missing-Destination", false, ImportExportHelper.IsImport(LocalPort, ""));

			AssertEquals("Missing-Both", IsImport, ImportExportHelper.IsImport("", ""));

			homePortReference.VerifyAll();
		}

		#endregion

		#region Export

		public void TestStaticIsExport()
		{
			AssertEquals("Import", false, ImportExportHelper.IsExport(ForeignPort, LocalPort));
			AssertEquals("Export", true, ImportExportHelper.IsExport(LocalPort, ForeignPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(m => m.IsLocalInRelationTo(ForeignPort)).Returns(true);

			AssertEquals("Import", false, ImportExportHelper.IsExport(LocalPort, ForeignPort, homePortReference.Object));

			AssertEquals("Trans-Shipment", false, ImportExportHelper.IsExport(ForeignPort, ForeignPort));

			AssertEquals("Trans-Shipment, but orgin considered Local", true, ImportExportHelper.IsExport(ForeignPort, ForeignPort2, homePortReference.Object));
			AssertEquals("Trans-Shipment, but destination considered Local", false, ImportExportHelper.IsExport(ForeignPort2, ForeignPort, homePortReference.Object));

			AssertEquals("Domestic", false, ImportExportHelper.IsExport(LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", false, ImportExportHelper.IsExport("", LocalPort));
			AssertEquals("Export Missing-Origin", true, ImportExportHelper.IsExport("", ForeignPort));

			AssertEquals("Import Missing-Destination", false, ImportExportHelper.IsExport(ForeignPort, ""));
			AssertEquals("Import or Trans? Missing-Destination, but orgin considered Local", true, ImportExportHelper.IsExport(ForeignPort, "", homePortReference.Object));
			AssertEquals("Export Missing-Destination", !IsDomestic, ImportExportHelper.IsExport(LocalPort, ""));

			AssertEquals("Missing-Both", IsExport, ImportExportHelper.IsExport("", ""));

			homePortReference.VerifyAll();
		}

		#endregion

		#region Cross Trade

		public void TestStaticIsCrossTrade()
		{
			AssertEquals("Import", false, ImportExportHelper.IsCrossTrade(ForeignPort, LocalPort));
			AssertEquals("Export", false, ImportExportHelper.IsCrossTrade(LocalPort, ForeignPort));
			AssertEquals("Domestic", false, ImportExportHelper.IsCrossTrade(ForeignPort, ForeignPort));
			AssertEquals("Trans-Shipment", true, ImportExportHelper.IsCrossTrade(ForeignPort, ForeignPort2));
			AssertEquals("Trans-Shipment", true, ImportExportHelper.IsCrossTrade(ForeignPort2, ForeignPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(m => m.IsLocalInRelationTo(ForeignPort)).Returns(true);
			AssertEquals("Not a Trans-Shipment", false, ImportExportHelper.IsCrossTrade(ForeignPort2, ForeignPort, homePortReference.Object));

			AssertEquals("Domestic", false, ImportExportHelper.IsCrossTrade(LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", false, ImportExportHelper.IsCrossTrade("", LocalPort));
			AssertEquals("Export Missing-Origin", false, ImportExportHelper.IsCrossTrade("", ForeignPort));

			AssertEquals("Import Missing-Destination", false, ImportExportHelper.IsCrossTrade(ForeignPort, ""));
			AssertEquals("Export Missing-Destination", false, ImportExportHelper.IsCrossTrade(LocalPort, ""));

			AssertEquals("Missing-Both", false, ImportExportHelper.IsCrossTrade("", ""));

			homePortReference.VerifyAll();
		}

		public void TestStaticIsCrossTrade_SameCountry()
		{
			var query = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany);
			var country = Factory.LoadTop1<RefCountry>(query);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Guid[] { country.PK.ToGuid() });

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("CrossTrade", false, ImportExportHelper.IsCrossTrade("DEHAM", "DEFRA"));
			}
		}

		#endregion

		#region Domestic

		public void TestStaticIsDomestic()
		{
			AssertEquals("Import", false, ImportExportHelper.IsDomestic(ForeignPort, LocalPort));
			AssertEquals("Export", false, ImportExportHelper.IsDomestic(LocalPort, ForeignPort));
			AssertEquals("Trans-Shipment", false, ImportExportHelper.IsDomestic(ForeignPort, ForeignPort2));
			AssertEquals("Domestic", true, ImportExportHelper.IsDomestic(ForeignPort, ForeignPort));
			AssertEquals("Domestic", true, ImportExportHelper.IsDomestic(LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", false, ImportExportHelper.IsDomestic("", LocalPort));
			AssertEquals("Export Missing-Origin", false, ImportExportHelper.IsDomestic("", ForeignPort));
			AssertEquals("Import Missing-Destination", false, ImportExportHelper.IsDomestic(ForeignPort, ""));
			AssertEquals("Export Missing-Destination", false, ImportExportHelper.IsDomestic(LocalPort, ""));
			AssertEquals("Missing-Both", false, ImportExportHelper.IsDomestic("", ""));
		}

		#endregion

		#region TestGetJobDirection_CommunityRegion

		public void TestGetJobDirection_CommunityRegion()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Sweden);
			var query2 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);

			var country1 = Factory.LoadTop1<RefCountry>(query1);
			var country2 = Factory.LoadTop1<RefCountry>(query2);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid(), country2.PK.ToGuid() });

			AssertEquals("Foreign -> Local", Directions.Import, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));
			AssertEquals("Local -> Foreign", Directions.Export, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.NewZealand));
			AssertEquals("Local -> Local", Directions.CrossTrade, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("Foreign -> Foreign", Directions.CrossTrade, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.NewZealand));
			AssertEquals("Local -> Home", Directions.Import, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.UnitedStates, LocalPort));
			AssertEquals("Home -> Local", Directions.Export, ImportExportHelper.GetJobDirection(LocalPort, Core.Constants.CountryCodes.Sweden));
			AssertEquals("Home -> Home", Directions.Domestic, ImportExportHelper.GetJobDirection(LocalPort, LocalPort));
		}

		#endregion

		public void TestGetJobDirection_CommunityRegion_SwitchCompany()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Sweden);

			var country1 = Factory.LoadTop1<RefCountry>(query1);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid() });
			AssertEquals("Foreign -> Local", Directions.Import, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompanyBranch.GB_GC = newCompany.PK;

			Factory.Save();

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Foreign -> Foreign", Directions.CrossTrade, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));
			}
		}

		#region GetJobDirection Performance

		public void TestJobDirectionPerformance()
		{
			var origin = ForeignPort;
			var destination = LocalPort;

			AssertEquals(Directions.Import, ImportExportHelper.GetJobDirection(origin, destination));

			origin = LocalPort;
			destination = ForeignPort;

			AssertEquals(Directions.Export, ImportExportHelper.GetJobDirection(origin, destination));

			AssertMaxTableHits("RefCountry hit 1 or 2 times", 2, "RefCountry", RegistryFactory.Instance);
		}

		#endregion

		public void TestIsInCommunityRegion()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);
			var query2 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			var country1 = Factory.LoadTop1<RefCountry>(query1);
			var country2 = Factory.LoadTop1<RefCountry>(query2);
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid() });
			var isInCommunityRegion = ImportExportHelper.IsInCommunityRegion(Core.Constants.CountryCodes.NewZealand);
			AssertEquals(isInCommunityRegion, true);
			isInCommunityRegion = ImportExportHelper.IsInCommunityRegion(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(isInCommunityRegion, false);
		}

		#region TestJobDirectionForCommunityRegions

		public void TestJobDirectionForCommunityRegions()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);
			var query2 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);

			var country1 = Factory.LoadTop1<RefCountry>(query1);
			var country2 = Factory.LoadTop1<RefCountry>(query2);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid(), country2.PK.ToGuid() });

			var homePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var origin = Core.Constants.CountryCodes.India;
			var destination = Core.Constants.CountryCodes.UnitedStates;

			AssertEquals("origin outside region and destination within region", Directions.Import, ImportExportHelper.GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.UnitedStates;
			destination = Core.Constants.CountryCodes.India;

			AssertEquals("origin within region and destination outside region", Directions.Export, ImportExportHelper.GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.UnitedStates;
			destination = Core.Constants.CountryCodes.NewZealand;

			AssertEquals("origin and destination within same region", Directions.CrossTrade, ImportExportHelper.GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.India;
			destination = Core.Constants.CountryCodes.Canada;

			AssertEquals("origin and destination outside region", Directions.CrossTrade, ImportExportHelper.GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.UnitedStates;
			destination = homePort.Substring(0, 2);

			AssertEquals("origin within region and destination in logged in country", Directions.Import, ImportExportHelper.GetJobDirection(origin, destination));

			origin = homePort.Substring(0, 2);
			destination = Core.Constants.CountryCodes.NewZealand;

			AssertEquals("origin in logged in country and destination within region", Directions.Export, ImportExportHelper.GetJobDirection(origin, destination));

			origin = homePort.Substring(0, 2);
			destination = homePort.Substring(0, 2);

			AssertEquals("origin and destination in logged in country", Directions.Domestic, ImportExportHelper.GetJobDirection(origin, destination));
		}

		#endregion

		public void TestGetCountryPkShouldCachePK()
		{
			var refCountry = (NewFactory().LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")));
			var cacheKey = "ImportExportHelper.GetCountryPK.US";
			var getCountryPK = typeof(ImportExportHelper).GetMethod("GetCountryPK", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			RegistryFactory.Instance.ClearCachedValue<RefCountry>(cacheKey);
			var cached = RegistryFactory.Instance.GetCachedValue<RefCountry>(cacheKey, () => null);
			AssertNull(cached);
			RegistryFactory.Instance.ClearCachedValue<RefCountry>(cacheKey);

			var pk = (Guid)getCountryPK.Invoke(null, new object[] { new ZString("US") });
			AssertEquals(refCountry.PK.ToGuid(), pk);

			cached = RegistryFactory.Instance.GetCachedValue<RefCountry>(cacheKey, () => null);
			AssertNotNull(cached);
			AssertEquals(refCountry.PK, cached.PK);
		}

		public void TestGetDirectionCode()
		{
			var errors = new ZStringBuilder();
			var misMatchedDirectionCodes = (new OrgDocumentLookups(null)).FilterDirections.GetAllCodes().ToList();
			misMatchedDirectionCodes.Remove(OrgDocumentLookups.FilterDirectionConstants.Codes.All);
			var misMatchedDirections = new List<string>();

			foreach (var name in Enum.GetNames(typeof(Directions)))
			{
				var matchCode = ImportExportHelper.GetDirectionCode((Directions)Enum.Parse(typeof(Directions), name));

				if (string.IsNullOrEmpty(matchCode))
				{
					misMatchedDirections.Add(name);
				}
				else
				{
					misMatchedDirectionCodes.Remove(matchCode);
				}
			}

			if (misMatchedDirections.Count > 0)
			{
				errors.AppendLine(string.Format(
					"ImportExportHelper.GetDirectionCode should have OrgDocumentLookUps.FilterDirections synchronized with ImportExportHelper.Directions, these directions of ImportExportHelper.Directions are not matched: {0}",
					string.Join(",", misMatchedDirections.ToArray())));
			}

			if (misMatchedDirectionCodes.Count > 0)
			{
				errors.AppendLine(string.Format(
					"ImportExportHelper.GetDirectionCode should have OrgDocumentLookUps.FilterDirections synchronized with ImportExportHelper.Directions, these codes of OrgDocumentLookUps.FilterDirections are not matched: {0}",
					string.Join(",", misMatchedDirectionCodes.ToArray())));
			}

			if (errors.Length > 0)
			{
				Fail(errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			IsDomestic = GlbDepartment.CurrentDepartment.GE_Domestic;
			IsImport = GlbDepartment.CurrentDepartment.GE_Import;
			IsExport = GlbDepartment.CurrentDepartment.GE_Export;

			LocalPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZQuery notLocal = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, LocalPort.Substring(0, 2));
			ForeignPort = (Factory.LoadTop1<RefUNLOCO>(notLocal)).RL_Code;

			ZQuery notLocal2 = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, LocalPort.Substring(0, 2));
			notLocal2.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, ForeignPort.Substring(0, 2));
			ForeignPort2 = (Factory.LoadTop1<RefUNLOCO>(notLocal2)).RL_Code;
		}

		ZString LocalPort;
		ZString ForeignPort;
		ZString ForeignPort2;
		ZBool IsDomestic;
		ZBool IsImport;
		ZBool IsExport;

		#endregion
	}
}
