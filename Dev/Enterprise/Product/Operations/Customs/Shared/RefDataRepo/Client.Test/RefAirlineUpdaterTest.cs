using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefAirlineUpdaterTest : DSUpdaterTest<RefAirline, IRefAirline>
	{
		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IRefAirline), 1);
			yield return Tuple.Create(typeof(IStmNote), 1);
		}

		protected override RefAirline GetServerData()
		{
			var airLine = new RefAirline
			{
				RM_AccountingCode = "391",
				RM_TwoCharacterCode = "Q3",
				RM_ThreeLetterCode = "MYD",
				RM_AddressLine1 = "address line",
				RM_IsActive = true,
				RM_EagleAddedAirlinePrefixOrAccountingCode = "391",
				RM_AccountingSecondaryFlag = "X",
				RM_AddressLine2 = "X",
				RM_AirlineCity = "X",
				RM_AirlineCountry = "X",
				RM_AirlineName1 = "Zambian Airways",
				RM_AirlineName2 = "X",
				RM_AirlinePostalCode = "X",
				RM_RN_NKAirlineCountry = "",
				RM_AirlinePrefix = "X",
				RM_AirlinePrefixSecondaryFlag = "X",
				RM_AirlineState = "X",
				RM_ContactNameOCIIdentifier = "X",
				RM_ContactPhoneOCIIdentifier = "X",
				RM_DuplicateFlagIndicator = false,
				RM_EmergencyContactName = "X",
				RM_EmergencyContactTitle = "X",
				RM_EmergencyTeletype = "X",
				RM_IsCASSControlled = false,
				RM_LabelShortName = "",
				RM_MembershipFlagARINC = false,
				RM_MembershipFlagATA = false,
				RM_MembershipFlagIATA = false,
				RM_MembershipFlagSITA = false,
				RM_ReservationsContactName = "X",
				RM_ReservationsContactTeletype = "X",
				RM_ReservationsContactTitle = "X",
				RM_ReservationsDeptTeletype = "X",
				RM_TypeOfOperationsCode = "X"
			};

			airLine.StmNotes = new StmNote[]
			{
				new StmNote
				{
					ST_Description = "Terms and Conditions",
					ST_NoteContext = "XXX",
					ST_NoteText = "X",
					ST_ForceRead = false,
					ST_NoteType = "X",
					ST_Table = nameof(RefAirline)
				}
			};

			return airLine;
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var refCountry = serverData as RefAirline;
			if (refCountry != null)
			{
				refCountry.RM_AirlineCountry = "XX";
				return Tuple.Create(nameof(RefAirline.RM_AirlineCountry), "XX");
			}
			var stmNote = serverData as StmNote;
			if (stmNote != null)
			{
				stmNote.ST_NoteText = "XX";
				return Tuple.Create(nameof(StmNote.ST_NoteText), "XX");
			}
			return null;
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefAirlineUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter());
		}

		protected override void TearDown()
		{
			base.TearDown();
			using (var mainConn = Db.NewAdminConnection())
			{
				mainConn.ExecuteNonQuery(tearDownScript);
			}
		}
		public override void AssertUpdatePreCondition()
		{
			return;
		}

		public override void AssertDeletePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
		}

		public void TestIgnoreColumns()
		{
			var propertyNames = typeof(IRefAirline).GetProperties().Where(x => !x.PropertyType.Name.Contains("IEnumerable")).Select(x => x.Name);
			foreach (var columnName in ignoreColumns)
			{
				AssertEquals($"IRefAirline should not contain {columnName}", false, propertyNames.Contains(columnName));
			}
		}
		public void TestMerge()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline (RM_PK, RM_IsActive, RM_IsSystem, RM_AirlineName1, RM_AirlineName2, RM_AccountingCode, RM_ThreeLetterCode, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2, RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype,
			RM_EmergencyTeletype, RM_EmergencyContactName, RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag, RM_EagleAddedAirlinePrefixOrAccountingCode)
			VALUES ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', 1, 1, 'American Airlines Inc.', '', '001', 'AAL', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '001', '', '001'),
			('01FEF126-6910-4525-B024-262D977214E9', 1, 1, 'Zambian Airways', '', '391', 'MAZ', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', '', '391'),
			('C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5', 1, 1, 'Maya Island Air', '', '', 'MYD', 'MW', 0, 'P.O. Box 458', '', 'Belize City', '', 'Belize', '', 'HDQSDMW', '', '', '', 'HDQSDMW', '', '', 1, 1, 0, 0, 'A', '', '', '', '');");

			var serverData = GetServerData();
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = 'C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
			AssertEquals("MAZ", conn.ExecuteScalar("SELECT RM_ThreeLetterCode FROM dbo.RefAirline WHERE RM_PK = '01FEF126-6910-4525-B024-262D977214E9'"));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = 'C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
			AssertEquals("MYD", conn.ExecuteScalar("SELECT RM_ThreeLetterCode FROM dbo.RefAirline WHERE RM_PK = '01FEF126-6910-4525-B024-262D977214E9'"));
			conn.ExecuteNonQuery(@"DELETE FROM dbo.RefAirline;");
		}

		public void TestNotDeactivateValidRefAirlines()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline (RM_PK, RM_IsActive, RM_IsSystem, RM_AirlineName1, RM_AirlineName2, RM_AccountingCode, RM_ThreeLetterCode, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2, RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype,
			RM_EmergencyTeletype, RM_EmergencyContactName, RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag, RM_EagleAddedAirlinePrefixOrAccountingCode)
			VALUES ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', 1, 1, 'American Airlines Inc.', '', '391', 'AAL', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', '', '391'),
			('01FEF126-6910-4525-B024-262D977214E9', 0, 1, 'Zambian Airways', '', '391', '', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', '', '391'),
			('C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5', 1, 1, 'Air Busan', '', '982', '', 'MW', 0, 'P.O. Box 458', '', 'Belize City', '', 'Belize', '', 'HDQSDMW', '', '', '', 'HDQSDMW', '', '', 1, 1, 0, 0, 'A', '', '982', '', '982');");
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_AirlineName1 = 'Air Busan'"));

			var serverData = GetServerData();
			serverData.RM_ThreeLetterCode = "";
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_AirlineName1 = 'Air Busan'"));
		}

		public void TestMerge_OneClientMatchMultiServer()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'MYD', 'Zambian Airways', 1, 1, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', ''),
					('01FEF126-6910-4525-B024-262D977214E9', '', 'ABC', 'MYD Inc.', 1, 1, '', 1, '', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '', '');");
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));

			var serverData1 = GetServerData();
			serverData1.RM_ThreeLetterCode = "ABC";
			var serverData2 = GetServerData();
			serverData2.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			var proxy = Helper.GetServerProxy(new RefAirline[] { serverData1, serverData2 });
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'ABC' AND RM_PK='6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='' AND RM_ThreeLetterCode = 'ABC' AND RM_PK='01FEF126-6910-4525-B024-262D977214E9'"));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='123' AND RM_ThreeLetterCode = 'MYD'"));
		}

		public void TestMerge_MultiClientMatchMultiServer_WhenOneClientNotUpdatable()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'MYD', 'Zambian Airways', 1, 0, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', ''),
					('01FEF126-6910-4525-B024-262D977214E9', '', 'ABC', 'MYD Inc.', 1, 1, '', 1, '', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '', '');");
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));

			var serverData1 = GetServerData();
			serverData1.RM_ThreeLetterCode = "ABC";
			var serverData2 = GetServerData();
			serverData2.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			var proxy = Helper.GetServerProxy(new RefAirline[] { serverData1, serverData2 });
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK='6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='' AND RM_ThreeLetterCode = 'ABC' AND RM_PK='01FEF126-6910-4525-B024-262D977214E9'"));
		}

		public void TestMerge_MultiClientMatchOneServer()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'AAL', 'AAL Inc.', 1, 1, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', ''),
					('01FEF126-6910-4525-B024-262D977214E9', '982', 'MYD', 'MYD Inc.', 1, 1, '982', 1, '', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', ''),
					('C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5', '391', 'MYD', 'Zambian Airways', 0, 1, '391', 1, '', 'MW', 0, 'P.O. Box 458', '', 'Belize City', '', 'Belize', '', 'HDQSDMW', '', '', '', 'HDQSDMW', '', '', 1, 1, 0, 0, 'A', '', '982', '');");

			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='982' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = '01FEF126-6910-4525-B024-262D977214E9'"));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = 'C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
			AssertNotEquals("Zambian Airways", conn.ExecuteScalar("SELECT RM_AirlineName1 FROM dbo.RefAirline WHERE RM_PK='C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
		}

		public void TestMerge_MultiClientMatchOneServer1()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'AAL', 'AAL Inc.', 1, 1, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', ''),
					('01FEF126-6910-4525-B024-262D977214E9', '982', 'MYD', 'MYD Inc.', 1, 1, '982', 1, '', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', '');");

			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='982' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = '01FEF126-6910-4525-B024-262D977214E9'"));
		}

		public void TestMerge_MultiClientMatchOneServer2()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'AAL', 'AAL Inc.', 1, 1, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', ''),
					('C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5', '391', 'MYD', 'Zambian Airways', 0, 1, '391', 1, '', 'MW', 0, 'P.O. Box 458', '', 'Belize City', '', 'Belize', '', 'HDQSDMW', '', '', '', 'HDQSDMW', '', '', 1, 1, 0, 0, 'A', '', '982', '');");

			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = 'C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
			AssertNotEquals("Zambian Airways", conn.ExecuteScalar("SELECT RM_AirlineName1 FROM dbo.RefAirline WHERE RM_PK='C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
		}

		public void TestMerge_MultiClientMatchOneServer3()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('01FEF126-6910-4525-B024-262D977214E9', '982', 'MYD', 'MYD Inc.', 1, 1, '982', 1, '', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', ''),
					('C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5', '391', 'MYD', 'Zambian Airways', 0, 1, '391', 1, '', 'MW', 0, 'P.O. Box 458', '', 'Belize City', '', 'Belize', '', 'HDQSDMW', '', '', '', 'HDQSDMW', '', '', 1, 1, 0, 0, 'A', '', '982', '');");

			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK='01FEF126-6910-4525-B024-262D977214E9'"));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = 'C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
			AssertNotEquals("Zambian Airways", conn.ExecuteScalar("SELECT RM_AirlineName1 FROM dbo.RefAirline WHERE RM_PK='C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
		}

		public void TestMerge_SingleInactiveClientMatchOneActiveServer()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5', '391', 'MYD', 'Zambian Airways', 0, 1, '391', 1, '', 'MW', 0, 'P.O. Box 458', '', 'Belize City', '', 'Belize', '', 'HDQSDMW', '', '', '', 'HDQSDMW', '', '', 1, 1, 0, 0, 'A', '', '982', '');");

			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode='391' AND RM_ThreeLetterCode = 'MYD' AND RM_PK = 'C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
			AssertEquals("Zambian Airways", conn.ExecuteScalar("SELECT RM_AirlineName1 FROM dbo.RefAirline WHERE RM_PK='C4C6AC83-4B2B-4EBA-B702-F2C8C5BB1EC5'"));
		}

		public void TestMerge_InactiveAirlinesFromServer()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'MYD', 'Zambian Airways', 1, 1, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', '');");
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));

			var serverData = GetServerData();
			serverData.RM_IsActive = false;
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy), "InactiveRefAirline");
			AssertEquals(false, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
		}

		public void TestMerge_NotUpdatableInactiveAirlinesFromServer()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_IsUpdatable, RM_AccountingCode, RM_IsSystem, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
								RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
								RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '391', 'MYD', 'Zambian Airways', 1, 0, '391', 1, '', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', '');");
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));

			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy), "InactiveRefAirline");
			AssertEquals(true, conn.ExecuteScalar("SELECT RM_IsActive FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
		}

		public void TestNotUpdateStmNoteWhichIsNotTermsAndConditions()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline (RM_PK, RM_IsActive, RM_IsSystem, RM_AirlineName1, RM_AirlineName2, RM_AccountingCode, RM_ThreeLetterCode, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2, RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype,
			RM_EmergencyTeletype, RM_EmergencyContactName, RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag, RM_EagleAddedAirlinePrefixOrAccountingCode)
			VALUES ('01FEF126-6910-4525-B024-262D977214E9', 1, 1, 'Zambian Airways', '', '391', 'MYD', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', '', '391');

			INSERT INTO dbo.StmNote (ST_PK, ST_ParentId, ST_Table, ST_Description, ST_ForceRead, ST_NoteText, ST_NoteType, ST_NoteContext)
			VALUES (NEWID(), '01FEF126-6910-4525-B024-262D977214E9', 'RefAirline', 'Custom Defined', 1, 'A','A','AAA');");

			var serverData = GetServerData();

			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote WHERE ST_Description = 'Custom Defined'"));

			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));

			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote WHERE ST_Description = 'Custom Defined'"));
		}

		public void TestUpdateStmNoteWhoseAirlineIsNotUpdatable()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline (RM_PK, RM_IsUpdatable, RM_IsActive, RM_IsSystem, RM_AirlineName1, RM_AirlineName2, RM_AccountingCode, RM_ThreeLetterCode, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2, RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype,
			RM_EmergencyTeletype, RM_EmergencyContactName, RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag, RM_EagleAddedAirlinePrefixOrAccountingCode)
			VALUES ('01FEF126-6910-4525-B024-262D977214E9', 0, 1, 1, 'Zambian Airways', '', '391', 'MYD', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', '', '391');

			INSERT INTO dbo.StmNote (ST_PK, ST_ParentId, ST_Table, ST_Description, ST_ForceRead, ST_NoteText, ST_NoteType, ST_NoteContext)
			VALUES (NEWID(), '01FEF126-6910-4525-B024-262D977214E9', 'RefAirline', 'Terms and Conditions', 1, 'A','A','AAA'),
(NEWID(), '01FEF126-6910-4525-B024-262D977214E9', 'RefAirline', 'User Defined', 1, 'B','B','BBB');");

			var serverData = GetServerData();

			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote JOIN dbo.RefAirline ON ST_ParentId = RM_PK WHERE ST_NoteContext = 'AAA'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote JOIN dbo.RefAirline ON ST_ParentId = RM_PK WHERE ST_NoteContext = 'BBB'"));

			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));

			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote JOIN dbo.RefAirline ON ST_ParentId = RM_PK WHERE ST_NoteContext = 'AAA'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote JOIN dbo.RefAirline ON ST_ParentId = RM_PK WHERE ST_NoteContext = 'BBB'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote JOIN dbo.RefAirline ON ST_ParentId = RM_PK WHERE ST_NoteContext = 'XXX'"));
		}

		public void TestDeleteStmNoteWhoseAirlineIsNotUpdatableWithNullStmNote()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline (RM_PK, RM_IsUpdatable, RM_IsActive, RM_IsSystem, RM_AirlineName1, RM_AirlineName2, RM_AccountingCode, RM_ThreeLetterCode, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2, RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype,
			RM_EmergencyTeletype, RM_EmergencyContactName, RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag, RM_EagleAddedAirlinePrefixOrAccountingCode)
			VALUES ('01FEF126-6910-4525-B024-262D977214E9', 0, 1, 1, 'Zambian Airways', '', '391', 'MYD', 'Q3', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '391', '', '391');

			INSERT INTO dbo.StmNote (ST_PK, ST_ParentId, ST_Table, ST_Description, ST_ForceRead, ST_NoteText, ST_NoteType, ST_NoteContext)
			VALUES (NEWID(), '01FEF126-6910-4525-B024-262D977214E9', 'RefAirline', 'Terms and Conditions', 1, 'A','A','AAA');");

			var serverData = GetServerData();
			serverData.StmNotes = null;

			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote WHERE ST_NoteContext = 'AAA'"));

			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));

			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefAirline WHERE RM_AddressLine1 = 'address line'"));
			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote WHERE ST_NoteContext = 'AAA'"));
			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmNote WHERE ST_NoteContext = 'XXX'"));
		}

		public void TestDeleteDependentRefAirlineEFreightRuleRecord()
		{
			conn.ExecuteNonQuery(@"
			INSERT dbo.RefAirline (RM_PK, RM_IsActive, RM_IsSystem, RM_AirlineName1, RM_AirlineName2, RM_AccountingCode, RM_ThreeLetterCode, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2, RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype,
			RM_EmergencyTeletype, RM_EmergencyContactName, RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag, RM_EagleAddedAirlinePrefixOrAccountingCode)
			VALUES ('6C22D427-C0F8-4200-99E9-ACD2F424D4AB', 1, 1, 'American Airlines Inc.', '', '001', 'AAL', 'AA', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '001', '', '001');

			INSERT INTO dbo.RefAirlineEFreightRule (RME_PK,RME_OriginLocation,RME_DestinationLocation,RME_EFreightStatus,RME_RM)
			VALUES ('25191E53-AD2A-4263-ADD5-DB6966A89B87', 'AUSYD', 'USLAX', 'EAW', '6C22D427-C0F8-4200-99E9-ACD2F424D4AB');");

			var serverData = new RefAirline
			{
				RM_AccountingCode = "001",
				RM_TwoCharacterCode = "AA",
				RM_ThreeLetterCode = "AAL",
				RM_AddressLine1 = "address line",
				RM_IsActive = true,
				RM_EagleAddedAirlinePrefixOrAccountingCode = "AAA",
				RM_AccountingSecondaryFlag = "X",
				RM_AddressLine2 = "X",
				RM_AirlineCity = "X",
				RM_AirlineCountry = "X",
				RM_AirlineName1 = "American Airlines Inc.",
				RM_AirlineName2 = "X",
				RM_AirlinePostalCode = "X",
				RM_RN_NKAirlineCountry = "",
				RM_AirlinePrefix = "X",
				RM_AirlinePrefixSecondaryFlag = "X",
				RM_AirlineState = "X",
				RM_ContactNameOCIIdentifier = "X",
				RM_ContactPhoneOCIIdentifier = "X",
				RM_DuplicateFlagIndicator = false,
				RM_EmergencyContactName = "X",
				RM_EmergencyContactTitle = "X",
				RM_EmergencyTeletype = "X",
				RM_IsCASSControlled = false,
				RM_LabelShortName = "",
				RM_MembershipFlagARINC = false,
				RM_MembershipFlagATA = false,
				RM_MembershipFlagIATA = false,
				RM_MembershipFlagSITA = false,
				RM_ReservationsContactName = "X",
				RM_ReservationsContactTeletype = "X",
				RM_ReservationsContactTitle = "X",
				RM_ReservationsDeptTeletype = "X",
				RM_TypeOfOperationsCode = "X"
			};
			serverData.Deleted = true;

			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(1) FROM dbo.RefAirline WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AB'"));
			AssertEquals("RefAirlineEFreightRule should be deleted.", 0, conn.ExecuteScalar("SELECT COUNT(1) FROM dbo.RefAirlineEFreightRule WHERE RME_RM = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(nameof(RefAirline));
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(nameof(RefAirline), DateTime.UtcNow.AddDays(-1), null, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			conn.ExecuteNonQuery(@"
			DELETE FROM dbo.RefAirlineEFreightRule;
			DELETE FROM dbo.RefAirline;
			DELETE FROM dbo.StmNote;");
		}

		protected override void PrepareDatabase()
		{
		}

		readonly string tearDownScript = $@"
			IF EXISTS (SELECT 1 FROM dbo.RefDbVersionalControl WHERE RVC_DataSet = 'RefAirline')
			DELETE FROM dbo.RefDbVersionalControl WHERE RVC_DataSet = 'RefAirline';
			DELETE FROM dbo.RefAirlineEFreightRule;
			DELETE FROM dbo.RefAirline;
			DELETE FROM dbo.StmNote;";

		readonly string[] ignoreColumns = new string[] { "RM_LabelShortName", "RM_IsCASSControlled", "RM_ContactNameOCIIdentifier", "RM_ContactPhoneOCIIdentifier" };
	}
}
