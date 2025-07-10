declare @id uniqueidentifier
select @id = newid(); insert into refcustariffrule (zz1_pk,zz1_tariffcode,zz1_ZZZ_NKDataGrouping) values (@id,'46017','ZA'); insert into RefCusTariffAttributeRule (zz3_zz1_tariff,zz3_name,zz3_value) values (@id,'PRCC','PRCC');
select @id = newid(); insert into refcustariffrule (zz1_pk,zz1_tariffcode,zz1_ZZZ_NKDataGrouping) values (@id,'31703','ZA'); insert into RefCusTariffAttributeRule (zz3_zz1_tariff,zz3_name,zz3_value) values (@id,'PRCC','PRCC');
