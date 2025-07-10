using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class RefFacilityUpdaterTest : DSUpdaterTest<RefFacility, IRefFacility>
	{
		protected override RefFacility GetServerData()
		{
			var facility = new RefFacility
			{
				RFT_Code = "00000000001",
				RFT_Name = "123",
				RFT_FacilityType = "CTO",
				RFT_RL_NKLocationCode = "BBAAA",
				RFT_RN_NKCountryCode = "BB",
				RFT_GeoLocation = "POINT EMPTY",
				RFT_Address1 = "",
				RFT_Address2 = "",
				RFT_City = "",
				RFT_IATACode = "",
				RFT_State = "",
				RFT_PostCode = "",
				RFT_SMDGCode = "",
				RFT_BICCode = "",
				RFT_IHSGlobalPortId = "",
			};

			facility.RefFacilityLocalCodes = new RefFacilityLocalCode[]
			{
				new RefFacilityLocalCode
				{
					RFL_Usage = "ABC",
					RFL_RFT_NKFacilityCode = "00000000001",
					RFL_Code = "123",
					RFL_RN_NKCountryCode = "BB"
				}
			};
			return facility;
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return new Tuple<Type, int>(typeof(IRefFacility), 1);
			yield return new Tuple<Type, int>(typeof(IRefFacilityLocalCode), 1);
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new TwoTableDataSetUpdater<RefFacility, RefFacilityLocalCode, IRefFacility, IRefFacilityLocalCode>(proxy, dbHelper, versionControlManager);
		}

		protected override void PrepareDatabase()
		{
		}

		public override void AssertInsert(Tuple<Type, int> type)
		{
			base.AssertInsert(type);
			AssertEquals("IsSystem", 0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefFacility WHERE RFT_IsSystem = 0"));
			AssertEquals("IsSystem in RefFacilityLocalCode", 0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefFacilityLocalCode WHERE RFL_IsSystem = 0"));
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var facility = serverData as RefFacility;
			if (facility != null)
			{
				facility.RFT_Name = "QWE";
				return Tuple.Create(nameof(RefFacility.RFT_Name), "QWE");
			}

			var facilityLocalCode = serverData as RefFacilityLocalCode;
			if (facilityLocalCode != null)
			{
				facilityLocalCode.RFL_Code = "EWQ";
				return Tuple.Create(nameof(RefFacilityLocalCode.RFL_Code), "EWQ");
			}
			return null;
		}
	}
}
