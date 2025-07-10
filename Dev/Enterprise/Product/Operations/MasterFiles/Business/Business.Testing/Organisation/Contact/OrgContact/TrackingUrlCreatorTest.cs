using System;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TrackingUrlCreatorTest : TestCaseWithFactory
	{
		public void TestCreateUrl_RootPathNotSet()
		{
			ZGuid contactPK = ZGuid.NewZGuid();
			TrackingConstants.BusinessContext businessContext = TrackingConstants.BusinessContext.WarehouseOrder;
			ZGuid businessContextPK = ZGuid.NewZGuid();

			AssertEquals("Should return empty if root path is not set", "", TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK));

			SetTestWebTrackerUrl(string.Empty);
			AssertEquals("Should return empty if root path is not set", "", TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK));
		}

		public void TestCreateUrl_InvalidContactPK()
		{
			ZGuid contactPK = ZGuid.Invalid;
			TrackingConstants.BusinessContext businessContext = TrackingConstants.BusinessContext.WarehouseOrder;
			ZGuid businessContextPK = ZGuid.NewZGuid();
			SetTestWebTrackerUrl("http://www.tracking.edi.com.au");
			AssertEquals("Should not return empty if contact is invalid", TrackingUrlCreator.Instance.CreateUrl(Guid.Empty, businessContext, businessContextPK), TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK));
		}

		public void TestCreateUrl_NoBusinessContext()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var contactPK = orgContact.PK;
			SetTestWebTrackerUrl("http://www.tracking.edi.com.au");

			var queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, nameof(TrackingConstants.BusinessContext.NoBusinessContext));
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextNKKey, string.Empty);
			var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK);
			var expectedUrl = GetExpectedWebTrackerUrlForCompany(queryString);
			AssertEquals(expectedUrl, generatedUrl);
		}

		public void TestCreateUrl()
		{
			OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			ZGuid contactPK = orgContact.PK;
			TrackingConstants.BusinessContext businessContext = TrackingConstants.BusinessContext.WarehouseOrder;
			ZGuid businessContextPK = ZGuid.NewZGuid();
			ZGuid additionalRef = ZGuid.NewZGuid();
			ZGuid additionalRef2 = ZGuid.NewZGuid();

			ZString businessContextNK = "123456789";
			SetTestWebTrackerUrl("http://www.tracking.edi.com.au");

			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, nameof(TrackingConstants.BusinessContext.WarehouseOrder));
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());
			string generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK);
			string expectedUrl = GetExpectedWebTrackerUrlForCompany(queryString);
			AssertEquals(expectedUrl, generatedUrl);

			SecureQueryString queryStringNK = new SecureQueryString();
			queryStringNK.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryStringNK.Add(TrackingConstants.AutoLogin.BusinessContextKey, nameof(TrackingConstants.BusinessContext.WarehouseOrder));
			queryStringNK.Add(TrackingConstants.AutoLogin.BusinessContextNKKey, businessContextNK.ToString());
			generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextNK);
			expectedUrl = GetExpectedWebTrackerUrlForCompany(queryStringNK);
			AssertEquals(expectedUrl, generatedUrl);

			SetTestWebTrackerUrl("http://localhost/tracking");

			businessContext = TrackingConstants.BusinessContext.Booking;
			queryString[TrackingConstants.AutoLogin.BusinessContextKey] = nameof(TrackingConstants.BusinessContext.Booking);
			generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK);
			expectedUrl = GetExpectedWebTrackerUrlForCompany(queryString);
			AssertEquals(expectedUrl, generatedUrl);

			businessContext = TrackingConstants.BusinessContext.Booking;
			queryStringNK[TrackingConstants.AutoLogin.BusinessContextKey] = nameof(TrackingConstants.BusinessContext.Booking);
			generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextNK);
			expectedUrl = GetExpectedWebTrackerUrlForCompany(queryStringNK);
			AssertEquals(expectedUrl, generatedUrl);

			businessContext = TrackingConstants.BusinessContext.eDoc;
			queryString[TrackingConstants.AutoLogin.BusinessContextKey] = nameof(TrackingConstants.BusinessContext.eDoc);
			queryString[TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey] = additionalRef.ToString() + "," + additionalRef2.ToString();
			generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK, additionalRef, additionalRef2);
			expectedUrl = GetExpectedWebTrackerUrlForCompany(queryString);
			AssertEquals(expectedUrl, generatedUrl);

			contactPK = ZGuid.Empty;
			businessContext = TrackingConstants.BusinessContext.Booking;
			queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, nameof(TrackingConstants.BusinessContext.Booking));
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());
			generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK);
			expectedUrl = GetExpectedWebTrackerUrlForCompany(queryString);
			AssertEquals(expectedUrl, generatedUrl);
		}

		public void TestCreateUrl_MultipleCompanies()
		{
			var company1 = Factory.New<GlbCompany>();
			var company2 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			company2.GC_Code = "GC2";

			var branch1 = company1.Branches.AddNew();
			var branch2 = company2.Branches.AddNew();
			branch1.GB_Code = "GB1";
			branch2.GB_Code = "GB2";

			Guid orgContactPK = Factory.NewWithValidTestData<OrgContact>().PK.ToGuid(),
				branch1PK = branch1.PK.ToGuid(),
				branch2PK = branch2.PK.ToGuid();

			Factory.Save();

			SetTestWebTrackerUrl("http://www.trackingcompany1.edi.com.au", company1.PK.ToGuid());
			SetTestWebTrackerUrl("http://www.trackingcompany2.edi.com.au", company2.PK.ToGuid());
			SetTestWebTrackerUrl("http://www.basetracker.edi.com.au");

			var businessContext = TrackingConstants.BusinessContext.Shipment;
			var businessContextPK = Guid.NewGuid();

			var queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, orgContactPK.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString());

			using (Env.SetTemporaryUserContext(orgContactPK, branch1PK, Env.CurrentDepartment.PK))
			{
				var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(orgContactPK, businessContext, businessContextPK);
				var expectedCompany1Url = GetExpectedWebTrackerUrlForCompany(queryString);
				AssertEquals(expectedCompany1Url, generatedUrl);
			}

			using (Env.SetTemporaryUserContext(orgContactPK, branch2PK, Env.CurrentDepartment.PK))
			{
				var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(orgContactPK, businessContext, businessContextPK);
				var expectedCompany2Url = GetExpectedWebTrackerUrlForCompany(queryString);
				AssertEquals(expectedCompany2Url, generatedUrl);
			}

			var generatedSystemUrl = TrackingUrlCreator.Instance.CreateUrl(orgContactPK, businessContext, businessContextPK);
			var expectedSystemUrl = GetExpectedWebTrackerUrlForCompany(queryString);
			AssertEquals(expectedSystemUrl, generatedSystemUrl);
		}

		public void TestCreateUrl_Neo()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					"https://glowdev/Portals");

				var contactPK = ZGuid.NewZGuid();
				var testCases = new[]
				{
				(TrackingConstants.BusinessContext.Declaration, "TrackingDeclaration"),
				(TrackingConstants.BusinessContext.Order, "TrackingOrder"),
				(TrackingConstants.BusinessContext.WarehouseOrder, "TrackingWarehouseOrder"),
				(TrackingConstants.BusinessContext.Booking, "TrackingBooking"),
				(TrackingConstants.BusinessContext.Transaction, "TrackingInvoice"),
				(TrackingConstants.BusinessContext.Receive, "TrackingWarehouseReceipt"),
				(TrackingConstants.BusinessContext.Cartage, "TrackingTransportJob"),
				(TrackingConstants.BusinessContext.ISF, "TrackingISF"),
				(TrackingConstants.BusinessContext.Quotations, "TrackingQuote"),
				(TrackingConstants.BusinessContext.SupplierBooking, "TrackingSupplierBooking"),
				(TrackingConstants.BusinessContext.ContainerLoadList, "TrackingContainerLoadList"),
			};
				var businessContextPK = ZGuid.NewZGuid();

				foreach (var testCase in testCases)
				{
					var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, testCase.Item1, businessContextPK);

					AssertEquals($"https://glowdev/Portals/goto/{testCase.Item2}?entityPK={businessContextPK}", generatedUrl);
				}
			}
		}

		public void TestCreateUrl_Neo_ByNK()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");

				var contactPK = ZGuid.NewZGuid();
				var testCases = new[]
				{
					(TrackingConstants.BusinessContext.Declaration, "TrackingDeclaration"),
					(TrackingConstants.BusinessContext.Order, "TrackingOrder"),
					(TrackingConstants.BusinessContext.WarehouseOrder, "TrackingWarehouseOrder"),
					(TrackingConstants.BusinessContext.Booking, "TrackingBooking"),
					(TrackingConstants.BusinessContext.Transaction, "TrackingInvoice"),
					(TrackingConstants.BusinessContext.Receive, "TrackingWarehouseReceipt"),
					(TrackingConstants.BusinessContext.Cartage, "TrackingTransportJob"),
					(TrackingConstants.BusinessContext.ISF, "TrackingISF"),
					(TrackingConstants.BusinessContext.Quotations, "TrackingQuote"),
					(TrackingConstants.BusinessContext.SupplierBooking, "TrackingSupplierBooking"),
					(TrackingConstants.BusinessContext.ContainerLoadList, "TrackingContainerLoadList"),
				};
				var businessContextNK = "BIZO1234";

				foreach (var testCase in testCases)
				{
					var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, testCase.Item1, businessContextNK);

					AssertEquals($"https://glowdev/Portals/goto/{testCase.Item2}?entityNK={businessContextNK}", generatedUrl);
				}
			}
		}

		public void TestCreateUrl_Neo_GuestTracking()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");

				var contactPK = ZGuid.NewZGuid();
				var businessContext = TrackingConstants.BusinessContext.Shipment;
				var businessContextPK = ZGuid.NewZGuid();

				var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK);

				AssertEquals($"https://glowdev/Portals/NEO/Desktop#/tracker?trackingKey={businessContextPK}", generatedUrl);
			}
		}

		public void TestCreateUrl_Neo_GuestTracking_ByNK()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");

				var contactPK = ZGuid.NewZGuid();
				var businessContext = TrackingConstants.BusinessContext.Shipment;
				var businessContextNK = "S00001234";

				var generatedUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextNK);

				AssertEquals($"https://glowdev/Portals/NEO/Desktop#/tracker?trackingNumber={businessContextNK}", generatedUrl);
			}
		}

		public void TestCreateUrl_Neo_HyperlinksNotEnabled()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");
				SetTestWebTrackerUrl("http://www.basetracker.edi.com.au");

				var contactPK = ZGuid.NewZGuid();
				var businessContextPK = ZGuid.NewZGuid();

				var testCases = new[]
				{
					TrackingConstants.BusinessContext.NoBusinessContext,
					TrackingConstants.BusinessContext.Shipment,
				TrackingConstants.BusinessContext.Declaration,
			};

				foreach (var testCase in testCases)
				{
					var queryString = new SecureQueryString
				{
					{ TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString() },
					{ TrackingConstants.AutoLogin.BusinessContextKey, testCase.ToString() },
					{ TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString() },
				};

					var generatedSystemUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, testCase, businessContextPK);
					var expectedSystemUrl = GetExpectedWebTrackerUrlForCompany(queryString);

					AssertEquals(expectedSystemUrl, generatedSystemUrl);
				}
			}
		}

		public void TestCreateUrl_Neo_ByNK_HyperlinksNotEnabled()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");
				SetTestWebTrackerUrl("http://www.basetracker.edi.com.au");

				var contactPK = ZGuid.NewZGuid();
				var businessContextNK = "BIZO1234";

				var testCases = new[]
				{
					TrackingConstants.BusinessContext.NoBusinessContext,
					TrackingConstants.BusinessContext.Shipment,
					TrackingConstants.BusinessContext.Declaration,
				};

				foreach (var testCase in testCases)
				{
					var queryString = new SecureQueryString
					{
						{ TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString() },
						{ TrackingConstants.AutoLogin.BusinessContextKey, testCase.ToString() },
						{ TrackingConstants.AutoLogin.BusinessContextNKKey, businessContextNK },
					};

					var generatedSystemUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, testCase, businessContextNK);
					var expectedSystemUrl = GetExpectedWebTrackerUrlForCompany(queryString);

					AssertEquals(expectedSystemUrl, generatedSystemUrl);
				}
			}
		}

		public void TestCreateUrl_Neo_WebtrackerFallback()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");
				SetTestWebTrackerUrl("http://www.basetracker.edi.com.au");

				var contactPK = ZGuid.NewZGuid();
				var testCases = new[]
				{
					TrackingConstants.BusinessContext.NoBusinessContext,
					TrackingConstants.BusinessContext.Consol,
					TrackingConstants.BusinessContext.eDoc,
					TrackingConstants.BusinessContext.FreightLabel,
				TrackingConstants.BusinessContext.HouseBill,
				TrackingConstants.BusinessContext.QuotationClientReplyAccept,
				TrackingConstants.BusinessContext.QuotationClientReplyNotAccept,
			};
				var businessContextPK = ZGuid.NewZGuid();

				foreach (var testCase in testCases)
				{
					var queryString = new SecureQueryString
				{
					{ TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString() },
					{ TrackingConstants.AutoLogin.BusinessContextKey, testCase.ToString() },
					{ TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString() },
				};

					var generatedSystemUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, testCase, businessContextPK);
					var expectedSystemUrl = GetExpectedWebTrackerUrlForCompany(queryString);

					AssertEquals(expectedSystemUrl, generatedSystemUrl);
				}
			}
		}

		public void TestCreateUrl_Neo_ByNK_WebtrackerFallback()
		{
			GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistryTestHelper.SetFeatureFlagNeo())
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals");
				SetTestWebTrackerUrl("http://www.basetracker.edi.com.au");

				var contactPK = ZGuid.NewZGuid();
				var testCases = new[]
				{
					TrackingConstants.BusinessContext.NoBusinessContext,
					TrackingConstants.BusinessContext.Consol,
					TrackingConstants.BusinessContext.eDoc,
					TrackingConstants.BusinessContext.FreightLabel,
					TrackingConstants.BusinessContext.HouseBill,
					TrackingConstants.BusinessContext.QuotationClientReplyAccept,
					TrackingConstants.BusinessContext.QuotationClientReplyNotAccept,
				};
				var businessContextNK = "BIZO1234";

				foreach (var testCase in testCases)
				{
					var queryString = new SecureQueryString
					{
						{ TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString() },
						{ TrackingConstants.AutoLogin.BusinessContextKey, testCase.ToString() },
						{ TrackingConstants.AutoLogin.BusinessContextNKKey, businessContextNK },
					};

					var generatedSystemUrl = TrackingUrlCreator.Instance.CreateUrl(contactPK, testCase, businessContextNK);
					var expectedSystemUrl = GetExpectedWebTrackerUrlForCompany(queryString);

					AssertEquals(expectedSystemUrl, generatedSystemUrl);
				}
			}
		}

		void SetTestWebTrackerUrl(string value, Guid companyPK = default(Guid))
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		string GetExpectedWebTrackerUrlForCompany(SecureQueryString queryString)
		{
			return string.Format("{0}/{1}?{2}={3}",
				WebDataRegistry.Instance.WebTrackerUrl.Value,
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				TrackingConstants.AutoLogin.SecureQueryStringDataKey,
				WebUtility.UrlEncode(queryString.ToString()));
		}
	}
}
