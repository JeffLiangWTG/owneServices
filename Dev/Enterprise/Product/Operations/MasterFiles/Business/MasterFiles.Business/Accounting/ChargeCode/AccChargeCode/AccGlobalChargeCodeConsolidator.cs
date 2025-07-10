using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business
{
	public class AccGlobalChargeCodeConsolidator
	{
		readonly BusinessObjectFactory factory;

		public AccGlobalChargeCodeConsolidator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public AccChargeCode ConsolidateToGlobal(IEnumerable<AccChargeCode> localCodesToConsolidate)
		{
			var consolidatedCode = AccChargeCode.CreateGlobalChargeCode(factory);
			consolidatedCode.AllowCodeToMatchExisting = true;

			localCodesToConsolidate = localCodesToConsolidate.ToList();

			if (!localCodesToConsolidate.Any())
			{
				return consolidatedCode;
			}

			var sourceChargeCode = localCodesToConsolidate.First();

			// Copy fields that are the same in every charge code
			consolidatedCode.CopyPersistentValuesFrom(sourceChargeCode,
				new BusinessObjectCloneArgs(
					null, AccChargeCode.ColumnNamesToExcludeFromGlobalChargeCodeCopy, null, false, (bo1, bo2, c) => CopyDecide(localCodesToConsolidate, bo1, c)));

			// Ensure that an acceptable charge sub group has been copied
			if (!consolidatedCode.AC_ChargeSubGroup.IsEmpty &&
				!consolidatedCode.Lookups.ChargeSubGroupList.ContainsCode(consolidatedCode.AC_ChargeSubGroup))
			{
				consolidatedCode.AC_ChargeSubGroup = ZString.Empty;
			}

			// Copy collections that are the same in every charge code
			foreach (var snapshotMap in AccChargeCode.GlobalChargeCodeSnapshotMap)
			{
				var sourceVersion = snapshotMap(sourceChargeCode).GetCurrentSnapshot();
				var shouldCopy = localCodesToConsolidate
									.Where(c => c != sourceChargeCode)
									.All(chargeCode => BusinessObjectCollectionCopier.AreEqual(sourceVersion, snapshotMap(chargeCode).GetCurrentSnapshot()));

				if (shouldCopy)
				{
					snapshotMap(consolidatedCode).ImportSnapshot(sourceVersion);
				}
			}

			return consolidatedCode;
		}

		static bool CopyDecide(IEnumerable<AccChargeCode> localCodesToConsolidate, IBusinessObjectInternals sourceChargeCode, DataColumn col)
		{
			var sourceVersion = sourceChargeCode.GetValueFromRowSafely(col, DataRowVersion.Current);

			return localCodesToConsolidate
				.Where(c => c != sourceChargeCode)
				.All(c => sourceVersion.Equals(((IBusinessObjectInternals)c).GetValueFromRowSafely(col, DataRowVersion.Current)));
		}
	}
}
