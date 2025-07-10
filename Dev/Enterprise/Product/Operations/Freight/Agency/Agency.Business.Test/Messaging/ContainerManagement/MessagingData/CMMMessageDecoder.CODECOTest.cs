using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	partial class CMMMessageDecoderTest
	{
		public void TestCODECOMessage34_Short()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+1+CODECO:D:95B:UN:ITG10'" +
				"BGM+34+PPA8818855-1+9'" +
				// group 1
				"TDT+20+0033+1++CSH:172:184+++9290127:146'" +
				// group 2
				"NAD+MS+SHCB1:160:ZZZ'" +
				"NAD+CF+CSH:160:184'" +
				// group 3
				"GID+1'" +
				// group 5
				"EQD+CN+CCLU4214635+42R0:102:5+1++4'" +
				"DTM+7:200806271010:203'" +
				"LOC+165+AUSYD:139:6+SHCB1'" +
				"LOC+8+AUSYD'" +
				"FTX+DAR++2::184'" +
				"FTX+ABS++68::184'" +
				// group 0
				"CNT+16:1'" +
				"UNT+14+1'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "SHCB1", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.MutuallyDefined, data.MessageSender.CodeType);
				AssertEquals("LloydsNumber", "9290127", data.LloydsNumber);
				AssertEquals("VoyageNumber", "0033", data.VoyageNumber);
				AssertEquals("Type", CMMMessageType.GateIn, data.Type);
			});

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", 1, equipment.Count);

			CombineAssertions(delegate
			{
				AssertEquals("Equipment[0].ContainerNumber", "CCLU4214635", equipment[0].ContainerNumber);
				AssertEquals("Equipment[0].ISOType", "42R0", equipment[0].ISOType);
				AssertEquals("Equipment[0].IsEmpty", true, equipment[0].IsEmpty);
				AssertEquals("Equipment[0].BookingReference", "", equipment[0].BookingReference);
				AssertEquals("Equipment[0].BillOfLading", "", equipment[0].BillOfLading);
				AssertEquals("Equipment[0].GoodsDeclarationNumber", "", equipment[0].GoodsDeclarationNumber);
				AssertEquals("Equipment[0].GrossWeightInKG", null, equipment[0].GrossWeightKG);
				AssertEquals("Equipment[0].PositioningDateTime", new DateTime(2008, 06, 27, 10, 10, 00), equipment[0].PositioningDateTime);
				AssertEquals("Equipment[0].EquipmentSupplier", CMMEquipmentSupplier.Shipper, equipment[0].EquipmentSupplier);
				AssertContainsExactElementsInAnyOrder("Equipment[0].SealNumbers", Array.Empty<string>(), equipment[0].SealNumbers);
			});
		}

		public void TestCODECOMessage34_Long()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+02950998001+CODECO:D:95B:UN:ITG14'" +
				"BGM+34+02950998001+9'" +
				// group 1
				"TDT+20+017N+1++ANL:172:87+++9324849:146:11:ANL WINDARRA'" +
				"LOC+9+AUADL:139:6+DPIADL'" +
				// group 2
				"NAD+CA+CSC:160:184'" +
				"NAD+MS+CSXWTADL:ZZZ'" +
				// group 3
				"GID+1'" +
				// group 5
				"EQD+CN+CCLU1034809+22R1:102:5++2+5'" +
				"RFF+BN:CSG127168'" +
				"RFF+AAE:AALKMPA7H'" +
				"DTM+7:200806270956:203'" +
				"LOC+8+SGSIN:139:6'" +
				"LOC+9+AUADL:139:6'" +
				"LOC+11+SGSIN:139:6'" +
				"LOC+165+AUADL:139:6+DPIADL:TER:ZZZ'" +
				"MEA+AAE+G+KGM:17624'" +
				"SEL+881055+SH'" +
				"SEL+88105X+SH'" +
				"FTX+AAA+++HARD FROZEN MEAT'" +
				// group 7
				"TDT+1++3++MAC:172:87+++SX55DP:146:ZZZ'" +
				// group 0
				"CNT+16:1'" +
				"UNT+22+02950998001'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "CSXWTADL", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.Unknown, data.MessageSender.CodeType);
				AssertEquals("LloydsNumber", "9324849", data.LloydsNumber);
				AssertEquals("VoyageNumber", "017N", data.VoyageNumber);
				AssertEquals("Type", CMMMessageType.GateIn, data.Type);
			});

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", 1, equipment.Count);

			CombineAssertions(delegate
			{
				AssertEquals("Equipment[0].ContainerNumber", "CCLU1034809", equipment[0].ContainerNumber);
				AssertEquals("Equipment[0].ISOType", "22R1", equipment[0].ISOType);
				AssertEquals("Equipment[0].IsEmpty", false, equipment[0].IsEmpty);
				AssertEquals("Equipment[0].BookingReference", "CSG127168", equipment[0].BookingReference);
				AssertEquals("Equipment[0].BillOfLading", "", equipment[0].BillOfLading);
				AssertEquals("Equipment[0].GoodsDeclarationNumber", "AALKMPA7H", equipment[0].GoodsDeclarationNumber);
				AssertEquals("Equipment[0].GrossWeightInKG", 17624m, equipment[0].GrossWeightKG);
				AssertEquals("Equipment[0].PositioningDateTime", new DateTime(2008, 06, 27, 09, 56, 00), equipment[0].PositioningDateTime);
				AssertEquals("Equipment[0].EquipmentSupplier", CMMEquipmentSupplier.Unknown, equipment[0].EquipmentSupplier);
				AssertContainsExactElementsInAnyOrder("Equipment[0].SealNumbers", new string[] { "881055", "88105X" }, equipment[0].SealNumbers);
			});
		}

		public void TestCODECOMessage34_Multi()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+1879+CODECO:D:95B:UN:ITG10'" +
				"BGM+34+0806261647+9'" +
				// group 1
				"NAD+MS+AALME:160:184'" +
				"NAD+CF+ACEPOOL'" +
				// group 5
				"EQD+CN+CCLU4156884+42R0:102:5+++4'" +
				"DTM+7:200806260842:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4949300+42R0:102:5+++4'" +
				"DTM+7:200806260848:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4161221+42R0:102:5+++4'" +
				"DTM+7:200806260917:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4391250+42R0:102:5+++4'" +
				"DTM+7:200806260940:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+TRLU8737476+42R0:102:5+++4'" +
				"DTM+7:200806260942:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4027660+42R0:102:5+++4'" +
				"DTM+7:200806261031:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU5031710+42R0:102:5+++4'" +
				"DTM+7:200806261042:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4908512+42R0:102:5+++4'" +
				"DTM+7:200806261155:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4790040+42R0:102:5+++4'" +
				"DTM+7:200806261300:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4715832+42R0:102:5+++4'" +
				"DTM+7:200806261446:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4595589+42R0:102:5+++4'" +
				"DTM+7:200806261450:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 5
				"EQD+CN+CCLU4300950+42R0:102:5+++4'" +
				"DTM+7:200806261503:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				// group 7
				"TDT+20++3'" +
				// group 0
				"CNT+16:12'" +
				"UNT+66+1879'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "AALME", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.OneStop, data.MessageSender.CodeType);
				AssertEquals("LloydsNumber", null, data.LloydsNumber);
				AssertEquals("VoyageNumber", null, data.VoyageNumber);
				AssertEquals("Type", CMMMessageType.GateIn, data.Type);
			});

			var expected = new[]
			{
				new { ContainerNum = "CCLU4156884", PositioningDateTime = new DateTime(2008,06,26,08,42,00) },
				new { ContainerNum = "CCLU4949300", PositioningDateTime = new DateTime(2008,06,26,08,48,00) },
				new { ContainerNum = "CCLU4161221", PositioningDateTime = new DateTime(2008,06,26,09,17,00) },
				new { ContainerNum = "CCLU4391250", PositioningDateTime = new DateTime(2008,06,26,09,40,00) },
				new { ContainerNum = "TRLU8737476", PositioningDateTime = new DateTime(2008,06,26,09,42,00) },
				new { ContainerNum = "CCLU4027660", PositioningDateTime = new DateTime(2008,06,26,10,31,00) },
				new { ContainerNum = "CCLU5031710", PositioningDateTime = new DateTime(2008,06,26,10,42,00) },
				new { ContainerNum = "CCLU4908512", PositioningDateTime = new DateTime(2008,06,26,11,55,00) },
				new { ContainerNum = "CCLU4790040", PositioningDateTime = new DateTime(2008,06,26,13,00,00) },
				new { ContainerNum = "CCLU4715832", PositioningDateTime = new DateTime(2008,06,26,14,46,00) },
				new { ContainerNum = "CCLU4595589", PositioningDateTime = new DateTime(2008,06,26,14,50,00) },
				new { ContainerNum = "CCLU4300950", PositioningDateTime = new DateTime(2008,06,26,15,03,00) },
			};

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", expected.Length, equipment.Count);

			CombineAssertions(delegate
			{
				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("Equipment[{0}].ContainerNumber", i), expected[i].ContainerNum, equipment[i].ContainerNumber);
					AssertEquals(string.Format("Equipment[{0}].ISOType", i), "42R0", equipment[i].ISOType);
					AssertEquals(string.Format("Equipment[{0}].IsEmpty", i), true, equipment[i].IsEmpty);
					AssertEquals(string.Format("Equipment[{0}].BookingReference", i), "", equipment[i].BookingReference);
					AssertEquals(string.Format("Equipment[{0}].BillOfLading", i), "", equipment[i].BillOfLading);
					AssertEquals(string.Format("Equipment[{0}].GoodsDeclarationNumber", i), "", equipment[i].GoodsDeclarationNumber);
					AssertEquals(string.Format("Equipment[{0}].GrossWeightKG", i), null, equipment[i].GrossWeightKG);
					AssertEquals(string.Format("Equipment[{0}].PositioningDateTime", i), expected[i].PositioningDateTime, equipment[i].PositioningDateTime);
					AssertContainsExactElementsInAnyOrder(string.Format("Equipment[{0}].SealNumbers", i), Array.Empty<string>(), equipment[i].SealNumbers);
				}
			});
		}

		public void TestCODECOMessage36_Short()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+29045+CODECO:D:95B:UN:ITG10'" +
				"BGM+36+194092502+9'" +
				// group 1
				"TDT+20+002S+1++CSC:172:184+++9355769:146'" +
				// group 2
				"NAD+MS+CONFI:160:184'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"GID+1'" +
				"FTX+AAA+++GEN'" +
				// group 5
				"EQD+CN+CCLU6640671+45G0:102:5+2+3+5'" +
				"DTM+7:200806262200:203'" +
				"LOC+9+CNSHA'" +
				"LOC+165+AUBNE'" +
				"MEA+AAE+G+KGM:5100'" +
				"FTX+ABS++55::184'" +
				// group 0
				"CNT+16:1'" +
				"UNT+15+29045'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "CONFI", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.OneStop, data.MessageSender.CodeType);
				AssertEquals("LloydsNumber", "9355769", data.LloydsNumber);
				AssertEquals("VoyageNumber", "002S", data.VoyageNumber);
				AssertEquals("Type", CMMMessageType.GateOut, data.Type);
			});

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", 1, equipment.Count);

			CombineAssertions(delegate
			{
				AssertEquals("Equipment[0].ContainerNumber", "CCLU6640671", equipment[0].ContainerNumber);
				AssertEquals("Equipment[0].ISOType", "45G0", equipment[0].ISOType);
				AssertEquals("Equipment[0].IsEmpty", false, equipment[0].IsEmpty);
				AssertEquals("Equipment[0].BookingReference", "", equipment[0].BookingReference);
				AssertEquals("Equipment[0].BillOfLading", "", equipment[0].BillOfLading);
				AssertEquals("Equipment[0].GoodsDeclarationNumber", "", equipment[0].GoodsDeclarationNumber);
				AssertEquals("Equipment[0].GrossWeightKG", 5100m, equipment[0].GrossWeightKG);
				AssertEquals("Equipment[0].PositioningDateTime", new DateTime(2008, 06, 26, 22, 00, 00), equipment[0].PositioningDateTime);
				AssertEquals("Equipment[0].EquipmentSupplier", CMMEquipmentSupplier.Carrier, equipment[0].EquipmentSupplier);
				AssertContainsExactElementsInAnyOrder("Equipment[0].SealNumbers", Array.Empty<string>(), equipment[0].SealNumbers);
			});
		}

		public void TestCODECOMessage36_Long()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+02940269001+CODECO:D:95B:UN:ITG14'" +
				"BGM+36+02940269001+9'" +
				// group 1
				"TDT+20+018S+1++ANL:172:87+++9324837:146:11:ANL WARRINGA'" +
				"LOC+11+AUADL:139:6+DPIADL'" +
				// group 2
				"NAD+CA+CSC:160:184'" +
				"NAD+MS+CSXWTADL:ZZZ'" +
				// group 3
				"GID+1'" +
				// group 5
				"EQD+CN+CAXU3295589+22G1:102:5++3+5'" +
				"RFF+BM:BILL1'" +
				"DTM+7:200806241924:203'" +
				"LOC+8+AUADL:139:6'" +
				"LOC+9+SGSIN:139:6'" +
				"LOC+11+AUADL:139:6'" +
				"LOC+165+AUADL:139:6+DPIADL:TER:ZZZ'" +
				"MEA+AAE+G+KGM:20200'" +
				"FTX+AAA+++GENERAL CARGO'" +
				// group 7
				"TDT+1++3++SDJ:172:87+++SB41AX:146:ZZZ'" +
				// group 0
				"CNT+16:1'" +
				"UNT+18+02940269001'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			AssertEquals("MessageSender.Code", "CSXWTADL", data.MessageSender.Code);
			AssertEquals("MessageSender.CodeType", CMMOrganisationType.Unknown, data.MessageSender.CodeType);
			AssertEquals("LloydsNumber", "9324837", data.LloydsNumber);
			AssertEquals("VoyageNumber", "018S", data.VoyageNumber);
			AssertEquals("Type", CMMMessageType.GateOut, data.Type);

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", 1, equipment.Count);
			AssertEquals("Equipment[0].ContainerNumber", "CAXU3295589", equipment[0].ContainerNumber);
			AssertEquals("Equipment[0].ISOType", "22G1", equipment[0].ISOType);
			AssertEquals("Equipment[0].IsEmpty", false, equipment[0].IsEmpty);
			AssertEquals("Equipment[0].BookingReference", "", equipment[0].BookingReference);
			AssertEquals("Equipment[0].BillOfLading", "BILL1", equipment[0].BillOfLading);
			AssertEquals("Equipment[0].GoodsDeclarationNumber", "", equipment[0].GoodsDeclarationNumber);
			AssertEquals("Equipment[0].GrossWeightKG", 20200m, equipment[0].GrossWeightKG);
			AssertEquals("Equipment[0].PositioningDateTime", new DateTime(2008, 06, 24, 19, 24, 00), equipment[0].PositioningDateTime);
			AssertEquals("Equipment[0].EquipmentSupplier", CMMEquipmentSupplier.Unknown, equipment[0].EquipmentSupplier);
			AssertContainsExactElementsInAnyOrder("Equipment[0].SealNumbers", Array.Empty<string>(), equipment[0].SealNumbers);
		}

		public void TestCODECOMessage36_Multi()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+1881+CODECO:D:95B:UN:ITG10'" +
				"BGM+36+1881+9'" +
				// group 1
				"NAD+MS+AALME:160:184'" +
				"NAD+CF+ACEPOOL'" +
				// group 5 - container 1
				"EQD+CN+CCLU4741410+42R0:102:5+++4'" +
				"RFF+BN:ACE TO CHINA'" +
				"DTM+7:200806260954:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				"FTX+ABS++50'" +
				"NAD+CN+++ACS'" +
				// group 5 - container 2
				"EQD+CN+CCLU4408380+42R0:102:5+++4'" +
				"RFF+BN:ACE TO CHINA'" +
				"DTM+7:200806261145:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				"FTX+ABS++50'" +
				// group 7
				"TDT+20++1++ACEPOOL:172:184+++:146'" +
				// group 5
				"NAD+CN+++ACS'" +
				// group 5 - container 3
				"EQD+CN+CCLU4595589+42R0:102:5+++4'" +
				"RFF+BN:ACE TO CHINA'" +
				"DTM+7:200806261525:203'" +
				"LOC+165+AUMEL:139:6'" +
				"FTX+DAR++2'" +
				"FTX+ABS++50'" +
				// group 7
				"TDT+20++1++ACEPOOL:172:184+++:146'" +
				// group 5
				"NAD+CN+++acs'" +
				// group 0
				"CNT+16:3'" +
				"UNT+30+1881'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "AALME", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.OneStop, data.MessageSender.CodeType);
				AssertEquals("LloydsNumber", null, data.LloydsNumber);
				AssertEquals("VoyageNumber", null, data.VoyageNumber);
				AssertEquals("Type", CMMMessageType.GateOut, data.Type);
			});

			var expected = new[]
			{
				new { ContainerNum = "CCLU4741410", PositioningDateTime = new DateTime(2008,06,26,09,54,00) },
				new { ContainerNum = "CCLU4408380", PositioningDateTime = new DateTime(2008,06,26,11,45,00) },
				new { ContainerNum = "CCLU4595589", PositioningDateTime = new DateTime(2008,06,26,15,25,00) },
			};

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", expected.Length, equipment.Count);

			CombineAssertions(delegate
			{
				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("Equipment[{0}].ContainerNumber", i), expected[i].ContainerNum, equipment[i].ContainerNumber);
					AssertEquals(string.Format("Equipment[{0}].ISOType", i), "42R0", equipment[i].ISOType);
					AssertEquals(string.Format("Equipment[{0}].IsEmpty", i), true, equipment[i].IsEmpty);
					AssertEquals(string.Format("Equipment[{0}].BookingReference", i), "ACE TO CHINA", equipment[0].BookingReference);
					AssertEquals(string.Format("Equipment[{0}].BillOfLading", i), "", equipment[0].BillOfLading);
					AssertEquals(string.Format("Equipment[{0}].GrossWeightInKG", i), null, equipment[i].GrossWeightKG);
					AssertEquals(string.Format("Equipment[{0}].PositioningDateTime", i), expected[i].PositioningDateTime, equipment[i].PositioningDateTime);
					AssertContainsExactElementsInAnyOrder(string.Format("Equipment[{0}].SealNumbers", i), Array.Empty<string>(), equipment[i].SealNumbers);
				}
			});
		}
	}
}
