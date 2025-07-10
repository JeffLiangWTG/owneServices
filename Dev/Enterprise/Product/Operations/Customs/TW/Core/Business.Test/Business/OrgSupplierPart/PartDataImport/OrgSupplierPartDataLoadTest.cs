using System.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	sealed class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new OrgSupplierPartDataLoad();
		}

		public void TestImportingCountrySpecificFields()
		{
			var dataLoad = new OrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code   ,Description,ClassificationType ,Tariff     ,Supplier   ,LocalPartNumber ,LocalPartDescription,UnitPrice,UnitPriceCurrency ,ClassificationDescription,ChineseDescription,UnitOfMeasure,AssignedNumber1,AssignedNumber2,AssignedNumber3,AssignedNumber4,AssignedNumber5,AssignedNumber6,AssignedNumber7,AssignedNumber8,AssignedNumber9,AssignedNumber10,PermitNumber1,PermitLineNo1,PermitNumber2,PermitLineNo2,PermitNumber3,PermitLineNo3,PermitNumber4,PermitLineNo4,PermitNumber5,PermitLineNo5");
					sw.WriteLine("PoolCue,Pool Cue   ,HTI                ,89080020003,ABIGAS     ,1112            ,1345                ,124      ,TWD               ,English Des. Test        ,Chinese Des. Test ,ACR          ,A1             ,A2             ,A3             ,A4             ,A5             ,A6             ,A7             ,A8             ,A9             ,A10             ,P1           ,1            ,P2           ,2            ,P3           ,3            ,P4           ,4            ,P5           ,5            ");
				}

				dataLoad.ImportProductData(tempFile.Filename, false, false);
				AssertEquals("Records Created", 1, dataLoad.RunCounters.RecsCreated);
				var supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				var product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("PoolCue", null, supplier);
				var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				CombineAssertions(() =>
				{
					AssertEquals("CI_Description must be", "English Des. Test", importPivot.CI_Description);
					AssertEquals("CI_NDescription must be", "Chinese Des. Test", importPivot.CI_NDescription);
					AssertEquals("CI_PartPivotUOM must be", "ACR", importPivot.CI_PartPivotUOM);
				});

				var assignedCusClassPartPivotRefs = importPivot.AssignedCusClassPartPivotRefCollection;
				CombineAssertions(() =>
				{
					AssertEquals("assignedCusClassPartPivotRefs count", 10, assignedCusClassPartPivotRefs.Count);
					AssertContainsExactElementsInAnyOrder("CIR_ReferenceNumber List", new[] { "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "A10" }, assignedCusClassPartPivotRefs.Select(c => c.CIR_ReferenceNumber));
				});

				var permits = importPivot.ProductPermitCusSupportingCollection;
				CombineAssertions(() =>
				{
					AssertEquals("permits count", 5, permits.Count);
					AssertContainsExactElementsInAnyOrder("CSI_ReferenceNumber List", new[] { "P1", "P2", "P3", "P4", "P5" }, permits.Select(c => c.CSI_ReferenceNumber));
					AssertContainsExactElementsInAnyOrder("CSI_LineNo List", new ZInt[] { 1, 2, 3, 4, 5 }, permits.Select(c => c.CSI_LineNo));
				});

				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code    ,Description,ClassificationType ,Tariff     ,Supplier   ,LocalPartNumber ,LocalPartDescription,UnitPrice,UnitPriceCurrency ,ClassificationDescription,ChineseDescription,UnitOfMeasure,AssignedNumber1,AssignedNumber2,AssignedNumber3,AssignedNumber4,AssignedNumber5,AssignedNumber6,AssignedNumber7,AssignedNumber8,AssignedNumber9,AssignedNumber10,PermitNumber1,PermitLineNo1,PermitNumber2,PermitLineNo2,PermitNumber3,PermitLineNo3,PermitNumber4,PermitLineNo4,PermitNumber5,PermitLineNo5");
					sw.WriteLine("PoolCue1,Pool Cue1  ,HTI                ,89080020003,ABIGAS     ,1112            ,1345                ,124      ,TWD               ,English Des. Test        ,Chinese Des. Test ,ACR          ,A11            ,A11            ,A11            ,A33            ,A33            ,A33            ,A33            ,A44            ,A44            ,A44             ,P11          ,1            ,P11          ,2            ,P11           ,1            ,P4           ,4            ,P5           ,5            ");
				}

				dataLoad.ImportProductData(tempFile.Filename, false, false);
				AssertEquals("Records Created", 1, dataLoad.RunCounters.RecsCreated);
				product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("PoolCue1", null, supplier);
				importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				assignedCusClassPartPivotRefs = importPivot.AssignedCusClassPartPivotRefCollection;
				CombineAssertions(() =>
				{
					AssertEquals("assignedCusClassPartPivotRefs count", 3, assignedCusClassPartPivotRefs.Count);
					AssertContainsExactElementsInAnyOrder("CIR_ReferenceNumber List", new[] { "A11", "A33", "A44" }, assignedCusClassPartPivotRefs.Select(c => c.CIR_ReferenceNumber));
				});

				permits = importPivot.ProductPermitCusSupportingCollection;
				CombineAssertions(() =>
				{
					AssertEquals("permits count", 4, permits.Count);
					AssertContainsExactElementsInAnyOrder("CSI_ReferenceNumber List", new[] { "P11", "P11", "P4", "P5" }, permits.Select(c => c.CSI_ReferenceNumber));
					AssertContainsExactElementsInAnyOrder("CSI_LineNo List", new ZInt[] { 1, 2, 4, 5 }, permits.Select(c => c.CSI_LineNo));
				});
			}
		}
	}
}
