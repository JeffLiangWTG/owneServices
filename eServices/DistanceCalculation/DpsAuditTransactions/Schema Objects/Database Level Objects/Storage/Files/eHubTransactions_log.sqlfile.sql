ALTER DATABASE [$(DatabaseName)]
    ADD LOG FILE (NAME = [eHubTransactions_log], FILENAME = '$(Path1)eHubTransactions_Log.ldf', MAXSIZE = 2097152 MB, FILEGROWTH = 262144 KB);

