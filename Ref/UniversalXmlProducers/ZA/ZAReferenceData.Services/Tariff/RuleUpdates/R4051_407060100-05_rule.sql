-- Create new rule for 407.06.01.00/05
DECLARE @ZZ1_PK UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.RefCusTariffRule ([ZZ1_PK], [ZZ1_TariffCode], [ZZ1_ZZI_TariffType], [ZZ1_ZZZ_NKDataGrouping], [ZZ1_Applied])
VALUES (@ZZ1_PK, '407060100', (select ZZI_PK FROM RefCusTariffType where ZZI_ZZZ_NKDataGrouping = 'ZA' AND ZZI_TariffType = '4P1'), 'ZA', 0)

INSERT INTO dbo.RefCusTariffAttributeRule([ZZ3_PK], [ZZ3_ZZ1_Tariff], [ZZ3_Name], [ZZ3_Value])
VALUES (NEWID(), @ZZ1_PK, 'CheckDigit', '05')

INSERT INTO dbo.RefCusRateRule([ZZ2_PK], [ZZ2_ZZ1_Tariff], [ZZ2_RateFormula], [ZZ2_SelectorFormula], [ZZ2_ZZZ_NKDataGrouping], [ZZ2_RateFormulaDerivedFrom])
VALUES (NEWID(), @ZZ1_PK, '1P1+12A+12B+13A+13B+13C+13D+15A+15B', '', 'ZA', '(R4051) FULL DUTY EXCLUDING 13E')

-- Update existing Rate
UPDATE RefCusRate SET ZZ2_RateFormula = '1P1+12A+12B+13A+13B+13C+13D+15A+15B'
from RefCusTariff
join RefCusTariffAttribute on ZZ3_ZZ1_Tariff = ZZ1_PK and ZZ3_Name = 'CheckDigit'
join RefCusRate on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusRateCode on ZY1_PK = ZZ2_ZY1_RateCode
where
	ZZ1_ZZZ_NKDataGrouping = 'ZA'
	and ZZ1_TariffCode = '407060100'
	and ZZ3_Value = '05'
	and ZZ1_EndDate > GETDATE()
	and ZY1_RateCode = '4P1'
	and ZZ2_RateFormula = '1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B'

