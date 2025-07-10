using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class ComplianceRiskFeatureControlHelperTest
	{
		public static IDisposable SetupComplianceWiseCommodityScreeningMocksForTest(bool enable)
		{
			return SetupComplianceWiseFeatureMocksForTest(enable, LicenceFeatureCodeList.Codes.ComplianceWiseCommodityScreening);
		}

		public static IDisposable SetupComplianceWiseCommodityInvoiceLineMocksForTest(bool enable)
		{
			return SetupComplianceWiseFeatureMocksForTest(enable, LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, LicenceFeatureCodeList.Codes.ComplianceWiseCommodityScreening, LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice);
		}

		static IDisposable SetupComplianceWiseFeatureMocksForTest(bool enable, params string[] featureCodes)
		{
			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = enable };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
			foreach (var featureCode in featureCodes)
			{
				featureControlMock.Setup(x => x.GetFeatureDataAsync(featureCode, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			}

			return ObjectFactory.Substitute(featureControlMock.Object);
		}
	}
}
