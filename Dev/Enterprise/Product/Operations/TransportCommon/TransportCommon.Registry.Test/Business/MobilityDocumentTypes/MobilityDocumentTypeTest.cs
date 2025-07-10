using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(MobilityDocumentType))]
	internal class MobilityDocumentTypeTest : RegistryBusinessObjectTemplateTestCase<MobilityDocumentType>
	{
		public void TestRT_PK()
		{
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
			MobilityDocumentType oaDocType = NewPopulatedBusinessObject();
			oaDocType.RT_PK = docType.PK;

			AssertEquals(docType.PK, oaDocType.RT_PK);
		}

		public void TestDocTypeDescription()
		{
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_Desc = "A test doc type";
			MobilityDocumentType oaDocType = NewPopulatedBusinessObject();
			oaDocType.RT_PK = docType.PK;

			AssertEquals("A test doc type", oaDocType.DocTypeDescription);
		}

		public void TestRefDocTypes_WhenDefault_OnlyIncludesTypesOfCategorySCL()
		{
			MobilityDocumentType oaDocType = NewPopulatedBusinessObject();
			var countAll = oaDocType.RefDocTypes.Count;
			int countSCL = oaDocType.RefDocTypes.Count(t => t.RT_ReferenceType == Core.Constants.ReferenceTypes.SupplyChainLogistics);

			AssertNotEquals($"RefDocTypes should contain at least one Reference Type", countAll, 0);
			AssertEquals($"RefDocTypes should only contain Reference Type {Core.Constants.ReferenceTypes.SupplyChainLogistics}", countAll, countSCL);
		}

		public void TestRefDocTypes_HasSameNumberOfItems_AfterSettingRTDocTypeTwice()
		{
			MobilityDocumentType oaDocType = NewPopulatedBusinessObject();
			var countBefore = oaDocType.RefDocTypes.Count;

			oaDocType.RT_DocType = "CAR";
			var countFirst = oaDocType.RefDocTypes.Count;
			oaDocType.RT_DocType = "DOR";
			var countSecond = oaDocType.RefDocTypes.Count;

			AssertEquals(countBefore, countFirst);
			AssertEquals(countBefore, countSecond);
		}

		public void TestValidation()
		{
			MobilityDocumentType oaDocType = NewPopulatedBusinessObject();
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();

			oaDocType.RunPreSaveValidation();
			AssertHasErrors("Mandatory", oaDocType.RT_PKInfo);

			oaDocType.RT_PK = ZGuid.Invalid;
			AssertHasErrors("Should have errors when RT_PK is invalid", oaDocType.RT_PKInfo);

			oaDocType.RT_PK = docType.PK;
			AssertHasErrors("Should have errors when RT_PK is valid, but not a SCL RefDocType", oaDocType.RT_PKInfo);

			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			oaDocType.ClearAllNotifications();
			oaDocType.ValidateRT_PK();
			AssertNoErrors("Should not have errors when MobilityDocumentType is a valid SCL RefDocType", oaDocType.RT_PKInfo);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override MobilityDocumentType GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override MobilityDocumentType GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		MobilityDocumentType NewPopulatedBusinessObject()
		{
			MobilityDocumentType result = new MobilityDocumentType(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MobilityDocumentType();
		}
	}
}
