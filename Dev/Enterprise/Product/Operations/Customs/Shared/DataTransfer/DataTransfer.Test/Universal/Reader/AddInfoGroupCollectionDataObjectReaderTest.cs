using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusAddInfoDeletionAndAddition()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var declarationCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)declaration;
			Type type = null;
			declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USOGADisposition, out type);
			AssertNotNull(type);
			var ogaDispositionAddInfo = USOGADispositionDataAddInfoSchema.Constants.US_Code.Substring(3) + "=I3";
			Type itdocType = null;
			declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITDoc, out itdocType);
			var itdocAddInfo = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=AREA1234";
			var itdoc = (CusAddInfo)Factory.New(itdocType);
			itdoc.B7_AddInfoData = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=BYE";
			itdoc.B7_ParentID = declaration.PK;
			itdoc.B7_ParentTableCode = declaration.TablePrefix;
			Type deliveryOrderType = null;
			declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader, out deliveryOrderType);
			var deliveryOrder = (CusAddInfo)Factory.New(deliveryOrderType);
			deliveryOrder.B7_AddInfoData = USDeliveryOrderHeaderAddInfoSchema.Constants.US_OrderReference.Substring(3) + "=BYE";
			deliveryOrder.B7_ParentID = declaration.PK;
			deliveryOrder.B7_ParentTableCode = declaration.TablePrefix;
			Type linkedEntryType = null;
			declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USLinkedEntry, out linkedEntryType);
			var linkedEntry = (CusAddInfo)Factory.New(linkedEntryType);
			linkedEntry.B7_AddInfoData = USLinkedEntryAddInfoSchema.Constants.US_LE_EntryNumber.Substring(3) + "=BYE";
			linkedEntry.B7_ParentID = declaration.PK;
			linkedEntry.B7_ParentTableCode = declaration.TablePrefix;

			var itdocDataObject1 = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USITDoc },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(itdocAddInfo)
			};
			var linkedEntryDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USLinkedEntry }
			};
			var ogaDispositionDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USOGADisposition },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(ogaDispositionAddInfo)
			};
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>(new[]
			{
				itdocDataObject1, ogaDispositionDataObject, linkedEntryDataObject
				}));
			var reader = new AddInfoGroupCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates));
			var cusAddInfoBOs = reader.ReadIntoDataRows(declaration.PK, declaration.TablePrefix, declaration.IsInDatabase, shipmentData);

			AssertNotNull(cusAddInfoBOs);

			CombineAssertions(delegate
			{
				AssertEquals("should be deleted as it was specified in xml", true, itdoc.IsDeleted);
				AssertEquals("should be deleted as it was specified in xml", true, linkedEntry.IsDeleted);
				AssertEquals("should be not deleted as it was not specified in xml", false, deliveryOrder.IsDeleted);
				AssertEquals("cusAddInfoBOs", 2, cusAddInfoBOs.Length);
				var itDocBO = cusAddInfoBOs[0];
				var ogaDispositionBO = cusAddInfoBOs[1];
				if (CusAddInfoTypeAttribute.Codes.USITDoc.Equals(ogaDispositionBO.GetValue(CusAddInfoSchema.B7_Type)))
				{
					itDocBO = cusAddInfoBOs[1];
					ogaDispositionBO = cusAddInfoBOs[0];
				}
				AssertCusAddInfoContents(itDocBO, declaration.TablePrefix, declaration.PK, CusAddInfoTypeAttribute.Codes.USITDoc, partialAddInfoData: itdocAddInfo);
				AssertCusAddInfoContents(ogaDispositionBO, declaration.TablePrefix, declaration.PK, CusAddInfoTypeAttribute.Codes.USOGADisposition, partialAddInfoData: ogaDispositionAddInfo);

				var declarationCusAddInfos = LoadCusAddInfo(declaration.TablePrefix, declaration.PK);
				AssertEquals("declarationCusAddInfos.Length", 3, declarationCusAddInfos.Length);
				AssertEquals("deliveryOrder matched", ((IBusinessObjectInternals)deliveryOrder).Row, declarationCusAddInfos.FirstOrDefault(x => x.GetValue(CusAddInfoSchema.PK) == deliveryOrder.PK));
				AssertEquals("itDocBO matched", itDocBO, declarationCusAddInfos.FirstOrDefault(x => x.GetValue(CusAddInfoSchema.PK) == itDocBO.GetValue(CusAddInfoSchema.PK)));
				AssertEquals("ogaDispositionBO matched", ogaDispositionBO, declarationCusAddInfos.FirstOrDefault(x => x.GetValue(CusAddInfoSchema.PK) == ogaDispositionBO.GetValue(CusAddInfoSchema.PK)));
			});
		}

		public void TestCusAddInfoFieldMappingsForUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry((Core.Constants.CountryCodes.UnitedStates)))
			{
				var provider = Factory.BOFactory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.UnitedStates);
				var list = provider.TableSpecificCusAddInfoTypeList(JobDeclarationSchema.Constants.Prefix, "");
				Assert("TableSpecificCusAddInfoTypeList for US Declaration should exist", list.Count > 0);
				var type = "USI";
				AssertEquals("USI should be a valid code for US Declaration TableSpecificCusCodeDataTypeList; if not then please update the test with a valid code", true, list.ContainsCode(type));
				var unknownType = "S!D";
				AssertEquals(false, list.ContainsCode(unknownType));
				var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
				var validAddinfoString = string.Format("{0}={1}", USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3), "HELLO WORLD");
				var addinfos = SetupAddInfos(validAddinfoString + "*DG2=23");
				var addInfo1 = SetupAddInfoGroup(new CodeDescriptionPair() { Code = type }, addinfos);
				var addInfo2 = SetupAddInfoGroup(new CodeDescriptionPair() { Code = unknownType }, addinfos);
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				var header = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[] { addInfo1, addInfo2 })
				};
				var cusAddInfoBOs = new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(declaration.PK, JobDeclarationSchema.Constants.Prefix, declaration.IsInDatabase, header);

				AssertNotNull(cusAddInfoBOs);

				CombineAssertions(delegate
				{
					AssertEquals("CusAddInfo", 1, cusAddInfoBOs.Length);
					var cusAddInfoBO = cusAddInfoBOs[0];
					AssertCusAddInfoContents(cusAddInfoBO, JobDeclarationSchema.Constants.Prefix, declaration.PK, type, validAddinfoString);
				});
			}
		}

		public void TestExistingAddInfosDeleted()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var billCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)bill;
			Type itnType = null;
			billCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITNumber, out itnType);

			var cusAddInfoOne = (CusAddInfo)Factory.New(itnType);
			cusAddInfoOne.B7_ParentID = bill.PK;
			cusAddInfoOne.B7_ParentTableCode = "CU";
			cusAddInfoOne.B7_Type = "ITN";
			cusAddInfoOne.B7_AddInfoData = "ITNumber=111111114*NoOfPacks=20";

			var cusAddInfoTwo = (CusAddInfo)Factory.New(itnType);
			cusAddInfoTwo.B7_ParentID = bill.PK;
			cusAddInfoTwo.B7_ParentTableCode = "CU";
			cusAddInfoTwo.B7_Type = "ITN";
			cusAddInfoTwo.B7_AddInfoData = "ITNumber=222222221*NoOfPacks=30";

			Factory.SaveForTesting();

			var additionalBillCollection = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance);
			additionalBillCollection.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>()
			{
				new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "ITN" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "ITNumber", Value = "123456782" }
					}
				},
				new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "ITN" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "ITNumber", Value = "234567804" }
					}
				}
			});

			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var cusAddInfoBOs = new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(bill.PK, CusDecHouseBillSchema.Constants.Prefix, bill.IsInDatabase, additionalBillCollection);
			Assert(cusAddInfoOne.IsDeleted);
			Assert(cusAddInfoTwo.IsDeleted);
			AssertEquals(2, cusAddInfoBOs.Length);
		}
	}
}
