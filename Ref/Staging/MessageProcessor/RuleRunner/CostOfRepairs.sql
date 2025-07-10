declare @id uniqueidentifier 
select @id = newid()
insert into RefCusTariffRule (ZZ1_PK, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping)
select @id, '40907', ZZI_PK, 'ZA' from RefCusTariffType  where ZZI_TariffType = '4P1';
insert into RefCusTariffAttributeRule (ZZ3_Name, ZZ3_Value, ZZ3_ZZ1_Tariff)
values ('CostOfRepair', 'CostOfRepair', @id)

select @id = newid()
insert into RefCusTariffRule (ZZ1_PK, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping)
select @id, '40904', ZZI_PK, 'ZA' from RefCusTariffType  where ZZI_TariffType = '4P1';
insert into RefCusTariffAttributeRule (ZZ3_Name, ZZ3_Value, ZZ3_ZZ1_Tariff)
values ('CostOfRepair', 'CostOfRepair', @id)