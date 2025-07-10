using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Constants = Enterprise.Core.Constants;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using ValidationRule = Enterprise.DocumentVisualizer.Core.ValidationRule;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightLibraryTest : TestCaseWithFactory
	{
		#region IsValidContainerNumber

		public void TestIsValidContainerNumber()
		{
			var dummy = new Dummy
			{
				Code = "CONT123456789"
			};

			var data = dummy.MakeDynamic();

			var code = data.GetDynamicProperty("Code");

			var rules = GetRules(code);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Code.ErrorIf(!IsValidContainerNumber, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<FreightLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(code);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			code.SetValue("CONT3545458");

			validationResult = rules[0](code);

			AssertNull("rule result", validationResult);
		}

		#endregion

		#region IsValidISOContainer

		public void TestIsValidISOContainer()
		{
			var dummy = new Dummy
			{
				Code = "42RR"
			};

			var data = dummy.MakeDynamic();

			var code = data.GetDynamicProperty("Code");

			var rules = GetRules(code);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Code.ErrorIf(!IsValidISOContainer, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<FreightLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(code);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			code.SetValue("42R3");

			validationResult = rules[0](code);

			AssertNull("rule result", validationResult);
		}

		#endregion

		#region IsValidPackType

		public void TestIsValidPackType()
		{
			var dummy = new Dummy
			{
				Code = "XYZ"
			};

			var data = dummy.MakeDynamic();

			var code = data.GetDynamicProperty("Code");

			var rules = GetRules(code);

			AssertEquals("no rules", 0, rules.Length);

			const string macro = "Code.ErrorIf(!IsValidPackType, \"validation message\")";

			var expr = macro
				.With<MetaDataLibrary>()
				.And<FreightLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			rules = GetRules(code);

			AssertEquals("one rule", 1, rules.Length);

			var validationResult = rules[0](code);

			AssertEquals("rule result", NotificationType.Error, validationResult.Type);
			AssertEquals("rule result", "validation message", validationResult.Message);

			code.SetValue("PKG");

			validationResult = rules[0](code);

			AssertNull("rule result", validationResult);
		}

		#endregion

		#region DepartureConsol

		public void TestGetDepartureConsol_SingleConsol()
		{
			var consol = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingConsol, "CON0000001");
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000001");
		}

		public void TestGetDepartureConsol_ResolveConsolAmbiguity_OnLoad()
		{
			var consol1 = CreateShipment("AUSYD", "AUBNE", DataContextType.ForwardingConsol, "CON0000001");
			var consol2 = CreateShipment("AUSYD", "USCHI", DataContextType.ForwardingConsol, "CON0000002");
			var consol3 = CreateShipment("AUBNE", "CNSHA", DataContextType.ForwardingConsol, "CON0000003");

			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol1,
				consol2,
				consol3
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000001");
		}

		public void TestGetDepartureConsol_ResolveConsolAmbiguity_OnTransit()
		{
			var consol1 = CreateShipment("AUSYD", "AUBNE", DataContextType.ForwardingConsol, "CON0000001");
			var consol2 = CreateShipment("AUBNE", "USCHI", DataContextType.ForwardingConsol, "CON0000002");
			var consol3 = CreateShipment("USCHI", "CNSHA", DataContextType.ForwardingConsol, "CON0000003");
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol1,
				consol2,
				consol3
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000001");

			var consol4 = CreateShipment("AUMEL", "CNNJG", DataContextType.ForwardingConsol, "CON0000004");

			shipment.SetParentShipmentCollection(() => new List<Shipment>()
			{
				consol1,
				consol2,
				consol3,
				consol4,
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000001");
		}

		public void TestGetDepartureConsolMatchesTransportMode_DirectMatch()
		{
			var consol1 = CreateShipment("AUSYD", "AUBNE", DataContextType.ForwardingConsol, "CON0000001", Constants.TransportModes.Road);
			var consol2 = CreateShipment("AUBNE", "USCHI", DataContextType.ForwardingConsol, "CON0000002", Constants.TransportModes.Sea);
			var consol3 = CreateShipment("USCHI", "CNSHA", DataContextType.ForwardingConsol, "CON0000003");
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001", Constants.TransportModes.Sea);

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol1,
				consol2,
				consol3
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000002");
		}

		public void TestGetDepartureConsolMatchesTransportMode_MatchesCountry()
		{
			var consol1 = CreateShipment("AUSYD", "AUBNE", DataContextType.ForwardingConsol, "CON0000001", Constants.TransportModes.Road);
			var consol2 = CreateShipment("AUBNE", "USCHI", DataContextType.ForwardingConsol, "CON0000002", Constants.TransportModes.Sea);
			var consol3 = CreateShipment("USCHI", "CNSHA", DataContextType.ForwardingConsol, "CON0000003");
			var shipment = CreateShipment("AUMEL", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001", Constants.TransportModes.Sea);

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol1,
				consol2,
				consol3
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000002");
		}

		public void TestGetDepartureConsol_ResolveConsolAmbiguity_OnArrival()
		{
			var consol1 = CreateShipment("USCHI", "USLAX", DataContextType.ForwardingConsol, "CON0000001");
			var consol2 = CreateShipment("USLAX", "CNGZG", DataContextType.ForwardingConsol, "CON0000002");
			var consol3 = CreateShipment("CNGZG", "CNSHA", DataContextType.ForwardingConsol, "CON0000003");
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol1,
				consol2,
				consol3
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000001");

			var consol4 = CreateShipment("HKHKG", "CNSHA", DataContextType.ForwardingConsol, "CON0000004");

			shipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol1,
				consol2,
				consol3,
				consol4,
			});

			AssertGetDepartureConsolMacroRun(shipment, "CON0000001");
		}

		public void TestGetDepartureConsol_RecursiveMatch()
		{
			var consol = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingConsol, "CON0000001");
			var masterShipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");
			var subShipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000002");

			masterShipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				consol
			});

			subShipment.SetParentShipmentCollection(() => new List<Shipment>
			{
				masterShipment
			});

			AssertGetDepartureConsolMacroRun(subShipment, "CON0000001");
		}

		Shipment CreateShipment(ZString loadingPort, ZString dischargePort, DataContextType dataType, ZString uniqueRef, string transportMode = "")
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransportMode = new CodeDescriptionPair() { Code = transportMode },
				PortOfLoading = new UNLOCO
				{
					Code = loadingPort
				},
				PortOfDischarge = new UNLOCO
				{
					Code = dischargePort
				},
				AgentsReference = uniqueRef,
				DataContext = DataContextFactory.New()
			};

			shipment.DataContext.AddDataSource(dataType, uniqueRef);

			return shipment;
		}

		void AssertGetDepartureConsolMacroRun(Shipment shipment, ZString? expectedConsolID)
		{
			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "@data.GetDepartureConsol().AgentsReference"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = (IDynamicData)expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals(expectedConsolID, Convert.ToString(result));
			}
		}

		#endregion

		#region TestGetRequiredTaxInfo

		public void TestGetRequiredTaxInfo()
		{
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GovRegNumType = new RegistrationNumberType() { Code = "VAT" },
					Country = new Country() { Code = "BD" },
					GovRegNum = "12345",
					AddressType = "NotifyParty"
				}
			});

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def notifyParty = OrganizationAddressCollection.GetOrCreate(\"NotifyParty\"); GetRequiredTaxInfo(\"\", \"BD\", @notifyParty, \"AlsoNotify\", \"General\")"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals("BIN: 12345", result);
			}
		}

		public void TestGetRequiredTaxInfo_InvalidOrgType()
		{
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def notifyParty = OrganizationAddressCollection.GetOrCreate(\"NotifyParty\"); GetRequiredTaxInfo(\"\", \"BD\", @notifyParty, \"InvalidOrgType\", \"General\")"
					.With<FreightLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				AssertEquals("Value of 'InvalidOrgType' is not valid. Valid values are Shipper, Consignee, AlsoNotify.", expr.ToFormatString());
			}
		}

		public void TestGetRequiredTaxInfo_TaxNumberInRegistrationNumberCollection()
		{
			var shipment = CreateShipment("AUSYD", "CNSHA", DataContextType.ForwardingShipment, "SHP0000001");

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new Country() { Code = "BD" },
				AddressType = "NotifyParty",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType() { Code = "VAT" },
					CountryOfIssue = new Country() { Code = "BD" },
					Value = "23456"
				}
			});
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def notifyParty = OrganizationAddressCollection.GetOrCreate(\"NotifyParty\"); GetRequiredTaxInfo(\"\", \"BD\", @notifyParty, \"AlsoNotify\",\"General\")"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals("BIN: 23456", result);
			}
		}

		public void TestGetRequiredTaxInfo_India_ShippingInstructionGST()
		{
			var shipment = CreateShipment("IN5PA", "MX2NB", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GovRegNumType = new RegistrationNumberType() { Code = "GST" },
					Country = new Country() { Code = "IN" },
					GovRegNum = "12345",
					AddressType = "Consignee"
				}
			});

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def consignee = OrganizationAddressCollection.GetOrCreate(\"Consignee\"); GetRequiredTaxInfo(\"MX\", \"IN\", @consignee, \"Consignee\", \"ShippingInstruction\")"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals("GST: 12345", result);
			}
		}

		public void TestGetRequiredTaxInfo_India_GeneralDocumentsNoGST()
		{
			var shipment = CreateShipment("MX2NB", "IN5PA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GovRegNumType = new RegistrationNumberType() { Code = "GST" },
					Country = new Country() { Code = "IN" },
					GovRegNum = "12345",
					AddressType = "Shipper"
				}
			});

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def shipper = OrganizationAddressCollection.GetOrCreate(\"Shipper\"); GetRequiredTaxInfo(\"IN\", \"MX\", @shipper, \"Shipper\", \"General\")"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals(ZString.Empty, result);
			}
		}

		public void TestGetRequiredTaxInfo_InvalidDocumentType()
		{
			var shipment = CreateShipment("MX2NB", "ARABA", DataContextType.ForwardingShipment, "SHP0000001");

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GovRegNumType = new RegistrationNumberType() { Code = "RFC" },
					Country = new Country() { Code = "MX" },
					GovRegNum = "12345",
					AddressType = "Consignee"
				}
			});

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def consignee = OrganizationAddressCollection.GetOrCreate(\"Consignee\"); GetRequiredTaxInfo(\"AR\", \"MX\", @consignee, \"Consignee\", \"SomethingElse\")"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertEquals("Value of 'SomethingElse' is not valid. Valid values are General, AirwayBill, ShippingInstruction.", expr.ToFormatString());
			}
		}

		#region GetTaxNumberForChineseCustoms

		public void TestGetTaxNumberForChineseCustoms()
		{
			var shipment = CreateShipment("CNSHA", "AUSYD", DataContextType.ForwardingShipment, "SHP0000001");

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new Country { Code = "RU" },
				AddressType = "Consignee",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = "OGR" },
					CountryOfIssue = new Country { Code = "RU" },
					Value = "123456"
				}
			});
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def consignee = OrganizationAddressCollection.GetOrCreate(\"Consignee\"); GetTaxNumberForChineseCustoms(@consignee)"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals("Comp. ID: 123456", result);
			}
		}

		public void TestGetTaxNumberTypesForChineseCustoms()
		{
			var shipment = CreateShipment("CNSHA", "AUSYD", DataContextType.ForwardingShipment, "SHP0000001");

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new Country { Code = "BR" },
				AddressType = "Consignee",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = "CJN" },
					CountryOfIssue = new Country { Code = "BR" },
					Value = "ABC"
				},
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = "GCR" },
					CountryOfIssue = new Country { Code = "BR" },
					Value = "DEF"
				}
			});
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "OrganizationAddressCollection.SetNaturalKey(\"AddressType\");"
					.With<MetaDataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				expr = "def consignee = OrganizationAddressCollection.GetOrCreate(\"Consignee\"); GetTaxNumberTypesForChineseCustoms(@consignee, \" or \")"
					.With<FreightLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", "", expr.ToFormatString());
				AssertEquals("CJN (CNPJ Cadastro Nacional da Pessoa Jurídica / Business Tax Payer Registration) or GCR (NIRE Número de Identificação no Registro de Empresas / Company Registration Number)", result);
			}
		}

		#endregion

		#endregion

		#region GetRoundedValue

		public void TestGetRoundedValue()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 3;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			const string macro1 = "GetRoundedValue(Weight, \"SEA\", \"KG\")";
			const string macro2 = "GetRoundedValue(Weight, \"UNK\", \"KG\")";
			const string macro3 = "GetRoundedValue(Weight, \"SEA\", \"AA\")";
			const string macro4 = "GetRoundedValue(Weight, \"SEA\", \"LB\")";
			const string macro5 = "GetRoundedValue(Weight, \"AIR\", \"KG\")";

			AssertMacroRun(macro1, 3.2584m, 3.259m, string.Empty);
			AssertMacroRun(macro2, 3.2584m, 0m, "Invalid transport mode: UNK.");
			AssertMacroRun(macro3, 3.2584m, 0m, "Invalid unit: AA.");
			AssertMacroRun(macro4, 3.2584m, 3.258m, string.Empty); // using default, NumberOfDecimalPlaces: 3, RoundingMode: BankersRounding
			AssertMacroRun(macro5, 3.2584m, 3.258m, string.Empty); // using default, NumberOfDecimalPlaces: 3, RoundingMode: BankersRounding
		}

		void AssertMacroRun(string macro, decimal originalValue, decimal expectedValue, string errorMessage)
		{
			var dummy = new Dummy
			{
				Weight = originalValue
			};

			var data = dummy.MakeDynamic();

			var expr = macro
				.With<MetaDataLibrary>()
				.And<FreightLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				var result = expr.Evaluate(scope);

				if (string.IsNullOrEmpty(errorMessage))
				{
					AssertEquals(expectedValue, (decimal)result);
				}
				else
				{
					AssertMultilineASCIIEquals("expect errors", errorMessage, string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				}
			}
		}

		#endregion

		#region Implementation

		class Dummy
		{
			public ZString Code { get; set; }
			public ZDecimal Weight { get; set; }
		}

		ValidationRule[] GetRules(IDynamicData dynamicData)
		{
			return dynamicData.ValidationRules.ToArray();
		}

		#endregion
	}
}
