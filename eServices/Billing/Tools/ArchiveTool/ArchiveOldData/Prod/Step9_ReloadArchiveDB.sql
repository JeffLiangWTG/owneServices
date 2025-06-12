USE [tempdb]

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Billing2017')
BEGIN
    RAISERROR('Database already exists. Reload operation aborted.', 16, 1);
    RETURN;
END

RESTORE DATABASE [Billing2017] 
FROM URL = N'https://saauedevehubprod.blob.core.windows.net/ehub-archive-prod/Billing/Billing2017.bak';
