CREATE PROC [dbo].[spCleanupTestData]
AS BEGIN
	-- clean up audit requests
	delete from DpsAuditTransactions.dbo.eHubAuditRequest
	where B0_LicenceCode like 'EDI%'
	
	-- clean up billing entities
	delete from DeniedPartyTransactions.dbo.DpsBillingEntity 	
	where C5_LicenceCode like 'EDI%'
	
	-- clean up screening requests	
	--as per Henry's advise, not using cascaded deletes
	--so we're cleaning up FK'd tabled one by one

	declare @tmp table (PK [uniqueidentifier])

	-- looks like 1000 is a sweet spot in terms of execution time

	insert into @tmp (PK)	
	select top 1000 C1_PK from DeniedPartyTransactions.dbo.DpsRequest
	with (nolock)
	left outer join DpsAuditTransactions.dbo.eHubAuditRequest on C1_B0 = B0_PK
	where B0_PK is null	

	--DEBUG ONLY	
	--begin try DROP TABLE #tmp end try begin catch end catch ;
	--select * into #tmp from @tmp
	
	delete DeniedPartyTransactions.dbo.DpsPendingRescreen
	from DeniedPartyTransactions.dbo.DpsPendingRescreen
	join @tmp on C4_C1_Request = PK
	
	delete DeniedPartyTransactions.dbo.DpsRequestListPivot
	from DeniedPartyTransactions.dbo.DpsRequestListPivot
	join @tmp on C3_C1_Request = PK
	
	delete DeniedPartyTransactions.dbo.DpsRequestMatches
	from DeniedPartyTransactions.dbo.DpsRequestMatches
	join @tmp on C2_C1_Request = PK

	delete from DeniedPartyTransactions.dbo.DpsRequest
	where C1_PK in (select PK from @tmp)

END





