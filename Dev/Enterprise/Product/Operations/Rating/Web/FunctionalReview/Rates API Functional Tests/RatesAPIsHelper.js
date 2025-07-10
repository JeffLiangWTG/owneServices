const axios = require('axios');
const https = require('https');

const RatesAPIsHelper =
{
    GetRateProviders: function()
    {
        var rateProviders = [];
        bru.getEnvVar("RateProviders").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                rateProviders.push(item.trim());
            }
        });
        
        return rateProviders;
    },

    GetServiceProvidersWithCW1Codes: function()
    {
        var serviceProviders = [];
        bru.getEnvVar("ServiceProvidersWithCWCodes").split(",").forEach((provider) => {

            if (provider.trim() !== "")
            {
                var serviceProvider = {
                    CWCode: provider.trim()
                };
                serviceProviders.push(serviceProvider);
            }
        });
        
        return serviceProviders;
    },

    GetServiceProvidersWithSCACCodes: function()
    {
        var serviceProviders = [];
        bru.getEnvVar("ServiceProvidersWithSCACCodes").split(",").forEach((provider) => {
            if (provider.trim() !== "")
            {
                var serviceProvider = {
                    SCAC: provider.trim()
                };
                serviceProviders.push(serviceProvider);
            }
        });
        
        return serviceProviders;
    }, 

    GetServiceProvidersWithIATACodes: function()
    {
        var serviceProviders = [];
        bru.getEnvVar("ServiceProvidersWithIATACodes").split(",").forEach((provider) => {
            if (provider.trim() !== "")
            {            
                var serviceProvider = {
                    IATACode: provider.trim()
                };
                serviceProviders.push(serviceProvider);
            }
        });
        
        return serviceProviders;
    },

    GetServiceProvidersWithCICCodes: function()
    {
        var serviceProviders = [];
        bru.getEnvVar("ServiceProvidersWithC1CCodes").split(",").forEach((provider) => {
            if (provider.trim() !== "")
            {
                var serviceProvider = {
                    C1CCode: provider.trim()
                };

                serviceProviders.push(serviceProvider);
            }
        });
        
        return serviceProviders;
    },

    GetAllServiceProviders: function()
    {
        var serviceProviders = [];

        this.GetServiceProvidersWithCW1Codes().forEach((item) => serviceProviders.push(item));
        this.GetServiceProvidersWithSCACCodes().forEach((item) => serviceProviders.push(item));
        this.GetServiceProvidersWithIATACodes().forEach((item) => serviceProviders.push(item));
        this.GetServiceProvidersWithCICCodes().forEach((item) => serviceProviders.push(item));

        return serviceProviders;
    },

  GetCarriersWithCW1Codes: function () {
    var carriers = [];
    bru.getEnvVar("CarriersWithCWCodes").split(",").forEach((provider) => {

      if (provider.trim() !== "") {
        var carrier = {
          CWCode: provider.trim()
        };
        carriers.push(carrier);
      }
    });

    return carriers;
  },

  GetCarriersWithSCACCodes: function () {
    var carriers = [];
    bru.getEnvVar("CarriersWithSCACCodes").split(",").forEach((provider) => {
      if (provider.trim() !== "") {
        var carrier = {
          SCAC: provider.trim()
        };
        carriers.push(carrier);
      }
    });

    return carriers;
  },

  GetCarriersWithIATACodes: function () {
    var carriers = [];
    bru.getEnvVar("CarriersWithIATACodes").split(",").forEach((provider) => {
      if (provider.trim() !== "") {
        var carrier = {
          IATACode: provider.trim()
        };
        carriers.push(carrier);
      }
    });

    return carriers;
  },

  GetCarriersWithCICCodes: function () {
    var carriers = [];
    bru.getEnvVar("CarriersWithC1CCodes").split(",").forEach((provider) => {
      if (provider.trim() !== "") {
        var carrier = {
          C1CCode: provider.trim()
        };

        carriers.push(carrier);
      }
    });

    return carriers;
  },

  GetAllCarriers: function () {
    var carriers = [];

    this.GetCarriersWithCW1Codes().forEach((item) => carriers.push(item));
    this.GetCarriersWithSCACCodes().forEach((item) => carriers.push(item));
    this.GetCarriersWithIATACodes().forEach((item) => carriers.push(item));
    this.GetCarriersWithCICCodes().forEach((item) => carriers.push(item));

    return carriers;
  },

    GetCarrierContracts: function()
    {
        var carrierContracts = [];
        if (bru.getEnvVar("CarrierContracts") != "")
        {
            bru.getEnvVar("CarrierContracts").split(",").forEach((contract) => {
                carrierContracts.push(contract.trim());
            });
        }
        return carrierContracts;
    },

    GetCarrierServiceLevelssWithCW1Codes: function()
    {
        var carrierServiceLevels = [];

        if (bru.getEnvVar("CarrierServiceLevelsWithCW1Codes") != "")
        {
            bru.getEnvVar("CarrierServiceLevelsWithCW1Codes").split(",").forEach((sl) => {
                var serviceLevel = {
                    Type: "CW",
                    Value: sl.trim()
                };

                carrierServiceLevels.push(serviceLevel);
            });
        }
        
        return carrierServiceLevels;
    },

    GetCarrierServiceLevelssWithUniversalCodes: function()
    {
        var carrierServiceLevels = [];
        bru.getEnvVar("CarrierServiceLevelsWithUniversalCodes").split(",").forEach((sl) => {
            if (sl.trim() != "")
            {
                var serviceLevel = {
                    Type: "UC",
                    Value: sl.trim()
                };

                carrierServiceLevels.push(serviceLevel);
            }
        });
        
        return carrierServiceLevels;
    },

    GetAllCarrierServiceLevels: function()
    {
        var serviceLevels = [];

        this.GetCarrierServiceLevelssWithCW1Codes().forEach((item) => serviceLevels.push(item));
        this.GetCarrierServiceLevelssWithUniversalCodes().forEach((item) => serviceLevels.push(item));

        return serviceLevels;        
    },

    GetServiceLevels: function()
    {
        var serviceLevels = [];

        if (bru.getEnvVar("ServiceLevels") != "")
        {
            bru.getEnvVar("ServiceLevels").split(",").forEach((sl) => {
                serviceLevels.push(sl.trim());
            });
        }

        return serviceLevels;
    },

    GetGatewayServiceLevels: function()
    {
        var gatewayServiceLevels = [];

        if (bru.getEnvVar("GatewayServiceLevels") != "")
        {
            bru.getEnvVar("GatewayServiceLevels").split(",").forEach((sl) => {
                gatewayServiceLevels.push(sl.trim());
            });
        }

        return gatewayServiceLevels;
    },

    GetShipmentGatewayServiceLevels: function()
    {
        var shipmentGatewayServiceLevels = [];

        if (bru.getEnvVar("ShipmentGatewayServiceLevels") != "")
        {
            bru.getEnvVar("ShipmentGatewayServiceLevels").split(",").forEach((sl) => {
                shipmentGatewayServiceLevels.push(sl.trim());
            });
        }

        return shipmentGatewayServiceLevels;
    },

    GetContainerTypesWithCW1Codes: function()
    {
        var containerTypes = [];
        bru.getEnvVar("ContainerTypesWithCW1Codes").split(",").forEach((ct) => {
            if (ct.trim() !== "")
            {
                var containerType = {
                    Type: "CW",
                    Value: ct.trim()
                };

                containerTypes.push(containerType);
            }
        });
        
        return containerTypes;
    },

    GetContainerTypesWithISOCodes: function()
    {
        var containerTypes = [];
        bru.getEnvVar("ContainerTypesWithISOCodes").split(",").forEach((ct) => {
            if (ct.trim() !== "")
            {
                var containerType = {
                    Type: "ISO",
                    Value: ct.trim()
                };

                containerTypes.push(containerType);
            }
        });
        
        return containerTypes;
    },

    GetAllContainerTypes: function()
    {
        var containerTypes = [];

        this.GetContainerTypesWithCW1Codes().forEach((item) => containerTypes.push(item));
        this.GetContainerTypesWithISOCodes().forEach((item) => containerTypes.push(item));

        return containerTypes;        
    },

    GetCommoditiesWithCW1Codes: function()
    {
        var commodities = [];

        if (bru.getEnvVar("CommoditiesWithCW1Codes") != "")
        {
            bru.getEnvVar("CommoditiesWithCW1Codes").split(",").forEach((c) => {
                var commodity = {
                    Type: "CW",
                    Value: c.trim()
                };

                commodities.push(commodity);
            });
        }
        
        return commodities;
    },

    GetCommoditiesWithUniversalCommodityGroupCodes: function()
    {
        var commodities = [];
        bru.getEnvVar("CommoditiesWithUniversalCommodityGroupCodes").split(",").forEach((c) => {

            if (c.trim() != "")
            {
                var commodity = {
                    Type: "UCG",
                    Value: c.trim()
                };

                commodities.push(commodity);
            }
        });
        
        return commodities;
    },

    GetAllCommodities: function()
    {
        var commodities = [];

        this.GetCommoditiesWithCW1Codes().forEach((item) => commodities.push(item));
        this.GetCommoditiesWithUniversalCommodityGroupCodes().forEach((item) => commodities.push(item));

        return commodities;        
    },

    GetNamedAccountsWithCW1Codes: function()
    {
        var namedAccounts = [];
        bru.getEnvVar("NamedAccountsWithCW1Codes").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                var namedAccount = {
                    Type: "CW",
                    Value: item.trim()
                };

                namedAccounts.push(namedAccount);
            }
        });
        
        return namedAccounts;
    },

    GetNamedAccountsWithNACCodes: function()
    {
        var namedAccounts = [];
        bru.getEnvVar("NamedAccountsWithNACCodes").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                var namedAccount = {
                    Type: "NAC",
                    Value: item.trim()
                };

                namedAccounts.push(namedAccount);
            }
        });
        
        return namedAccounts;
    },

    GetAllNamedAccounts: function()
    {
        var namedAccounts = [];

        this.GetNamedAccountsWithCW1Codes().forEach((item) => namedAccounts.push(item));
        this.GetNamedAccountsWithNACCodes().forEach((item) => namedAccounts.push(item));

        return namedAccounts;        
    },

    GetCSFilter : function()
    {
        var rateTypes = [];
        bru.getEnvVar("CSFilter_RateTypes").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                rateTypes.push(item.trim());
            }
        });

        var rateTypes2 = [];
        bru.getEnvVar("CSFilter_RateTypes2").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                rateTypes2.push(item.trim());
            }
        });

        var serviceStrings = [];
        bru.getEnvVar("CSFilter_ServiceStrings").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                serviceStrings.push(item.trim());
            }
        });        
        
        return {
            RateTypes: rateTypes, 
            RateTypes2: rateTypes2,
            ServiceStrings: serviceStrings
        };

    },

    GetCGFilter : function()
    {
        var rateClasses = [];
        bru.getEnvVar("CGFilter_RateClasses").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                rateClasses.push(parseInt(item.trim()));
            }
        });

        var references = [];
        bru.getEnvVar("CGFilter_References").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                references.push(item.trim());
            }
        });

        var products = [];
        bru.getEnvVar("CGFilter_Products").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                products.push(item.trim());
            }
        });

        var vias = [];
        bru.getEnvVar("CGFilter_Vias").split(",").forEach((item) => {
            if (item.trim() !== "")
            {
                vias.push(item.trim());
            }
        });        
        
        return {
            RateClasses: rateClasses, 
            References: references,
            Products: products,
            Vias: vias
        };

    },    

    GetClientContracts: function()
    {
        var clientContracts = [];
        if (bru.getEnvVar("ClientContracts") != "")
        {
            bru.getEnvVar("ClientContracts").split(",").forEach((contract) => {
                clientContracts.push(contract.trim());
            });
        }
        return clientContracts;
    },

    GetRateQueryWithMandatoryFieldsForProvidedVariables: function(includeRateProviders = true)
    {
        var query = {
            Origin: {
                Type: bru.getEnvVar("OriginType"),
                Value: bru.getEnvVar("OriginValue")
            },
            Destination: {
                Type: bru.getEnvVar("DestinationType"),
                Value: bru.getEnvVar("DestinationValue"),
            },
            TransportMode: bru.getEnvVar("TransportMode"),
            ContainerMode: bru.getEnvVar("ContainerMode"),
            EffectiveDate: bru.getEnvVar("EffectiveDate"),
        };

        if (includeRateProviders)
        {
            query.RateProviders = RatesAPIsHelper.GetRateProviders();
        }        

        return query;
    },

    GetVia: function()
    {
        var type = bru.getEnvVar("ViaType");
        var value = bru.getEnvVar("ViaValue");
        
        if (type.trim() !== "" && value.trim() !== "")
        {
            var result = 
            {
                Type: bru.getEnvVar("ViaType"),
                Value: bru.getEnvVar("ViaValue")
            };

            return result;
        }        
        return null;
    },

    GetLocations: function()
    {
        var locations = bru.getEnvVar("LocationsJSON");

        if (locations.trim() !== "")
        {
            return JSON.parse(locations);
        }

        return null;
    },

    GetRateParties: function()
    {
        var parties = [];

        const partyMap = {
            "LC": "LocalClientCW1Code",
            "AG": "AgentCW1Code",
            "CNE": "ConsigneeCW1Code",
            "CNR": "ConsignorCW1Code",
            "SAG": "SendingAgentCW1Code",
            "RAG": "ReceivingAgentCW1Code",
            "CCUS": "ControllingCustomerCW1Code",
            "IB": "ImportBrokerCW1Code",
            "EB": "ExportBrokerCW1Code",
            "DA": "DeliveryAgentCW1Code",
            "PA": "PickupAgentCW1Code",
            "DTC": "DeliveryTransportCompanyCW1Code",
            "PTC": "PickupTransportCompanyCW1Code",
            "ICFS": "ImportCFSCW1Code",
            "ECFS": "ExportCFSCW1Code",
            "CA": "ControllingAgentCW1Code",
            "CCR": "CreditorCW1Code",
            "COR": "CreditorOnRouteCW1Code",
            "CAR": "CarrierCW1Code",
            "DCTO": "DepartureCTOCW1Code",
            "DCTR": "DepartureCTOOnRouteCW1Code",
            "DCFS": "DepartureCFSCW1Code",
            "DCFT": "DepartureCFSTransportCW1Code",
            "ACTO": "ArrivalCTOCW1Code",
            "ACTR": "ArrivalCTOOnRouteCW1Code",
            "ACFS": "ArrivalCFSCW1Code",
            "ACFT": "ArrivalCFSTransportCW1Code"
        };

        for (const [role, variable] of Object.entries(partyMap)) {
            const code = bru.getEnvVar(variable).trim();
            if (code) {
                parties.push({ Role: role, Code: code });
            }
        }

        if (!parties.length) {
            throw new Error("Please specify at least one code associated with a rate party, eg: ConsignorCW1Code");
        }
        return parties;
    },

    GetCalculationScope : function()
    {
        if (bru.getEnvVar("CalculationScope").trim() != "")
        {
            return bru.getEnvVar("CalculationScope").trim();
        }

        return "";
    },    

    GetCarrierPayTerm : function()
    {
        if (bru.getEnvVar("CarrierPayTerm").trim() != "")
        {
            return bru.getEnvVar("CarrierPayTerm").trim();
        }

        return "";
    },

    GetPaymentTermOverride : function()
    {
        if (bru.getEnvVar("PaymentTermOverride").trim() != "")
        {
            return bru.getEnvVar("PaymentTermOverride").trim();
        }

        return "";
    },    

    GetIncoterm : function()
    {
        if (bru.getEnvVar("Incoterm").trim() != "")
        {
            return bru.getEnvVar("Incoterm").trim();
        }

        return "";
    },

    GetHBLDeliveryMode : function()
    {
        if (bru.getEnvVar("HBLDeliveryMode").trim() != "")
        {
            return bru.getEnvVar("HBLDeliveryMode").trim();
        }

        return "";
    },

    GetFMCTariffID : function()
    {
        if (bru.getEnvVar("FMCTariffID").trim() != "")
        {
            return bru.getEnvVar("FMCTariffID").trim();
        }

        return "";
    },

    GetJobInfo : function()
    {
        if (bru.getEnvVar("JobInfoJSON").trim() != "")
        {
            return JSON.parse(bru.getEnvVar("JobInfoJSON").trim());
        }

        return {
            Containers: []
        };
    },

    GetClientRateQueryWithMandatoryFieldsForProvidedVariables: function()
    {
        var query = RatesAPIsHelper.GetRateQueryWithMandatoryFieldsForProvidedVariables(false);

        query.Client = bru.getEnvVar("Client");

        return query;
    },

    GetAValidRateQuery: function()
    {
        var query = {
            RateProviders: ["CW", "CG/CS"],
            Origin: {
                Type: "UNLOCO",
                Value: "AUSYD"
            },
            Destination: {
                Type: "UNLOCO",
                Value: "USLAX"
            },
            TransportMode: "SEA",
            ContainerMode: "FCL",
            ServiceProviders: [],
            Carriers: [],
            CarrierContracts:[],
            ClientContracts:[],
            NamedAccounts:[],
            CarrierServiceLevels:[],
            ServiceLevels:[],
            GatewayServiceLevels:[],
            ContainerTypes:[],
            Commodities:[],
            EffectiveDate: new Date(),
            CalculationScope: "",
            JobInfos:[],
            CTLevel:0,
            PlannedLoad:null,
            PlannedDischarge:null,
            RateOrigin:null,
            RateDestination:null,
            CSFilter:null,
            CGFilter:null,
        };

        return query;
    },

    GetAValidRateQueryForCompanyTariffs: function()
    {
        var query = this.GetAValidRateQuery();
        query.RateProviders = null;
        return query;
    },

    GetAValidRateQueryForIntercompanyTariffs: function()
    {
        var query = this.GetAValidRateQuery();
        query.RateProviders = null;
        return query;
    },

    GetAValidRateQueryForClientRates: function()
    {
        var query = this.GetAValidRateQuery();
        query.RateProviders = null;
        return query;
    },

    GetAValidRateQueryForJobCharges: function()
    {
        var query = this.GetAValidRateQuery();

        query.RateProviders = null;
        query.CalculationScope = "A";
        query.Incoterm = "EXW";
        query.CarrierPayTerm = "PPD";

        query.RateParties = [
            {Code: "ABCD", Role: "SAG"},
            {Code: "EFGH", Role: "RAG"},
        ];

        query.JobInfo = {
            Containers: [
                {
                    ContainerTypeCWCode : "20GP",
                    Number : "2983743874",
                    Unit: 2,
                    Commodity: "GEN",
                    Ownership: "CAR",
                    Packlines: [
                        {
                            PackageType : "PLT",
                            Unit: 2,
                            Commodity: null, 
                            Weight: 3400,
                            WeightUnit: "KG",
                            Volume: 4.32,
                            VolumeUnit: "M3"
                        },
                    ]
                },
            ]
        };
        
        return query;
    },

    SetAuthorizationForRequests: function()
    {
        if (pm.variables.get('TokenRequest') == 'Y')
        {
            return;
        }

        var tokenURL = bru.getEnvVar('BaseApplicationURL') + "/api/rating/token" ;
        var details = {
            'grant_type': 'password',
            'username': bru.getEnvVar('UserName'),
            'password': bru.getEnvVar('Password')            
        };

        var formBody = [];
        for (var property in details) {
            var encodedKey = encodeURIComponent(property);
            var encodedValue = encodeURIComponent(details[property]);
            formBody.push(encodedKey + "=" + encodedValue);
        }
        formBody = formBody.join("&");

        var postRequest = {
            url: tokenURL,
            method: 'POST',
            header: {
                'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8'
            },
            body: {
                mode: 'raw',
                raw: formBody
            }
        };

        setTimeout(() => pm.sendRequest(postRequest, (error, response) => {
        if (error) {
            console.log(error);
        } else {
            var authorizationValue = "bearer " + response.json().access_token;
            pm.request.headers.add({key: 'Authorization', value: authorizationValue });
        }
        }), 500);
    }, 

    SetRequestContentTypeAsJSONForDefault: function()
    {
        if (pm.request.headers.get("Accept") == undefined)
        {
            pm.request.headers.remove('Accept');
            pm.request.headers.add({key: "Accept", value: 'application/json'});
        }
    },

    GetValidPickupAddress: function() {
        let organizationCode = bru.getEnvVar("PickupOrg") || "";
        organizationCode = organizationCode.trim();
        let addressCode = bru.getEnvVar("PickupAddrCode") || "";
        addressCode = addressCode.trim();

        if (!organizationCode) {
            throw new Error("Cannot get pickup address without organization. Please specify variable PickupOrg");
        }
        if (!addressCode) {
            throw new Error("No pickup address specified. Please specify variable PickupAddrCode");
        }

        return {
            "OrganisationCode": organizationCode,
            "AddressShortCode": addressCode
        };
    },

    GetCTLevelOverride : function()
    {
        if (bru.getEnvVar("CTLevelOverride").trim() != "")
        {
            return bru.getEnvVar("CTLevelOverride").trim();
        }

        return "";
    },    

    GetValidDeliveryAddress: function() {
        let organizationCode = bru.getEnvVar("DeliveryOrg") || "";
        organizationCode = organizationCode.trim();
        let addressCode = bru.getEnvVar("DeliveryAddrCode") || "";
        addressCode = addressCode.trim();

        if (!organizationCode) {
            throw new Error("Cannot get delivery address without organization. Please specify variable DeliveryOrg");
        }
        if (!addressCode) {
            throw new Error("No delivery address specified. Please specify variable DeliveryAddrCode");
        }

        return {
            "OrganisationCode": organizationCode,
            "AddressShortCode": addressCode
        };
    },
    
    GetValidCustomFields: function () {
        function get(nameVariable, valueVariable) {

            let customFieldName = bru.getEnvVar(nameVariable) || "";
            customFieldName = customFieldName.trim();
            let customFieldValue = bru.getEnvVar(valueVariable) || "";
            customFieldValue = customFieldValue.trim();

            if (!customFieldName) {
                throw new Error(`Cannot get custom field without name/value pair. Please specify variable ${nameVariable}`);
            }
            if (!customFieldValue) {
                throw new Error(`Cannot get custom field without name/value pair. Please specify variable ${valueVariable}`);
            }

            return { "Name": customFieldName, "Value": customFieldValue }
        }

        const result = [get("CustomFieldName", "CustomFieldValue")];
        for (let i = 2; i < 100; i++) {
            try {
                result.push(get(`CustomFieldName${i}`, `CustomFieldValue${i}`));
            } catch { }
        }

        return result;
    },
}

RatesAPIsHelper.SetAuthorizationForRequests = async function()
{
	var tokenUrl = bru.getEnvVar('BaseApplicationURL') + '/api/rating/token' ;
	var details = {
		'grant_type': 'password',
		'username': bru.getEnvVar('UserName'),
		'password': bru.getEnvVar('Password')            
	};

	var tokenRequest = await axios.post(tokenUrl, details, {headers: {'content-type': 'application/x-www-form-urlencoded'}, httpsAgent: new https.Agent({ rejectUnauthorized: false })});

	req.setHeader('Authorization', `Bearer ${tokenRequest.data.access_token}`);

	console.log('Token successfully obtained');
}

module.exports = RatesAPIsHelper;
