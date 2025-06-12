/*
Do not change the database path or name variables.
Any sqlcmd variables will be properly substituted during 
build and deployment.
*/
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month01]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month02]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month03]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month04]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month05]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month06]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month07]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month08]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month09]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month10]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month11]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Month12]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Year0]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Year1]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Year2]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Year3]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [Year4]
go
ALTER DATABASE [$(DatabaseName)] ADD FILEGROUP [YearArchive]
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month01], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month01.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month01;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month02], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month02.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month02;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month03], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month03.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month03;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month04], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month04.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month04;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month05], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month05.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month05;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month06], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month06.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month06;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month07], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month07.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month07;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month08], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month08.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month08;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month09], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month09.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month09;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month10], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month10.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month10;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month11], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month11.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month11;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Month12], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Month12.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Month12;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Year0], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Year0.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Year0;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Year1], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Year1.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Year1;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Year2], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Year2.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Year2;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Year3], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Year3.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Year3;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [Year4], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_Year4.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP Year4;
go
ALTER DATABASE [$(DatabaseName)] ADD FILE (NAME = [YearArchive], FILENAME = '$(DefaultDataPath)$(DefaultFilePrefix)_YearArchive.ndf', SIZE = 1MB, MAXSIZE = UNLIMITED, FILEGROWTH = 100MB) TO FILEGROUP YearArchive;
go

