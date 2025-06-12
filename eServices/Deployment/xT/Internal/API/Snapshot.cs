using System;
using System.Data.SqlClient;

namespace XT.Internal.API
{
    public class Snapshot
    {
        readonly SqlConnection Connection;
        public readonly string DatabaseName;
        public readonly string SnapshotName;

        public Snapshot(string connectionString, string dbName = null, string snapshotName = null)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();

            DatabaseName = !string.IsNullOrWhiteSpace(dbName) ? dbName : Connection.Database;
            SnapshotName = !string.IsNullOrWhiteSpace(snapshotName) ? snapshotName : $"{DatabaseName}-SS";

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = $@"
                    SET NOCOUNT ON;

                    IF EXISTS (SELECT name FROM sys.databases WHERE name = @SnapshotName) DROP DATABASE [@SnapshotName];

					DECLARE @FileNameOutput VARCHAR(MAX);

                    DECLARE @GetFileName NVARCHAR(MAX) = '
                    SELECT
	                    @FileName = ''(NAME='' + name + '', FILENAME='''''' + physical_name + ''.ss'''')''
                    FROM [' + @DatabaseName + '].sys.database_files
                    WHERE [type] = 0;
                    '

                    EXECUTE sp_executesql  @GetFileName,
	                     N'@FileName VARCHAR(MAX) OUTPUT', 
	                     @FileName = @FileNameOutput OUTPUT

                    DECLARE @CreateSnapshot varchar(max) = 'CREATE DATABASE [' + @SnapshotName + '] ON ' + @FileNameOutput + ' AS SNAPSHOT OF [' + @DatabaseName + ']';

                    EXEC (@CreateSnapshot);";
                command.Parameters.AddWithValue("@DatabaseName", DatabaseName);
                command.Parameters.AddWithValue("@SnapshotName", SnapshotName);
                command.ExecuteNonQuery();
            }
        }

        public void Restore()
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection must be open.");
            }
            using (var dropCommand = Connection.CreateCommand())
            {
                Connection.ChangeDatabase("master");
                dropCommand.CommandText = @"
                            DECLARE @Restore NVARCHAR(MAX) = '
                            ALTER DATABASE [' + @DatabaseName + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                            
                            RESTORE DATABASE [' + @DatabaseName + '] FROM database_snapshot = ''' + @SnapshotName + '''

                            ALTER DATABASE [' + @DatabaseName + '] SET MULTI_USER'
                            EXEC (@Restore)";
                dropCommand.Parameters.AddWithValue("@DatabaseName", DatabaseName);
                dropCommand.Parameters.AddWithValue("@SnapshotName", SnapshotName);
                dropCommand.ExecuteNonQuery();
            }
        }

        public void Delete()
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection must be open.");
            }
            using (var dropCommand = Connection.CreateCommand())
            {
                Connection.ChangeDatabase("master");
                dropCommand.CommandText = @"
                            DECLARE @Delete NVARCHAR(MAX) = '
                            ALTER DATABASE [' + @DatabaseName + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                            
                            DROP DATABASE [' + @SnapshotName + ']

                            ALTER DATABASE [' + @DatabaseName + '] SET MULTI_USER'
                            EXEC (@Delete)";
                dropCommand.Parameters.AddWithValue("@DatabaseName", DatabaseName);
                dropCommand.Parameters.AddWithValue("@SnapshotName", SnapshotName);
                dropCommand.ExecuteNonQuery();
            }
        }
    }
}
