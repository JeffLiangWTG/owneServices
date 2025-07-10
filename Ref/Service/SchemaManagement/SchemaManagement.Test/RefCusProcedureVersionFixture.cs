using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusProcedureVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var procedure = new RefCusProcedure
			{
				ZZ6_PK = Guid.NewGuid(),
				ZZ6_Category = "A",
				ZZ6_ProcedureCode = "13",
				ZZ6_PreviousProcedureCode = "41",
				ZZ6_Concession = "4",
				ZZ6_Group = "ISD",
				ZZ6_Description = "XX",
				ZZ6_ZZZ_NKDataGrouping = "ZA",
				ZZ6_ShipmentType = "EXB",
				ZZ6_IntoWarehouse = "Y",
				ZZ6_OutOfWarehouse = "Y",
				ZZ6_StartDate = new DateTime(1900, 01, 01),
				ZZ6_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZ6_IsGuaranteeConsumed = "Y",
				ZZ6_IsGuaranteeReleased = "I",
				ZZ6_IntoInwardProcessing = "N",
				ZZ6_OutOfInwardProcessing = "N",
				ZZ6_IntoOutwardProcessing = "N",
				ZZ6_OutofOutwardProcessing = "N",
				ZZ6_IntoTemporaryImport = "N",
				ZZ6_OutOfTemporaryImport = "N",
				ZZ6_IntoTemporaryExport = "N",
				ZZ6_OutOfTemporaryExport = "N",
				ZZ6_IsTransit = "N",
				ZZ6_IntoVATWarehouse = "N",
				ZZ6_OutOfVATWarehouse = "N"
			};
			result.Add(procedure);
			result.Add(new RefCusProcedureAttribute
			{
				ZXB_PK = Guid.NewGuid(),
				ZXB_ZZ6_ProcedureCode = procedure.ZZ6_PK,
				ZXB_Name = "XX",
				ZXB_Value = "XX"
			});
			result.Add(new RefCusProcedureLanguage
			{
				ZXV_Description = "D",
				ZXV_PK = Guid.NewGuid(),
				ZXV_ZZ6_Procedure = procedure.ZZ6_PK,
				ZXV_ZX6_NKLanguage = "EN"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusProcedure procedure)
			{
				procedure.ZZ6_Description = "YY";
			}
			else if (data is RefCusProcedureAttribute procedureAttribute)
			{
				procedureAttribute.ZXB_Name = "YY";
			}
			else if (data is RefCusProcedureLanguage procedureLanguage)
			{
				procedureLanguage.ZXV_Description = "DES";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusProcedure
			{
				ZZ6_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZ6_Category = "B",
				ZZ6_ProcedureCode = "11",
				ZZ6_PreviousProcedureCode = "11",
				ZZ6_Concession = "1",
				ZZ6_Group = "IFD",
				ZZ6_Description = "BB",
				ZZ6_ZZZ_NKDataGrouping = "ZA",
				ZZ6_ShipmentType = "EXB",
				ZZ6_IntoWarehouse = "Y",
				ZZ6_OutOfWarehouse = "Y",
				ZZ6_StartDate = new DateTime(1900, 01, 01),
				ZZ6_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZ6_IsGuaranteeConsumed = "Y",
				ZZ6_IsGuaranteeReleased = "I",
				ZZ6_IntoInwardProcessing = "N",
				ZZ6_OutOfInwardProcessing = "N",
				ZZ6_IntoOutwardProcessing = "N",
				ZZ6_OutofOutwardProcessing = "N",
				ZZ6_IntoTemporaryImport = "N",
				ZZ6_OutOfTemporaryImport = "N",
				ZZ6_IntoTemporaryExport = "N",
				ZZ6_OutOfTemporaryExport = "N",
				ZZ6_IsTransit = "N",
				ZZ6_IntoVATWarehouse = "N",
				ZZ6_OutOfVATWarehouse = "N"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusProcedureAttribute attr)
			{
				attr.ZXB_ZZ6_ProcedureCode = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCusProcedureLanguage language)
			{
				language.ZXV_ZZ6_Procedure = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				context.SaveChanges();
			}
		}
	}
}
