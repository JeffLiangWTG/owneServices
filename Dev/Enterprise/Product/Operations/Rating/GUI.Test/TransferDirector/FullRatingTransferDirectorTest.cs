using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	sealed class FullRatingTransferDirectorTest : TestCaseWithFactory
	{
		public void TestAdapter()
		{
			var director = new FullRatingTransferDirectorForTest();
			AssertEquals("Adapter", typeof(FullClientRatesValueObjectDataAdapter), director.Adapter.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleRatingSavesSuccessfulEvenWhenOthersSkip()
		{
			ZGuid orgPk1 = new ZGuid("1b334475-15ee-4890-9c8d-db8ee8ea36ee");
			ZGuid orgPk2 = new ZGuid("17e67617-1e39-469a-bd12-d3a602bfcfc5");
			string sqlText = string.Format("INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{0}', 'Org1')", orgPk1.ToString());
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			var org1BeforeImport = Factory.Load<OrgHeader>(orgPk1);
			var org2BeforeImport = Factory.Load<OrgHeader>(orgPk2);

			AssertNotNull("[PRE-CONDITION] OrgPk1 should exist", org1BeforeImport);
			AssertNull("[PRE-CONDITION] OrgPk2 should not exist", org2BeforeImport);

			Factory.Save();

			var director = new FullRatingTransferDirectorForTest();
			string fileName1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Rating\GUI.Test\TransferDirector\Testing\TestMultipleRating.xml");
			director.Import(fileName1, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			//Load RatingHeader from Database using a new factory
			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			var ratingHeader1 = loadFactory.Load<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_OH, orgPk1));
			var ratingHeader2 = loadFactory.Load<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_OH, orgPk2));

			AssertEquals("ratingHeader1 should have been imported", 1, ratingHeader1.Length);
			AssertEquals("ratingHeader2 should not have been imported", 0, ratingHeader2.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleRateEntryOverlap()
		{
			//To ensure context.FactoryProvider.CreateNewWithoutSave retrieves the latest registry data from database that cause the error
			EnvProxy.Instance.Registry.RawRegistry.ReportCrossThreadFactoryAccess.Options |= RegistryOptions.NotCached;

			var orgPk1 = new ZGuid("1b334475-15ee-4890-9c8d-db8ee8ea36ee");
			var orgPk2 = new ZGuid("17e67617-1e39-469a-bd12-d3a602bfcfc5");

			var sqlText1 = string.Format("INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{0}', 'Org1')", orgPk1.ToString());
			var sqlText2 = string.Format("INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{0}', 'Org2')", orgPk2.ToString());
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText1);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText2);

			var org1BeforeImport = Factory.Load<OrgHeader>(orgPk1);
			var org2BeforeImport = Factory.Load<OrgHeader>(orgPk2);

			AssertNotNull("[PRE-CONDITION] OrgPk1 should exist", org1BeforeImport);
			AssertNotNull("[PRE-CONDITION] OrgPk2 should exist", org2BeforeImport);

			Factory.Save();

			var director = new FullRatingTransferDirectorForTest();
			var fileName = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Rating\GUI.Test\TransferDirector\Testing\TestMultipleRatingDuplicate.xml");
			var notification = new NotificationBuffer();
			director.Import(fileName, notification, SourceInfo.EmptySourceInfo);

			AssertEquals("Error", true, notification.HasErrors);
			var errorEvents = notification.GetEventsByType(ErrorType.Error);
			AssertStartsWith
			("error message", $@"
Error: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: RateEntry",
				errorEvents[0].Message
			);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleRatingSavesSuccessfulEvenWhenOthersFail()
		{
			ZGuid orgPk1 = new ZGuid("1b334475-15ee-4890-9c8d-db8ee8ea36ee");
			ZGuid orgPk2 = new ZGuid("17e67617-1e39-469a-bd12-d3a602bfcfc5");
			string sqlText = string.Format("INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{0}', 'Org1')", orgPk1.ToString());
			string sqlText2 = string.Format("INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{0}', 'Org2')", orgPk2.ToString());
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText2);

			var org1BeforeImport = Factory.Load<OrgHeader>(orgPk1);
			var org2BeforeImport = Factory.Load<OrgHeader>(orgPk2);

			AssertNotNull("[PRE-CONDITION] OrgPk1 should exist", org1BeforeImport);
			AssertNotNull("[PRE-CONDITION] OrgPk2 should exist", org2BeforeImport);

			Factory.Save();

			FullRatingTransferDirectorForTest director = new FullRatingTransferDirectorForTest();
			string fileName1 = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Rating\GUI.Test\TransferDirector\Testing\TestMultipleRating_WithFailure.xml");
			director.Import(fileName1, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			//Load RatingHeader from Database using a new factory
			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			var ratingHeader1 = loadFactory.Load<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_OH, orgPk1));
			var ratingHeader2 = loadFactory.Load<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_OH, orgPk2));

			AssertEquals("ratingHeader1 should not have been imported", 0, ratingHeader1.Length);
			AssertEquals("ratingHeader2 should have been imported", 1, ratingHeader2.Length);
		}

		class FullRatingTransferDirectorForTest : FullRatingTransferDirector
		{
			public new IValueObjectDataAdapter Adapter
			{
				get { return base.Adapter; }
			}
		}
	}
}
