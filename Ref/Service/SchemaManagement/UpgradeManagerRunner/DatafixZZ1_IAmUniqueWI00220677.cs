using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DatafixZZ1_IAmUniqueWI00220677 : DataTransformation, IDataTransformationTask
	{
		public DatafixZZ1_IAmUniqueWI00220677(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
Declare @ZZ1_PK_ToInsert uniqueidentifier
Declare @TariffTypePKFor2P1 uniqueidentifier
Select top 1 @TariffTypePKFor2P1 = zzi_PK from refcustarifftype where zzi_tarifftype = '2P1'

if (Select Count(1) From refcustariff Where ZZ1_TariffCode = '213030208' and ZZ1_StartDate = '2017-07-26 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 1) = 0
Begin
	Update refcustariff Set ZZ1_IAMUnique = 1 Where ZZ1_TariffCode = '213030208' and ZZ1_StartDate = '2017-07-26 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA'
	Set @ZZ1_PK_ToInsert = newid()
	Insert Into RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
	Select @ZZ1_PK_ToInsert,ZZ1_ZZI_TariffType,ZZ1_TariffCode,0,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
	From refcustariff Where ZZ1_TariffCode = '213030208' and ZZ1_StartDate = '2017-07-26 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 1
	Update RefDbVersionControl Set RVC_Deleted = 1,[RVC_LastUpdatedUTC] = GETUTCDATE() Where RVC_ParentPK = @ZZ1_PK_ToInsert And RVC_ParentCode = 'ZZ1'
End

If (Select Count(1) From RefCusTariff Where ZZ1_TariffCode = '215020108' and ZZ1_StartDate = '2017-11-17 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 4) = 0
Begin
	Set @ZZ1_PK_ToInsert = newid()
	Update refcustariff Set ZZ1_IAMUnique = 4 Where ZZ1_TariffCode = '215020108' and ZZ1_StartDate = '2017-11-17 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA'
	Insert Into RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
	Select @ZZ1_PK_ToInsert,ZZ1_ZZI_TariffType,ZZ1_TariffCode,0,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
	From refcustariff Where ZZ1_TariffCode = '215020108' and ZZ1_StartDate = '2017-11-17 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 4
	Update RefDbVersionControl Set RVC_Deleted = 1,[RVC_LastUpdatedUTC] = GETUTCDATE() Where RVC_ParentPK = @ZZ1_PK_ToInsert And RVC_ParentCode = 'ZZ1'
End

If (Select Count(1) From RefCusTariff Where ZZ1_TariffCode = '215020108' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 5) = 0
Begin
	Set @ZZ1_PK_ToInsert = newid()
	Update refcustariff Set ZZ1_IAMUnique = 5 Where ZZ1_TariffCode = '215020108' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA'
	Insert Into RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
	Select @ZZ1_PK_ToInsert,ZZ1_ZZI_TariffType,ZZ1_TariffCode,0,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
	From refcustariff Where ZZ1_TariffCode = '215020108' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 5
	Update RefDbVersionControl Set RVC_Deleted = 1,[RVC_LastUpdatedUTC] = GETUTCDATE() Where RVC_ParentPK = @ZZ1_PK_ToInsert And RVC_ParentCode = 'ZZ1'
End

If (Select Count(1) From RefCusTariff Where ZZ1_TariffCode = '215020208' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 3) = 0
Begin
	Set @ZZ1_PK_ToInsert = newid()
	Update refcustariff Set ZZ1_IAMUnique = 3 Where ZZ1_TariffCode = '215020208' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA'
	Insert Into RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
	Select @ZZ1_PK_ToInsert,ZZ1_ZZI_TariffType,ZZ1_TariffCode,0,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
	From refcustariff Where ZZ1_TariffCode = '215020208' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 3
	Update RefDbVersionControl Set RVC_Deleted = 1,[RVC_LastUpdatedUTC] = GETUTCDATE() Where RVC_ParentPK = @ZZ1_PK_ToInsert And RVC_ParentCode = 'ZZ1'
End

If (Select Count(1) From RefCusTariff Where ZZ1_TariffCode = '215020308' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAmUnique = 1) = 0
Begin
	Set @ZZ1_PK_ToInsert = newid()
	Update refcustariff Set ZZ1_IAMUnique = 1 Where ZZ1_TariffCode = '215020308' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA'
	Insert Into RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
	Select @ZZ1_PK_ToInsert,ZZ1_ZZI_TariffType,ZZ1_TariffCode,0,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
	From refcustariff Where ZZ1_TariffCode = '215020308' and ZZ1_StartDate = '2017-11-15 00:00:00' and ZZ1_ZZI_TariffType = @TariffTypePKFor2P1 and ZZ1_ZZZ_NKDataGrouping = 'ZA' And ZZ1_IAMUnique = 1
	Update RefDbVersionControl Set RVC_Deleted = 1,[RVC_LastUpdatedUTC] = GETUTCDATE() Where RVC_ParentPK = @ZZ1_PK_ToInsert And RVC_ParentCode = 'ZZ1'
End

DROP TABLE IF EXISTS #temp

Select t.ZZ1_TariffCode,t.ZZ1_StartDate,t.ZZ1_ZZI_TariffType,t.ZZ1_ZZZ_NKDataGrouping,t.ZZ1_IAMUnique,tr.zzh_tariffcode,ta.zz3_value
Into #temp
From RefCusTariff t
Join RefDbVersionControl on RVC_ParentPK = ZZ1_PK
Join RefCusTariffAttribute ta on ZZ3_ZZ1_Tariff = ZZ1_PK and ZZ3_Name = 'CheckDigit'
Join RefCusTariffRelationship tr on ZZH_ZZ1_Tariff = ZZ1_PK
Join 
(Select zz1_tariffcode,zzh_tariffcode,zz3_value,min(ZZ1_StartDate) as minstartdate,ZZ1_ZZI_TariffType
From RefCusTariff
Join RefDbVersionControl on RVC_ParentPK = ZZ1_PK
Join RefCusTariffAttribute on ZZ3_ZZ1_Tariff = ZZ1_PK and ZZ3_Name = 'CheckDigit'
Join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
Where ZZ1_ZZZ_NKDataGrouping = 'ZA' 
And RVC_Deleted = 0
And zz1_tariffcode +','+ zzh_tariffcode +','+ zz3_value In (
'320120105,56031,52'
,'320120105,56039,50'
,'320120106,391910,66'
,'320120106,392010,63'
,'320120106,392020,60'
,'320120106,540411,65'
,'320120106,590390,67')
Group By zz1_tariffcode,zzh_tariffcode,zz3_value,ZZ1_ZZI_TariffType
) correctRecords 
On correctRecords.ZZ1_TariffCode = t.ZZ1_TariffCode and correctRecords.ZZ1_ZZI_TariffType = t.ZZ1_ZZI_TariffType and correctRecords.ZZ3_Value = ta.ZZ3_Value and correctRecords.ZZH_TariffCode = tr.ZZH_TariffCode and correctRecords.minstartdate = t.ZZ1_StartDate
Where ZZ1_ZZZ_NKDataGrouping = 'ZA' 
And RVC_Deleted = 0

Update t Set t.ZZ1_IAMUnique = temp.ZZ1_IAMUnique
From RefCusTariff t
Join RefDbVersionControl On RVC_ParentPK = ZZ1_PK And RVC_deleted = 0
Join RefCusTariffAttribute ta On ZZ3_ZZ1_Tariff = ZZ1_PK And ZZ3_Name = 'CheckDigit'
Join RefCusTariffRelationship tr On ZZH_ZZ1_Tariff = ZZ1_PK
Join #temp temp 
On temp.ZZ1_TariffCode = t.ZZ1_TariffCode 
And temp.ZZ1_ZZI_TariffType = t.ZZ1_ZZI_TariffType 
And temp.ZZ1_ZZZ_NKDataGrouping = t.ZZ1_ZZZ_NKDataGrouping 
And temp.ZZ1_StartDate <> t.ZZ1_StartDate
And temp.ZZ3_Value = ta.ZZ3_Value
And temp.ZZH_TariffCode = tr.ZZH_TariffCode

--Fix duplicate IAmUnique
Declare @MaxIAMUnique smallint
declare @PreviousTariffCode varchar(35)
set @PreviousTariffCode = null 
DECLARE outer_cursor CURSOR FOR 
Select ZZ1_TariffCode, ZZ1_IAMUnique,ZZ1_ZZI_TariffType from
(
select distinct ZZ1_TariffCode, ISNULL(ZZ3_Value, '') AS ZZ3_Value, ISNULL(ZZH_TariffCode, '') AS ZZH_TariffCode, ZZ1_IAMUnique, ZZ1_ZZI_TariffType
From dbo.RefCusTariff
join dbo.RefDbVersionControl on RVC_ParentPK = ZZ1_PK and RVC_Deleted = 0
join dbo.RefCusTariffAttribute on ZZ3_ZZ1_Tariff = ZZ1_PK and ZZ3_Name = 'CheckDigit'
join dbo.RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
Join dbo.RefCusTariffType 
on ZZI_PK = ZZ1_ZZI_TariffType 
and ZZI_TariffType not like '1%'
where ZZ1_ZZZ_NKDataGrouping = 'ZA'
) AA
group by ZZ1_TariffCode, ZZ1_IAMUnique, ZZ1_ZZI_TariffType
having count(*) > 1
order by ZZ1_TariffCode

DECLARE @ZZ1_TariffCode VARCHAR(35);
DECLARE @ZZ1_IAMUnique smallint;
DECLARE @ZZ1_ZZI_TariffType uniqueidentifier;
OPEN outer_cursor;
FETCH NEXT FROM outer_cursor INTO @ZZ1_TariffCode, @ZZ1_IAMUnique, @ZZ1_ZZI_TariffType;
WHILE @@FETCH_STATUS = 0
BEGIN  
		
		if (@PreviousTariffCode <> @ZZ1_TariffCode or @PreviousTariffCode is null)
		Begin
			--Get max IAmUnique	if tariffcode not the same as previous row	
			set @MaxIAMUnique = null		
			Select @MaxIAMUnique = Max(ZZ1_IAMUnique)
			FROM dbo.RefCusTariff t
			JOIN dbo.RefDbVersionControl vc on vc.RVC_ParentPK = t.ZZ1_PK and vc.RVC_Deleted = 0
			WHERE ZZ1_ZZZ_NKDataGrouping in ('ZA')
			AND ZZ1_TariffCode = @ZZ1_TariffCode
			AND ZZ1_ZZI_TariffType = @ZZ1_ZZI_TariffType

			Set @PreviousTariffCode = @ZZ1_TariffCode
		End

       --iterate through all duplicates skipping the first row
		DECLARE inner_cursor CURSOR FOR 
		SELECT ZZ1_PK
		FROM dbo.RefCusTariff t
		JOIN dbo.RefDbVersionControl vc on vc.RVC_ParentPK = t.ZZ1_PK and vc.RVC_Deleted = 0
		WHERE ZZ1_ZZZ_NKDataGrouping in ('ZA')
		AND ZZ1_TariffCode = @ZZ1_TariffCode
		AND ZZ1_IAmUnique = @ZZ1_IAMUnique
		AND ZZ1_ZZI_TariffType = @ZZ1_ZZI_TariffType
		Order By ZZ1_StartDate
		OFFSET 1 ROWS
		
		DECLARE @ZZ1_PK uniqueidentifier;
		OPEN inner_cursor;
		FETCH NEXT FROM inner_cursor INTO @ZZ1_PK ;
		WHILE @@FETCH_STATUS = 0
		BEGIN

				Declare @ExistingIAmUnique smallint
				set @ExistingIAmUnique  = null
				Select Top 1 @ExistingIAmUnique = otherMatches.ZZ1_IAMUnique
				From dbo.RefCusTariff t
				join dbo.RefDbVersionControl vc on RVC_ParentPK = ZZ1_PK and RVC_Deleted = 0
				join dbo.RefCusTariffAttribute ta on ZZ3_ZZ1_Tariff = ZZ1_PK and ZZ3_Name = 'CheckDigit'
				join dbo.RefCusTariffRelationship tr on ZZH_ZZ1_Tariff = ZZ1_PK
				Join dbo.RefCusTariffType tt on ZZI_PK = ZZ1_ZZI_TariffType and ZZI_TariffType not like '1%'
				Join 
				(
				Select ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_StartDate,ZZH_TariffCode,ZZ3_Value
				From dbo.RefCusTariff
				join dbo.RefDbVersionControl on RVC_ParentPK = ZZ1_PK and RVC_Deleted = 0
				join dbo.RefCusTariffAttribute on ZZ3_ZZ1_Tariff = ZZ1_PK and ZZ3_Name = 'CheckDigit'
				join dbo.RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
				Join dbo.RefCusTariffType on ZZI_PK = ZZ1_ZZI_TariffType and ZZI_TariffType not like '1%'
				where ZZ1_ZZZ_NKDataGrouping = 'ZA'
				AND ZZ1_TariffCode = @ZZ1_TariffCode
				AND ZZ1_ZZI_TariffType = @ZZ1_ZZI_TariffType
				and ZZ1_PK <> @ZZ1_PK
				) otherMatches
				On otherMatches.ZZ1_TariffCode = t.ZZ1_TariffCode
				And otherMatches.ZZH_TariffCode = tr.ZZH_TariffCode
				And otherMatches.ZZ3_Value = ta.ZZ3_Value
				where ZZ1_PK = @ZZ1_PK
				order by otherMatches.ZZ1_StartDate

				if (@ExistingIAmUnique is null)
				Begin
					set @MaxIAMUnique = @MaxIAMUnique + 1;
				End

				--Check if IAmUnique should change
				if (@ExistingIAmUnique is null OR @ExistingIAmUnique <> (Select Top 1 ZZ1_IAMUnique From RefCusTariff Where ZZ1_PK = @ZZ1_PK))
				Begin --if IAmUnique should change
					--Update IAmUnique wth max+1
					update RefCusTariff 
					Set ZZ1_IAMUnique = ISNULL(@ExistingIAmUnique,@MaxIAMUnique)
					where ZZ1_PK = @ZZ1_PK

					--insert RefCusTariff record to mark as deleted
					Declare @DeletedZZ1_PK uniqueidentifier
					Set @DeletedZZ1_PK = newid()
					Insert Into RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5)
					Select @DeletedZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,@ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
					From RefCusTariff
					Where ZZ1_PK = @ZZ1_PK;

					--Mark new record RefCusRecord as deleted
					Update RefDbVersionControl set RVC_Deleted = 1,[RVC_LastUpdatedUTC] = GETUTCDATE() Where RVC_ParentPK = @DeletedZZ1_PK and RVC_ParentCode = 'ZZ1';
				End --if IAmUnique should change
				FETCH NEXT FROM inner_cursor INTO @ZZ1_PK;
		END;
		CLOSE inner_cursor;
		DEALLOCATE inner_cursor;
		
       FETCH NEXT FROM outer_cursor INTO @ZZ1_TariffCode, @ZZ1_IAMUnique, @ZZ1_ZZI_TariffType;
END;
CLOSE outer_cursor;
DEALLOCATE outer_cursor;

--Mark schedule 5 and 6 record with no RefCusTariffRelationship as deleted
update vc set RVC_deleted = 1,vc.[RVC_LastUpdatedUTC] = GETUTCDATE()
from refcustariff t
join RefDbVersionControl vc on RVC_ParentPK = ZZ1_PK
join refcustarifftype tt on tt.zzi_pk = zz1_zzi_tarifftype 
left join RefCusTariffRelationship tr on tr.ZZH_ZZ1_Tariff = t.zz1_pk
where ZZ1_ZZZ_NKDataGrouping = 'ZA' and RVC_Deleted = 0
and (zzi_tarifftype like '5%' or zzi_tarifftype like '6%')
and zzh_pk is null
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
