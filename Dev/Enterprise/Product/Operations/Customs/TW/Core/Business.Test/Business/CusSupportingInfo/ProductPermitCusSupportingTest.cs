using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ProductPermitCusSupporting))]
	sealed class ProductPermitCusSupportingTest : Customs.Business.Testing.CusSupportingInfoTest<ProductPermitCusSupporting>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew().ProductPermitCusSupportingCollection.AddNew();
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ProductPermitCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.PermitNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(CusClassPartPivotSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCSI_ReferenceNumber()
		{
			var productPermitCusSupportingCollection = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew().ProductPermitCusSupportingCollection;
			var supporting = productPermitCusSupportingCollection.AddNew();
			supporting.CSI_ReferenceNumber = "1";
			supporting.CSI_LineNo = 1;

			var lineListChanged = false;
			((IBindingList)supporting).ListChanged += new ListChangedEventHandler((e, s) => { lineListChanged = true; });

			supporting.CSI_ReferenceNumber = "2";
			NUnit.Framework.Assert.That(lineListChanged, NUnit.Framework.Is.True, "JobComInvoiceLine refreshed");
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCSI_LineNo()
		{
			var pivot = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew();

			var permitCusSupportingLineNo1InfoRefreshed = false;
			pivot.PermitCusSupportingLineNo1Info.ValueChanged += (e, s) => { permitCusSupportingLineNo1InfoRefreshed = true; };
			var permitCusSupportingLineNo2InfoRefreshed = false;
			pivot.PermitCusSupportingLineNo2Info.ValueChanged += (e, s) => { permitCusSupportingLineNo2InfoRefreshed = true; };
			var permitCusSupportingLineNo3InfoRefreshed = false;
			pivot.PermitCusSupportingLineNo3Info.ValueChanged += (e, s) => { permitCusSupportingLineNo3InfoRefreshed = true; };
			var permitCusSupportingLineNo4InfoRefreshed = false;
			pivot.PermitCusSupportingLineNo4Info.ValueChanged += (e, s) => { permitCusSupportingLineNo4InfoRefreshed = true; };
			var permitCusSupportingLineNo5InfoRefreshed = false;
			pivot.PermitCusSupportingLineNo5Info.ValueChanged += (e, s) => { permitCusSupportingLineNo5InfoRefreshed = true; };

			var permitCusSupporting1 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permitCusSupporting1.CSI_LineNo = 1;
			var permitCusSupporting2 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permitCusSupporting2.CSI_LineNo = 2;
			var permitCusSupporting3 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permitCusSupporting3.CSI_LineNo = 3;
			var permitCusSupporting4 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permitCusSupporting4.CSI_LineNo = 4;
			var permitCusSupporting5 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permitCusSupporting5.CSI_LineNo = 5;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(permitCusSupportingLineNo1InfoRefreshed, NUnit.Framework.Is.True, "permitCusSupportingLineNo1InfoRefreshed");
				NUnit.Framework.Assert.That(permitCusSupportingLineNo2InfoRefreshed, NUnit.Framework.Is.True, "permitCusSupportingLineNo2InfoRefreshed");
				NUnit.Framework.Assert.That(permitCusSupportingLineNo3InfoRefreshed, NUnit.Framework.Is.True, "permitCusSupportingLineNo3InfoRefreshed");
				NUnit.Framework.Assert.That(permitCusSupportingLineNo4InfoRefreshed, NUnit.Framework.Is.True, "permitCusSupportingLineNo4InfoRefreshed");
				NUnit.Framework.Assert.That(permitCusSupportingLineNo5InfoRefreshed, NUnit.Framework.Is.True, "permitCusSupportingLineNo5InfoRefreshed");
			});
		}

		protected override IEnumerable<ProductPermitCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			var productPermitCusSupporting = part.PivotsForBinding.AddNew().ProductPermitCusSupportingCollection.AddNew();
			productPermitCusSupporting.CSI_ReferenceNumber = "1";
			productPermitCusSupporting.CSI_LineNo = 1;
			yield return productPermitCusSupporting;
		}
	}
}
