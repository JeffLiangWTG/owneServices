using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusSupportingInfoDeletionAndAddition()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				declaration.JE_MasterBill = "MYMASTER";
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				var declarationCusSupportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)declaration;
				var declarationCusSupportingInfoTypes = declarationCusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes();
				var supportingDocumentType = declarationCusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument];
				var supportingDocument = (CusSupportingInfo)Factory.New(supportingDocumentType);
				supportingDocument.CSI_ParentID = declaration.PK;
				supportingDocument.CSI_ParentTableCode = declaration.TablePrefix;
				supportingDocument.CSI_ReferenceNumber = "BYE";

				var additionalInfoType = declarationCusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo];
				var additionalInfo = (CusSupportingInfo)Factory.New(additionalInfoType);
				additionalInfo.CSI_ParentID = declaration.PK;
				additionalInfo.CSI_ParentTableCode = declaration.TablePrefix;
				additionalInfo.CSI_ReferenceNumber = "BYE";

				var previousDocumentType = declarationCusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument];
				var previousDocument = (CusSupportingInfo)Factory.New(previousDocumentType);
				previousDocument.CSI_ParentID = declaration.PK;
				previousDocument.CSI_ParentTableCode = declaration.TablePrefix;
				previousDocument.CSI_ReferenceNumber = "BYE";

				var cusSupportingInfoUknown = Factory.New<CusSupportingInfo>();
				cusSupportingInfoUknown.CSI_Type = "AGA";
				cusSupportingInfoUknown.CSI_ParentID = declaration.PK;
				cusSupportingInfoUknown.CSI_ParentTableCode = declaration.TablePrefix;
				cusSupportingInfoUknown.CSI_ReferenceNumber = "BYE";

				var supportingDocumentDataObject1 = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
					ReferenceNumber = "HI2"
				};
				var additionalInfoDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo }
				};
				var previousDocumentDataObject = new UniversalCustoms.CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
					ReferenceNumber = "HI1"
				};
				var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentData.SetCustomsSupportingInformationCollection(() => new List<UniversalCustoms.CustomsSupportingInformation>(new[]
					{
						supportingDocumentDataObject1, previousDocumentDataObject, additionalInfoDataObject
					}));
				var reader = new CustomsSupportingInformationCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom));
				var cusSupportingInfoBOs = reader.ReadIntoDataRows(declaration.PK, declaration.TablePrefix, declaration.IsInDatabase, shipmentData);

				AssertNotNull(cusSupportingInfoBOs);

				#region Check Contents of Business Object

				CombineAssertions(delegate
				{
					AssertEquals("should be deleted as it was specified in xml", true, additionalInfo.IsDeleted);
					AssertEquals("should be not deleted as it was not specified in xml", false, cusSupportingInfoUknown.IsDeleted);
					AssertEquals("cusSupportingInfoBOs", 2, cusSupportingInfoBOs.Length);
					var supportingDocumentBO = cusSupportingInfoBOs[0];
					var previousDocumentBO = cusSupportingInfoBOs[1];
					if (Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument.Equals(previousDocumentBO.GetValue(CusSupportingInfoSchema.CSI_Type)))
					{
						supportingDocumentBO = cusSupportingInfoBOs[1];
						previousDocumentBO = cusSupportingInfoBOs[0];
					}
					AssertCusSupportingInfoContents(supportingDocumentBO, declaration.TablePrefix, declaration.PK, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, referenceNumber: "HI2");
					AssertCusSupportingInfoContents(previousDocumentBO, declaration.TablePrefix, declaration.PK, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, referenceNumber: "HI1");

					var declarationCusSupportingInfos = LoadCusSupportingInfo(declaration.TablePrefix, declaration.PK);
					AssertEquals("declarationCusSupportingInfos.Length", 3, declarationCusSupportingInfos.Length);
					AssertEquals("supportingDocument matched", supportingDocumentBO, declarationCusSupportingInfos.FirstOrDefault(x => x.GetValue(CusSupportingInfoSchema.PK) == supportingDocumentBO.GetValue(CusSupportingInfoSchema.PK)));
					AssertEquals("previousDocument matched", previousDocumentBO, declarationCusSupportingInfos.FirstOrDefault(x => x.GetValue(CusSupportingInfoSchema.PK) == previousDocumentBO.GetValue(CusSupportingInfoSchema.PK)));
					AssertEquals("cusSupportingInfoUknown matched", ((IBusinessObjectInternals)cusSupportingInfoUknown).Row, declarationCusSupportingInfos.FirstOrDefault(x => x.GetValue(CusSupportingInfoSchema.PK) == cusSupportingInfoUknown.PK));
				});

				#endregion
			}
		}
	}
}
