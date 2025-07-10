using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V3.Business.Testing
{
	[TestedType(typeof(V3JobDeclaration))]
	public class V3JobDeclarationTest : BaseJobDeclarationAbstractTest
	{
		public void TestReadonly()
		{
			AssertEquals(true, V3JobDeclaration.ReadOnly);
		}

		public void TestMessages()
		{
			Factory.New<V3Message>();
			Factory.New<V3Message>();
			Factory.New<V3Message>();
			V3Message message = Factory.New<V3Message>();
			message.EM_LinkUniqueID = V3JobDeclaration.PK;
			AssertEquals(1, V3JobDeclaration.Messages.Count);
		}

		public void TestPermitNumber()
		{
			CusEntryNumber permitNumber = Factory.New<CusEntryNumber>();
			permitNumber.CE_EntryNum = "PERMIT";
			permitNumber.CE_EntryType = "PMT";
			AssertEquals("", V3JobDeclaration.PermitNumber);
			permitNumber.CE_ParentID = V3JobDeclaration.PK;
			AssertEquals("PERMIT", V3JobDeclaration.PermitNumber);
		}

		public void TestImporters()
		{
			Assert(V3JobDeclaration.Importers is OrgHeaderCollection);
		}

		public void TestExporters()
		{
			Assert(V3JobDeclaration.Exporters is OrgHeaderCollection);
		}

		#region implementation
		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("Branch will be loaded in JE_EntryStatus.", true);
		}

		protected override BusinessObject GetNewBusinessObject() => V3JobDeclaration;
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.BaseJobDeclaration);
		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new JobDeclarationLightValidationTester(bizObjToTest);
		}

		class JobDeclarationLightValidationTester : LightValidationTester
		{
			public JobDeclarationLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != "E2_ParentID" && propertyName != "E2_AddressOverride" && propertyName != "E2_ParentTableCode" && propertyName != "E2_AddressType" && propertyName != "E2_OA_Address";
			}
		}

		V3JobDeclaration V3JobDeclaration => v3JobDeclaration ?? (v3JobDeclaration = Factory.New<V3JobDeclaration>());
		V3JobDeclaration v3JobDeclaration;
		#endregion
	}
}
