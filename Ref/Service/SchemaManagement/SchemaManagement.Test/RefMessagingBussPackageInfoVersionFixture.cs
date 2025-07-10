using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefMessagingBussPackageInfoVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var packageInfo = new RefMessagingBussPackageInfo
			{
				ZMP_PK = Guid.NewGuid(),
				ZMP_PackageName = "A"
			};
			result.Add(packageInfo);
			result.Add(new RefMessagingBussPackageVersion
			{
				ZMV_PK = Guid.NewGuid(),
				ZMV_ZMP_PackageInfo = packageInfo.ZMP_PK,
				ZMV_Version = "XX"
			});
			result.Add(new RefMessagingBussCarrierInfo
			{
				ZMC_PK = Guid.NewGuid(),
				ZMC_CarrierCode = "AAAAA",
				ZMC_CarrierName = "D",
				ZMC_ZMP_PackageInfo = packageInfo.ZMP_PK,
				ZMC_CountryCode = "EN"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefMessagingBussPackageInfo packageInfo)
			{
				packageInfo.ZMP_PackageName = "YY";
			}
			else if (data is RefMessagingBussPackageVersion packageVersion)
			{
				packageVersion.ZMV_Version = "YY";
			}
			else if (data is RefMessagingBussCarrierInfo carrierInfo)
			{
				carrierInfo.ZMC_CarrierName = "DES";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefMessagingBussPackageInfo
			{
				ZMP_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZMP_PackageName = "B"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefMessagingBussPackageVersion packageVersion)
			{
				packageVersion.ZMV_ZMP_PackageInfo = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefMessagingBussCarrierInfo carrierInfo)
			{
				carrierInfo.ZMC_ZMP_PackageInfo = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
