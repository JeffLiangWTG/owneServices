using System.Threading;
using System.Threading.Tasks;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class FeatureControlTestDataFactory
	{
		public Mock<IFeatureControlManager> CreateKoreaSouthComplianceSubTypeFeatureControlMock(bool isComplianceSubTypeFeatureEnabled = true)
		{
			var featureControlData = new AccountingEInvoicingKoreaSubTypesFeatureControlData();
			featureControlData.IsComplianceSubTypeFeatureEnabled = isComplianceSubTypeFeatureEnabled;

			var mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out featureControlData)).Returns(true);

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingKoreaSouthComplianceSubTypeFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			return mockIFeatureControlManager;
		}

		public Mock<IFeatureControlManager> CreateAdvancePaymentFeatureControlMock(bool isFeatureEnabled = true)
		{
			var featureControlData = new AccAdvancePaymentFeatureControlData();
			featureControlData.IsAdvancePaymentFeatureEnabled = isFeatureEnabled;

			var mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out featureControlData)).Returns(true);

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingAdvancePaymentFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			return mockIFeatureControlManager;
		}
	}
}
