using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(NZDiagnosticConsol))]
	internal sealed class NZDiagnosticConsolTest : DiagnosticConsolTest
	{
		public void TestRegistryAndCertificateChecks()
		{
			const string message = @"There is no Customs Registration Number set for the current company.Please set registry 'Customs > Country or Region Specific > New Zealand > Brokerage ID'.";

			var diagnosticConsol = new NZDiagnosticConsol(Factory);
			var (messages, isAllHealthorWarning) = diagnosticConsol.ProcessRegistryAndCertificateCheckResults();
			AssertCollectionContains("Registry NZBrokerageID is not set", message, messages);

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "xyz"))
			{
				diagnosticConsol = new NZDiagnosticConsol(Factory);
				var (messages1, isAllHealthorWarning1) = diagnosticConsol.ProcessRegistryAndCertificateCheckResults();
				AssertCollectionNotContains("Registry NZBrokerageID is set", message, messages1);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZDiagnosticConsol(Factory);
		}

		#endregion
	}
}
