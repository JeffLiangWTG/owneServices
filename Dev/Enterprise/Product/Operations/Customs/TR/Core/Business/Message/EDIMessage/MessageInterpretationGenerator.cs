using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public static class MessageInterpretationGenerator
	{
		public static ZString FormatOutputInTemplate(ZString title, ZString detailBody, bool withImage = false)
		{
			var result = ZString.Empty;
			var filePath = withImage ? "Enterprise.Customs.TR.Business.Message.EDIMessage.HtmlTemplates.MessageDetailTemplateWithImage.htm" : "Enterprise.Customs.TR.Business.Message.EDIMessage.HtmlTemplates.MessageDetailTemplate.htm";
			using (Stream stream = typeof(TRManifestMessage).Assembly.GetManifestResourceStream(filePath))
			{
				result = new StreamReader(stream).ReadToEnd();
			}

			result = result.Replace("<!--Message Title-->", title);
			result = result.Replace("<!--Style Sheet Section-->", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			result = result.Replace("<!--Message Detail Section-->", detailBody);
			return result;
		}

		public static ZString FormatOutputInTemplateWithSuccessFailureImage(ZString formattedHtmlBody, bool isSuccess)
		{
			var imageText = isSuccess ? ResString.GetMultilingualString("4FEBA163-EF2E-43E9-AEB6-6094196FF336", "Success") : ResString.GetMultilingualString("D4A9082A-30EC-4537-A936-B15805C494AE", "Failure");

			var imagePath = $"Enterprise.Customs.TR.Business.Message.EDIMessage.HtmlTemplates.{(isSuccess ? (NoResString)"Success" : (NoResString)"Failure")}.jpg";
			string imageBase64 = default;
			using (var imageStream = typeof(TRManifestMessage).Assembly.GetManifestResourceStream(imagePath))
			{
				var imageBytes = new byte[imageStream.Length];
				_ = imageStream.Read(imageBytes, 0, imageBytes.Length);
				imageBase64 = Convert.ToBase64String(imageBytes);
			}
			(int width, int height) imageSize = isSuccess ? (200, 150) : (200, 150);

			formattedHtmlBody = formattedHtmlBody.Replace("<!--Success Failure Image-->", $"<img src=\"data:image/jpeg;base64, {imageBase64}\" width=\"{imageSize.width}\" height=\"{imageSize.height}\" alt=\"{imageText}\" />");
			return formattedHtmlBody;
		}

		public static ZString GetFormattedMessageText(XmlDocument xmlDocument)
		{
			ZString formattedMessageText = null;
			using (var stringWriter = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(stringWriter))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlDocument.WriteTo(xmlWriter);
				xmlWriter.Flush();
				formattedMessageText = stringWriter.ToString();
			}

			return formattedMessageText;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public static ZString CreateInterpretationContentForSPTSRTx(ZString messageType, XmlDocument xmlDocument)
		{
			var title = ZString.Empty;
			var htmlBody = new System.Text.StringBuilder();

			var tableCreator = new HtmlTableCreator("table", new string[] { ResString.GetMultilingualString("0F958592-EEC1-463C-9283-E00CC2EA8AFD", "Label"), ResString.GetMultilingualString("0955561C-2571-4F49-AECA-2ED1C0C505E7", "Value") });
			if (messageType == TRMessageTypes.Codes.TSP)
			{
				var nodeList = new List<ZString>() { (NoResString)"Gelen" };
				var refID = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefID").Split('|');

				nodeList.Add("AktarmaBilgisi");
				var tasimaSekli = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "TasimaSekli");
				var hareketGumrukIdaresi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "HareketGumrukIdaresi");
				var varisGumrukIdaresi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "VarisGumrukIdaresi");
				var seferNumarasi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "SeferNumarasi");
				var seferTarihi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "SeferTarihi");

				tableCreator.WriteRow(ResString.GetMultilingualString("1029569D-BF32-4633-B2E7-EEDF23AEFB3F", "Job Number:"), refID.Length > 0 ? refID[0].Replace(TRMessageConstants.ReferencePrefix, "") : ZString.Empty);
				tableCreator.WriteRow(ResString.GetMultilingualString("7B9A56C9-ADF0-45CB-8FC3-DB9827CD8159", "Transport Type:"), tasimaSekli);
				tableCreator.WriteRow(ResString.GetMultilingualString("00635A99-45B9-4750-A3BC-5C7C51F27D17", "Departure Customs Office:"), hareketGumrukIdaresi);
				tableCreator.WriteRow(ResString.GetMultilingualString("331B3D36-81C5-4D56-B858-C34A3682A1A9", "Arrival Customs Office:"), varisGumrukIdaresi);
				tableCreator.WriteRow(ResString.GetMultilingualString("63C4A6B2-20A7-4F91-98C1-A440C25077E4", "Voyage No:"), seferNumarasi);
				tableCreator.WriteRow(ResString.GetMultilingualString("4F1C6897-EB7C-4512-B8EA-AA0B2BC1F698", "Voyage Date:"), seferTarihi);
			}
			else
			{
				title = ResString.GetMultilingualString("4D9EF13C-E024-47E4-8001-9460E80A8337", "SPTS Message Type {0} sent successfully.", messageType);
				var nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "IslemSonucGetir2" };
				var guIDof = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "GUIDof");
				tableCreator.WriteRow(ResString.GetMultilingualString("CD78C51F-E037-4F57-985B-0FA93579D4B0", "Query GUID:"), guIDof);
			}

			htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			return FormatOutputInTemplate(title, htmlBody.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public static ZString CreateInterpretationContentForETradeRTx(ZString messageType, XmlDocument xmlDocument)
		{
			var title = ResString.GetMultilingualString("3AD13957-6563-4645-829A-82BC1FD98996", "E-Trade Message Type {0} sent successfully.", messageType);
			var htmlBody = new System.Text.StringBuilder();
			var nodeValue = ZString.Empty;
			var nodeList = new List<ZString>();

			var tableCreator = new HtmlTableCreator("table", new string[] { ResString.GetMultilingualString("0F958592-EEC1-463C-9283-E00CC2EA8AFD", "Label"), ResString.GetMultilingualString("0955561C-2571-4F49-AECA-2ED1C0C505E7", "Value") });
			if (messageType == TRMessageTypes.Codes.TRE)
			{
				nodeList = new List<ZString>() { (NoResString)"Root" };
				var refID = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefID");

				nodeList.Add("RequestMessage");
				nodeList.Add("TCGB");
				var yuklemeBosaltmaYeri = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "yuklemeBosaltmaYeri");
				var girisCikisGumrukIdaresiKodu = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "girisCikisGumrukIdaresiKodu");
				var toplamKapAdedi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "toplamKapAdedi");

				nodeList.Add("cikistakiAracBilgileri");
				nodeList.Add((NoResString)"arac");
				var tipi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "tipi");
				var numarasi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "numarasi");

				tableCreator.WriteRow(ResString.GetMultilingualString("B772CC55-5FB6-46DA-A58B-2818495811C0", "Job Number:"), refID.SubstringSafe(0, refID.IndexOf("|")));
				tableCreator.WriteRow(ResString.GetMultilingualString("2C895DF5-8742-47D3-A902-F3D9F75AB334", "Vehicle Type:"), tipi);
				tableCreator.WriteRow(ResString.GetMultilingualString("100BB807-C6B5-450E-83BF-25D848B52450", "Discharge Customs Office:"), yuklemeBosaltmaYeri);
				tableCreator.WriteRow(ResString.GetMultilingualString("60E651AD-AF28-4D5B-B4EB-714C789DC836", "Arrival Customs Office:"), girisCikisGumrukIdaresiKodu);
				tableCreator.WriteRow(ResString.GetMultilingualString("73D023C5-7487-42EE-9ED5-11F5FB246B0A", "Voyage No:"), numarasi);
				tableCreator.WriteRow(ResString.GetMultilingualString("999ED765-BA7C-4996-B3F2-653B21E40C14", "Total Packs:"), toplamKapAdedi);
			}
			else
			{
				tableCreator.WriteRow(ResString.GetMultilingualString("8B6D482B-C15D-4431-83A9-B17797F3B848", "Action:"), new TRMessageTypes().GetDescriptionFromCode(messageType));
				if (messageType == TRMessageTypes.Codes.T1E || messageType == TRMessageTypes.Codes.T1D || messageType == TRMessageTypes.Codes.T2D || messageType == TRMessageTypes.Codes.T1S)
				{
					nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "ServisCevabiSorgulama" };
					nodeValue = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "KayitNo");
					tableCreator.WriteRow(ResString.GetMultilingualString("98A8C366-49BF-4884-B9A6-901346ADC24E", "Query GUID:"), nodeValue);
				}
				else if (messageType == TRMessageTypes.Codes.TRQ || messageType == TRMessageTypes.Codes.TRS)
				{
					nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "GeciciTescildenTescilNoSorgula" };
					if (messageType == TRMessageTypes.Codes.TRS)
					{
						nodeList = new List<ZString>() { (NoResString)"Root", "RequestMessage", "TCGBTescil" };
					}
					nodeValue = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "geciciTescilNo");
					tableCreator.WriteRow(ResString.GetMultilingualString("8EE7D38F-F0AC-4AFA-83E9-457A918A85BF", "Temporary Registration No:"), nodeValue);
				}
				else
				{
					if (messageType == TRMessageTypes.Codes.TRI || messageType == TRMessageTypes.Codes.TRB || messageType == TRMessageTypes.Codes.TCD)
					{
						if (messageType == TRMessageTypes.Codes.TRI)
						{
							nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "ETGBMuayeneMemuruSorgula" };
						}
						else if (messageType == TRMessageTypes.Codes.TRB)
						{
							nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "AyrilanTasimaSenediSorgula" };
						}
						else if (messageType == TRMessageTypes.Codes.TCD)
						{
							nodeList = new List<ZString>() { (NoResString)"Root", "RequestMessage", "tamamlayiciBeyan" };
						}
						nodeValue = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "beyannameNo");
					}
					else if (messageType == TRMessageTypes.Codes.TRD)
					{
						nodeList = new List<ZString>() { (NoResString)"Root", "RequestMessage", "BosaltmaListesi" };
						nodeValue = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "BeyannameNo");
					}
					else if (messageType == TRMessageTypes.Codes.TRL)
					{
						nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "BeyannameDurumSorgula" };
						nodeValue = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "tescilNo");
					}

					tableCreator.WriteRow(ResString.GetMultilingualString("DFE3B086-B75F-40B6-B908-8F7EF7FB0A66", "Registration No:"), nodeValue);
				}
			}

			htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			return FormatOutputInTemplate(title, htmlBody.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public static ZString CreateInterpretationContentForManifestRTx(ZString messageType, XmlDocument xmlDocument)
		{
			var title = ZString.Empty;
			var htmlBody = new System.Text.StringBuilder();
			var tableCreator = new HtmlTableCreator("table", new string[] { ResString.GetMultilingualString("0F958592-EEC1-463C-9283-E00CC2EA8AFD", "Label"), ResString.GetMultilingualString("0955561C-2571-4F49-AECA-2ED1C0C505E7", "Value") });

			if (messageType == TRMessageTypes.Codes.TRO)
			{
				var nodeList = new List<ZString>() { (NoResString)"Gelen" };
				var refIDs = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefID").Split('|');

				nodeList.Add("OzetBeyanBilgisi");
				var beyanTuru = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "BeyanTuru");
				var gumrukIdaresi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "GumrukIdaresi");
				var limanYerAdiYuk = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "LimanYerAdiYuk");
				var limanYerAdiBos = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "LimanYerAdiBos");
				var tasitinAdi = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "TasitinAdi");
				var plakaSeferNo = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "PlakaSeferNo");

				title = ResString.GetMultilingualString("518B9EBC-BCDD-4303-B7DA-7B833DA6CF2F", @"Global Manifest Message for job {0} sent successfully.", refIDs[0].Replace(TRMessageConstants.ReferencePrefix, ""));
				tableCreator.WriteRow(ResString.GetMultilingualString("148C4C42-2C52-4566-AFDA-27F338BBADC5", "Job Number:"), refIDs.Length > 0 ? refIDs[0] : ZString.Empty);
				tableCreator.WriteRow(ResString.GetMultilingualString("520441AC-2B4D-4E11-9E3D-2CDD4FCED9DB", "Manifest Type:"), beyanTuru);
				tableCreator.WriteRow(ResString.GetMultilingualString("AF676719-02A0-4E57-970F-F7F3FF61A3DB", "Customs Office:"), gumrukIdaresi);
				tableCreator.WriteRow(ResString.GetMultilingualString("A2BA47EA-626C-4013-BCA2-C75653EDAB0E", "Load Port:"), limanYerAdiYuk);
				tableCreator.WriteRow(ResString.GetMultilingualString("A178C62F-A246-4809-BB96-60CBD350DAD5", "Customs Discharge:"), limanYerAdiBos);
				tableCreator.WriteRow(ResString.GetMultilingualString("D6DCCAD6-FC31-42C3-A5EF-917B35EB0AB2", "Vessel:"), tasitinAdi);
				tableCreator.WriteRow(ResString.GetMultilingualString("AE2F755D-6DFB-4D08-AFE1-B2F5F9447E68", "Voyage:"), plakaSeferNo);
			}
			else if (messageType == TRMessageTypes.Codes.TRM)
			{ 
				var nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "ozbyMuayeneMemuruAdiSorgula" };
				title = ResString.GetMultilingualString("FB17B12D-9C23-437A-A03A-571A5F5B6075", @"Global Manifest Message Type {0} sent successfully.", messageType);
				var customsOffice = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "Gumruk");
				var registrationNumber = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "TescilNo");
				tableCreator.WriteRow(ResString.GetMultilingualString("C849E57F-A9F0-4693-B870-864D08EBD723", "Customs Office:"), customsOffice);
				tableCreator.WriteRow(ResString.GetMultilingualString("4CB63AA0-BD98-4692-9AAA-458B3E264F76", "Registration Number:"), registrationNumber);
			}
			else
			{
				var nodeName = messageType == TRMessageTypes.Codes.T2O ? "IslemSorgula3" : messageType == TRMessageTypes.Codes.T1O ? "IslemSonucGetir2" : "IslemSonucGetir4";
				var nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", nodeName };
				title = ResString.GetMultilingualString("FB17B12D-9C23-437A-A03A-571A5F5B6075", @"Global Manifest Message Type {0} sent successfully.", messageType);

				if (messageType == TRMessageTypes.Codes.T2O)
				{
					var refID = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefID");
					var basIslemGunu = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "BasIslemGunu");
					tableCreator.WriteRow(ResString.GetMultilingualString("8254E486-BDFB-4D9A-BD89-87C400F4C95B", "Job Number:"), refID.Replace(TRMessageConstants.ReferencePrefix, ""));
					tableCreator.WriteRow(ResString.GetMultilingualString("1AB5D8DD-EFD7-462D-BE1B-E001CECF462C", "Query Date:"), basIslemGunu);
				}
				else
				{
					var guIDof = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "GUIDof");
					tableCreator.WriteRow(ResString.GetMultilingualString("CD78C51F-E037-4F57-985B-0FA93579D4B0", "Query GUID:"), guIDof);
				}
			}

			htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			return FormatOutputInTemplate(title, htmlBody.ToString());
		}

		public static ZString CreateInterpretationContentForDeclarationRTx(ZString messageType, XmlDocument xmlDocument, ZString entryType, ZString supplierCompanyName,
																		   ZString importerCompanyName, ZString portOfLoading, ZString portOfArrival, ZString voyage)
		{
			var title = ZString.Empty;
			var htmlBody = new System.Text.StringBuilder();
			var tableCreator = new HtmlTableCreator((NoResString)"table", new string[] { ResString.GetMultilingualString("B005BD6D-BEB3-42CE-8274-874343BB020E", "Label"), ResString.GetMultilingualString("81C96CBB-AADB-4AF0-AFB1-F7FE6BA139D4", "Value") });

			if (messageType == TRMessageTypes.Codes.DKO || messageType == TRMessageTypes.Codes.DTE)
			{
				var nodeList = new List<ZString>();
				if (messageType == TRMessageTypes.Codes.DKO)
				{
					nodeList.AddRange(new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", (NoResString)"Kontrol" });
				}
				nodeList.Add((NoResString)"Gelen");
				var refIDs = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefID").Split('|');

				nodeList.Add("BeyannameBilgi");
				var customsProcedureCode = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, (NoResString)"Rejim");
				var customsOffice = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "GUMRUK");
				var boxQuantity = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "Kap_adedi");
				var invoiceTotal = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "Toplam_fatura");
				var invoiceCurrency = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "Toplam_fatura_dovizi");
				var vessel = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "Cikistaki_aracin_kimligi");

				var jobNumber = refIDs[0].Replace(TRMessageConstants.ReferencePrefix, "");
				if (messageType == TRMessageTypes.Codes.DKO)
				{
					title = ResString.GetMultilingualString("730B7C58-52E0-49CB-9A7B-9EE0A8F0D287", @"Declaration Control Message for job {0} sent successfully.", jobNumber);
				}
				else
				{
					title = ResString.GetMultilingualString("BD775630-1371-40AD-AEAE-504006B3CBB8", @"Declaration Registration Message for job {0} sent successfully.", jobNumber);
				}

				tableCreator.WriteRow(ResString.GetMultilingualString("BF41ACA0-D0A3-46E8-84BE-3B2802F55536", "Job Number:"), refIDs[0].Length > 0 ? refIDs[0] : ZString.Empty);
				tableCreator.WriteRow(ResString.GetMultilingualString("0B335983-352A-43FD-A2A1-65A4459706ED", "Entry Type:"), entryType);
				tableCreator.WriteRow(ResString.GetMultilingualString("A60D823F-710F-4593-9211-A22D9EAFDDEA", "Customs Procedure Code:"), customsProcedureCode);
				tableCreator.WriteRow(ResString.GetMultilingualString("C56F8522-07EE-4BE8-A958-6A23FE40C8EB", "Customs Office:"), customsOffice);
				tableCreator.WriteRow(ResString.GetMultilingualString("49F3E20F-CDCB-4A71-B2C2-454D083A1161", "Box Quantity:"), boxQuantity);
				tableCreator.WriteRow(ResString.GetMultilingualString("FEF880FA-F5F3-4F18-9B32-82A3A52944D1", "Supplier:"), supplierCompanyName);
				tableCreator.WriteRow(ResString.GetMultilingualString("D2BFDE9A-E75F-4DE2-8946-3D81ABA6F2F2", "Importer:"), importerCompanyName);
				tableCreator.WriteRow(ResString.GetMultilingualString("50F5DD61-6141-4DDE-B9AB-70F2D78D604A", "Invoice Total:"), invoiceTotal + " " + invoiceCurrency);
				tableCreator.WriteRow(ResString.GetMultilingualString("9F773D87-B0B7-4119-9BD5-1A2408FA3BEA", "Load Port:"), portOfLoading);
				tableCreator.WriteRow(ResString.GetMultilingualString("01978905-9B6C-43C9-B7FD-A1D1646A79F5", "Discharge Port:"), portOfArrival);
				tableCreator.WriteRow(ResString.GetMultilingualString("00147126-922A-49D9-A722-153E90D1E05A", "Vessel:"), vessel);
				tableCreator.WriteRow(ResString.GetMultilingualString("9CC809FF-20E5-40BF-8E60-B254C7C94E9B", "Voyage:"), voyage);
			}
			else
			{
				if (messageType == TRMessageTypes.Codes.EUT)
				{
					// TODO: In other WIs, when other Export Union sending message types are added
				}
				else
				{
					var nodeName = messageType == TRMessageTypes.Codes.DT2 ? "IslemSorgula3" : messageType == TRMessageTypes.Codes.DT1 || messageType == TRMessageTypes.Codes.DK1 ? "IslemSonucGetir2" : "IslemSonucGetir4";
					var nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", nodeName };
					title = ResString.GetMultilingualString("63A67D21-C0B4-4E04-917D-840D165520E1", @"Declaration Message Type {0} sent successfully.", messageType);

					if (messageType == TRMessageTypes.Codes.DT2)
					{
						var refID = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefID");
						var basIslemGunu = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "BasIslemGunu");
						tableCreator.WriteRow(ResString.GetMultilingualString("5166236A-B55A-4CC2-839A-F80F389CBF76", "Job Number:"), refID.Replace(TRMessageConstants.ReferencePrefix, ""));
						tableCreator.WriteRow(ResString.GetMultilingualString("8B519B57-3BC3-49B9-8935-AEF68F236B69", "Query Date:"), basIslemGunu);
					}
					else
					{
						var guIDof = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "GUIDof");
						tableCreator.WriteRow(ResString.GetMultilingualString("F8F332AB-43B6-4A61-8CDE-F2FE2E61C845", "Query GUID:"), guIDof);
					}
				}
			}

			htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			return FormatOutputInTemplate(title, htmlBody.ToString());
		}

		public static ZString CreateInterpretationContentForNCTSRTx(ZString messageType, XmlDocument xmlDocument)
		{
			var title = ZString.Empty;
			var htmlBody = new System.Text.StringBuilder();

			var tableCreator = new HtmlTableCreator((NoResString)"table", new string[] { ResString.GetMultilingualString("0F958592-EEC1-463C-9283-E00CC2EA8AFD", "Label"), ResString.GetMultilingualString("0955561C-2571-4F49-AECA-2ED1C0C505E7", "Value") });
			if (messageType == TRMessageTypes.Codes.TRN)
			{
				var nodeList = new List<ZString>() { "CC015B" };
				var interchangeControlReference = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "IntConRefMES11");
				var customsOfficeDepartureCode = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "CUSOFFDEPEPT");
				var customsOfficeDestinationCode = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "CUSOFFDESEST");

				nodeList.Add("HEAHEA");
				var typeOfDeclaration = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "TypOfDecHEA24");
				var referenceNumberCustomsData = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "RefNumEBT1");
				var identityOfMeansOfTransportAtDeparture = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "IdeOfMeaOfTraAtDHEA78");
				nodeList.Remove("HEAHEA");

				nodeList.Add("TRAPRIPC1");
				var principal = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "NamPC17");
				nodeList.Remove("TRAPRIPC1");

				nodeList.Add("TRACONCO1");
				var consignor = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "NamCO17");
				nodeList.Remove("TRACONCO1");

				nodeList.Add("TRACONCE1");
				var consignee = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "NamCE17");
				nodeList.Remove("TRACONCE1");

				nodeList.Add("GOOITEGDS");
				nodeList.Add("CONNR2");
				var containerList = TRMessageHelper.GetNodeValues(xmlDocument.OuterXml, nodeList, "ConNumNR21");

				title = ResString.GetMultilingualString("30DE7851-AD03-459E-8175-B5075CEC4D8C", "NCTS Message for job {0} sent successfully.", interchangeControlReference);
				tableCreator.WriteRow(ResString.GetMultilingualString("FE120C1F-A339-465C-97E7-7BA73374FBB3", "Job Number:"), interchangeControlReference);
				tableCreator.WriteRow(ResString.GetMultilingualString("A32A31A4-260B-421E-A9ED-A56F2DCBDEAA", "Declaration Type:"), typeOfDeclaration);
				tableCreator.WriteRow(ResString.GetMultilingualString("76DC0F9E-B0F7-4D8A-A383-1E232E7567B5", "DEP Customs Office:"), customsOfficeDepartureCode);
				tableCreator.WriteRow(ResString.GetMultilingualString("02E4F0C2-671D-46DB-8D2F-FC12C9E4A62D", "DES Customs Office:"), customsOfficeDestinationCode);
				tableCreator.WriteRow(ResString.GetMultilingualString("585851C1-7129-4FD1-9C81-AACB91C13506", "Exit Customs Office:"), referenceNumberCustomsData);
				tableCreator.WriteRow(ResString.GetMultilingualString("269FD605-36F9-45B9-A09A-CE822F5FAEA8", "Principal:"), principal);
				tableCreator.WriteRow(ResString.GetMultilingualString("1B58AD0E-5640-496C-A803-C307A4AA85E0", "Consignor:"), consignor);
				tableCreator.WriteRow(ResString.GetMultilingualString("867ACBFB-5CE3-48D0-AC49-587098CE5105", "Consignee:"), consignee);
				tableCreator.WriteRow(ResString.GetMultilingualString("77FA9D90-7414-4C78-B2A9-BEA0152C9C50", "Transport ID (DEP):"), identityOfMeansOfTransportAtDeparture);
				tableCreator.WriteRow(ResString.GetMultilingualString("C90F2241-C2CE-4759-B920-1C43580108C9", "Containers:"), string.Join(System.Environment.NewLine, containerList));
			}
			else
			{
				title = ResString.GetMultilingualString("74CC1FFC-A255-4466-98CA-2C4360178382", "NCTS Message Type {0} sent successfully.", messageType);
				var nodeList = new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body" };
				if (messageType == TRMessageTypes.Codes.T1N)
				{
					nodeList.Add("getMessagesListByGuid");
					var guIDof = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "CORR_GUID");
					tableCreator.WriteRow(ResString.GetMultilingualString("3FB18910-955C-4A9C-8427-D83C69B59C14", "Query GUID:"), guIDof);
				}
				else if (messageType == TRMessageTypes.Codes.T2N)
				{
					nodeList.Add((NoResString)"downloadmessagebyindex");
					var index = TRMessageHelper.GetNodeValue(xmlDocument, nodeList, "INDEX");
					tableCreator.WriteRow(ResString.GetMultilingualString("C11949D0-49A3-49AA-BBDA-D715B7C5BAFD", "Index:"), index);
				}
			}
			htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			return FormatOutputInTemplate(title, htmlBody.ToString());
		}
	}
}
