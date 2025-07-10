using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusAddInfoMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var org1 = Factory.New<OrgHeader>();
				org1.OH_FullName = "BOB";
				org1.OH_Code = "AB1@#";
				org1.MainAddress.OA_Address1 = "BOB ADD";
				var org2 = Factory.New<OrgHeader>();
				org2.OH_FullName = "JACK";
				org2.OH_Code = "AB2@#";
				org2.MainAddress.OA_Address1 = "JACK ADD";

				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var invoiceLineCusAddInfoSupporter = (ICusAddInfoTypeSupporter)invoiceLine;
				Type dotType = null;
				invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDOT, out dotType);
				var dot = (CusAddInfo)Factory.New(dotType);
				dot.B7_ParentID = invoiceLine.PK;
				dot.B7_ParentTableCode = invoiceLine.TablePrefix;
				dot.B7_AddInfoData = USDOTAddInfoSchema.Constants.US_DOTCommercialDesc.Substring(3) + "=DESC1234";
				var dotCusAddInfoSupporter = (ICusAddInfoTypeSupporter)dot;
				Type dotVinType = null;
				dotCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDOTVIN, out dotVinType);
				var dotVin = (CusAddInfo)Factory.New(dotVinType);
				dotVin.B7_ParentID = dot.PK;
				dotVin.B7_ParentTableCode = dot.TablePrefix;
				dotVin.B7_AddInfoData = USDOTVINAddInfoSchema.Constants.US_DOTVIN.Substring(3) + "=VN2342";
				Type fdaType = null;
				invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USFDA, out fdaType);
				var fda = (CusAddInfo)Factory.New(fdaType);
				fda.B7_ParentID = invoiceLine.PK;
				fda.B7_ParentTableCode = invoiceLine.TablePrefix;
				fda.B7_AddInfoData = USFDAAddInfoSchema.Constants.US_FDACommercialDesc.Substring(3) + "=FDADESC123";

				Factory.SaveForTesting();
				var writeManager = new DataWritingManager(new ActionInfo(null, declaration));
				var helperMock = new Mock<UniversalDataObjectWriterHelper>(declaration.Factory, declaration.CountryCode);
				helperMock.CallBase = true;

				void CusAddInfo(IOrganizationAddressCollectionParent parent, CusAddInfo cusAddInfo, IDataWritingManager writeManagerArg)
				{
					if (parent != null && cusAddInfo != null)
					{
						switch (cusAddInfo.B7_Type)
						{
							case CusAddInfoTypeAttribute.Codes.USFDA:
								parent.AddOrgAddress(writeManagerArg, org1, DocAddressType.Manufacturer);
								parent.AddOrgAddress(writeManagerArg, org2, DocAddressType.BuyingParty);
								break;
							case CusAddInfoTypeAttribute.Codes.USDOTVIN:
								parent.AddOrgAddress(writeManagerArg, org2, DocAddressType.ConsigneeAddress);
								break;
						}
					}
					return;
				}

				helperMock
					.Protected()
					.Setup("UpdateOrganizationAddressCollectionCore", ItExpr.IsAny<IOrganizationAddressCollectionParent>(), ItExpr.IsAny<CusAddInfo>(), ItExpr.IsAny<IDataWritingManager>())
					.Callback<IOrganizationAddressCollectionParent, CusAddInfo, IDataWritingManager>(CusAddInfo);
				var helper = helperMock.Object;
				AssertNull(AddInfoGroupCollectionCreator.CreateCollection(helper, null, writeManager));
				var collection = AddInfoGroupCollectionCreator.CreateCollection(helper, invoiceLine, writeManager);
				AssertEquals(2, collection.Count);
				var fdaDataObject = collection[0];
				AssertContents(fdaDataObject, GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USFDA, "FDA"), new List<AddInfo>(new[] { new AddInfo() { Key = USFDAAddInfoSchema.Constants.US_FDACommercialDesc.Substring(3), Value = "FDADESC123" } }));
				AssertEquals(2, fdaDataObject.OrganizationAddressCollection.Count);
				AssertNotNull(fdaDataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.Manufacturer) && x.CompanyName.GetValueOrDefault() == "BOB"));
				AssertNotNull(fdaDataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.BuyingParty) && x.CompanyName.GetValueOrDefault() == "JACK"));
				AssertNull(fdaDataObject.AddInfoGroupCollection);

				var dotDataObject = collection[1];
				AssertContents(dotDataObject, GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USDOT, "DOT"), new List<AddInfo>(new[] { new AddInfo() { Key = USDOTAddInfoSchema.Constants.US_DOTCommercialDesc.Substring(3), Value = "DESC1234" } }));
				AssertNull(dotDataObject.OrganizationAddressCollection);
				AssertEquals(1, dotDataObject.AddInfoGroupCollection.Count);
				var dotVinDataObject = dotDataObject.AddInfoGroupCollection[0];
				AssertContents(dotVinDataObject, GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USDOTVIN, "DOT VIN"), new List<AddInfo>(new[] { new AddInfo() { Key = USDOTVINAddInfoSchema.Constants.US_DOTVIN.Substring(3), Value = "VN2342" } }));
				AssertNull(dotVinDataObject.AddInfoGroupCollection);
				AssertEquals(1, dotVinDataObject.OrganizationAddressCollection.Count);
				AssertNotNull(dotVinDataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ConsigneeAddress) && x.CompanyName.GetValueOrDefault() == "JACK"));
			}
		}

		void AssertContents(UniversalCustoms.AddInfoGroup cusAddInfoData, ICodeDescription type, List<AddInfo> addInfos)
		{
			AssertNotNull("Precondition: cusAddInfoData", cusAddInfoData);
			CombineAssertions(delegate
			{
				AssertNotNull("cusAddInfoData.Type", cusAddInfoData.Type);
				AssertEquals("cusAddInfoData.Type.Code", type.Code, cusAddInfoData.Type.Code);
				AssertEquals("cusAddInfoData.Type.Description", type.Description, cusAddInfoData.Type.Description);
				if (addInfos == null)
				{
					AssertNull("cusAddInfoData.AddInfoCollection", cusAddInfoData.AddInfoCollection);
				}
				else
				{
					AssertEquals("cusAddInfoData.AddInfoCollection.Count", addInfos.Count, cusAddInfoData.AddInfoCollection.Count);
					foreach (var addInfo in cusAddInfoData.AddInfoCollection)
					{
						var key = addInfo.Key;
						var value = addInfo.Value;
						var expectedAddInfo = addInfos.FirstOrDefault(x => x.Key == key && x.Value == value);
						AssertNotNull(string.Format("AddInfo data not in expected list (Key='{0}', Value='{1}')", key, value), expectedAddInfo);
						addInfos.Remove(expectedAddInfo);
					}
				}
			});
		}
	}
}
