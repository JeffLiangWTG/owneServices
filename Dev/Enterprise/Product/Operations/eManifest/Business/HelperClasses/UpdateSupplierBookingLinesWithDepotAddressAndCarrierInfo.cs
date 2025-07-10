using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.eManifest.Business
{
	public static class UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo
	{
		public static void Execute(Guid headerPk)
		{
			using (var command = Db.Connection.Command("UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@headerPk", SqlDbType.UniqueIdentifier, headerPk);
				command.ExecuteNonQuery();
			}
		}
	}
}
