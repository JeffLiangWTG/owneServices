using System;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[UseSnapshotProtection]
	sealed class MHUBSettingsProviderTest : TestCase
	{
		public void TestClientVersion()
		{
			var clientVersion = new StringEffectiveDate()
			{
				NewValue = "4.0.4",
				EffectiveDate = ZDateTime.Now.AddDays(-1)
			};

			using (SGCustomsDataRegistry.Instance.MhaccessVersionString.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, clientVersion))
			{
				AssertEquals("4.0.4", settingsProvider.ClientVersion);
			}
		}

		public void TestDigestForLibs()
		{
			var digestForLibs = new StringEffectiveDate()
			{
				NewValue = "E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D",
				EffectiveDate = ZDateTime.Now.AddDays(-1)
			};

			using (SGCustomsDataRegistry.Instance.DigestForLibs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, digestForLibs))
			{
				AssertEquals("E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D", settingsProvider.DigestForLibs);
			}
		}

		public void TestEncryptionKey()
		{
			using (SGCustomsDataRegistry.Instance.EncryptionKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UdJCd/zLFmybL6HgjSD08TkJKA9tGcis"))
			{
				AssertEquals("UdJCd/zLFmybL6HgjSD08TkJKA9tGcis", settingsProvider.EncryptionKey);
			}
		}

		public void TestEndpointPartialPathForDownload()
		{
			using (SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForDownload.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "/txmhbweb/download/downloadService"))
			{
				AssertEquals("/txmhbweb/download/downloadService", settingsProvider.EndpointPartialPathForDownload);
			}
		}

		public void TestEndpointPartialPathForServlet()
		{
			using (SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForServlet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "/txmhbweb/mhb/EDIServlet"))
			{
				AssertEquals("/txmhbweb/mhb/EDIServlet", settingsProvider.EndpointPartialPathForServlet);
			}
		}

		public void TestEndpointPartialPathForUpload()
		{
			using (SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForUpload.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "/txmhbweb/upload/uploadService"))
			{
				AssertEquals("/txmhbweb/upload/uploadService", settingsProvider.EndpointPartialPathForUpload);
			}
		}

		public void TestJreVersion()
		{
			var jreVersion = new StringEffectiveDate()
			{
				NewValue = "1.8.0_131",
				EffectiveDate = ZDateTime.Now.AddDays(-1)
			};

			using (SGCustomsDataRegistry.Instance.JREVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jreVersion))
			{
				AssertEquals("1.8.0_131", settingsProvider.JreVersion);
			}
		}

		public void TestRecipientID_Production()
		{
			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SGCustomsDataRegistry.Instance.RecipientIDLive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DCS4001"))
			{
				AssertEquals("DCS4001", settingsProvider.RecipientID);
			}
		}

		public void TestRecipientID_Test()
		{
			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SGCustomsDataRegistry.Instance.RecipientIDTrial.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DCST401"))
			{
				AssertEquals("DCST401", settingsProvider.RecipientID);
			}
		}

		public void TestSoapDownloadDirectory()
		{
			using (SGCustomsDataRegistry.Instance.SoapDownloadDirectory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "/fshome/sftphome/<<LOGIN>>/mhxdownload/"))
			{
				AssertEquals("/fshome/sftphome/<<LOGIN>>/mhxdownload/", settingsProvider.SoapDownloadDirectory);
			}
		}

		public void TestSynchronousReadWriteTimeout()
		{
			using (SGCustomsDataRegistry.Instance.SynchronousMHUBReadWriteTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 600000))
			{
				AssertEquals(600000, settingsProvider.SynchronousReadWriteTimeout);
			}
		}

		public void TestSynchronousTimeout()
		{
			using (SGCustomsDataRegistry.Instance.SynchronousMHUBTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 300000))
			{
				AssertEquals(300000, settingsProvider.SynchronousTimeout);
			}
		}

		public void TestTrustStore_Production()
		{
			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SGCustomsDataRegistry.Instance.MhaccessTrustStore_Prod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MHX_ProdTruststore.db"))
			{
				AssertEquals("MHX_ProdTruststore.db", settingsProvider.TrustStore);
			}
		}

		public void TestTrustStore_Test()
		{
			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SGCustomsDataRegistry.Instance.MhaccessTrustStore_Trial.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MHX_TrialTruststore.db"))
			{
				AssertEquals("MHX_TrialTruststore.db", settingsProvider.TrustStore);
			}
		}

		public void TestUseDirectWebServicesInsteadOfScripting()
		{
			using (SGCustomsDataRegistry.Instance.UseMhxDirectWebServicesInsteadOfScripting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, settingsProvider.UseDirectWebServicesInsteadOfScripting);
			}
		}

		public void TestVendorID()
		{
			using (SGCustomsDataRegistry.Instance.VendorID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "A7"))
			{
				AssertEquals("A7", settingsProvider.VendorID);
			}
		}

		public void TestWebAddress_Production()
		{
			var webAddress = new StringEffectiveDate()
			{
				NewValue = "https://www.tradenet.gov.sg",
				EffectiveDate = ZDateTime.Now.AddDays(-1)
			};

			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SGCustomsDataRegistry.Instance.WebAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webAddress))
			{
				AssertEquals("https://www.tradenet.gov.sg", settingsProvider.WebAddress);
			}
		}

		public void TestWebAddress_Test()
		{
			var webAddress = new StringEffectiveDate()
			{
				NewValue = "https://trial.tradenet.gov.sg",
				EffectiveDate = ZDateTime.Now.AddDays(-1)
			};

			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SGCustomsDataRegistry.Instance.WebAddressTrial.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webAddress))
			{
				AssertEquals("https://trial.tradenet.gov.sg", settingsProvider.WebAddress);
			}
		}

		public void TestWebProxyAddress()
		{
			using (SGCustomsDataRegistry.Instance.WebProxyAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SYD-PROXY-1:888"))
			{
				AssertEquals("SYD-PROXY-1:888", settingsProvider.WebProxyAddress);
			}
		}

		readonly MHUBSettingsProvider settingsProvider = new MHUBSettingsProvider();
	}
}
