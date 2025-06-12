DECLARE @NAME VARCHAR(256)

CREATE TABLE #TableList
(
	TableName	varchar(256)
)

INSERT INTO #TableList VALUES ('LicenceCompany'),('LicenceDatabase'),('LicenceEnterprise'),('LicenceHeader'),('OrgHeader'),('RefAirline'),('RefCityPCodePivot'),('RefCityTown'),('RefCountry'),('RefCountryStates'),('RefPostCode'),('RefTimeZone'),('RefTimeZoneRule'),('RefTimeZoneSet'),('RefUNLOCO'),('ReleaseBuild')

DECLARE CUR CURSOR FOR
  SELECT TableName
  FROM   #TableList

OPEN CUR

FETCH NEXT FROM CUR INTO @NAME

WHILE @@FETCH_STATUS = 0
  BEGIN
	  EXEC('DELETE FROM ediProd.dbo.' + @NAME)
	  EXEC('INSERT INTO ediProd.dbo.' + @NAME + ' SELECT * FROM ediProdCache.dbo.' + @NAME)
      FETCH NEXT FROM CUR INTO @NAME
  END

CLOSE CUR

DEALLOCATE CUR 

DROP Table #TableList