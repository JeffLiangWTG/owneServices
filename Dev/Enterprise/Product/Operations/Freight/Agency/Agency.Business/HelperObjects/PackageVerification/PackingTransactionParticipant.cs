using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PackingTransactionParticipant : BaseTransactionParticipant
	{
		public PackingTransactionParticipant(AgencyShipment shipment)
			: base(shipment.Factory)
		{
			this.shipment = shipment;
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			if (!shipment.IsDeleted)
			{
				using (DbCommand command = Db.Connection.Command(Sql)) // Complex query (having)
				{
					command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipment.PK.ToGuid());

					using (IDataReader reader = command.ExecuteReader()) // Complex query (having)
					{
						if (reader.Read())
						{
							throw new PackedIntoMultipleContainersException();
						}
					}
				}
			}
			return ChangedTableNames.Empty;
		}

		readonly AgencyShipment shipment;

		#region Sql

		const string Sql = @"
select top 1 1
from dbo.JobPackLines
join dbo.JobContainerPackPivot on J6_JL = JL_PK
where JL_JS = @shipmentPK
group by JL_PK
having COUNT(*) > 1
";

		#endregion
	}
}
