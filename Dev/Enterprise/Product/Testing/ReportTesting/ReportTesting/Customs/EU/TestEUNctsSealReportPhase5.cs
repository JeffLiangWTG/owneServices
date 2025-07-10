using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.EU
{
	[TemplateName("EU NCTS Seal Report (Phase 5)")]
	public class TestEUNctsSealReportPhase5 : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}

		protected override void FillReportWithDefaultValues()
		{
			var header = Factory.New<Integration.Customs.DE.ICusInBondHeader>();

			(header as BusinessObject)?.FillWithValidTestData();
			Factory.Save();
			Db.Connection.ExecuteNonQuery($@"
insert into dbo.CusCodeData ( CY_PK, CY_IsValid, CY_IsOverridden, CY_Order, CY_Type, CY_Code, CY_ParentID
							, CY_ParentTableCode, CY_Data, CY_Date, CY_AutoVersion, CY_SystemCreateTimeUtc
							, CY_SystemCreateUser, CY_SystemLastEditTimeUtc, CY_SystemLastEditUser)
values ('142425fb-b337-410f-b75e-2c939b4d2de7', 1, 0, 0, 'EUO', 'DEP', {header.PK.ToSqlGuid()}, 'BH', 'DE0001', GETDATE(), 0, GETDATE(), '~SP', GETDATE(), '~SP');
");

			Db.Connection.ExecuteNonQuery($@"
insert into dbo.CusCodeData ( CY_PK, CY_IsValid, CY_IsOverridden, CY_Order, CY_Type, CY_Code, CY_ParentID
							, CY_ParentTableCode, CY_Data, CY_Date, CY_AutoVersion, CY_SystemCreateTimeUtc
							, CY_SystemCreateUser, CY_SystemLastEditTimeUtc, CY_SystemLastEditUser)
values ('c3f91065-6f91-4e51-bd60-5a289e0288fd', 1, 0, 0, 'EUO', 'DES', {header.PK.ToSqlGuid()}, 'BH', 'DE0002', GETDATE(), 0, GETDATE(), '~SP', GETDATE(), '~SP');
");

			var org = Factory.NewWithValidTestData<OrgHeader>();

			if (Report.FilterCollection["Destination Office"] is LookupField desField)
			{
				desField.Value = Guid.Parse("c3f91065-6f91-4e51-bd60-5a289e0288fd");
			}

			if (Report.FilterCollection["Departure Office"] is LookupField depField)
			{
				depField.Value = Guid.Parse("142425fb-b337-410f-b75e-2c939b4d2de7");
			}

			if (Report.FilterCollection["Departure Date"] is DateRangeField date)
			{
				date.ValueLow = new DateTime(2022, 12, 30);
				date.ValueHigh = new DateTime(2022, 12, 31);
			}

			if (Report.FilterCollection["Principal"] is LookupField principal)
			{
				principal.Value = org.PK.ToGuid();
			}
			Factory.Save();
		}
	}
}
