USE [tempdb]

BACKUP DATABASE [BillingPre2017] 
TO URL = N'https://saauedevehubprod.blob.core.windows.net/ehub-archive-prod/Billing/BillingPre2017.bak'
WITH COPY_ONLY, FORMAT, CHECKSUM, COMPRESSION, BLOCKSIZE = 65536, MAXTRANSFERSIZE = 4194304;
