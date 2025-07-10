using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class InstructionDivotHelperTest : DtbBookingConsignmentUniversalTestCase
	{
		#region TestPopulatePackageDivots
		[TestDate(2022, 1, 2)]
		public void TestPopulatePackageDivots()
		{
			// set up consignment with instruction
			var consolidation = Factory.RowFactory.NewRowWithPK(DtbBookingConsolidationSchema.Instance);
			consolidation.SetValue(DtbBookingConsolidationSchema.KB_JobID, (ZString)"CM1");
			consolidation.SetValue(DtbBookingConsolidationSchema.KB_JobType, (ZString)TransportConsolidationJobTypes.Codes.Consignment);
			consolidation.SetValue(DtbBookingConsolidationSchema.KB_SystemCreateTimeUtc, ZDateTime.UtcNow);
			consolidation.SetValue(DtbBookingConsolidationSchema.KB_SystemCreateUser, (ZString)"E");
			consolidation.SetValue(DtbBookingConsolidationSchema.KB_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			consolidation.SetValue(DtbBookingConsolidationSchema.KB_SystemLastEditUser, (ZString)"E");
			var consignment = Factory.RowFactory.NewRowWithPK(DtbBookingSchema.Instance);
			consignment.SetValue(DtbBookingSchema.KM_JobID, (ZString)"CN01");
			consignment.SetValue(DtbBookingSchema.KM_JobType, consolidation.GetValue(DtbBookingConsolidationSchema.KB_JobType));
			consignment.SetValue(DtbBookingSchema.KM_KB_Booking, consolidation.GetValue(DtbBookingConsolidationSchema.PK));
			consignment.SetValue(DtbBookingSchema.KM_GB_Branch, GlbBranch.CurrentBranch.PK);
			consignment.SetValue(DtbBookingSchema.KM_SystemCreateTimeUtc, ZDateTime.UtcNow);
			consignment.SetValue(DtbBookingSchema.KM_SystemCreateUser, (ZString)"E");
			consignment.SetValue(DtbBookingSchema.KM_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			consignment.SetValue(DtbBookingSchema.KM_SystemLastEditUser, (ZString)"E");
			var instruction = Factory.RowFactory.NewRowWithPK(DtbBookingInstructionSchema.Instance);
			instruction.SetValue(DtbBookingInstructionSchema.KN_InstructionType, (ZString)InstructionTypes.Codes.PickUp);
			instruction.SetValue(DtbBookingInstructionSchema.KN_KM_BookingMovement, consignment.GetValue(DtbBookingSchema.PK));
			instruction.SetValue(DtbBookingInstructionSchema.KN_Sequence, (ZInt)1);
			instruction.SetValue(DtbBookingInstructionSchema.KN_SystemCreateTimeUtc, ZDateTime.UtcNow);
			instruction.SetValue(DtbBookingInstructionSchema.KN_SystemCreateUser, (ZString)"E");
			instruction.SetValue(DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			instruction.SetValue(DtbBookingInstructionSchema.KN_SystemLastEditUser, (ZString)"E");
			new InstructionDivotHelper(new Instruction(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory).PopulatePackageDivots(instruction, consignment);
			var query = new ZQuery();
			query.AddToFilter(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instruction.GetValue(DtbBookingInstructionSchema.PK));
			AssertEquals("No Package Job, should not create Divots.", 0, Factory.RowFactory.Load(DtbBookingInstructionPkgDivotSchema.Constants.TableName, query).Length);
			var packageJob = Factory.RowFactory.NewRowWithPK(PkgPackageJobSchema.Instance);
			packageJob.SetValue(PkgPackageJobSchema.KJ_ParentID, consignment.GetValue(DtbBookingSchema.PK));
			packageJob.SetValue(PkgPackageJobSchema.KJ_ParentTableCode, (ZString)DtbBookingSchema.Constants.Prefix);
			packageJob.SetValue(PkgPackageJobSchema.KJ_JobID, (ZString)"P1");
			packageJob.SetValue(PkgPackageJobSchema.KJ_SystemCreateTimeUtc, ZDateTime.UtcNow);
			packageJob.SetValue(PkgPackageJobSchema.KJ_SystemCreateUser, (ZString)"A");
			packageJob.SetValue(PkgPackageJobSchema.KJ_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			packageJob.SetValue(PkgPackageJobSchema.KJ_SystemLastEditUser, (ZString)"A");
			new InstructionDivotHelper(new Instruction(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory).PopulatePackageDivots(instruction, consignment);
			AssertEquals("No Packages, should not create Divots.", 0, Factory.RowFactory.Load(DtbBookingInstructionPkgDivotSchema.Constants.TableName, query).Length);
			var package1 = Factory.RowFactory.NewRowWithPK(PkgPackageSchema.Instance);
			var package2 = Factory.RowFactory.NewRowWithPK(PkgPackageSchema.Instance);
			var childPackage = Factory.RowFactory.NewRowWithPK(PkgPackageSchema.Instance);
			package1.SetValue(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.GetValue(PkgPackageJobSchema.PK));
			package2.SetValue(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.GetValue(PkgPackageJobSchema.PK));
			package1.SetValue(PkgPackageSchema.KP_PackageQty, (ZInt)5);
			package2.SetValue(PkgPackageSchema.KP_PackageQty, (ZInt)1);
			package1.SetValue(PkgPackageSchema.KP_Sequence, (ZShort)1);
			package2.SetValue(PkgPackageSchema.KP_Sequence, (ZShort)2);
			package1.SetValue(PkgPackageSchema.KP_F3_NKPackType, (ZString)"PKG");
			package2.SetValue(PkgPackageSchema.KP_F3_NKPackType, (ZString)"PKG");
			package1.SetValue(PkgPackageSchema.KP_SystemCreateTimeUtc, ZDateTime.UtcNow);
			package2.SetValue(PkgPackageSchema.KP_SystemCreateTimeUtc, ZDateTime.UtcNow);
			package1.SetValue(PkgPackageSchema.KP_SystemCreateUser, (ZString)"A");
			package2.SetValue(PkgPackageSchema.KP_SystemCreateUser, (ZString)"A");
			package1.SetValue(PkgPackageSchema.KP_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			package2.SetValue(PkgPackageSchema.KP_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			package1.SetValue(PkgPackageSchema.KP_SystemLastEditUser, (ZString)"A");
			package2.SetValue(PkgPackageSchema.KP_SystemLastEditUser, (ZString)"A");
			childPackage.SetValue(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.GetValue(PkgPackageJobSchema.PK));
			childPackage.SetValue(PkgPackageSchema.KP_KP_ParentPackage, package1.GetValue(PkgPackageSchema.PK));
			childPackage.SetValue(PkgPackageSchema.KP_PackageQty, (ZInt)1);
			childPackage.SetValue(PkgPackageSchema.KP_F3_NKPackType, (ZString)"PKG");
			childPackage.SetValue(PkgPackageSchema.KP_SystemCreateTimeUtc, ZDateTime.UtcNow);
			childPackage.SetValue(PkgPackageSchema.KP_SystemCreateUser, (ZString)"A");
			childPackage.SetValue(PkgPackageSchema.KP_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			childPackage.SetValue(PkgPackageSchema.KP_SystemLastEditUser, (ZString)"A");
			// Should create two Divots, one for each Outer.
			new InstructionDivotHelper(new Instruction(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory).PopulatePackageDivots(instruction, consignment);
			var divots1 = Array.ConvertAll(Factory.RowFactory.Load(DtbBookingInstructionPkgDivotSchema.Constants.TableName, query), DataObjectReader.GetColumnIndexerFromRow);
			var divotForPackage1 = divots1.Single(d => d.GetValue(DtbBookingInstructionPkgDivotSchema.KD_KP_Package) == package1.GetValue(PkgPackageSchema.PK));
			var divotForPackage2 = divots1.Single(d => d.GetValue(DtbBookingInstructionPkgDivotSchema.KD_KP_Package) == package2.GetValue(PkgPackageSchema.PK));
			AssertEquals(instruction.GetValue(DtbBookingInstructionSchema.PK), divotForPackage1.GetValue(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction));
			AssertEquals(instruction.GetValue(DtbBookingInstructionSchema.PK), divotForPackage2.GetValue(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction));
			AssertEquals((ZInt)5, divotForPackage1.GetValue(DtbBookingInstructionPkgDivotSchema.KD_Quantity));
			AssertEquals((ZInt)1, divotForPackage2.GetValue(DtbBookingInstructionPkgDivotSchema.KD_Quantity));
			CombineAssertions("Audit columns are populated correctly", () =>
			{
				AssertEquals("KD_SystemCreateUser", GlbStaff.CurrentUser.GS_Code, divotForPackage1.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemCreateUser));
				AssertEquals("KD_SystemCreateTimeUtc", ZDateTime.UtcNow, divotForPackage1.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemCreateTimeUtc));
				AssertEquals("KD_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code, divotForPackage1.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemLastEditUser));
				AssertEquals("KD_SystemLastEditTimeUtc", ZDateTime.UtcNow, divotForPackage1.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemLastEditTimeUtc));
				AssertEquals("KD_SystemCreateUser", GlbStaff.CurrentUser.GS_Code, divotForPackage2.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemCreateUser));
				AssertEquals("KD_SystemCreateTimeUtc", ZDateTime.UtcNow, divotForPackage2.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemCreateTimeUtc));
				AssertEquals("KD_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code, divotForPackage2.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemLastEditUser));
				AssertEquals("KD_SystemLastEditTimeUtc", ZDateTime.UtcNow, divotForPackage2.GetValue(DtbBookingInstructionPkgDivotSchema.KD_SystemLastEditTimeUtc));
			});
			Factory.SaveForTesting();
			var otherFactory = new UniversalObjectFactory();
			var divotsInOtherFactory = Array.ConvertAll(otherFactory.RowFactory.Load(DtbBookingInstructionPkgDivotSchema.Constants.TableName, query), DataObjectReader.GetColumnIndexerFromRow);
			var divotForPackage1InOtherFactory = divotsInOtherFactory.Single(d => d.GetValue(DtbBookingInstructionPkgDivotSchema.KD_KP_Package) == package1.GetValue(PkgPackageSchema.PK));
			var divotForPackage2InOtherFactory = divotsInOtherFactory.Single(d => d.GetValue(DtbBookingInstructionPkgDivotSchema.KD_KP_Package) == package2.GetValue(PkgPackageSchema.PK));
			var divotBizO1 = otherFactory.Load<DtbConsignmentInstructionPkgDivot>(divotForPackage1InOtherFactory.GetValue(DtbBookingInstructionPkgDivotSchema.PK));
			var divotBizO2 = otherFactory.Load<DtbConsignmentInstructionPkgDivot>(divotForPackage2InOtherFactory.GetValue(DtbBookingInstructionPkgDivotSchema.PK));
			AssertEquals(false, divotBizO1.HasChanges);
			AssertEquals(false, divotBizO2.HasChanges);
			// Existing Divots should get Deleted and new ones created.
			new InstructionDivotHelper(new Instruction(DefaultDataObjectWriterStrategy.TestInstance), Logger, otherFactory).PopulatePackageDivots(instruction, consignment);
			AssertEquals(DataRowState.Deleted, ((DataRow)divotForPackage1InOtherFactory).RowState);
			AssertEquals(DataRowState.Deleted, ((DataRow)divotForPackage2InOtherFactory).RowState);
			AssertEquals(true, divotBizO1.HasChanges);
			AssertEquals(true, divotBizO2.HasChanges);
			var newDivots = otherFactory.RowFactory.Load(DtbBookingInstructionPkgDivotSchema.Constants.TableName, query);
			AssertEquals(2, newDivots.Length);
			AssertCollectionNotContains(divotForPackage1InOtherFactory, newDivots);
			AssertCollectionNotContains(divotForPackage2InOtherFactory, newDivots);
			otherFactory.SaveForTesting();
			AssertEquals(2, new UniversalObjectFactory().Load<DtbConsignmentInstructionPkgDivot>(new ZQuery()).Length);
		}
		#endregion
	}
}
