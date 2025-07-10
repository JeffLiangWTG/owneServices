using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UnmatchNoteCreatorTest : TestCaseWithFactory
	{
		public void TestUseSameUnmatchedNoteForDifferentSTTable()
		{
			UnmatchNoteCreator creator = new UnmatchNoteCreator(Factory);
			string description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;

			IQuotedBooking booking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			string sql = @"INSERT INTO [dbo].[StmNote]
           ([ST_PK]
           ,[ST_ParentID]
           ,[ST_Table]
           ,[ST_Description]
           ,[ST_IsCustomDescription]
           ,[ST_ForceRead]
           ,[ST_NoteText]
           ,[ST_NoteType]
           ,[ST_NoteContext])
     VALUES
           ('{0}'
           ,'{1}'
           ,'ViewQuotedBooking'
           ,'{2}'
           ,0
           ,1
           ,'<UnmatchOrgRecords></UnmatchOrgRecords>'
           ,'INT'
           ,'AAA')";

			Db.Connection.ExecuteNonQuery(string.Format(sql, ZGuid.NewZGuid(), booking.ViewPK, description));

			ZQuery loadQuery = new ZQuery(StmNoteSchema.ST_ParentID, booking.ViewPK);
			loadQuery.AddToFilter(StmNoteSchema.ST_Description, description);
			loadQuery.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.NotEqual, "OrgHeader");//not including the default record for org "UnMatched"
			var notes = Factory.Load<StmNote>(loadQuery);
			AssertEquals(1, notes.Length);

			BusinessObject shipment = Factory.Load<Enterprise.Integration.Freight.ICommonShipment>(booking.ViewPK) as BusinessObject;
			EntityInfo info = EntityInfo.New(shipment);
			creator.Create(info, new UnmatchOrgRecord());

			notes = Factory.Load<StmNote>(loadQuery);
			AssertEquals("Should use the existing stmnote created for booking.", 1, notes.Length);
		}
	}
}
