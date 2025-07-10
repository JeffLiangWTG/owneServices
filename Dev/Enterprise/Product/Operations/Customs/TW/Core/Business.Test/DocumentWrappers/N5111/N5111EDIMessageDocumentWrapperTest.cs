using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5111EDIMessageDocumentWrapper))]
	sealed class N5111EDIMessageDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		N5111EDIMessageDocumentWrapper wrapper;

		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.NewWithValidTestData<N5111EDIMessage>();
			message.EM_MessageType = "TAD";
			return new N5111EDIMessageDocumentWrapper(message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var message1 = Factory.New<N5111EDIMessage>();
			message1.EM_MessageType = "TAD";
			message1.EM_MessageText = MessageTextForTest;
			wrapper = new N5111EDIMessageDocumentWrapper(message1);
		}

		[ExpectNoExceptions]
		public void TestObligationGuaranteeSuretyID()
		{
			NUnit.Framework.Assert.That(wrapper.ObligationGuaranteeSuretyID, NUnit.Framework.Is.EqualTo("03489200").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestObligationGuaranteeSuretyName()
		{
			NUnit.Framework.Assert.That(wrapper.ObligationGuaranteeSuretyName, NUnit.Framework.Is.EqualTo("裕隆汽車製造股份有限公司").Using(CustomComparers.TypeComparison));

			var message = Factory.New<N5111EDIMessage>();
			message.EM_MessageType = "TAD";
			message.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime></IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>AA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName></tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			wrapper = new N5111EDIMessageDocumentWrapper(message);
			NUnit.Framework.Assert.That(wrapper.ObligationGuaranteeSuretyName, NUnit.Framework.Is.EqualTo("YULONMOTORCO.,LTD.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAgentID()
		{
			NUnit.Framework.Assert.That(wrapper.AgentID, NUnit.Framework.Is.EqualTo("207").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepositType()
		{
			NUnit.Framework.Assert.That(wrapper.DepositType, NUnit.Framework.Is.EqualTo("押金(現金)").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPaymentAmount()
		{
			NUnit.Framework.Assert.That(wrapper.PaymentAmount, NUnit.Framework.Is.EqualTo(91928m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIssueDateTime()
		{
			NUnit.Framework.Assert.That(wrapper.IssueDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2018, 11, 23)));
		}

		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("DA  07207F1266").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestObligationGuaranteeReferenceID()
		{
			NUnit.Framework.Assert.That(wrapper.ObligationGuaranteeReferenceID, NUnit.Framework.Is.EqualTo("DA025698").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeReferenceID()
		{
			NUnit.Framework.Assert.That(wrapper.DutyTaxFeeReferenceID, NUnit.Framework.Is.EqualTo("DAI12172113711").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBankAccountID()
		{
			NUnit.Framework.Assert.That(wrapper.BankAccountID, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBankAccountReferenceID()
		{
			NUnit.Framework.Assert.That(wrapper.BankAccountReferenceID, NUnit.Framework.Is.EqualTo("3070500000    ").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclarationOffice()
		{
			NUnit.Framework.Assert.That(wrapper.DeclarationOffice, NUnit.Framework.Is.EqualTo("台中關").Using(CustomComparers.TypeComparison));

			var message1 = Factory.New<N5111EDIMessage>();
			message1.EM_MessageType = "TAD";
			message1.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime></IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>AA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5111EDIMessageDocumentWrapper(message1);
			NUnit.Framework.Assert.That(wrapper1.DeclarationOffice, NUnit.Framework.Is.EqualTo("基隆關").Using(CustomComparers.TypeComparison));

			var message2 = Factory.New<N5111EDIMessage>();
			message2.EM_MessageType = "TAD";
			message2.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime></IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>BA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper2 = new N5111EDIMessageDocumentWrapper(message2);
			NUnit.Framework.Assert.That(wrapper2.DeclarationOffice, NUnit.Framework.Is.EqualTo("高雄關").Using(CustomComparers.TypeComparison));

			var message3 = Factory.New<N5111EDIMessage>();
			message3.EM_MessageType = "TAD";
			message3.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime></IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>CA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper3 = new N5111EDIMessageDocumentWrapper(message3);
			NUnit.Framework.Assert.That(wrapper3.DeclarationOffice, NUnit.Framework.Is.EqualTo("台北關").Using(CustomComparers.TypeComparison));

			var message4 = Factory.New<N5111EDIMessage>();
			message4.EM_MessageType = "TAD";
			message4.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime></IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>EA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper4 = new N5111EDIMessageDocumentWrapper(message4);
			NUnit.Framework.Assert.That(wrapper4.DeclarationOffice, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyImage()
		{
			var governmentAgencyIDPrefix = "R99";
			var governmentAgencyID = ZString.Empty;
			for (int id = 1; id <= 21; id++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + id.ToString().PadLeft(2, '0');
				var message = Factory.New<N5111EDIMessage>();
				message.EM_MessageType = "TAD";
				message.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime>2018-11-23</IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>{governmentAgencyID}</ID></ResponsibleGovernmentAgency></Declaration></Response>";
				var messageString = $"The government gency id is {governmentAgencyID}";
				var wrapper = new N5111EDIMessageDocumentWrapper(message);
				if (id >= 1 && id <= 20)
				{
					NUnit.Framework.Assert.That(ConvertImageToString(wrapper.GovernmentAgencyImage), NUnit.Framework.Is.EqualTo(ConvertImageToString(GetImage(governmentAgencyID))), messageString);
				}
				else if (id >= 21)
				{
					NUnit.Framework.Assert.That(wrapper.GovernmentAgencyImage, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)), "messageString - should be [null]");
				}
			}
		}

		string ConvertImageToString(Image image)
		{
			var result = ZString.Empty;
			using (MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, image.RawFormat);
				result = System.Text.Encoding.UTF8.GetString(ms.ToArray());
			}
			return result;
		}

		Image GetImage(ZString governmentAgencyID)
		{
			ZString path = @"Enterprise.Customs.TW.Business.DocumentWrappers.N5111.Resources.";
			Image image = null;
			if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID))
			{
				path += "GovernmentAgencyImage.png";
			}
			else
			{
				path = ZString.Empty;
			}

			if (!path.IsEmpty)
			{
				using (var stream = typeof(N5111EDIMessageDocumentWrapper).Assembly.GetManifestResourceStream(path))
				{
					if (stream != null)
					{
						image = Image.FromStream(stream);
					}
				}
			}
			return image;
		}

		[ExpectNoExceptions]
		public void TestBarcode()
		{
			var xx = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime>2018-11-23</IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode></tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.Barcode1, NUnit.Framework.Is.EqualTo("0712076AX").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper.Barcode2, NUnit.Framework.Is.EqualTo("R9901DAI12172113711").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper.Barcode3, NUnit.Framework.Is.EqualTo("612RT0000091928").Using(CustomComparers.TypeComparison), "The barcode 3");
			});
			var message1 = Factory.New<N5111EDIMessage>();
			message1.EM_MessageType = "TAD";
			message1.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime></IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode>D10</tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5111EDIMessageDocumentWrapper(message1);

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper1.Barcode1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 1 when IssueDateTime is empty");
				NUnit.Framework.Assert.That(wrapper1.Barcode2, NUnit.Framework.Is.EqualTo("R9901DAI12172113711").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper1.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message2 = Factory.New<N5111EDIMessage>();
			message2.EM_MessageType = "TAD";
			message2.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime>2018-11-23</IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode></tw_CollectionTypeCode><tw_DepositTypeCode></tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper2 = new N5111EDIMessageDocumentWrapper(message2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper2.Barcode1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 1 when collection type is empty");
				NUnit.Framework.Assert.That(wrapper2.Barcode2, NUnit.Framework.Is.EqualTo("R9901DAI12172113711").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper2.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message3 = Factory.New<N5111EDIMessage>();
			message3.EM_MessageType = "TAD";
			message3.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime>2018-11-23</IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode></tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID></ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper3 = new N5111EDIMessageDocumentWrapper(message3);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper3.Barcode1, NUnit.Framework.Is.EqualTo("0712076AX").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper3.Barcode2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 2 when government agency is empty");
				NUnit.Framework.Assert.That(wrapper3.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message4 = Factory.New<N5111EDIMessage>();
			message4.EM_MessageType = "TAD";
			message4.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime>2018-11-23</IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>91928</PaymentAmount><ReferenceID></ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode></tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper4 = new N5111EDIMessageDocumentWrapper(message4);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper4.Barcode1, NUnit.Framework.Is.EqualTo("0712076AX").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper4.Barcode2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 2 when duty and tax fee reference id is empty");
				NUnit.Framework.Assert.That(wrapper4.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message5 = Factory.New<N5111EDIMessage>();
			message5.EM_MessageType = "TAD";
			message5.EM_MessageText = $@"<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05""><IssueDateTime>2018-11-23</IssueDateTime><BankAccount><ID>AAA</ID><ReferenceID>3070500000</ReferenceID></BankAccount><Status><NameCode>C2</NameCode></Status><Declaration><DeclarationOfficeID>DA</DeclarationOfficeID><ID>DA07207F1266</ID><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><DutyTaxFee><Payment><PaymentAmount>0</PaymentAmount><ReferenceID>DAI12172113711</ReferenceID><tw_BelongDate>201811</tw_BelongDate><tw_CollectionTypeCode>6AX</tw_CollectionTypeCode><tw_DepositTypeCode></tw_DepositTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode><ObligationGuarantee><ReferenceID>DA025698</ReferenceID><SecurityDetailsCode>09</SecurityDetailsCode><Surety><ID>03489200</ID><Name>YULONMOTORCO.,LTD.</Name><tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Surety></ObligationGuarantee></Payment></DutyTaxFee><ResponsibleGovernmentAgency><ID>R9901</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper5 = new N5111EDIMessageDocumentWrapper(message5);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper5.Barcode1, NUnit.Framework.Is.EqualTo("0712076AX").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper5.Barcode2, NUnit.Framework.Is.EqualTo("R9901DAI12172113711").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper5.Barcode3, NUnit.Framework.Is.EqualTo("612R00000000000").Using(CustomComparers.TypeComparison), "The barcode 3 when total pay amount is 0");
			});
		}

		public const string MessageTextForTest = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5111:R-00-05"">
	<IssueDateTime>2018-11-23</IssueDateTime>
	<BankAccount>
		<ID>AAA</ID>
		<ReferenceID>3070500000    </ReferenceID>
	</BankAccount>
	<Status>
		<NameCode>C2</NameCode>
	</Status>
	<Declaration>
		<DeclarationOfficeID>DA</DeclarationOfficeID>
		<ID>DA  07207F1266</ID>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<DutyTaxFee>
			<Payment>
				<PaymentAmount>91928</PaymentAmount>
				<ReferenceID>DAI12172113711</ReferenceID>
				<tw_BelongDate>201811</tw_BelongDate>
				<tw_CollectionTypeCode>6AX</tw_CollectionTypeCode>
				<tw_DepositTypeCode>D10</tw_DepositTypeCode>
				<tw_IssueReasonCode>2</tw_IssueReasonCode>
				<ObligationGuarantee>
					<ReferenceID>DA025698</ReferenceID>
					<SecurityDetailsCode>09</SecurityDetailsCode>
					<Surety>
						<ID>03489200</ID>
						<Name>YULON MOTOR CO., LTD.</Name>
						<tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName>
						<tw_TypeCode>58</tw_TypeCode>
					</Surety>
				</ObligationGuarantee>
			</Payment>
		</DutyTaxFee>
		<ResponsibleGovernmentAgency>
			<ID>R9901</ID>
		</ResponsibleGovernmentAgency>
	</Declaration>
</Response>";
	}
}
