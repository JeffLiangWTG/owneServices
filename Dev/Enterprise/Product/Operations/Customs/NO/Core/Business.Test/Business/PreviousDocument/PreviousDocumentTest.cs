using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(PreviousDocument))]
sealed class PreviousDocumentTest : CusSupportingInfoTest<PreviousDocument>
{
	public void TestCSI_CodeAttributes() => CombineAssertions(() =>
	{
		AssertEntity<PreviousDocument>()
			.HasProperty(p => p.CSI_Code)
			.WithCaption("Type");
	});

	public void TestCSI_ReferenceNumber_Attributes() => CombineAssertions(() =>
	{
		AssertEntity<PreviousDocument>()
			.HasProperty(p => p.CSI_ReferenceNumber)
			.WithCaption("Reference")
			.WithAttribute<MaxLengthAttribute>(m => m.MaxLength == 35);
	});

	public void TestCSI_LineNo_Attributes() => CombineAssertions(() =>
	{
		AssertEntity<PreviousDocument>()
			.HasProperty(p => p.CSI_LineNo)
			.WithCaption("Line No.");
	});

	public void TestCSI_Quantity_Attributes() => CombineAssertions(() =>
	{
		AssertEntity<PreviousDocument>()
			.HasProperty(p => p.CSI_Quantity)
			.WithCaption("Package Qty.");
	});

	public void TestCSI_ReferenceNumber2_Attributes() => CombineAssertions(() =>
	{
		AssertEntity<PreviousDocument>()
			.HasProperty(p => p.CSI_ReferenceNumber2)
			.WithCaption("Reference 2")
			.WithAttribute<MaxLengthAttribute>(m => m.MaxLength == 35);
	});

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject()
		=> Factory.New<JobDeclaration>()
			.CustomsEntryInstructions.AddNew()
			.PreviousDocuments.AddNew();

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		yield return entryInstruction.PreviousDocuments.AddNew();
	}
}
