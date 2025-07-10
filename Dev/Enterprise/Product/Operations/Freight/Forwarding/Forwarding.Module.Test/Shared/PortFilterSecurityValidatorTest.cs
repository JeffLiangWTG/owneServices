using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	class PortFilterSecurityValidatorTest : TestCaseWithFactory
	{
		public void TestValidatePorts()
		{
			Action<ModuleLocationFilter, bool, bool> assertFilterValidation = (moduleLocationFilter, errorsExpectedOnProperty1, errorsExpectedOnProperty2) =>
			{
				moduleLocationFilter.Validation.ValidateAll();
				AssertEquals(errorsExpectedOnProperty1, moduleLocationFilter.Property1Info.HasErrors());
				AssertEquals(errorsExpectedOnProperty2, moduleLocationFilter.Property2Info.HasErrors());
			};

			ModuleLocationFilter originDestinationFilter = ((ModuleLocationFilter)Bizo["Origin / Destination"]);
			originDestinationFilter.IsActive = true;
			ModuleLocationFilter loadDischargeFilter = ((ModuleLocationFilter)Bizo["Load / Discharge"]);
			loadDischargeFilter.IsActive = true;

			CheckPoint.IsAllowed = true;
			originDestinationFilter.Property1 = "USLAX";
			originDestinationFilter.Property2 = "NZAKL";
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			CheckPoint.IsAllowed = false;
			originDestinationFilter.Property1 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(loadDischargeFilter, true, true);

			originDestinationFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(loadDischargeFilter, true, true);

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = "AUBNE";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			originDestinationFilter.Property1 = "AUPER";
			originDestinationFilter.Property2 = "AUDRW";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			originDestinationFilter.Property1 = "NZAKL";
			originDestinationFilter.Property2 = "USLAX";
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(loadDischargeFilter, true, true);

			CheckPoint.IsAllowed = true;
			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = "NZAKL";
			loadDischargeFilter.Property2 = "USLAX";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			CheckPoint.IsAllowed = false;
			loadDischargeFilter.Property1 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(loadDischargeFilter, true, true);

			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(loadDischargeFilter, true, true);

			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUBNE";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			loadDischargeFilter.Property1 = "AUPER";
			loadDischargeFilter.Property2 = "AUDRW";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			loadDischargeFilter.Property1 = "AUDRW";
			loadDischargeFilter.Property2 = "";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);

			ModuleLocationFilter originDestinationFilter2 = (ModuleLocationFilter)Bizo.CreateDuplicateFor("Origin / Destination");
			originDestinationFilter2.IsActive = true;

			originDestinationFilter2.OrCategory = FilterOrCategory.Red;

			originDestinationFilter.Property1 = "AUPER";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUPER";
			originDestinationFilter2.Property1 = ZString.Empty;
			originDestinationFilter2.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter2, true, true);

			originDestinationFilter.Property1 = "AUPER";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUDRW";
			originDestinationFilter2.Property1 = ZString.Empty;
			originDestinationFilter2.Property2 = "AUDRW";
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter2, false, false);

			loadDischargeFilter.OrCategory = FilterOrCategory.None;

			loadDischargeFilter.Property1 = "NZAKL";
			loadDischargeFilter.Property2 = "USLAX";
			originDestinationFilter.Property1 = "NZAKL";
			originDestinationFilter.Property2 = "USLAX";
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(loadDischargeFilter, true, true);

			try
			{
				Globals.IsWeb = true;
				assertFilterValidation(originDestinationFilter, false, false);
				assertFilterValidation(loadDischargeFilter, false, false);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestValidatePorts_DateLocationFilter()
		{
			Action<DateLocationFilter, bool> assertFilterValidation = (dateLocationFilter, errorsExpectedOnProperty) =>
			{
				dateLocationFilter.Validation.ValidateAll();
				AssertEquals(errorsExpectedOnProperty, dateLocationFilter.Property3Info.HasErrors());
			};

			JobConsolFilterBusinessObject consolFilter = new JobConsolFilterBusinessObject();
			DateLocationFilter filter = ((DateLocationFilter)consolFilter[JobConsolFilterBusinessObject.Descriptions.ETDLoad]);
			filter.IsActive = true;

			Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = true;
			filter.Property3 = "USLAX";
			assertFilterValidation(filter, false);

			filter.Property3 = "";
			assertFilterValidation(filter, false);

			filter.Property3 = "AUSYD";
			assertFilterValidation(filter, false);

			Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;
			filter.Property3 = "USLAX";
			assertFilterValidation(filter, true);

			filter.Property3 = "";
			assertFilterValidation(filter, true);

			filter.Property3 = "AUSYD";
			assertFilterValidation(filter, false);

			try
			{
				Globals.IsWeb = true;
				filter.Property3 = "USLAX";
				assertFilterValidation(filter, false);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void SetUp()
		{
			base.SetUp();

			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_RL_NKHomePort = "AUBNE";
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUPER";
			newBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUDRW";
			newBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";

			Factory.Save();

			initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			Bizo = new JobShipmentFilterBusinessObject();

			CheckPoint = Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches;

			GlbBranchCollection branches = new GlbBranchCollection(Factory);
			branches.Load();

			for (int i = branches.Count - 1; i > 0; i--)
			{
				GlbBranch branch = branches[i];
				if (branch.PK != GlbBranch.CurrentBranch.PK)
				{
					branches.RemoveAndDelete(branch);
				}
			}
		}

		IUserContext initialUserContext;
		SecurityCheckpoint CheckPoint;
		JobShipmentFilterBusinessObject Bizo;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			base.TearDown();
			Env.SetUserContext(initialUserContext);
		}

		#endregion
	}
}
