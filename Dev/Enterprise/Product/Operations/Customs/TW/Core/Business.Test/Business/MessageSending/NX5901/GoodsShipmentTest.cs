using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business.NX5901;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsShipmentTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var cusHead = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead.CH_JE = declaration.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			cusHead.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var messageSendingObject = new NX5901MessageSendingObject(cusHead);
			messageSendingObject.ContactOffice = "A";
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.EDoc = doc1.UniqueKey;
			var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
			docLine2.EDoc = doc2.UniqueKey;
			IGoodsShipment goodsShipment = new GoodsShipment(messageSendingObject);
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)));
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
			NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGovernmentAgencyGoodsItem>)));
			NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.Consignment, NUnit.Framework.Is.EqualTo(default(IConsignment)));
			NUnit.Framework.Assert.That(goodsShipment.Consignor, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.CustomsValuation, NUnit.Framework.Is.EqualTo(default(ICustomsValuation)));
			NUnit.Framework.Assert.That(goodsShipment.DeliveryDestinationName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGoodsShipmentDutyTaxFee>)));
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsShipment.UCR, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.Exporter, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(goodsShipment.GoodsMeasures, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGoodsMeasure>)));
			NUnit.Framework.Assert.That(goodsShipment.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
		}
	}
}
