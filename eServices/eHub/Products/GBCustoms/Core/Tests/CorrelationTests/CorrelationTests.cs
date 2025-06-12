using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation;

using NUnit.Framework;

using Rhino.Mocks;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests
{
    [TestFixture]
    public class CorrelationTests
    {
        [Test]
        public void TestCorrelation_XMLBody_Success()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "XMLBody://Level1/Element1/text()" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "XMLBody://Level1/Element2/text()" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "<Level1><Element1>JouMa</Element1><Element2>JouPa</Element2></Level1>", true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = "JouMa"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = "JouPa"
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content <Level1><Element1>JouMa</Element1><Element2>JouPa</Element2></Level1>."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using XMLBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 2 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:JouMa"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:JouPa"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_NoCXCode_Log()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "<Level1><Element1>JouMa</Element1><Element2>JouPa</Element2></Level1>", true, mockClientRegistrationAccessor);

            Assert.IsTrue(logger.Log.Contains("Warn - GBCustomsCorrelation - Could not find a path from CX_Code Value []"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_XMLBody_NotFound()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "XMLBody://Level1/Element1/text()" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "XMLBody://Level1/Element2/text()" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustomsTest-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "<Level1></Level1>", false, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = ""
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content <Level1></Level1>."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using XMLBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 0 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:."), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_JSONBody_Success()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "JSONBody:Element1" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "JSONBody:message.Element2" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "{\"Element1\":\"JouMa\", \"message\": {\"Element2\":\"JouPa\"}}", true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = "JouMa"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = "JouPa"
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content {\"Element1\":\"JouMa\", \"message\": {\"Element2\":\"JouPa\"}}."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 2 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:JouMa"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:JouPa"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_JSONBody_NotFound()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "JSONBody:Element1" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustomsTest-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "JSONBody:Element2" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustomsTest-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "{\"Element3\":\"JouMa\",\"Element4\":\"JouPa\"}", false, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = ""
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content {\"Element3\":\"JouMa\",\"Element4\":\"JouPa\"}."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 0 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:."), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_Headers_Success()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "Header:Header-1" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "Header:Header-2" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var mockRequest = new HttpResponseMessage()
            {
                Headers =
                {
                    { "header-1", "JouMa" },
                    { "Header-2", "JouPa" }
                }
            };

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, mockRequest.Headers.ToString(), true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = "JouMa"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = "JouPa"
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content {mockRequest.Headers}."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using HeaderExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 2 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:JouMa"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:JouPa"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_Headers_NotFound()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "Header:Header-1" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "Header:Header-2" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var mockRequest = new HttpResponseMessage()
            {
                Headers =
                {
                    { "Header-3", "JouMa" },
                    { "Header-4", "JouPa" }
                }
            };

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, mockRequest.Headers.ToString(), true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = ""
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content {mockRequest.Headers.ToString()}."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using HeaderExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 0 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:."), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_Parameter_Success()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", @"Parameter:/something/(?<CorrelationId>\S+)/subscribed" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", @"Parameter:/something/(?<CorrelationId>\S+)/subscribed" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "/something/JouMa/subscribed", true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = "JouMa"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = "JouMa"
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content /something/JouMa/subscribed."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using ParameterExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 2 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:JouMa"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:JouMa"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_Parameter_NotFound()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "Parameter:Anything" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "Parameter:Anything" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, null, true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = ""
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Create, Source Synchronous, Content NULL."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using ParameterExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 2 identifier(s). Successfully extracted 0 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:CorrelationID, Type:Subscribed, Value:."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:AdditionalID, Type:Additional, Value:."), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_MixedExtractionTypes()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "XMLBody://Level1/Element1/text()" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "JSONBody:Element1" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "Header:Header-1" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", @"Parameter:/anything/(?<CorrelationId>\S+)/more" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };

            var mockRequest = new HttpResponseMessage()
            {
                Headers =
                {
                    { "Header-1", "HeadersJouMa" },
                    { "Header-2", "HeadersJouPa" }
                }
            };

            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "<Level1><Element1>XMLJouMa</Element1><Element2>JouPa</Element2></Level1>", true, mockClientRegistrationAccessor);
            Assert.AreEqual(gBCustomsCorrelation.GBCustomsCorrelationIdentifiers[0].Value, "XMLJouMa");

            gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "{\"Element1\":\"JSONJouMa\",\"Element2\":\"JouPa\"}", true, mockClientRegistrationAccessor);
            Assert.AreEqual(gBCustomsCorrelation.GBCustomsCorrelationIdentifiers[1].Value, "JSONJouMa");

            gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, mockRequest.Headers.ToString(), true, mockClientRegistrationAccessor);
            Assert.AreEqual(gBCustomsCorrelation.GBCustomsCorrelationIdentifiers[2].Value, "HeadersJouMa");

            gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "/anything/ParameterJouMa1/more", true, mockClientRegistrationAccessor);
            Assert.AreEqual(gBCustomsCorrelation.GBCustomsCorrelationIdentifiers[3].Value, "ParameterJouMa1");
        }

        [Test]
        public void TestCorrelation_MultipleServices()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create-1"},
                    { "CX_Code", "XMLBody://Level1/Element1/text()" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "Subscribed" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create-2" },
                    { "CX_Code", "XMLBody://Level1/Element2/text()" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "<Level1><Element1>JouMa</Element1><Element2>JouPa</Element2></Level1>", true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "CorrelationID",
                    Type = CorrelationType.Subscribed,
                    Value = "JouMa"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "AdditionalID",
                    Type = CorrelationType.Additional,
                    Value = "JouPa"
                }
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);
        }

        [Test]
        public void TestCorrelation_Exception()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = new List<Dictionary<string, object>>();
            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            CorrelationTestHelper.AssertException<ArgumentException>(() => new GBCustomsCorrelation(logger, "InvalidProvider", "Create", "Synchronous", "<Level1></Level1>", true, mockClientRegistrationAccessor), "Provider InvalidProvider is invalid");
            CorrelationTestHelper.AssertException<ArgumentException>(() => new GBCustomsCorrelation(logger, "ICSGB", "Create", "InvalidSource", "<Level1></Level1>", true, mockClientRegistrationAccessor), "Source InvalidSource is invalid");
            CorrelationTestHelper.AssertException<NullReferenceException>(() => new GBCustomsCorrelation(logger, "ICSGB", "Create", "Synchronous", "<Level1></Level1>", true, mockClientRegistrationAccessor), "Could not find a valid ClientRegistration record for CorrelationID extraction");
        }

        [Test]
        public void TestCorrelation_InvalidDatabaseValues()
        {
            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();

            var mockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create"},
                    { "CX_Code", "XMLBody://Level1/Element1/text()" },
                    { "CX_Flag1", "Synchronous"},
                    { "CX_Flag2", "InvalidFlag2" },
                    { "CX_Attr1", "CorrelationID" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Create" },
                    { "CX_Code", "InvalidExtractionType://Level1/Element2/text()" },
                    { "CX_Flag1", "Synchronous" },
                    { "CX_Flag2", "Additional" },
                    { "CX_Attr1", "AdditionalID" }
                }
            };

            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Create%", flag1: 0, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Create", GBCustomsSource.Synchronous, "<Level1></Level1>", true, mockClientRegistrationAccessor);

            Assert.IsTrue(logger.Log.Contains("Warn - GBCustomsCorrelation - Could not find a CorrelationType from string 'InvalidFlag2'"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Warn - GBCustomsCorrelation - Could not find an ExtractionType from string 'InvalidExtractionType'"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_CTC_Body_Arrival()
        {
            var inputMessage = GetMessage("InputBodyArrival.json");

            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = CTCMockClientRegistrations;

            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Notification%", flag1: 1, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Notification", GBCustomsSource.Notification, inputMessage, true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "RequestId",
                    Type = CorrelationType.Data,
                    Value = "/customs/transits/movements/arrivals/775"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ResponseBody",
                    Type = CorrelationType.Data,
                    Value = "<CC008A>blahblah</CC008A>"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "MessageUri",
                    Type = CorrelationType.Data,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ArrivalId",
                    Type = CorrelationType.Data,
                    Value = "775"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "DepartureId",
                    Type = CorrelationType.Data,
                    Value = ""
                },
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Notification, Source Notification, Content {inputMessage}"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 5 identifier(s). Successfully extracted 3 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:RequestId, Type:Data, Value:/customs/transits/movements/arrivals/775"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:ResponseBody, Type:Data, Value:<CC008A>blahblah</CC008A>"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:ArrivalId, Type:Data, Value:775"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_CTC_Body_Departure()
        {
            var inputMessage = GetMessage("InputBodyDeparture.json");

            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = CTCMockClientRegistrations;

            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Notification%", flag1: 1, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Notification", GBCustomsSource.Notification, inputMessage, true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "RequestId",
                    Type = CorrelationType.Data,
                    Value = "/customs/transits/movements/arrivals/775"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ResponseBody",
                    Type = CorrelationType.Data,
                    Value = "<CC008A>blahblah</CC008A>"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "MessageUri",
                    Type = CorrelationType.Data,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ArrivalId",
                    Type = CorrelationType.Data,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "DepartureId",
                    Type = CorrelationType.Data,
                    Value = "775"
                },
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Notification, Source Notification, Content {inputMessage}"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 5 identifier(s). Successfully extracted 3 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:RequestId, Type:Data, Value:/customs/transits/movements/arrivals/775"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:ResponseBody, Type:Data, Value:<CC008A>blahblah</CC008A>"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:DepartureId, Type:Data, Value:775"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_CTC_MessageUri_Arrival()
        {
            var inputMessage = GetMessage("InputMessageUriArrival.json");

            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = CTCMockClientRegistrations;

            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Notification%", flag1: 1, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Notification", GBCustomsSource.Notification, inputMessage, true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "RequestId",
                    Type = CorrelationType.Data,
                    Value = "/customs/transits/movements/arrivals/775"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ResponseBody",
                    Type = CorrelationType.Data,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "MessageUri",
                    Type = CorrelationType.Data,
                    Value = "/customs/transits/movements/arrivals/775/messages/1"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ArrivalId",
                    Type = CorrelationType.Data,
                    Value = "775"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "DepartureId",
                    Type = CorrelationType.Data,
                    Value = ""
                },
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Notification, Source Notification, Content {inputMessage}"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 5 identifier(s). Successfully extracted 3 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:RequestId, Type:Data, Value:/customs/transits/movements/arrivals/775"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:MessageUri, Type:Data, Value:/customs/transits/movements/arrivals/775/messages/1"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:ArrivalId, Type:Data, Value:775"), "Log entry expected");
        }

        [Test]
        public void TestCorrelation_CTC_MessageUri_Departure()
        {
            var inputMessage = GetMessage("InputMessageUriDeparture.json");

            var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
            var mockClientRegistrations = CTCMockClientRegistrations;

            mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Notification%", flag1: 1, useLike: true)).Return(mockClientRegistrations);

            var logger = new MockLogger();

            var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Notification", GBCustomsSource.Notification, inputMessage, true, mockClientRegistrationAccessor);

            var expectedResult = new List<GBCustomsCorrelationIdentifier>()
            {
                new GBCustomsCorrelationIdentifier
                {
                    Name = "RequestId",
                    Type = CorrelationType.Data,
                    Value = "/customs/transits/movements/arrivals/775"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ResponseBody",
                    Type = CorrelationType.Data,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "MessageUri",
                    Type = CorrelationType.Data,
                    Value = "/customs/transits/movements/arrivals/775/messages/1"
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "ArrivalId",
                    Type = CorrelationType.Data,
                    Value = ""
                },
                new GBCustomsCorrelationIdentifier
                {
                    Name = "DepartureId",
                    Type = CorrelationType.Data,
                    Value = "775"
                },
            };

            CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

            Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Notification, Source Notification, Content {inputMessage}"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 5 identifier(s). Successfully extracted 3 value(s)."), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:RequestId, Type:Data, Value:/customs/transits/movements/arrivals/775"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:MessageUri, Type:Data, Value:/customs/transits/movements/arrivals/775/messages/1"), "Log entry expected");
            Assert.IsTrue(logger.Log.Contains("Name:DepartureId, Type:Data, Value:775"), "Log entry expected");
        }

		[Test]
		public void TestCorrelation_CTC_FileUpload_Failed()
		{
			var inputMessage = GetMessage("FileUploadFailed.json");

			var mockClientRegistrationAccessor = MockRepository.GenerateStub<IClientRegistrationAccessor>();
			var mockClientRegistrations = CTCMockClientRegistrations;

			mockClientRegistrationAccessor.Stub(_ => _.ReadRegistrations("GBCustoms-CTCGB", "GBCustoms-Transport", qualifier: "Notification%", flag1: 1, useLike: true)).Return(mockClientRegistrations);

			var logger = new MockLogger();

			var gBCustomsCorrelation = new GBCustomsCorrelation(logger, ProviderType.CTCGB, "Notification", GBCustomsSource.Notification, inputMessage, true, mockClientRegistrationAccessor);

			var expectedResult = new List<GBCustomsCorrelationIdentifier>()
			{
				new GBCustomsCorrelationIdentifier
				{
					Name = "RequestId",
					Type = CorrelationType.Data,
					Value = ""
				},
				new GBCustomsCorrelationIdentifier
				{
					Name = "ResponseBody",
					Type = CorrelationType.Data,
					Value = ""
				},
				new GBCustomsCorrelationIdentifier
				{
					Name = "MessageUri",
					Type = CorrelationType.Data,
					Value = "/customs/transits/movements/departures/66275ff1c7ac6e53/messages/66275ff1103299e6"
				},
				new GBCustomsCorrelationIdentifier
				{
					Name = "ArrivalId",
					Type = CorrelationType.Data,
					Value = ""
				},
				new GBCustomsCorrelationIdentifier
				{
					Name = "DepartureId",
					Type = CorrelationType.Data,
					Value = "66275ff1c7ac6e53"
				},
			};

			CorrelationTestHelper.AssertGBCustomsCorrelationIdentifiers(expectedResult, gBCustomsCorrelation.GBCustomsCorrelationIdentifiers);

			Assert.IsTrue(logger.Log.Contains($"Info - GBCustomsCorrelation - Getting the correlation identifier(s) for Provider CTCGB, Service Notification, Source Notification, Content {inputMessage}"), "Log entry expected");
			Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Using JSONBodyExtractionHandler"), "Log entry expected");
			Assert.IsTrue(logger.Log.Contains("Info - GBCustomsCorrelation - Found 5 identifier(s). Successfully extracted 2 value(s)."), "Log entry expected");
			Assert.IsTrue(logger.Log.Contains("Name:MessageUri, Type:Data, Value:/customs/transits/movements/departures/66275ff1c7ac6e53/messages/66275ff1103299e6"), "Log entry expected");
			Assert.IsTrue(logger.Log.Contains("Name:DepartureId, Type:Data, Value:66275ff1c7ac6e53"), "Log entry expected");
		}

		private string GetMessage(string source)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests.TestFiles.{source}").ReadToEnd();
        }

        private List<Dictionary<string, object>>  CTCMockClientRegistrations = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Notification-RequestId"},
                    { "CX_Code", "JSONBody:message.requestId" },
                    { "CX_Flag1", "Notification"},
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "RequestId" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Notification-Body" },
                    { "CX_Code", "JSONBody:message.body" },
                    { "CX_Flag1", "Notification" },
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ResponseBody" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Notification-MessageUri" },
                    { "CX_Code", "JSONBody:message.messageUri" },
                    { "CX_Flag1", "Notification" },
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "MessageUri" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Notification-ArrivalId" },
                    { "CX_Code", "JSONBody:message.arrivalId" },
                    { "CX_Flag1", "Notification" },
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "ArrivalId" }
                },
                new Dictionary<string, object>
                {
                    { "CX_CC_ID", "GBCustoms-CTCGB" } ,
                    { "CX_Qualifier", "Notification-DepartureId" },
                    { "CX_Code", "JSONBody:message.departureId" },
                    { "CX_Flag1", "Notification" },
                    { "CX_Flag2", "Data" },
                    { "CX_Attr1", "DepartureId" }
                },
            };
    }
}
