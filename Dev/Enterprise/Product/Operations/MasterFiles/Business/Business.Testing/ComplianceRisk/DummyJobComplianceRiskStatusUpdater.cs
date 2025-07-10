using System;
using CargoWise.Data;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyJobComplianceRiskStatusUpdater : RelatedJobComplianceRiskStatusUpdater
	{
		int index;

		public Guid CompanyPK { get; set; }

		protected override DbCommand GetCommandForOrgUpdate(Guid orgPK, Guid companyPK)
		{
			var code = "NEWCODE" + index;
			index++;
			var cmd = Db.Connection.Command("Update dbo.OrgHeader set OH_Code = @Code where OH_PK = @OrgPk");
			cmd.AddParameter("@Code", System.Data.SqlDbType.NVarChar, code);
			cmd.AddParameter("@OrgPk", System.Data.SqlDbType.UniqueIdentifier, orgPK);

			CompanyPK = companyPK;

			return cmd;
		}

		protected override DbCommand GetCommandForVesselUpdate(Guid vesselPK, Guid companyPK)
		{
			var code = "NEWCODE" + index;
			index++;
			var cmd = Db.Connection.Command("Update dbo.RefVessel set RV_Code = @Code where RV_PK = @VesselPk");
			cmd.AddParameter("@Code", System.Data.SqlDbType.NVarChar, code);
			cmd.AddParameter("@VesselPk", System.Data.SqlDbType.UniqueIdentifier, vesselPK);

			CompanyPK = companyPK;

			return cmd;
		}
	}
}
