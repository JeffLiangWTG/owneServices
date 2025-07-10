using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestBasicCusAddInfoLevelFieldMappings()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCusAddInfoSupporter = (ICusAddInfoTypeSupporter)invoiceLine;
			Type dotType = null;
			invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDOT, out dotType);
			var dot = (CusAddInfo)Factory.New(dotType);
			dot.B7_ParentID = invoiceLine.PK;
			dot.B7_ParentTableCode = invoiceLine.TablePrefix;
			var dotCusAddInfoSupporter = (ICusAddInfoTypeSupporter)dot;
			Type dotVINType = null;
			dotCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDOTVIN, out dotVINType);
			AssertNotNull(dotVINType);
			Type fdaType = null;
			invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USFDA, out fdaType);
			var fda = (CusAddInfo)Factory.New(fdaType);
			fda.B7_ParentID = invoiceLine.PK;
			fda.B7_ParentTableCode = invoiceLine.TablePrefix;
			var fdaCusCodeDataSupporter = (ICusCodeDataTypeSupporter)fda;
			var affirmationCodeString = "AFM";
			var affirmationBOLString = "BOL";
			Type affirmationCodeType = null;
			fdaCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(affirmationCodeString, out affirmationCodeType);
			AssertNotNull(affirmationCodeType);
			dot.Delete();
			fda.Delete();

			var dotAddInfo = USDOTAddInfoSchema.Constants.US_DOTCommercialDesc.Substring(3) + "=DESC1234";
			var dotvinAddInfo = USDOTVINAddInfoSchema.Constants.US_DOTVIN.Substring(3) + "=VIN1234";
			var dotDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDOT },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(dotAddInfo),
				AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[]
				{
					new UniversalCustoms.AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDOTVIN },
						AddInfoCollection = AddInfoCollectionCreator.CreateCollection(dotvinAddInfo)
					}
				})
			};
			var fdaAddInfo = USFDAAddInfoSchema.Constants.US_FDACommercialDesc.Substring(3) + "=FDADESC123";
			var fdaDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USFDA },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(fdaAddInfo),
				CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
				{
					new UniversalCustoms.CustomsReference()
					{
						Type = new CodeDescriptionPair() { Code = affirmationCodeString },
						SubType = new CodeDescriptionPair35Char() { Code = affirmationBOLString },
						Reference = "ABCD12345"
					}
				})
			};
			var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine()
			{
				AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[] { dotDataObject, fdaDataObject })
			};
			var cusAddInfoBOs = new AddInfoGroupCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates)).ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, invoiceLineDataObject);

			AssertNotNull(cusAddInfoBOs);

			CombineAssertions(delegate
			{
				AssertEquals("CusAddInfo", 2, cusAddInfoBOs.Length);
				var dotBO = cusAddInfoBOs[0];
				var fdaBO = cusAddInfoBOs[1];
				if (CusAddInfoTypeAttribute.Codes.USDOT.Equals(fdaBO[CusAddInfoSchema.Constants.B7_Type]))
				{
					dotBO = cusAddInfoBOs[1];
					fdaBO = cusAddInfoBOs[0];
				}
				AssertCusAddInfoContents(dotBO, invoiceLine.TablePrefix, invoiceLine.PK, CusAddInfoTypeAttribute.Codes.USDOT, partialAddInfoData: dotAddInfo);
				var childCusAddInfoBOs = LoadCusAddInfo(CusAddInfoSchema.Constants.Prefix, dotBO.GetValue(CusAddInfoSchema.PK));
				AssertEquals("dotBO's CusAddInfos", 1, childCusAddInfoBOs.Length);
				var dotvinBO = childCusAddInfoBOs[0];
				AssertCusAddInfoContents(dotvinBO, CusAddInfoSchema.Constants.Prefix, dotBO.GetValue(CusAddInfoSchema.PK), CusAddInfoTypeAttribute.Codes.USDOTVIN, partialAddInfoData: dotvinAddInfo);
				AssertCusAddInfoContents(fdaBO, invoiceLine.TablePrefix, invoiceLine.PK, CusAddInfoTypeAttribute.Codes.USFDA, partialAddInfoData: fdaAddInfo);
				var childCusCodeDataBOs = LoadCusCodeData(CusAddInfoSchema.Constants.Prefix, fdaBO.GetValue(CusAddInfoSchema.PK));
				AssertEquals("Child CusCodeDatas", 1, childCusCodeDataBOs.Length);
				var affirmationCodeBO = childCusCodeDataBOs[0];
				AssertCusCodeDataContents(affirmationCodeBO, CusAddInfoSchema.Constants.Prefix, fdaBO.GetValue(CusAddInfoSchema.PK), affirmationCodeString, affirmationBOLString, "ABCD12345", ZBool.False, ZShort.Zero);
			});
		}
	}
}
