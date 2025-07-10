using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGCountryReferenceFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var reference = new UNDGCountryReference
			{
				DCR_PK = Guid.NewGuid(),
				DCR_Code = "TEST",
				DCR_Description = "TEST",
				DCR_FlashPointLowerCentigrade = 0,
				DCR_FlashPointUpperCentigrade = 0,
				DCR_HasFlashPointLower = true,
				DCR_HasFlashPointUpper = true,
				DCR_RN_NKCountry = "ZZ",
				DCR_Type = "PSA"
			};
			result.Add(reference);
			result.Add(new UNDGCountryReferencePivot
			{
				DCP_PK = Guid.NewGuid(),
				DCP_DCR = reference.DCR_PK,
				DCP_Standard = "T",
				DCP_UNNO = "TEST",
				DCP_Variant = "T"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is UNDGCountryReference reference)
			{
				reference.DCR_Description = "TestDescription";
			}
			if (data is UNDGCountryReferencePivot pivot)
			{
				pivot.DCP_UNNO = "X";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new UNDGCountryReference
			{
				DCR_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				DCR_Code = "BBB",
				DCR_Description = "BBB",
				DCR_FlashPointLowerCentigrade = 0,
				DCR_FlashPointUpperCentigrade = 0,
				DCR_HasFlashPointLower = true,
				DCR_HasFlashPointUpper = true,
				DCR_RN_NKCountry = "ZZ",
				DCR_Type = "PSA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is UNDGCountryReferencePivot pivot)
			{
				pivot.DCP_DCR = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
