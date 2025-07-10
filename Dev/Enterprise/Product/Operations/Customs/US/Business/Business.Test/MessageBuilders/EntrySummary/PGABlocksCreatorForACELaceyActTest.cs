using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForACELaceyActTest : PGABlocksCreatorTest
	{
		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample1()
		{
			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101475", true);

			invoiceLine.JI_Description = "USED RAILROAD TIES";

			var laceyAct = CreateNewLaceyActLine(invoiceLine, "USED MAPLE AND OAK RAILROAD TIES", 10000m);
			CreateNewConstituentElement(laceyAct, "ACER", "SACCHARUM", "MAPLE TIES", 100m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "QUERCUS", "RUBRA", "OAK TIES", 200m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "QUERCUS", "ALBA TIES", "OAK TIES", 300m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "ACER", "SACCHARUM", "MAPLE TIES", 400m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.UnitedStates);
			CreateNewConstituentElement(laceyAct, "QUERCUS", "RUBRA", "OAK TIES", 500m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.UnitedStates);
			CreateNewConstituentElement(laceyAct, "QUERCUS", "ALBA", "VACCINE MODIFIED ANKARA VIRUS (MVA)EXPRESSING GLYCOP", 500m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.UnitedStates);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        USED RAILROAD TIES                                                    
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   USED MAPLE AND OAK RAILROAD TIES                         
PG04 MAPLE TIES                                         000000010000KG          
PG05ACER                  SACCHARUM                                             
PG06HRVCA                                                                       
PG04 OAK TIES                                           000000020000KG          
PG05QUERCUS               RUBRA                                                 
PG06HRVCA                                                                       
PG04 OAK TIES                                           000000030000KG          
PG05QUERCUS               ALBA TIES                                             
PG06HRVCA                                                                       
PG04 MAPLE TIES                                         000000040000KG          
PG05ACER                  SACCHARUM                                             
PG06HRVUS                                                                       
PG04 OAK TIES                                           000000050000KG          
PG05QUERCUS               RUBRA                                                 
PG06HRVUS                                                                       
PG04 VACCINE MODIFIED ANKARA VIRUS (MVA)EXPRESSING GLYCO000000050000KG          
PG05QUERCUS               ALBA                                                  
PG06HRVUS                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000010000            
PG27WHXU4101474            WHXU4101475                                          
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample1a()
		{
			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			invoiceLine.JI_Description = "USED RAILROAD TIES";

			var laceyAct = CreateNewLaceyActLine(invoiceLine, "USED MAPLE AND OAK RAILROAD TIES", 2501m);
			CreateNewConstituentElement(laceyAct, "ACER", "SACCHARUM", "MAPLE TIES", 100m, ACELaceyUnitsOfMeasureList.Codes.CubicMillimeters, Core.Constants.CountryCodes.Canada);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        USED RAILROAD TIES                                                    
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   USED MAPLE AND OAK RAILROAD TIES                         
PG04 MAPLE TIES                                         000000010000MM3         
PG05ACER                  SACCHARUM                                             
PG06HRVCA                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000002501            
PG27WHXU4101474                                                                 
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample2()
		{
			SetUpData();

			invoiceLine.JI_Description = "WOODEN DESK";

			var laceyActOne = CreateNewLaceyActLine(invoiceLine, "WOODEN DESK MODEL 0415", 10000m);
			CreateNewConstituentElement(laceyActOne, "PINUS", "TAEDA", "PINE DESK TOPS LEGS SIDE DRAWERS", 100m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Canada);
			var laceyActTwo = CreateNewLaceyActLine(invoiceLine, "WOODEN DESK MODEL 0522", 20000m);
			CreateNewConstituentElement(laceyActTwo, "PINUS", "TAEDA", "PINE DESK TOPS LEGS SIDE DRAWERS", 200m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Canada);
			var laceyActThree = CreateNewLaceyActLine(invoiceLine, "PINE DESK MODEL 0553", 30000m);
			CreateNewConstituentElement(laceyActThree, "PINUS", "TAEDA", "PINE DESK TOPS LEGS SIDE DRAWERS", 300m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedStates);
			CreateNewConstituentElement(laceyActThree, "PINUS", "TAEDA", "PINE DESK TOPS LEGS SIDE", 400m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Canada);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        WOODEN DESK                                                           
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN DESK MODEL 0415                                   
PG04 PINE DESK TOPS LEGS SIDE DRAWERS                   000000010000M3          
PG05PINUS                 TAEDA                                                 
PG06HRVCA                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000010000            
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN DESK MODEL 0522                                   
PG04 PINE DESK TOPS LEGS SIDE DRAWERS                   000000020000M3          
PG05PINUS                 TAEDA                                                 
PG06HRVCA                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000020000            
PG01003APHAPL                                                                   
PG02P                                                                           
PG10                   PINE DESK MODEL 0553                                     
PG04 PINE DESK TOPS LEGS SIDE DRAWERS                   000000030000M3          
PG05PINUS                 TAEDA                                                 
PG06HRVUS                                                                       
PG04 PINE DESK TOPS LEGS SIDE                           000000040000M3          
PG05PINUS                 TAEDA                                                 
PG06HRVCA                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000030000            
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample3()
		{
			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			invoiceLine.JI_Description = "SOFTWOOD PULPWOOD";

			var laceyAct = CreateNewLaceyActLine(invoiceLine, "EUROPEAN UNION PINE PULPWOOD", 30000m, true);
			CreateNewConstituentElement(laceyAct, "PINUS", "TAEDA", "UK PINE PULPWOOD", 34m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewConstituentElement(laceyAct, "PINUS", "RIGIDA", "UK PINE PULPWOOD", 17m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewConstituentElement(laceyAct, "PINUS", "ECHINADA", "UK PINE PULPWOOD", 49m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewConstituentElement(laceyAct, "PINUS", "TAEDA", "FRENCH PINE PULPWOOD", 67m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.France);
			CreateNewConstituentElement(laceyAct, "PINUS", "RIGIDA", "FRENCH PINE PULPWOOD", 33m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.France);
			CreateNewConstituentElement(laceyAct, "PINUS", "ECHINADA", "FRENCH PINE PULPWOOD", 100m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.France);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   EUROPEAN UNION PINE PULPWOOD                             
PG04YUK PINE PULPWOOD                                   000000003400M3          
PG05PINUS                 TAEDA                                                 
PG06HRVGB                                                                       
PG04YUK PINE PULPWOOD                                   000000001700M3          
PG05PINUS                 RIGIDA                                                
PG06HRVGB                                                                       
PG04YUK PINE PULPWOOD                                   000000004900M3          
PG05PINUS                 ECHINADA                                              
PG06HRVGB                                                                       
PG04YFRENCH PINE PULPWOOD                               000000006700M3          
PG05PINUS                 TAEDA                                                 
PG06HRVFR                                                                       
PG04YFRENCH PINE PULPWOOD                               000000003300M3          
PG05PINUS                 RIGIDA                                                
PG06HRVFR                                                                       
PG04YFRENCH PINE PULPWOOD                               000000010000M3          
PG05PINUS                 ECHINADA                                              
PG06HRVFR                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000030000            
PG27WHXU4101474                                                                 
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample4()
		{
			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			invoiceLine.JI_Description = "SOFTWOOD PULPWOOD";

			var laceyActOne = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 20000m, true);
			CreateNewConstituentElement(laceyActOne, "PINUS", "ECHINADA", "EU PINE PULPWOOD", 66m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewConstituentElement(laceyActOne, "PINUS", "ECHINADA", "EU PINE PULPWOOD", 56m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.France);
			CreateNewConstituentElement(laceyActOne, "PINUS", "ECHINADA", "EU PINE PULPWOOD", 49m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Germany);

			var laceyActTwo = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 30000m, true);
			CreateNewConstituentElement(laceyActTwo, "PINUS", "ECHINADA", "NA PINE PULPWOOD", 15m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter);
			CreateNewConstituentElement(laceyActTwo, "PINUS", "ECHINADA", "NA PINE PULPWOOD", 200m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter);
			CreateNewConstituentElement(laceyActTwo, "PINUS", "ECHINADA", "NA PINE PULPWOOD", 85m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   SOFTWOOD PULPWOOD                                        
PG04YEU PINE PULPWOOD                                   000000006600M3          
PG05PINUS                 ECHINADA                                              
PG06HRVGB                                                                       
PG04YEU PINE PULPWOOD                                   000000005600M3          
PG05PINUS                 ECHINADA                                              
PG06HRVFR                                                                       
PG04YEU PINE PULPWOOD                                   000000004900M3          
PG05PINUS                 ECHINADA                                              
PG06HRVDE                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000020000            
PG27WHXU4101474                                                                 
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   SOFTWOOD PULPWOOD                                        
PG04YNA PINE PULPWOOD                                   000000001500M3          
PG05PINUS                 ECHINADA                                              
PG04YNA PINE PULPWOOD                                   000000020000M3          
PG05PINUS                 ECHINADA                                              
PG04YNA PINE PULPWOOD                                   000000008500M3          
PG05PINUS                 ECHINADA                                              
PG22             IM AP6 Y05042016                                               
PG25                                                    000000030000            
PG27WHXU4101474                                                                 
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample5()
		{
			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			invoiceLine.JI_Description = "SOFTWOOD PULPWOOD";

			var laceyAct = CreateNewLaceyActLine(invoiceLine, "NORTH AMERICAN SOFTWOOD PULPWOOD", 30000m, true);
			CreateNewConstituentElement(laceyAct, "PINUS", "ECHINADA", "PINE PULPWOOD", 50m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "PINUS", "ECHINADA", "PINE PULPWOOD", 42m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedStates);
			CreateNewConstituentElement(laceyAct, "PINUS", "TAEDA", "PINE PULPWOOD", 83m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "PINUS", "TAEDA", "PINE PULPWOOD", 37m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedStates);
			CreateNewConstituentElement(laceyAct, "PINUS", "RIGIDA", "PINE PULPWOOD", 201m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "PINUS", "RIGIDA", "PINE PULPWOOD", 137m, ACELaceyUnitsOfMeasureList.Codes.CubicMeter, Core.Constants.CountryCodes.UnitedStates);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   NORTH AMERICAN SOFTWOOD PULPWOOD                         
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS                 ECHINADA                                              
PG06HRVCA                                                                       
PG04YPINE PULPWOOD                                      000000004200M3          
PG05PINUS                 ECHINADA                                              
PG06HRVUS                                                                       
PG04YPINE PULPWOOD                                      000000008300M3          
PG05PINUS                 TAEDA                                                 
PG06HRVCA                                                                       
PG04YPINE PULPWOOD                                      000000003700M3          
PG05PINUS                 TAEDA                                                 
PG06HRVUS                                                                       
PG04YPINE PULPWOOD                                      000000020100M3          
PG05PINUS                 RIGIDA                                                
PG06HRVCA                                                                       
PG04YPINE PULPWOOD                                      000000013700M3          
PG05PINUS                 RIGIDA                                                
PG06HRVUS                                                                       
PG22             IM AP6 Y05042016                                               
PG25                                                    000000030000            
PG27WHXU4101474                                                                 
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 5, 4)]
		public void TestACELaceyActExample5a()
		{
			SetUpData();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			invoiceLine.JI_Description = "SOFTWOOD PULPWOOD";

			var laceyActOne = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 30000m, false, true, "EU PINE PULPWOOD", 100m, ACELaceyUnitsOfMeasureList.Codes.Kilogram);
			CreateNewConstituentElement(laceyActOne, "PINUS", "TAEDA");
			CreateNewConstituentElement(laceyActOne, "PINUS", "RIGIDA");
			CreateNewConstituentElement(laceyActOne, "PINUS", "ECHINADA");
			CreateNewCountry(laceyActOne, Core.Constants.CountryCodes.UnitedKingdom);

			var laceyActTwo = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 30000m, false, true, "EU PINE PULPWOOD", 200m, ACELaceyUnitsOfMeasureList.Codes.Kilogram);
			CreateNewConstituentElement(laceyActTwo, "PINUS", "TAEDA");
			CreateNewCountry(laceyActOne, Core.Constants.CountryCodes.France);
			CreateNewCountry(laceyActOne, Core.Constants.CountryCodes.Germany);
			CreateNewCountry(laceyActOne, Core.Constants.CountryCodes.Belgium);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   SOFTWOOD PULPWOOD                                        
PG22             IM AP6 Y05042016                                               
PG25                                                    000000030000            
PG04 EU PINE PULPWOOD                                                           
PG50                                                                            
PG05PINUS                 TAEDA                                                 
PG05PINUS                 RIGIDA                                                
PG05PINUS                 ECHINADA                                              
PG06HRVGB                                                                       
PG06HRVFR                                                                       
PG06HRVDE                                                                       
PG06HRVBE                                                                       
PG29KG 000000010000                                                             
PG51                                                                            
PG27WHXU4101474                                                                 
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   SOFTWOOD PULPWOOD                                        
PG22             IM AP6 Y05042016                                               
PG25                                                    000000030000            
PG04 EU PINE PULPWOOD                                                           
PG50                                                                            
PG05PINUS                 TAEDA                                                 
PG29KG 000000020000                                                             
PG51                                                                            
PG27WHXU4101474                                                                 
";
			AssertContains("ACE Entry Summary message including ACE Lacey data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2016, 7, 1)]
		public void TestACELaceyImporterEntityNameAndEmailOverflow()
		{
			SetUpData();

			var iorOrgHeader = Factory.New<OrgHeader>();
			declaration.IOROrgPK = iorOrgHeader.PK;
			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "IOR FIRST", "IAN TEST LONG OVERLOW NAME", "04 123456", "THIS.IS.A.VERY.LONG.EMAIL@ABCDEFGHI.COM", null);
			invoiceLine.JI_Description = "SOFTWOOD PULPWOOD";

			var laceyActOne = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 30000m, false, true, "EU PINE PULPWOOD", 100m, ACELaceyUnitsOfMeasureList.Codes.Kilogram);
			CreateNewConstituentElement(laceyActOne, "PINUS", "TAEDA");
			CreateNewCountry(laceyActOne, Core.Constants.CountryCodes.UnitedKingdom);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   SOFTWOOD PULPWOOD                                        
PG19IM                                                                          
PG20                                                             US             
PG21IM IOR FIRST IAN TEST LONG04123456       THIS.IS.A.VERY.LONG.EMAIL@ABCDEFGHI
PG60INA OVERLOW NAME                                                            
PG60EMA.COM                                                                     
PG22             IM AP6 Y07012016                                               
PG25                                                    000000030000            
PG04 EU PINE PULPWOOD                                                           
PG50                                                                            
PG05PINUS                 TAEDA                                                 
PG06HRVGB                                                                       
PG29KG 000000010000                                                             
PG51                                                                            
";
			AssertContains("PG60 blocks for entity name and email should be generated", expectedMessage, message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
		}

		PGA CreateNewLaceyActLine(JobComInvoiceLine invoiceLine, ZString commercialDescription, ZDecimal lineValue, bool unknowBreakdown = false, bool unknownBreakdownTotal = false, string nameOfConstituent = "", decimal quantity = 0m, string unitOfMeasure = "")
		{
			var result = invoiceLine.LaceyActLines.AddNew();
			result.US_PGACommercialDescription = commercialDescription;
			result.US_InvCurrPGAValue = lineValue;
			result.US_UnknownBreakdown = unknowBreakdown;
			result.US_UnknownBreakdownTotal = unknownBreakdownTotal;
			result.US_NameOfConstituentElement = nameOfConstituent;
			result.US_QuantityOfConstituentElement = quantity;
			result.US_UnitOfMeasure = unitOfMeasure;

			return result;
		}

		ConstituentElement CreateNewConstituentElement(PGA pga, ZString genusName, ZString speciesName, string name = "", decimal quantity = 0m, string unitOfMeasure = "", string countryCode = "")
		{
			var result = pga.PG04ConstituentElements.AddNew();
			result.US_PGANameOfTheConstituentElement = name;
			result.US_PGAQuantityOfConstituentElement = quantity;
			result.US_PGAUnitOfMeasure = unitOfMeasure;
			result.US_GenusName = genusName;
			result.US_SpeciesName = speciesName;
			result.US_UnknownBreakdownCountryCode = countryCode;

			return result;
		}

		LaceyCountry CreateNewCountry(PGA pga, ZString countryCode)
		{
			var result = pga.LaceyCountries.AddNew();
			result.US_CountryCode = countryCode;

			return result;
		}
	}
}
