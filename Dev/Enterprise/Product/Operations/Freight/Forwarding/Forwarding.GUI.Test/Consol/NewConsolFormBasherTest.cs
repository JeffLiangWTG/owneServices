using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ConsolForm))]
	internal sealed class NewConsolFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CCAAllocationsFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				ObjectFactory.Substitute(featureControlMock.Object);

				var consol = Factory.New<ForwardingConsol>();
				Factory.Save();
				var result = new ConsolForm(consol);
				result.ControllerID = ControllerIDs.JobConsol;
				return result;
			}
		}

		#endregion
	}
}
