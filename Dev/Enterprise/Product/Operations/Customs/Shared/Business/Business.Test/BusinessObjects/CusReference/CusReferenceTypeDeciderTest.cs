using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusReferenceTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadByType()
		{
			var bizObj = Factory.New<CusReference>();
			var row = ((INeedRow)bizObj).Row;
			var typeDecider = new CusReferenceTypeDecider();
			CombineAssertions(() =>
			{
				foreach ((string typeCode, Type expectedType) in new[]
				{
					(CusReferenceTypeList.Codes.NctsAuthorization, ObjectFactory.GetType<Integration.Customs.IT.INctsAuthorization>()),
					(CusReferenceTypeList.Codes.ComprehensiveValuations, ObjectFactory.GetType<Integration.Customs.JP.IComprehensiveValuation>()),
					(CusReferenceTypeList.Codes.OtherLawReference, ObjectFactory.GetType<Integration.Customs.JP.IOtherLawReference>()),
					(CusReferenceTypeList.Codes.GuaranteeReference, ObjectFactory.GetType<Integration.Customs.JP.IGuaranteeReference>())
				})
				{
					bizObj.CFR_Type = typeCode;
					AssertEquals(typeCode, expectedType, typeDecider.GetTypeForLoad(row, Factory));
				}
			});
		}

		public void TestGetTypeForLoadByParent()
		{
			var declaration = Factory.New<Integration.Customs.EU.IJobDeclaration>() as BaseJobDeclaration;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var bizObj = Factory.New<CusReference>();
			bizObj.CFR_ParentID = instruction.PK;
			bizObj.CFR_ParentTableCode = instruction.TablePrefix;
			var row = ((INeedRow)bizObj).Row;
			var typeDecider = new CusReferenceTypeDecider();

			bizObj.CFR_Type = CusReferenceTypeList.Codes.SupplyChainActor;
			AssertEquals(bizObj.CFR_Type, ObjectFactory.GetType<Integration.Customs.EU.ICusSupplyChainActorReference>(), typeDecider.GetTypeForLoad(row, Factory));

			bizObj.CFR_Type = CusReferenceTypeList.Codes.FiscalReference;
			AssertEquals(bizObj.CFR_Type, ObjectFactory.GetType<Integration.Customs.EU.ICusFiscalReference>(), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForLoadDefault()
		{
			var bizObj = Factory.New<CusReference>();
			var row = ((INeedRow)bizObj).Row;
			var typeDecider = new CusReferenceTypeDecider();
			bizObj.CFR_Type = "XXX";
			bizObj.CFR_ParentTableCode = "JZ";
			AssertEquals("Type decided", typeof(CusReference), typeDecider.GetTypeForLoad(row, Factory));
			AssertEquals("Cannot determine the CusReferenceTypeSupporter object, because parent could not be determined (Type: XXX, TableCode: JZ)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			bizObj.CFR_ParentID = Factory.New<BaseJobComInvoiceHeader>().PK;
			AssertEquals("Type decided", typeof(CusReference), typeDecider.GetTypeForLoad(row, Factory));
			AssertEquals("Either Enterprise.Customs.Business.BaseJobComInvoiceHeader has not implement Enterprise.Customs.Business.ICusReferenceTypeSupporter or is missing a support for Type 'XXX'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
