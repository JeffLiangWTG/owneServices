using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWJobRequiredDocumentDocumentSupporter))]
	sealed class TWJobRequiredDocumentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			var documentSupportable = GetDocumentSupportableBusinessObject();
			AssertType<TWJobRequiredDocumentDocumentSupporter>(documentSupportable.DocumentSupporter);
			var header = Factory.New<OrgHeader>();
			var requiredDocument = header.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocUsage = "BRK";
			requiredDocument.EQ_DocType = "POA";
			requiredDocument.EQ_RN_NKRelatedCountry = "IT";
			AssertType<JobRequiredDocumentDocumentSupporter>(requiredDocument.DocumentSupporter);
		}

		public void TestShowReasonForNotPrinting()
		{
			var documentSupportable = GetDocumentSupportableBusinessObject();
			AssertEquals(false, documentSupportable.DocumentSupporter.ShowReasonForNotPrinting(DataContext.Notes, null));
			AssertEquals(false, documentSupportable.DocumentSupporter.ShowReasonForNotPrinting(DataContext.None, null));
		}

		[TestDate(2020, 06, 08)]
		public void TestGetBODocDataProviders()
		{
			var documentSupportable = GetDocumentSupportableBusinessObject();
			var providers = documentSupportable.DocumentSupporter.GetBODocDataProviders(new DataContextValue(TWJobRequiredDocumentDocumentSupporter.TWLetterOfAuthorization), null);
			AssertEquals(1, providers.Length);
			AssertType<TWJobRequiredDocumentWrapper>(providers[0].ParentBusinessObject);
			var wrappers = providers.Select(x => x.ParentBusinessObject).Cast<TWJobRequiredDocumentWrapper>();
			Assert(wrappers.Any(x => x.BoxNumber == "111" && x.CustomsDistrictDescription == "基隆"));
			var documentSupporter = new TWJobRequiredDocumentDocumentSupporter(orgHeader);
			providers = documentSupporter.GetBODocDataProviders(new DataContextValue(TWJobRequiredDocumentDocumentSupporter.TWLetterOfAuthorization), null);
			AssertEquals(2, providers.Length);
			AssertType<TWJobRequiredDocumentWrapper>(providers[0].ParentBusinessObject);
			wrappers = providers.Select(x => x.ParentBusinessObject).Cast<TWJobRequiredDocumentWrapper>();
			Assert(wrappers.Any(x => x.BoxNumber == "111" && x.CustomsDistrictDescription == "基隆"));
			Assert(wrappers.Any(x => x.BoxNumber == "222" && x.CustomsDistrictDescription == "高雄"));
			TestDateAttribute.AddDays(20);
			providers = documentSupporter.GetBODocDataProviders(new DataContextValue(TWJobRequiredDocumentDocumentSupporter.TWLetterOfAuthorization), null);
			AssertEquals(2, providers.Length);
			wrappers = providers.Select(x => x.ParentBusinessObject).Cast<TWJobRequiredDocumentWrapper>();
			Assert(wrappers.Any(x => x.BoxNumber == "111" && x.CustomsDistrictDescription == "基隆"));
			Assert(wrappers.Any(x => x.BoxNumber == "222" && x.CustomsDistrictDescription == "高雄"));
		}

		public void TestDataContextSupported()
		{
			var documentSupportable = GetDocumentSupportableBusinessObject();
			AssertEquals(true, documentSupportable.DocumentSupporter.IsDataContextSupported(new DataContextValue(TWJobRequiredDocumentDocumentSupporter.TWLetterOfAuthorization)));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return OrgHeader.RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault();
		}

		OrgHeader OrgHeader
		{
			get
			{
				if (orgHeader == null)
				{
					orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_FullName = "Test tw";
					orgHeader.OH_RL_NKClosestPort = "TWTPE";
					orgHeader.MainAddress.Address1 = "Unit 2020";
					orgHeader.MainAddress.Address2 = "TAI WANG 110";
					orgHeader.MainAddress.City = "TPE";
					orgHeader.MainAddress.Postcode = "2020";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "TW";
					var requiredDocument = orgHeader.RequiredDocuments.AddNew();
					requiredDocument.EQ_DocCategory = Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship;
					requiredDocument.EQ_DocUsage = "BRK";
					requiredDocument.EQ_DocType = "POA";
					requiredDocument.EQ_RN_NKRelatedCountry = "TW";
					requiredDocument.Attributes.DeleteAll();
					requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 6, 10);
					var attrib = requiredDocument.Attributes.AddNew();
					attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
					attrib.D0_AttribDisplayValue = "A";
					attrib = requiredDocument.Attributes.AddNew();
					attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
					attrib.D0_AttribDisplayValue = "111";
					requiredDocument = orgHeader.RequiredDocuments.AddNew();
					requiredDocument.EQ_DocCategory = Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship;
					requiredDocument.EQ_DocUsage = "BRK";
					requiredDocument.EQ_DocType = "POC";
					requiredDocument.EQ_RN_NKRelatedCountry = "TW";
					attrib = requiredDocument.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
					attrib.D0_AttribDisplayValue = "B";
					attrib = requiredDocument.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
					attrib.D0_AttribDisplayValue = "222";
					requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 6, 20);
				}

				return orgHeader;
			}
		}

		OrgHeader orgHeader;
	}
}
