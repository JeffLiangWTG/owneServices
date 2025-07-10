using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefFacilityFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			result.Add(new RefFacility
			{
				RFT_PK = Guid.NewGuid(),
				RFT_Code = "00000000001",
				RFT_Name = "123",
				RFT_FacilityType = "CTO",
				RFT_RL_NKLocationCode = "BBAAA",
				RFT_RN_NKCountryCode = "BB",
				RFT_Address1 = "",
				RFT_Address2 = "",
				RFT_City = "",
				RFT_IATACode = "",
				RFT_State = "",
				RFT_PostCode = "",
				RFT_SMDGCode = "",
				RFT_BICCode = "",
				RFT_GeoLocation = TypeExtension.ConvertToGeometry(4326, "POINT EMPTY"),
				RFT_IHSGlobalPortId = ""
			});

			result.Add(new RefFacilityLocalCode
			{
				RFL_PK = Guid.NewGuid(),
				RFL_Usage = "ABC",
				RFL_RFT_NKFacilityCode = "00000000001",
				RFL_Code = "123",
				RFL_RN_NKCountryCode = "BB"
			});

			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefFacility facility)
			{
				facility.RFT_Name = "QWE";
			}
			if (data is RefFacilityLocalCode facilityLocalCode)
			{
				facilityLocalCode.RFL_Code = "EWQ";
			}
			return true;
		}
	}
}
