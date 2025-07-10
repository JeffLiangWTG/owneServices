using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefShippingLineUpdaterTest : DSUpdaterTest<RefShippingLine, IRefShippingLine>
	{
		protected string DataSetName => SharedSQLBuilder.GetTableName<IRefShippingLine>();

		protected override void SetUp()
		{
			base.SetUp();
			conn.ExecuteNonQuery(tearDownScript);
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefShippingLineUpdater(proxy, dbHelper, versionControlManager);
		}

		protected override void TearDown()
		{
			base.TearDown();
			using (var mainConn = Db.NewAdminConnection())
			{
				mainConn.ExecuteNonQuery(tearDownScript);
			}
		}

		protected override void PrepareDatabase()
		{
		}

		protected void PrepareData()
		{
			conn.ExecuteNonQuery(@"
INSERT INTO [dbo].[RefShippingLine]
([RSL_PK],[RSL_IsSystem],[RSL_IsActive],[RSL_IsNVO],[RSL_CarrierName],[RSL_StandardCarrierAlphaCode],[RSL_CargoWiseOneCode]
,[RSL_OceanCarrierMessagingAvailable],[RSL_GlobalSailingScheduleAvailable],[RSL_ContainerAutomationAvailable],[RSL_CargoSphereRatesAvailable],[RSL_InvoiceAvailable])
VALUES ('21508F90-C5FE-4F8C-BEAB-081F2B3F97CA',1,1,0,'Delete This','XXXX','TST1',0,0,0,0,0),
		('C8D96D32-9377-4125-94D9-63537E2CA343',1,1,0,'name02','YYYY','yyyy',0,0,0,0,0);

INSERT INTO [dbo].[OrgHeader] ([OH_PK],[OH_IsValid],[OH_Code],[OH_IsActive],[OH_FullName],[OH_IsShippingLine],[OH_IsSeaWholesaler],[OH_Category],[OH_RSL_ShippingLine])
VALUES ('997F7A84-41C2-4304-A163-28FA68363F10',1,'XXXRSLXXX',1,'Test ShppingLine record',1,1,'BUS','21508F90-C5FE-4F8C-BEAB-081F2B3F97CA'),
		('BC2E80CC-0BB9-4DBA-8650-014EAE8B5D0D',1,'YYYRSLYYY',1,'Test name 02',1,1,'BUS','C8D96D32-9377-4125-94D9-63537E2CA343');
");
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			if (serverData is RefShippingLine shippingLine)
			{
				shippingLine.RSL_CarrierName = "XX";
				return Tuple.Create(nameof(RefShippingLine.RSL_CarrierName), "XX");
			}
			else if (serverData is RefShippingLineMessagingRequirement shippingLineMessagingRequirement)
			{
				shippingLineMessagingRequirement.RSR_RST_NKType = "BBB";
				return Tuple.Create(nameof(RefShippingLineMessagingRequirement.RSR_RST_NKType), "BBB");
			}
			else if (serverData is RefShippingLineEBLProvider shippingLineEBLProvider)
			{
				shippingLineEBLProvider.RSE_Name = "New Name";
				return Tuple.Create(nameof(RefShippingLineEBLProvider.RSE_Name), "New Name");
			}
			else
			{
				return null;
			}
		}

		protected override RefShippingLine GetServerData()
		{
			var shippingLine = new RefShippingLine
			{
				RSL_CargoWiseOneCode = "TST1",
				RSL_StandardCarrierAlphaCode = "XXXX",
				RSL_CarrierName = "Delete This"
			};
			shippingLine.RefShippingLineMessagingRequirements = new RefShippingLineMessagingRequirement[]
			{
				new RefShippingLineMessagingRequirement
				{
					RSR_IsBookingRequest = false,
					RSR_IsShippingInstruction = false,
					RSR_RST_NKType = "AAA"
				}
			};
			shippingLine.RefShippingLineEBLProviders = new RefShippingLineEBLProvider[]
			{
				new RefShippingLineEBLProvider
				{
					RSE_Name = "EBL Name",
					RSE_IsAvailable = true,
					RSE_IsDefault = false
				}
			};
			return shippingLine;
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return new Tuple<Type, int>(typeof(IRefShippingLine), 1);
			yield return new Tuple<Type, int>(typeof(IRefShippingLineMessagingRequirement), 1);
			yield return new Tuple<Type, int>(typeof(IRefShippingLineEBLProvider), 1);
		}

		public void TestUpdateWhenMultipleClientMatchOneServer1()
		{
			PrepareData();
			var serverData = new RefShippingLine
			{
				RSL_StandardCarrierAlphaCode = "XXXX",
				RSL_CarrierName = "name02",
				RSL_CargoWiseOneCode = "NEWX",
				RSL_IsActive = true
			};
			var proxy = Helper.GetServerProxy(serverData);
			AssertNoExceptionThrown(() => Helper.RunUpdater(GetUpdater(proxy)));
			var newPK = conn.ExecuteScalar("SELECT RSL_PK FROM dbo.RefShippingLine WHERE RSL_StandardCarrierAlphaCode ='XXXX'").ToString();
			AssertNotEquals("RefShippingLine1 should be replaced.", "21508F90-C5FE-4F8C-BEAB-081F2B3F97CA", newPK);
			AssertEquals("RefShippingLine2 should be deleted.", 0, (int)conn.ExecuteScalar("SELECT COUNT(1) FROM dbo.RefShippingLine WHERE RSL_StandardCarrierAlphaCode ='YYYY'"));
			var oh_RSL_ShippingLine1 = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'").ToString();
			var oh_RSL_ShippingLine2 = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = 'BC2E80CC-0BB9-4DBA-8650-014EAE8B5D0D'");
			AssertEquals("OH_RSL_ShippingLine on OrgHeader1 should NOT be removed for updated records", newPK, oh_RSL_ShippingLine1);
			AssertEquals("OH_RSL_ShippingLine on OrgHeader2 should be removed after updating records", DBNull.Value, oh_RSL_ShippingLine2);
		}

		public void TestUpdateWhenMultipleClientMatchOneServer2()
		{
			PrepareData();
			var serverData = new RefShippingLine
			{
				RSL_StandardCarrierAlphaCode = "XXXX",
				RSL_CarrierName = "new name",
				RSL_CargoWiseOneCode = "yyyy",
				RSL_IsActive = true
			};
			var proxy = Helper.GetServerProxy(serverData);
			AssertNoExceptionThrown(() => Helper.RunUpdater(GetUpdater(proxy)));
			var newPK1 = conn.ExecuteScalar("SELECT RSL_PK FROM dbo.RefShippingLine WHERE RSL_StandardCarrierAlphaCode ='XXXX'").ToString();
			AssertNotEquals("RefShippingLine1 should be replaced.", "21508F90-C5FE-4F8C-BEAB-081F2B3F97CA", newPK1);
			AssertEquals("RefShippingLine2 should be deleted.", 0, (int)conn.ExecuteScalar("SELECT COUNT(1) FROM dbo.RefShippingLine WHERE RSL_StandardCarrierAlphaCode ='YYYY'"));
			var oh_RSL_ShippingLine1 = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'").ToString();
			var oh_RSL_ShippingLine2 = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = 'BC2E80CC-0BB9-4DBA-8650-014EAE8B5D0D'");
			AssertEquals("OH_RSL_ShippingLine on OrgHeader1 should NOT be removed for updated records", newPK1, oh_RSL_ShippingLine1);
			AssertEquals("OH_RSL_ShippingLine on OrgHeader2 should be removed after updating records", DBNull.Value, oh_RSL_ShippingLine2);
		}

		public void TestUpdateWhenMultipleClientMatchOneServer3()
		{
			PrepareData();
			var serverData = new RefShippingLine
			{
				RSL_StandardCarrierAlphaCode = "NEWX",
				RSL_CarrierName = "Delete This",
				RSL_CargoWiseOneCode = "yyyy",
				RSL_IsActive = true
			};
			var proxy = Helper.GetServerProxy(serverData);
			AssertNoExceptionThrown(() => Helper.RunUpdater(GetUpdater(proxy)));
			var newPK1 = conn.ExecuteScalar("SELECT RSL_PK FROM dbo.RefShippingLine WHERE RSL_CarrierName ='Delete This'").ToString();
			AssertNotEquals("RefShippingLine1 should be replaced.", "21508F90-C5FE-4F8C-BEAB-081F2B3F97CA", newPK1);
			AssertEquals("RefShippingLine2 should be deleted.", 0, (int)conn.ExecuteScalar("SELECT COUNT(1) FROM dbo.RefShippingLine WHERE RSL_StandardCarrierAlphaCode ='name02'"));
			var oh_RSL_ShippingLine1 = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'").ToString();
			var oh_RSL_ShippingLine2 = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = 'BC2E80CC-0BB9-4DBA-8650-014EAE8B5D0D'");
			AssertEquals("OH_RSL_ShippingLine on OrgHeader1 should NOT be removed for updated records", newPK1, oh_RSL_ShippingLine1);
			AssertEquals("OH_RSL_ShippingLine on OrgHeader2 should be removed after updating records", DBNull.Value, oh_RSL_ShippingLine2);
		}

		public void TestUpdateOnOrgHeader()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow, string.Empty, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			PrepareData();
			var formerLastEditTime = conn.ExecuteScalar("SELECT OH_SystemLastEditTimeUtc FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'");
			var storageData = Helper.GetStorageData(conn, "SELECT * FROM dbo.RefShippingLine WHERE RSL_PK = '21508F90-C5FE-4F8C-BEAB-081F2B3F97CA'");
			var serverData = Helper.CreateServerData<RefShippingLine>(storageData);
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			var newPK = conn.ExecuteScalar("SELECT RSL_PK FROM dbo.RefShippingLine WHERE RSL_CargoWiseOneCode='TST1'").ToString();
			AssertNotEquals("RefShippingLine should be replaced.", "21508F90-C5FE-4F8C-BEAB-081F2B3F97CA", newPK);

			var oh_RSL_ShippingLine = conn.ExecuteScalar("SELECT OH_RSL_ShippingLine FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'").ToString();
			AssertEquals("OH_RSL_ShippingLine on OrgHeader should NOT be removed for updated records", newPK, oh_RSL_ShippingLine);
			var currentLastEditTime = conn.ExecuteScalar("SELECT OH_SystemLastEditTimeUtc FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'");
			AssertNotEquals("OH_SystemLastEditTimeUtc on OrgHeader should be updated", formerLastEditTime, currentLastEditTime);
			AssertEquals("OH_SystemLastEditUser on OrgHeader should be updated", "~BP", conn.ExecuteScalar("SELECT OH_SystemLastEditUser FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'"));
		}

		public void TestDeleteOnOrgHeader()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			PrepareData();
			var formerLastEditTime = conn.ExecuteScalar("SELECT OH_SystemLastEditTimeUtc FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'");
			var storageData = Helper.GetStorageData(conn, "SELECT * FROM dbo.RefShippingLine WHERE RSL_PK = '21508F90-C5FE-4F8C-BEAB-081F2B3F97CA'");
			var serverData = Helper.CreateServerData<RefShippingLine>(storageData);
			serverData.Deleted = true;

			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefShippingLine WHERE RSL_PK = '21508F90-C5FE-4F8C-BEAB-081F2B3F97CA'"));
			AssertEquals("OH_RSL_ShippingLine on OrgHeader should be set to NULL for deleted records", 1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10' and OH_RSL_ShippingLine is null"));
			var currentLastEditTime = conn.ExecuteScalar("SELECT OH_SystemLastEditTimeUtc FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'");
			AssertNotEquals("OH_SystemLastEditTimeUtc on OrgHeader should be updated", formerLastEditTime, currentLastEditTime);
			AssertEquals("OH_SystemLastEditUser on OrgHeader should be updated", "~BP", conn.ExecuteScalar("SELECT OH_SystemLastEditUser FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10'"));
		}

		public void TestNewTableReferenceShippingLineByFK()
		{
			using (var cmd = Db.Connection.Command("sp_fkeys"))
			{
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.AddParameter("@pktable_name", SqlDbType.VarChar, SharedSQLBuilder.GetTableName<IRefShippingLine>());
				using (var reader = cmd.ExecuteReader())
				{
					var count = 0;
					var fkTableNames = new List<string>();
					var fkColNames = new List<string>();
					while (reader.Read())
					{
						count++;
						fkTableNames.Add((string)reader["FKTABLE_NAME"]);
						fkColNames.Add((string)reader["FKCOLUMN_NAME"]);
					}
					// Tables that has reference to Shipping Line: OrgHeader, RefShippingLineMessagingRequirement
					// if there are more, pls let Ref team know
					AssertEquals("If this test fails, pls see comment in the test", 3, count);
					AssertEquals("If this test fails, pls see comment in the test", 3, fkTableNames.Count);
					AssertContainsExactElementsInAnyOrder("If this test fails, pls see comment in the test", new string[] { "OrgHeader", "RefShippingLineEBLProvider", "RefShippingLineMessagingRequirement" }, fkTableNames);
					AssertEquals("If this test fails, pls see comment in the test", 3, fkColNames.Count);
					AssertContainsExactElementsInAnyOrder("If this test fails, pls see comment in the test", new string[] { "OH_RSL_ShippingLine", "RSE_RSL_ShippingLine", "RSR_RSL_ShippingLine" }, fkColNames);
				}
			}
		}

		readonly string tearDownScript = $@"
	IF EXISTS (SELECT 1 FROM dbo.RefDbVersionalControl WHERE RVC_DataSet = 'RefShippingLine')
		DELETE FROM dbo.RefDbVersionalControl WHERE RVC_DataSet = 'RefShippingLine';

		DELETE FROM dbo.OrgHeader WHERE OH_PK = '997F7A84-41C2-4304-A163-28FA68363F10';
";
	}
}
