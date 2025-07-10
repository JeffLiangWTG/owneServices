using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PackingGroup))]
sealed class PackingGroupTest : EU.Business.Declaration.Testing.TestPackingGroup
{
	public void TestShouldCopyDeclarationTotalNoOfPacksCore()
	{
		var declaration = base.Factory.New<JobDeclaration>();
		var packageGroup = Factory.New<PackingGroupForTest>();
		packageGroup.Declaration = declaration;

		CombineAssertions(() =>
		{
			var package = packageGroup.Packages.AddNew();
			package.CW_PackQty = 0;

			package.CW_PackType = "VQ";
			AssertEquals("Bulk Code package", false, packageGroup.ShouldCopyDeclarationTotalNoOfPacksCoreExposed);

			package.CW_PackType = "AA";
			AssertEquals("Not Bulk Code package", true, packageGroup.ShouldCopyDeclarationTotalNoOfPacksCoreExposed);

			package.CW_PackQty = 1;
			AssertEquals("Amount > 0", false, packageGroup.ShouldCopyDeclarationTotalNoOfPacksCoreExposed);
		});
	}

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			result.CustomsEntryInstructions.AddNew();
			result.Invoices.AddNew().InvoiceLines.AddNew();
			return result;
		}
	}

	class PackingGroupForTest : PackingGroup
	{
		public PackingGroupForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldCopyDeclarationTotalNoOfPacksCoreExposed => ShouldCopyDeclarationTotalNoOfPacksCore;
	}
}
