USE [tempdb]

SELECT @@VERSION
--Microsoft SQL Server 2019 (RTM-CU30) (KB5049235) - 15.0.4415.2 (X64)   Nov 18 2024 17:45:37   Copyright (C) 2019 Microsoft Corporation  Enterprise Edition: Core-based Licensing (64-bit) on Windows Server 2019 Standard 10.0 <X64> (Build 17763: ) (Hypervisor) 

BACKUP DATABASE [Billing2017] 
TO URL = N'https://saauedevehubprod.blob.core.windows.net/ehub-archive-prod/Billing/Billing2017.bak'
WITH COPY_ONLY, FORMAT, CHECKSUM, COMPRESSION, BLOCKSIZE = 65536, MAXTRANSFERSIZE = 4194304;
