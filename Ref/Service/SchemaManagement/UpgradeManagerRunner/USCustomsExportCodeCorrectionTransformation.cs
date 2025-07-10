using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class USCustomsExportCodeCorrectionTransformation : DataTransformation, IDataTransformationTask
	{
		public USCustomsExportCodeCorrectionTransformation(int version)
			: base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
-- Correct start date
UPDATE codes SET ZZD_StartDate = '1900-01-01' 
FROM RefCusCodeList codes
JOIN RefDbVersionControl ON RVC_ParentPK = ZZD_PK
WHERE ZZD_StartDate = '2018-01-19' AND RVC_Deleted = 0 AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US'

-- Create incorrect start date records to delete on clients
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
SELECT newid(), 'CUSOF', d1.ZZD_Code, d1.ZZD_Description, '2018-01-19', d1.ZZD_EndDate, d1.ZZD_ZZZ_NKDataGrouping
FROM RefCusCodeList d1 
LEFT JOIN RefCusCodeList d2 ON d1.ZZD_Code = d2.ZZD_Code AND d1.ZZD_ZZK_NKCodeType = d2.ZZD_ZZK_NKCodeType AND d1.ZZD_ZZZ_NKDataGrouping = d2.ZZD_ZZZ_NKDataGrouping AND d2.ZZD_StartDate = '2018-01-19'
WHERE d1.ZZD_StartDate = '1900-01-01' AND d1.ZZD_ZZK_NKCodeType = 'CUSOF' AND d1.ZZD_ZZZ_NKDataGrouping = 'US' AND d2.ZZD_PK IS NULL

UPDATE ref SET RVC_Deleted = 1
FROM RefDbVersionControl ref
JOIN RefCusCodeList ON RVC_ParentPK = ZZD_PK
WHERE ZZD_StartDate = '2018-01-19' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US'

-- Correct description
UPDATE RefCusCodeList SET ZZD_Description = 'OGDENSBURG, NY' WHERE ZZD_Code = '0701' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'MASSENA, NY' WHERE ZZD_Code = '0704' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'CAPE VINCENT, NY' WHERE ZZD_Code = '0706' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'ALEXANDRIA BAY, NY' WHERE ZZD_Code = '0708' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'CHAMPLAIN-ROUSES POINT, NY' WHERE ZZD_Code = '0712' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'CLAYTON, NY' WHERE ZZD_Code = '0714' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'TROUT RIVER, NY' WHERE ZZD_Code = '0715' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'BUFFALO-NIAGARA FALLS NY' WHERE ZZD_Code = '0901' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'ROCHESTER, NY' WHERE ZZD_Code = '0903' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'OSWEGO, NY' WHERE ZZD_Code = '0904' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'SODUS POINT, NY' WHERE ZZD_Code = '0905' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'SYRACUSE, NY' WHERE ZZD_Code = '0906' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'NEW YORK, NY' WHERE ZZD_Code = '1001' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'ALBANY, NY' WHERE ZZD_Code = '1002' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'MEMPHIS, TN' WHERE ZZD_Code = '2006' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'NASHVILLE, TN' WHERE ZZD_Code = '2007' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'CHATTANOOGA, TN' WHERE ZZD_Code = '2008' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'KNOXVILLE, TN' WHERE ZZD_Code = '2016' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'SHREVEPORT/BOSSIER CITY' WHERE ZZD_Code = '2018' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'TRI-CITY AIRPORT, TN' WHERE ZZD_Code = '2027' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'BOZEMAN YELLOWSTONEUSER FEE AIRPORT' WHERE ZZD_Code = '3386' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'WARROAD, MN' WHERE ZZD_Code = '3423' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'BAUDETTE, MN' WHERE ZZD_Code = '3424' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'PINECREEK, MN' WHERE ZZD_Code = '3425' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'ROSEAU, MN' WHERE ZZD_Code = '3426' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'LANCASTER, MN' WHERE ZZD_Code = '3430' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'MINNEAPOLIS-ST. PAUL, MN' WHERE ZZD_Code = '3501' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'USER FEE AIRPORT, MN' WHERE ZZD_Code = '3581' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'INTERNATIONAL FALLS, MN' WHERE ZZD_Code = '3604' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'GRAND PORTAGE, MN' WHERE ZZD_Code = '3613' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'GARY, IN' WHERE ZZD_Code = '3905' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'INDIANAPOLIS, IN' WHERE ZZD_Code = '4110' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'LOUISVILLE, KY' WHERE ZZD_Code = '4115' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'OWENSBORO, KY' WHERE ZZD_Code = '4116' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'FORT WAYNE AIRPORT, IN' WHERE ZZD_Code = '4183' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'BLUE GRASS AIRPORT, KY' WHERE ZZD_Code = '4184' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'UPS, LOUISVILLE, KY' WHERE ZZD_Code = '4196' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'JOHN F KENNEDY AIRPORT, NY' WHERE ZZD_Code = '4701' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'MICOM, NY' WHERE ZZD_Code = '4773' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'AIR FRANCE (MACH PLUS), NY' WHERE ZZD_Code = '4774' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
UPDATE RefCusCodeList SET ZZD_Description = 'TNT SKYPAK, NY' WHERE ZZD_Code = '4778' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'

-- Correct transport modes
DELETE mode
FROM RefCusCodeOrAttributeTransportMode mode
JOIN RefCusCodeList ON ZZU_ZZD_CodeList = ZZD_PK
WHERE ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01' 
AND ZZD_Code IN ('0701','0704','0706','0708','0712','0714','0715','0901','0903','0904','0905','0906','1001','1002','2006','2007','2008','2016','2018','2027','3386','3423','3424','3425','3426','3430','3501','3581','3604','3613','3905','4110','4115','4116','4183','4184','4196','4701','4773','4774','4778')

INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0905' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4196' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '2006' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3386' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3604' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0708' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4115' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0906' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0903' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3613' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'RAI', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3423' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0714' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3581' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0906' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3424' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '2018' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4701' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4183' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4773' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0706' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0701' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '2008' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'FIX', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0901' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4774' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'RAI', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3604' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0701' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0903' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3426' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3425' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '2016' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4778' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'RAI', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0712' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'FIX', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3430' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3604' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'RAI', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0715' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3423' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3501' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3604' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0708' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4184' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0704' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3430' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4116' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '2007' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '4110' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'RAI', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0901' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0901' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0712' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0704' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0715' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '1001' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0901' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0904' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '2027' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '1002' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '1002' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3905' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'AIR', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '1001' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'ROA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '3613' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0706' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0901' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)  SELECT newid(), 'SEA', ZZD_PK  FROM RefCusCodeList WHERE ZZD_Code = '0712' AND ZZD_ZZK_NKCodeType = 'CUSOF' AND ZZD_ZZZ_NKDataGrouping = 'US' AND ZZD_StartDate = '1900-01-01'
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
