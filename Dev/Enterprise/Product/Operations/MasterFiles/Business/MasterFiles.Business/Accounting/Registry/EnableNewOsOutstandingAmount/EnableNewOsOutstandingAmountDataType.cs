using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EnableNewOsOutstandingAmountDataType : BooleanRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!proposedValue && (bool)registryItem.Value && IsExistNewOSAmountData(companyPK))
			{
				throw new RegistryValidationException(Res.GetString("DE3493F0-F738-4927-9C21-B0DD04DF520D", @"The registry cannot be turned off once it's set to 'Yes'."));
			}
		}

		bool IsExistNewOSAmountData(Guid companyPK)
		{
			if (companyPK != Guid.Empty)
			{
				return Db.Connection.Exists("FROM dbo.AccTransactionHeader WHERE AH_GC = @CompanyPK AND AH_IsOSOutstandingAmountApplicable = 1", x => x.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK));
			}
			else
			{
				return Db.Connection.Exists("FROM dbo.AccTransactionHeader WHERE AH_IsOSOutstandingAmountApplicable = 1");
			}
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
