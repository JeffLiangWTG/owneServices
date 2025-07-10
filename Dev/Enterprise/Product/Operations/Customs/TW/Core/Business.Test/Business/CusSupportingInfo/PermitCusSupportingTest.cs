using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PermitCusSupporting))]
	sealed class PermitCusSupportingTest : Customs.Business.Testing.CusSupportingInfoTest<PermitCusSupporting>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PermitCusSupportingCollection.AddNew();
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<PermitCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.PermitNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCSI_ReferenceNumber()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var permitCusSupporting1 = line.PermitCusSupportingCollection.AddNew();
			permitCusSupporting1.CSI_ReferenceNumber = "1";
			permitCusSupporting1.CSI_LineNo = 1;

			var lineListChanged = false;
			((IBindingList)line).ListChanged += new ListChangedEventHandler((e, s) => { lineListChanged = true; });

			permitCusSupporting1.CSI_ReferenceNumber = "2";
			NUnit.Framework.Assert.That(lineListChanged, NUnit.Framework.Is.True, "JobComInvoiceLine refreshed");
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCSI_LineNo()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var permitCusSupportingLineNo1InfoRefreshed = false;
			line.PermitCusSupportingLineNo1Info.ValueChanged += (e, s) => { permitCusSupportingLineNo1InfoRefreshed = true; };
			var permitCusSupportingLineNo2InfoRefreshed = false;
			line.PermitCusSupportingLineNo2Info.ValueChanged += (e, s) => { permitCusSupportingLineNo2InfoRefreshed = true; };
			var permitCusSupportingLineNo3InfoRefreshed = false;
			line.PermitCusSupportingLineNo3Info.ValueChanged += (e, s) => { permitCusSupportingLineNo3InfoRefreshed = true; };
			var permitCusSupportingLineNo4InfoRefreshed = false;
			line.PermitCusSupportingLineNo4Info.ValueChanged += (e, s) => { permitCusSupportingLineNo4InfoRefreshed = true; };
			var permitCusSupportingLineNo5InfoRefreshed = false;
			line.PermitCusSupportingLineNo5Info.ValueChanged += (e, s) => { permitCusSupportingLineNo5InfoRefreshed = true; };

			var permitCusSupporting1 = line.PermitCusSupportingCollection.AddNew();
			permitCusSupporting1.CSI_LineNo = 1;
			var permitCusSupporting2 = line.PermitCusSupportingCollection.AddNew();
			permitCusSupporting2.CSI_LineNo = 2;
			var permitCusSupporting3 = line.PermitCusSupportingCollection.AddNew();
			permitCusSupporting3.CSI_LineNo = 3;
			var permitCusSupporting4 = line.PermitCusSupportingCollection.AddNew();
			permitCusSupporting4.CSI_LineNo = 4;
			var permitCusSupporting5 = line.PermitCusSupportingCollection.AddNew();
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

		[ExpectNoExceptions]
		public void TestShortSequenceNumberLine()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var permitCusSupportingCollection = line.PermitCusSupportingCollection;
			var permitCusSupporting1 = permitCusSupportingCollection.AddNew();
			var permitCusSupporting2 = permitCusSupportingCollection.AddNew();
			var permitCusSupporting3 = permitCusSupportingCollection.AddNew();
			NUnit.Framework.Assert.That(permitCusSupporting1.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting2.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting3.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)3).Using(CustomComparers.TypeComparison));

			permitCusSupporting1.CSI_ItemNumber = 2;
			NUnit.Framework.Assert.That(permitCusSupporting1.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting2.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting3.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)3).Using(CustomComparers.TypeComparison));

			permitCusSupporting3.CSI_ItemNumber = 1;
			NUnit.Framework.Assert.That(permitCusSupporting1.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)3).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting2.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting3.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)1).Using(CustomComparers.TypeComparison));

			permitCusSupportingCollection.RemoveAndDelete(permitCusSupporting3);
			NUnit.Framework.Assert.That(permitCusSupporting1.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(permitCusSupporting2.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)1).Using(CustomComparers.TypeComparison));

			var permitCusSupporting4 = permitCusSupportingCollection.AddNew();
			NUnit.Framework.Assert.That(permitCusSupporting4.CSI_ItemNumber, NUnit.Framework.Is.EqualTo((ZShort)3).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBizoCaptionsAndDescriptions()
		{
			var supporting = (PermitCusSupporting)GetNewBusinessObject();
			CombineAssertions("Captions on this bizo", () =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(supporting.CSI_ReferenceNumberInfo, "Permit No.", "The export/import permit number issued (including pre-allocation) by the controlling agency.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(supporting.CSI_LineNoInfo, "Permit Item Number", "Line No.", "The line numbers of the permit number issued (including pre-allocation) by the controlling agency.");
			});
		}

		protected override IEnumerable<PermitCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var permitCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PermitCusSupportingCollection.AddNew();
			permitCusSupporting.CSI_ReferenceNumber = "1";
			permitCusSupporting.CSI_LineNo = 1;
			yield return permitCusSupporting;
		}
	}
}
