using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class NctsCusCodeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void IUCCExportCodeListDetailMembers()
		{
			IUCCExportCodeListDetail codeListDetail = new NctsCusCodeType();

			Assert.Multiple(() =>
			{
				Assert.That(codeListDetail.CodeType, Is.EqualTo("CL016"));
				Assert.That(codeListDetail.CodeListType, Is.EqualTo("CUSCode"));
				Assert.That(codeListDetail.DataSource, Is.EqualTo("CUSCode"));
				Assert.That(codeListDetail.XmlDataItemForCode, Is.EqualTo("CUSCode"));
			});
		}

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsCusCodeType();
	}
}
