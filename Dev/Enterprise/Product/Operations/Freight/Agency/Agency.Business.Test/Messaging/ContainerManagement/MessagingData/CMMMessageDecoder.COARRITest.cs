using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	partial class CMMMessageDecoderTest
	{
		public void TestCOARRIMessage98()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+718+COARRI:D:95B:UN:ITG10'" +
				"BGM+98+13910559+9'" +
				// group 1
				"TDT+20+0084S+1++CSC:172:184+++9224336:146'" +
				"LOC+11+AUSYD'" +
				// group 2
				"NAD+CA+CSC:160:184'" +
				"NAD+MS+CTLPB:160:ZZZ'" +
				// group 3
				"EQD+CN+CCLU4798734+4300:102:5+1++5'" +
				"RFF+BM:Bill1'" +
				"DTM+203:200804051148:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0501190::5'" +
				"MEA+AAE+G+KGM:6800'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU6959308+4500:102:5+2++5'" +
				"RFF+BM:Bill2'" +
				"DTM+203:200804051154:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0500590::5'" +
				"MEA+AAE+G+KGM:9400'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU4554430+4300:102:5+++5'" +
				"RFF+BM:Bill3'" +
				"DTM+203:200804051202:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0500986::5'" +
				"MEA+AAE+G+KGM:13300'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU4479697+4300:102:5+++5'" +
				"RFF+BM:Bill4'" +
				"DTM+203:200804051207:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0501184::5'" +
				"MEA+AAE+G+KGM:12100'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 0
				"CNT+16:4'" +
				"UNT+44+718'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "CTLPB", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.MutuallyDefined, data.MessageSender.CodeType);
				AssertEquals("MessageSender.VoyageNumber", "0084S", data.VoyageNumber);
				AssertEquals("MessageSender.LloydsNumber", "9224336", data.LloydsNumber);
				AssertEquals("Type", CMMMessageType.Discharge, data.Type);
			});

			var expected = new[]
			{
				new
				{
					ContainerNum = "CCLU4798734", IsoType = "4300", BillOfLading="Bill1", GrossWeightKG = 6800m,
					PositioningDateTime = new DateTime(2008, 04, 05, 11, 48, 00),
					Supplier = CMMEquipmentSupplier.Shipper,
				},
				new
				{
					ContainerNum = "CCLU6959308", IsoType = "4500", BillOfLading="Bill2", GrossWeightKG = 9400m,
					PositioningDateTime = new DateTime(2008, 04, 05, 11, 54, 00),
					Supplier = CMMEquipmentSupplier.Carrier,
				},
				new
				{
					ContainerNum = "CCLU4554430", IsoType = "4300", BillOfLading="Bill3", GrossWeightKG = 13300m,
					PositioningDateTime = new DateTime(2008, 04, 05, 12, 02, 00),
					Supplier = CMMEquipmentSupplier.Unknown,
				},
				new
				{
					ContainerNum = "CCLU4479697", IsoType = "4300", BillOfLading="Bill4", GrossWeightKG = 12100m,
					PositioningDateTime = new DateTime(2008, 04, 05, 12, 07, 00),
					Supplier = CMMEquipmentSupplier.Unknown,
				},
			};

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", expected.Length, equipment.Count);

			CombineAssertions(delegate
			{
				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("Equipment[{0}].ContainerNumber", i), expected[i].ContainerNum, equipment[i].ContainerNumber);
					AssertEquals(string.Format("Equipment[{0}].ISOType", i), expected[i].IsoType, equipment[i].ISOType);
					AssertEquals(string.Format("Equipment[{0}].IsEmpty", i), false, equipment[i].IsEmpty);
					AssertEquals(string.Format("Equipment[{0}].BookingReference", i), "", equipment[i].BookingReference);
					AssertEquals(string.Format("Equipment[{0}].BillOfLading", i), equipment[i].BillOfLading, equipment[i].BillOfLading);
					AssertEquals(string.Format("Equipment[{0}].GoodsDeclarationNumber", i), "", equipment[i].GoodsDeclarationNumber);
					AssertEquals(string.Format("Equipment[{0}].GrossWeightKG", i), expected[i].GrossWeightKG, equipment[i].GrossWeightKG);
					AssertEquals(string.Format("Equipment[{0}].PositioningDateTime", i), expected[i].PositioningDateTime, equipment[i].PositioningDateTime);
					AssertEquals(string.Format("Equipment[{0}].EquipmentSupplier", i), expected[i].Supplier, equipment[i].EquipmentSupplier);
					AssertContainsExactElementsInAnyOrder(string.Format("Equipment[{0}].SealNumbers", i), Array.Empty<string>(), equipment[i].SealNumbers);
				}
			});
		}

		public void TestCOARRIMessage270()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+719+COARRI:D:95B:UN:ITG10'" +
				"BGM+270+13910566+9'" +
				// group 1
				"TDT+20+0085N+1++CSC:172:184+++9224336:146'" +
				"LOC+9+AUSYD'" +
				// group 2
				"NAD+CA+CSC:160:184'" +
				"NAD+MS+CTLPB:160:184'" +
				// group 3
				"EQD+CN+GESU4760670+45G0:102:5+++5'" +
				"RFF+BN:CSG119982'" +
				"DTM+203:200804051154:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340212::5'" +
				"MEA+AAE+G+KGM:24500'" +
				"SEL+654983'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU4084737+42G0:102:5+++4'" +
				"RFF+BN:CQD85PO'" +
				"DTM+203:200804051155:203'" +
				"LOC+11+CNSHA'" +
				"LOC+147+0340114::5'" +
				"MEA+AAE+G+KGM:4000'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU8601144+45R1:102:5+++5'" +
				"RFF+BN:CSG122105'" +
				"DTM+203:200804051224:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340282::5'" +
				"MEA+AAE+G+KGM:29200'" +
				"TMP+2+-20:CEL'" +
				"SEL+858 463'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU8537810+45R1:102:5+++5'" +
				"RFF+BN:CSG122269'" +
				"DTM+203:200804051224:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340182::5'" +
				"MEA+AAE+G+KGM:13600'" +
				"TMP+2+20:CEL'" +
				"SEL+L815443'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU8537301+45R1:102:5+++5'" +
				"RFF+BN:CSG122269'" +
				"DTM+203:200804051224:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340084::5'" +
				"MEA+AAE+G+KGM:7400'" +
				"TMP+2+20:CEL'" +
				"SEL+L815454'" +
				"NAD+CF+CSC:160:184'" +
				// group 0
				"CNT+16:5'" +
				"UNT+50+719'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "CTLPB", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.OneStop, data.MessageSender.CodeType);
				AssertEquals("MessageSender.VoyageNumber", "0085N", data.VoyageNumber);
				AssertEquals("MessageSender.LloydsNumber", "9224336", data.LloydsNumber);
				AssertEquals("Type", CMMMessageType.Load, data.Type);
			});

			var expected = new[]
			{
				new
				{
					ContainerNum = "GESU4760670", IsoType = "45G0", BookingRef = "CSG119982",
					GrossWeightKG = 24500m, PositioningDateTime = new DateTime(2008, 04, 05, 11, 54, 00),
					Seals = new string[] { "654983" },
				},
				new
				{
					ContainerNum = "CCLU4084737", IsoType = "42G0", BookingRef = "CQD85PO",
					GrossWeightKG = 4000m, PositioningDateTime = new DateTime(2008, 04, 05, 11, 55, 00),
					Seals = Array.Empty<string>(),
				},
				new
				{
					ContainerNum = "CCLU8601144", IsoType = "45R1", BookingRef = "CSG122105",
					GrossWeightKG = 29200m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 24, 00),
					Seals = new string[] { "858 463" },
				},
				new
				{
					ContainerNum = "CCLU8537810", IsoType = "45R1", BookingRef = "CSG122269",
					GrossWeightKG = 13600m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 24, 00),
					Seals = new string[] { "L815443" },
				},
				new
				{
					ContainerNum = "CCLU8537301", IsoType = "45R1", BookingRef = "CSG122269",
					GrossWeightKG = 7400m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 24, 00),
					Seals = new string[] { "L815454" },
				},
			};

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", expected.Length, equipment.Count);

			CombineAssertions(delegate
			{
				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("Equipment[{0}].ContainerNumber", i), expected[i].ContainerNum, equipment[i].ContainerNumber);
					AssertEquals(string.Format("Equipment[{0}].ISOType", i), expected[i].IsoType, equipment[i].ISOType);
					AssertEquals(string.Format("Equipment[{0}].IsEmpty", i), i == 1, equipment[i].IsEmpty);
					AssertEquals(string.Format("Equipment[{0}].BookingReference", i), expected[i].BookingRef, equipment[i].BookingReference);
					AssertEquals(string.Format("Equipment[{0}].BillOfLading", i), "", equipment[i].BillOfLading);
					AssertEquals(string.Format("Equipment[{0}].GoodsDesclarationNumber", i), "", equipment[i].GoodsDeclarationNumber);
					AssertEquals(string.Format("Equipment[{0}].GrossWeightKG", i), expected[i].GrossWeightKG, equipment[i].GrossWeightKG);
					AssertEquals(string.Format("Equipment[{0}].PositioningDateTime", i), expected[i].PositioningDateTime, equipment[i].PositioningDateTime);
					AssertContainsExactElementsInAnyOrder(string.Format("Equipment[{0}].SealNumbers", i), expected[i].Seals, equipment[i].SealNumbers);
				}
			});
		}

		public void TestCOARRIMessage46()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+718+COARRI:D:95B:UN:ITG10'" +
				"BGM+46+13910559+9'" +
				// group 1
				"TDT+20+0084S+1++CSC:172:184+++9224336:146'" +
				"LOC+11+AUSYD'" +
				// group 2
				"NAD+CA+CSC:160:184'" +
				"NAD+MS+CTLPB:160:ZZZ'" +
				// group 3
				"EQD+CN+CCLU4798734+4300:102:5+++5'" +
				"DTM+203:200804051148:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0501190::5'" +
				"MEA+AAE+G+KGM:6800'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU6959308+4500:102:5+++5'" +
				"DTM+203:200804051154:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0500590::5'" +
				"MEA+AAE+G+KGM:9400'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU4554430+4300:102:5+++5'" +
				"DTM+203:200804051202:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0500986::5'" +
				"MEA+AAE+G+KGM:13300'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU4479697+4300:102:5+++5'" +
				"DTM+203:200804051207:203'" +
				"LOC+7+AUSYD'" +
				"LOC+9+CNSHK'" +
				"LOC+11+AUSYD'" +
				"LOC+147+0501184::5'" +
				"MEA+AAE+G+KGM:12100'" +
				"FTX+AAA+++GEN'" +
				"NAD+CF+CSC:160:184'" +
				// group 0
				"CNT+16:4'" +
				"UNT+44+718'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "CTLPB", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.MutuallyDefined, data.MessageSender.CodeType);
				AssertEquals("MessageSender.VoyageNumber", "0084S", data.VoyageNumber);
				AssertEquals("MessageSender.LloydsNumber", "9224336", data.LloydsNumber);
				AssertEquals("Type", CMMMessageType.Load, data.Type);
			});

			var expected = new[]
			{
				new { ContainerNum = "CCLU4798734", IsoType = "4300", GrossWeightKG = 6800m, PositioningDateTime = new DateTime(2008, 04, 05, 11, 48, 00) },
				new { ContainerNum = "CCLU6959308", IsoType = "4500", GrossWeightKG = 9400m, PositioningDateTime = new DateTime(2008, 04, 05, 11, 54, 00) },
				new { ContainerNum = "CCLU4554430", IsoType = "4300", GrossWeightKG = 13300m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 02, 00) },
				new { ContainerNum = "CCLU4479697", IsoType = "4300", GrossWeightKG = 12100m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 07, 00) },
			};

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", expected.Length, equipment.Count);

			CombineAssertions(delegate
			{
				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("Equipment[{0}].ContainerNumber", i), expected[i].ContainerNum, equipment[i].ContainerNumber);
					AssertEquals(string.Format("Equipment[{0}].ISOType", i), expected[i].IsoType, equipment[i].ISOType);
					AssertEquals(string.Format("Equipment[{0}].IsEmpty", i), false, equipment[i].IsEmpty);
					AssertEquals(string.Format("Equipment[{0}].BookingReference", i), "", equipment[i].BookingReference);
					AssertEquals(string.Format("Equipment[{0}].BillOfLading", i), "", equipment[i].BillOfLading);
					AssertEquals(string.Format("Equipment[{0}].GoodsDeclarationNumber", i), "", equipment[i].GoodsDeclarationNumber);
					AssertEquals(string.Format("Equipment[{0}].GrossWeightKG", i), expected[i].GrossWeightKG, equipment[i].GrossWeightKG);
					AssertEquals(string.Format("Equipment[{0}].PositioningDateTime", i), expected[i].PositioningDateTime, equipment[i].PositioningDateTime);
					AssertContainsExactElementsInAnyOrder(string.Format("Equipment[{0}].SealNumbers", i), Array.Empty<string>(), equipment[i].SealNumbers);
				}
			});
		}

		public void TestCOARRIMessage44()
		{
			#region messageText

			const string messageText =
				// group 0
				"UNH+719+COARRI:D:95B:UN:ITG10'" +
				"BGM+44+13910566+9'" +
				// group 1
				"TDT+20+0085N+1++CSC:172:184+++9224336:146'" +
				"LOC+9+AUSYD'" +
				// group 2
				"NAD+CA+CSC:160:184'" +
				"NAD+MS+CTLPB:160:184'" +
				// group 3
				"EQD+CN+GESU4760670+45G0:102:5+++5'" +
				"RFF+BN:CSG119982'" +
				"DTM+203:200804051154:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340212::5'" +
				"MEA+AAE+G+KGM:24500'" +
				"SEL+654983'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU4084737+42G0:102:5+++4'" +
				"RFF+BN:CQD85PO'" +
				"DTM+203:200804051155:203'" +
				"LOC+11+CNSHA'" +
				"LOC+147+0340114::5'" +
				"MEA+AAE+G+KGM:4000'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU8601144+45R1:102:5+++5'" +
				"RFF+BN:CSG122105'" +
				"DTM+203:200804051224:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340282::5'" +
				"MEA+AAE+G+KGM:29200'" +
				"TMP+2+-20:CEL'" +
				"SEL+858 463'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU8537810+45R1:102:5+++5'" +
				"RFF+BN:CSG122269'" +
				"DTM+203:200804051224:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340182::5'" +
				"MEA+AAE+G+KGM:13600'" +
				"TMP+2+20:CEL'" +
				"SEL+L815443'" +
				"NAD+CF+CSC:160:184'" +
				// group 3
				"EQD+CN+CCLU8537301+45R1:102:5+++5'" +
				"RFF+BN:CSG122269'" +
				"DTM+203:200804051224:203'" +
				"LOC+11+TWKHH'" +
				"LOC+147+0340084::5'" +
				"MEA+AAE+G+KGM:7400'" +
				"TMP+2+20:CEL'" +
				"SEL+L815454'" +
				"NAD+CF+CSC:160:184'" +
				// group 0
				"CNT+16:5'" +
				"UNT+50+719'" +
				"";

			#endregion

			ICMMMessagingData data = CMMMessageDecoder.Parse(messageText);

			CombineAssertions(delegate
			{
				AssertEquals("MessageSender.Code", "CTLPB", data.MessageSender.Code);
				AssertEquals("MessageSender.CodeType", CMMOrganisationType.OneStop, data.MessageSender.CodeType);
				AssertEquals("MessageSender.VoyageNumber", "0085N", data.VoyageNumber);
				AssertEquals("MessageSender.LloydsNumber", "9224336", data.LloydsNumber);
				AssertEquals("Type", CMMMessageType.Discharge, data.Type);
			});

			var expected = new[]
			{
				new
				{
					ContainerNum = "GESU4760670", IsoType = "45G0", BookingRef = "CSG119982",
					GrossWeightInKG = 24500m, PositioningDateTime = new DateTime(2008, 04, 05, 11, 54, 00),
					Seals = new string[] { "654983" },
				},
				new
				{
					ContainerNum = "CCLU4084737", IsoType = "42G0", BookingRef = "CQD85PO",
					GrossWeightInKG = 4000m, PositioningDateTime = new DateTime(2008, 04, 05, 11, 55, 00),
					Seals = Array.Empty<string>(),
				},
				new
				{
					ContainerNum = "CCLU8601144", IsoType = "45R1", BookingRef = "CSG122105",
					GrossWeightInKG = 29200m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 24, 00),
					Seals = new string[] { "858 463" },
				},
				new
				{
					ContainerNum = "CCLU8537810", IsoType = "45R1", BookingRef = "CSG122269",
					GrossWeightInKG = 13600m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 24, 00),
					Seals = new string[] { "L815443" },
				},
				new
				{
					ContainerNum = "CCLU8537301", IsoType = "45R1", BookingRef = "CSG122269",
					GrossWeightInKG = 7400m, PositioningDateTime = new DateTime(2008, 04, 05, 12, 24, 00),
					Seals = new string[] { "L815454" },
				},
			};

			List<ICMMEquipmentData> equipment = new List<ICMMEquipmentData>(data.Equipment);
			AssertEquals("Equipment.Count", expected.Length, equipment.Count);

			CombineAssertions(delegate
			{
				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("Equipment[{0}].ContainerNumber", i), expected[i].ContainerNum, equipment[i].ContainerNumber);
					AssertEquals(string.Format("Equipment[{0}].ISOType", i), expected[i].IsoType, equipment[i].ISOType);
					AssertEquals(string.Format("Equipment[{0}].IsEmpty", i), i == 1, equipment[i].IsEmpty);
					AssertEquals(string.Format("Equipment[{0}].BookingReference", i), expected[i].BookingRef, equipment[i].BookingReference);
					AssertEquals(string.Format("Equipment[{0}].BillOfLading", i), "", equipment[i].BillOfLading);
					AssertEquals(string.Format("Equipment[{0}].GoodsDeclarationNumber", i), "", equipment[i].GoodsDeclarationNumber);
					AssertEquals(string.Format("Equipment[{0}].GrossWeightKG", i), expected[i].GrossWeightInKG, equipment[i].GrossWeightKG);
					AssertEquals(string.Format("Equipment[{0}].PositioningDateTime", i), expected[i].PositioningDateTime, equipment[i].PositioningDateTime);
					AssertContainsExactElementsInAnyOrder(string.Format("Equipment[{0}].SealNumbers", i), expected[i].Seals, equipment[i].SealNumbers);
				}
			});
		}
	}
}
