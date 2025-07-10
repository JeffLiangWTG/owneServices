using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing;

public static class RefGlbReleaseNoteTestDataHelper
{
	public static void InsertRefGlbReleaseNotes(DbConnection connection, params DummyRefGlbReleaseNote[] notes)
	{
		var refGlbReleaseNoteSqlBuilder = new StringBuilder("INSERT INTO dbo.RefDatabase_RefGlbReleaseNote (ZGF_PK, ZGF_Section, ZGF_Summary, ZGF_RN_NKCountryForReleaseNote, ZGF_URL, ZGF_ReleaseNoteDate, ZGF_QuickStartPK, ZGF_MinVersion) VALUES").AppendLine();
		var glbReleaseNoteReadSqlBuilder = new StringBuilder($"INSERT INTO dbo.{GlbReleaseNoteReadSchema.Constants.TableName} ({GlbReleaseNoteReadSchema.Constants.PK}, {GlbReleaseNoteReadSchema.Constants.GR_ReleaseNoteID}, {GlbReleaseNoteReadSchema.Constants.GR_GS_Staff}, {GlbReleaseNoteReadSchema.Constants.GR_IsValid}) VALUES");
		var isReadChecked = false;

		for (var i = 0; i < notes.Length; i++)
		{
			if (i != 0)
			{
				refGlbReleaseNoteSqlBuilder.AppendLine(",");
			}

			refGlbReleaseNoteSqlBuilder.AppendFormat("(NEWID(), @section{0}, @summary{0}, @countryCode{0}, @url{0}, @date{0}, @quickStartPK{0}, @minVersion{0})", i);

			if (notes[i].IsRead)
			{
				isReadChecked = true;
				glbReleaseNoteReadSqlBuilder.AppendLine();
				glbReleaseNoteReadSqlBuilder.AppendFormat("(NEWID(), @quickStartPK{0}, @staff, 1),", i);
			}
		}

		if (isReadChecked)
		{
			refGlbReleaseNoteSqlBuilder.AppendLine(";");
			refGlbReleaseNoteSqlBuilder.AppendLine(glbReleaseNoteReadSqlBuilder.ToString().TrimEnd(','));
		}

		using var cmd = connection.Command(refGlbReleaseNoteSqlBuilder.ToString());

		if (isReadChecked)
		{
			cmd.AddParameter("@staff", SqlDbType.UniqueIdentifier, GlbStaff.CurrentUser.PK.ToGuid());
		}

		for (var i = 0; i < notes.Length; i++)
		{
			cmd.AddParameter("@section" + i, SqlDbType.VarChar, 3, notes[i].Section);
			cmd.AddParameter("@summary" + i, SqlDbType.NVarChar, 1024, notes[i].Summary);
			cmd.AddParameter("@countryCode" + i, SqlDbType.VarChar, 2, notes[i].CountryCode);
			cmd.AddParameter("@url" + i, SqlDbType.VarChar, 256, notes[i].Url);
			cmd.AddParameter("@date" + i, SqlDbType.DateTime, notes[i].PublishedDate);
			cmd.AddParameter("@quickStartPK" + i, SqlDbType.UniqueIdentifier, notes[i].PK);
			cmd.AddParameter("@minVersion" + i, SqlDbType.VarChar, 50, notes[i].MinVersion);
		}

		cmd.ExecuteNonQuery();
	}
}
