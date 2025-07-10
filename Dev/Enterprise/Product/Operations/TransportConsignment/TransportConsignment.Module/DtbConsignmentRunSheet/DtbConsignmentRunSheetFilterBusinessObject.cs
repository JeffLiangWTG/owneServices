using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbConsignmentRunSheetFilterBusinessObject : FilterStripBusinessObject, IDtbConsignmentRunSheetFilterBusinessObject
	{
		#region Filter constants

		public static class FilterConstants
		{
			public const string RunSheetNumber = "RunSheetNumber";
			public const string IsActiveStatus = "IsActive";
			public const string StartTime = "StartTime";
			public const string EndTime = "EndTime";
			public const string DriverLicense = "DriverLicense";
			public const string DriverName = "DriverName";
			public const string DriverBranch = "DriverBranch";
			public const string StaffDriver = "StaffDriver";
			public const string TransportCompany = "TransportCompany";
			public const string TransportCompanyName = "TransportCompanyName";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Vehicle = "Vehicle";
			public const string VehicleRegistration = "VehicleRegistration";
			public const string HasFailedRunSheetInstructions = "HasFailedRunSheetInstructions";
		}

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddDatesFilters(filters);
			AddOrganisationFilters(filters);
			AddStatusAndFlagsFilters(filters);

			return filters;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var runSheetNumberFilter = new ModuleFountainFilter(FilterConstants.RunSheetNumber, DtbConsignmentRunSheetSchema.KG_RunSheetNumber, "CR");
			runSheetNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|RunSheetNumber", "Run Sheet Number");
			runSheetNumberFilter.Category = FilterCategories.NumbersAndReferences;
			runSheetNumberFilter.MaxLength = DtbConsignmentRunSheetSchema.KG_RunSheetNumber.MaxLength;
			return runSheetNumberFilter;
		}

		#endregion

		#region AddDatesFilters

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var startDateFilter = filters.AddDateFilter(FilterConstants.StartTime, DtbConsignmentRunSheetSchema.KG_StartTime);
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|StartTime", "Start Time");

			var endDateFilter = filters.AddDateFilter(FilterConstants.EndTime, DtbConsignmentRunSheetSchema.KG_EndTime);
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|EndTime", "End Time");
		}

		#endregion

		#region Organisations Filters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var driverLicenseFilter = filters.AddTextFilter(FilterConstants.DriverLicense, GetDriverLicenseQuery);
			driverLicenseFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|DriverLicense", "Driver's License");
			driverLicenseFilter.Category = FilterCategories.Organisations;
			driverLicenseFilter.MaxLength = Math.Min(GenRegCertAccredMaintListSchema.XZ_RefNumber.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocDriversLicence.MaxLength);

			var driverNameFilter = filters.AddTextFilter(FilterConstants.DriverName, GetDriverNameQuery);
			driverNameFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|DriverName", "Driver's Name");
			driverNameFilter.Category = FilterCategories.Organisations;
			driverNameFilter.MaxLength = Math.Min(GlbStaffSchema.GS_FullName.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocDriversName.MaxLength);

			var driverBranchFilter = filters.AddGuidFilter(FilterConstants.DriverBranch, ModuleIDs.GlbBranch, GetDriverBranchQuery, Branches);
			driverBranchFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|DriverBranch", "Driver's Branch");
			driverBranchFilter.Category = FilterCategories.Organisations;

			var staffDriverFilter = filters.AddGuidFilter(FilterConstants.StaffDriver, ModuleIDs.GlbStaff, GetStaffDriverQuery, StaffDrivers);
			staffDriverFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|StaffDriver", "Staff Driver");
			staffDriverFilter.Category = FilterCategories.Organisations;

			var transportCompanyFilter = filters.AddGuidFilter(FilterConstants.TransportCompany, ModuleIDs.Organisation, DtbConsignmentRunSheetSchema.KG_OH_TransportCo, new OrgHeaderCollection(Factory));
			transportCompanyFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|TransportCompany", "Transport Company");
			transportCompanyFilter.Category = FilterCategories.Organisations;

			var transportCompanyNameFilter = filters.AddTextFilter(FilterConstants.TransportCompanyName, GetTransportCompanyNameQuery);
			transportCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|TransportCompanyName", "Transport Company Name");
			transportCompanyNameFilter.Category = FilterCategories.Organisations;
			transportCompanyNameFilter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocTransportCoName.MaxLength);

			var vehicleFilter = filters.AddGuidFilter(FilterConstants.Vehicle, ModuleIDs.RefEquipment, DtbConsignmentRunSheetSchema.KG_RQ_Truck, Vehicles);
			vehicleFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|Vehicle", "Vehicle");
			vehicleFilter.Category = FilterCategories.Organisations;

			var vehicleRegistrationFilter = filters.AddTextFilter(FilterConstants.VehicleRegistration, GetVechicleRegistrationQuery);
			vehicleRegistrationFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|VehicleRegistration", "Vehicle Registration");
			vehicleRegistrationFilter.Category = FilterCategories.Organisations;
			vehicleRegistrationFilter.MaxLength = Math.Min(RefEquipmentSchema.RQ_Registration.MaxLength, DtbConsignmentRunSheetSchema.KG_AdHocTruckRegistration.MaxLength);
		}

		ZQuery GetDriverLicenseQuery(SQLComparisonOperator comparisonOperator, ZString license)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));

			var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);

			var certificateSubQuery = new ZDBOnlySubQuery(typeof(GenRegCertAccredMaintList), GenRegCertAccredMaintListSchema.XZ_ParentID);
			certificateSubQuery.AddToFilter(GenRegCertAccredMaintListSchema.XZ_Type, CertificateTypePairList.Codes.CA1);
			certificateSubQuery.AddToFilter(GenRegCertAccredMaintListSchema.XZ_RefNumber, comparisonOperator, license);

			staffQuery.AddSubQuery(GlbStaffSchema.PK, certificateSubQuery, JoinCondition.And);
			query.AddSubQuery(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, staffQuery, JoinCondition.Or);

			query.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetSchema.KG_AdHocDriversLicence, comparisonOperator, license);

			return query;
		}

		ZQuery GetDriverNameQuery(SQLComparisonOperator comparisonOperator, ZString driverName)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));

			var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffQuery.AddToFilter(GlbStaffSchema.GS_FullName, comparisonOperator, driverName);
			query.AddSubQuery(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, staffQuery, JoinCondition.Or);
			query.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetSchema.KG_AdHocDriversName, comparisonOperator, driverName);

			return query;
		}

		#region GetDriverBranchQuery

		ZQuery GetDriverBranchQuery(ZGuid branchPK)
		{
			ZQuery result = null;

			var branch = Factory.Load<GlbBranch>(branchPK);
			var driversGroup = branch != null ? GetDriverGroup(branch.Company.PK.ToGuid(), branch.PK.ToGuid()) : ZGuid.Empty;
			if (!driversGroup.IsEmpty)
			{
				var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
				groupSubQuery.AddToFilter(GlbGroupSchema.PK, driversGroup);

				var groupLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
				groupLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, groupSubQuery, JoinCondition.And);

				var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
				staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
				staffQuery.AddSubQuery(groupLinkQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));
				query.AddSubQuery(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, staffQuery, JoinCondition.Or);

				result = query;
			}

			return result ?? ZQuery.NoResultQuery;
		}

		ZGuid GetDriverGroup(Guid companyPK, Guid branchPK)
		{
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			return new ZGuid(transportRegistry.TransportDriversGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
		}

		#endregion

		ZQuery GetStaffDriverQuery(ZGuid staffGuid)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));

			var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffQuery.AddToFilter(GlbStaffSchema.PK, staffGuid);
			query.AddSubQuery(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, staffQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));

			var orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), DtbConsignmentRunSheetSchema.KG_OH_TransportCo);
			orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName);
			query.AddSubQuery(orgQuery, JoinCondition.Or);
			query.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetSchema.KG_AdHocTransportCoName, comparisonOperator, companyName);

			return query;
		}

		ZQuery GetVechicleRegistrationQuery(SQLComparisonOperator comparisonOperator, ZString registration)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));

			var vehicleQuery = new ZDBOnlySubQuery(typeof(RefEquipment), DtbConsignmentRunSheetSchema.KG_RQ_Truck);
			vehicleQuery.AddToFilter(RefEquipmentSchema.RQ_Registration, comparisonOperator, registration);
			query.AddSubQuery(vehicleQuery, JoinCondition.Or);
			query.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetSchema.KG_AdHocTruckRegistration, comparisonOperator, registration);

			return query;
		}

		#endregion

		#region Status and Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var runActiveStatusFilter = filters.AddTextFilter(FilterConstants.IsActiveStatus, GetStatusQuery, ActiveStatuses);
			runActiveStatusFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|IsActive", "Run Sheet Status");
			runActiveStatusFilter.Category = FilterCategories.StatusAndFlags;

			var hasFailedInstructionsFilter = filters.AddFlagsFilter(FilterConstants.HasFailedRunSheetInstructions,
				new[] { Res.GetString("2c9f2858-3535-4c72-939b-b26f508ec27d", "Run Sheets which has failed instructions") },
				new GetFlagsQuery[] { GetHasFailedRunSheetInstructionsQuery });
			hasFailedInstructionsFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentRunSheetFilterBusinessObject|HasFailedRunSheetInstructions", "Failed Run Sheet Instructions");
			hasFailedInstructionsFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetHasFailedRunSheetInstructionsQuery(ZBool hasFailedInstructions)
		{
			var runSheetQuery = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));

			var notInFlag = !hasFailedInstructions;
			var hasFailedRunSheetInstructionsSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet, notInFlag);
			hasFailedRunSheetInstructionsSubQuery.AddToFilter(DtbConsignmentRunSheetInstructionSchema.K1_FailureReason, SQLComparisonOperator.NotEqual, "");
			// if hasFailedInstructions is set to TRUE, than we look for run sheets with PK "IN" K1_KG_RunSheet for failed instructions
			// if hasFailedInstructions is set to FALSE, than we look for run sheets with PK "NOT IN" K1_KG_RunSheet for failed instructions

			runSheetQuery.AddSubQuery(hasFailedRunSheetInstructionsSubQuery, JoinCondition.And);
			return runSheetQuery;
		}

		ZQuery GetStatusQuery(ZString activeStatus)
		{
			var runSheetQuery = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));
			if (activeStatus == IsActiveStatuses.Codes.Completed)
			{
				// at least has one instruction that has started
				var hasInstructionsSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
				hasInstructionsSubQuery.AddToFilter(GetInProgressInstructionQuery());
				runSheetQuery.AddSubQuery(hasInstructionsSubQuery, JoinCondition.And);

				// has no instructions with empty time in or time out
				var completeSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet, true);
				completeSubQuery.AddToFilter(GetNotCompletedInstructionQuery());
				runSheetQuery.AddSubQuery(completeSubQuery, JoinCondition.And);
			}
			else if (activeStatus == IsActiveStatuses.Codes.InProgress)
			{
				// has some instructions in progress, but not all complete
				var inProgressSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
				inProgressSubQuery.AddToFilter(GetInProgressInstructionQuery());

				var notCompletedSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
				notCompletedSubQuery.AddToFilter(GetNotCompletedInstructionQuery());

				runSheetQuery.AddSubQuery(inProgressSubQuery, JoinCondition.And);
				runSheetQuery.AddSubQuery(notCompletedSubQuery, JoinCondition.And);
			}
			else if (activeStatus == IsActiveStatuses.Codes.NotStarted)
			{
				// has no instructions that have started
				var notStartedInstructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet, true);
				notStartedInstructionSubQuery.AddToFilter(GetInProgressInstructionQuery());
				runSheetQuery.AddSubQuery(notStartedInstructionSubQuery, JoinCondition.And);

				// or has no instructions at all
				var noInstructionsSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet, true);
				runSheetQuery.AddSubQuery(noInstructionsSubQuery, JoinCondition.Or);
				runSheetQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_IsPlanning, false);
			}
			else if (activeStatus == IsActiveStatuses.Codes.Planning)
			{
				runSheetQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_IsPlanning, true);
			}

			return runSheetQuery;
		}

		ZQuery GetInProgressInstructionQuery()
		{
			var instructionQuery = new ZQuery();
			instructionQuery.AddToFilter(DtbConsignmentRunSheetInstructionSchema.K1_TimeIn, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			instructionQuery.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetInstructionSchema.K1_TimeOut, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			instructionQuery.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetInstructionSchema.K1_IsAcceptedByDriver, SQLComparisonOperator.Equal, true);
			instructionQuery.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetInstructionSchema.K1_FailureReason, SQLComparisonOperator.NotEqual, ZString.Empty);
			return instructionQuery;
		}

		ZQuery GetNotCompletedInstructionQuery()
		{
			var instructionQuery = new ZQuery();
			instructionQuery.AddToFilter(DtbConsignmentRunSheetInstructionSchema.K1_TimeIn, SQLComparisonOperator.Equal, ZDateTime.Empty);
			instructionQuery.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetInstructionSchema.K1_TimeOut, SQLComparisonOperator.Equal, ZDateTime.Empty);
			instructionQuery.AddToFilter(JoinCondition.Or, DtbConsignmentRunSheetInstructionSchema.K1_ReceivedBy, SQLComparisonOperator.Equal, ZString.Empty);
			return instructionQuery;
		}

		#endregion

		#region Lists

		#region ActiveStatuses

		IsActiveStatuses ActiveStatuses
		{
			get { return Factory.GetCachedValue("DtbConsignmentRunSheetFilterBusinessObject|IsActive", () => new IsActiveStatuses()); }
		}

		#endregion

		#region StaffDrivers

		StaffDriverCollection StaffDrivers
		{
			get
			{
				if (staffDrivers == null)
				{
					staffDrivers = new StaffDriverCollection(Factory);
				}
				return staffDrivers;
			}
		}
		StaffDriverCollection staffDrivers;

		#endregion

		#region Branches

		GlbBranchCollection Branches
		{
			get
			{
				if (branches == null)
				{
					branches = new GlbBranchCollection(Factory);
				}
				return branches;
			}
		}

		GlbBranchCollection branches;

		#endregion

		#region Vehicles

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter")]
		RefEquipmentCollection Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					vehicles = new RefEquipmentCollection(Factory, new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true));
					vehicles.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vehicle Status", "Property", (ZString)"Is a Vehicle"));
				}
				return vehicles;
			}
		}

		RefEquipmentCollection vehicles;

		#endregion

		#endregion
	}
}
