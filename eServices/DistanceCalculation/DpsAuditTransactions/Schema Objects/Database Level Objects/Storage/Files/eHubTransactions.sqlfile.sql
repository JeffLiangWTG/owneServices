ALTER DATABASE [$(DatabaseName)]
    ADD FILE (NAME = [eHubTransactions], FILENAME = '$(DefaultDataPath)eHubTransactions\eHubTransactions_Data.mdf', FILEGROWTH = 1024 KB) TO FILEGROUP [PRIMARY];

