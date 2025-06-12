/*
A partition per month for the current month and the previous 11 months.
A partition per year for the previous 5 calendar years.
Note, the partition for last year will only contain those months more than 11 months ago.
An implicit partition for all data occuring more than 5 calendar years ago.

For example, if this month is August 2016:
	12 x monthly: Sept 2015 to Aug 2016
	5 x yearly: 2011 to Aug 2015
	1 historical: before 2011

partition #
 1 - historical before 2011
 2 - 2011
 3 - 2012
 4 - 2013
 5 - 2014
 6 - 2015
 7 - 9/2015
 8 - 10/2015
 9 - 11/2015
10 - 12/2015
11 - 1/2016
12 - 2/2016
13 - 3/2016
14 - 4/2016
15 - 5/2016
16 - 6/2016
17 - 7/2016
18 - 8/2016

*/
CREATE PARTITION FUNCTION [PF_Period](int)
	AS RANGE RIGHT
	FOR VALUES 
	((YEAR(getutcdate()) - 5) * 100 + 1
	,(YEAR(getutcdate()) - 4) * 100 + 1
	,(YEAR(getutcdate()) - 3) * 100 + 1
	,(YEAR(getutcdate()) - 2) * 100 + 1
	,(YEAR(getutcdate()) - 1) * 100 + 1
	,YEAR(DATEADD(MONTH, -11, getutcdate())) * 100 + MONTH(DATEADD(MONTH, -11, getutcdate()))
	,YEAR(DATEADD(MONTH, -10, getutcdate())) * 100 + MONTH(DATEADD(MONTH, -10, getutcdate()))
	,YEAR(DATEADD(MONTH,  -9, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -9, getutcdate()))
	,YEAR(DATEADD(MONTH,  -8, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -8, getutcdate()))
	,YEAR(DATEADD(MONTH,  -7, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -7, getutcdate()))
	,YEAR(DATEADD(MONTH,  -6, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -6, getutcdate()))
	,YEAR(DATEADD(MONTH,  -5, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -5, getutcdate()))
	,YEAR(DATEADD(MONTH,  -4, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -4, getutcdate()))
	,YEAR(DATEADD(MONTH,  -3, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -3, getutcdate()))
	,YEAR(DATEADD(MONTH,  -2, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -2, getutcdate()))
	,YEAR(DATEADD(MONTH,  -1, getutcdate())) * 100 + MONTH(DATEADD(MONTH,  -1, getutcdate()))
	,YEAR(getutcdate()) * 100 + MONTH(getutcdate()))
