using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ReciprocalExchangeRatesMigrationTransactionParticipant : SaveInTransactionActionWithMainConnection
	{
		protected GlbCompany company;

		public ReciprocalExchangeRatesMigrationTransactionParticipant(GlbCompany company)
		{
			this.company = company;
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			string sqlCommand = "EXEC updateReciprocalExchangeRates @company, @SystemLastEditUser";
			DbCommand command = Db.Connection.Command(sqlCommand);
			command.AddParameter((NoResString)"@company", SqlDbType.UniqueIdentifier, company.PK.ToGuid());
			command.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
			command.ExecuteNonQuery();
			return ChangedTableNames.All;
		}
	}
}
