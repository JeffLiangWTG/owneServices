-- Note: this is the scheme for August 2016
-- The stored procedure edi.PeriodPartitionSetupScheme will re-create the scheme to align it with the current period in the period function
CREATE PARTITION SCHEME [PS_Period]
	AS PARTITION [PF_Period]
	TO ([YearArchive], [Year1], [Year2], [Year3], [Year4], [Year0], [Month09], [Month10], [Month11], [Month12], [Month01], [Month02], [Month03], [Month04], [Month05], [Month07], [Month08], [Month09])