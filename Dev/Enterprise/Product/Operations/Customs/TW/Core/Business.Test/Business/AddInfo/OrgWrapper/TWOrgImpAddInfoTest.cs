using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWOrgImpAddInfo))]
	sealed class TWOrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestZO_TWHideEXPBuyerZHTAddr_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(orgImpAddInfo.ZO_TWHideEXPBuyerZHTAddrInfo).Caption, NUnit.Framework.Is.EqualTo("Hide EXP Buyer Trad. Chinese Addr."), "Caption");
		}

		[ExpectNoExceptions]
		public void TestZO_TWHideEXPBuyerAddr_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(orgImpAddInfo.ZO_TWHideEXPBuyerAddrInfo).Caption, NUnit.Framework.Is.EqualTo("Hide EXP Buyer English Addr."), "Caption");
		}

		[ExpectNoExceptions]
		public void TestZO_TWHideEXPExporterZHTAddr_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(orgImpAddInfo.ZO_TWHideEXPExporterZHTAddrInfo).Caption, NUnit.Framework.Is.EqualTo("Hide EXP Exporter Trad. Chinese Addr."), "Caption");
		}

		[ExpectNoExceptions]
		public void TestZO_TWHideEXPExporterAddr_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(orgImpAddInfo.ZO_TWHideEXPExporterAddrInfo).Caption, NUnit.Framework.Is.EqualTo("Hide EXP Exporter English Addr."), "Caption");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TWOrgImpAddInfo.Get(Factory.New<OrgHeader>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgImpAddInfo = TWOrgImpAddInfo.Get(Factory.New<OrgHeader>());
		}

		TWOrgImpAddInfo orgImpAddInfo;
	}
}
