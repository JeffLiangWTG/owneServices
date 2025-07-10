using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class InstructionDivotHelper : DataObjectReader<Instruction>
	{
		public InstructionDivotHelper(Instruction dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region PopulatePackageDivots

		public void PopulatePackageDivots(IColumnIndexer instruction, IColumnIndexer consignment)
		{
			var packageJobQuery = new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment[DtbBookingSchema.Constants.PK]);
			var packageJob = factory.RowFactory.Load(PkgPackageJobSchema.Constants.TableName, packageJobQuery).SingleOrDefault();
			if (packageJob != null)
			{
				DeleteExistingPackageDivots(instruction);
				AssignAllPackagesToInstruction(instruction, GetColumnIndexerFromRow(packageJob));
			}
		}

		void DeleteExistingPackageDivots(IColumnIndexer instruction)
		{
			var query = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instruction.GetValue(DtbBookingInstructionSchema.PK));
			var divots = factory.RowFactory.Load(DtbBookingInstructionPkgDivotSchema.Constants.TableName, query);

			foreach (var divot in divots)
			{
				var divotRow = GetColumnIndexerFromRow(divot);
				factory.DeleteRowAndSetHasChanges<DtbConsignmentInstructionPkgDivot>(divotRow, DtbBookingInstructionPkgDivotSchema.PK);
			}
		}

		void AssignAllPackagesToInstruction(IColumnIndexer instruction, IColumnIndexer packageJob)
		{
			var packageQuery = new ZQuery();
			packageQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob[PkgPackageJobSchema.Constants.PK]);
			packageQuery.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);
			var packages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packageQuery);
			var instructionPK = instruction.GetValue(DtbBookingInstructionSchema.PK);

			foreach (IColumnIndexer package in packages)
			{
				AssignPackageToInstruction(instructionPK, package);
			}
		}

		void AssignPackageToInstruction(ZGuid instructionPK, IColumnIndexer package)
		{
			var divot = factory.RowFactory.NewRowWithPK(DtbBookingInstructionPkgDivotSchema.Instance);
			var utcNow = ZDateTime.UtcNow;
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instructionPK);
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_KP_Package, package.GetValue(PkgPackageSchema.PK));
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_Quantity, package.GetValue(PkgPackageSchema.KP_PackageQty));
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_SystemCreateTimeUtc, utcNow);
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_SystemLastEditTimeUtc, utcNow);
			SetValue(divot, DtbBookingInstructionPkgDivotSchema.KD_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code);
		}

		#endregion
	}
}
