using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	sealed class CustomsServiceTaskTest : TestCase
	{
		[SnailTest]
		public void TestRequiresCompanyInCountryAttribute()
		{
			var exceptionsList = new HashSet<ZString>()
			{
				"Enterprise.Customs.EU.NCTS.Messaging.ServiceTask.NctsDownloaderServiceTask",
				"Enterprise.Customs.EU.NCTS.Messaging.ServiceTask.NctsUploaderServiceTask",
				"Enterprise.Customs.ServiceTasks.EHubRegistryUpdateServiceTask",
				"Enterprise.Customs.ServiceTasks.CalculateGuaranteeBalanceServiceTask",
				"Enterprise.Customs.AsycudaCustoms.ServiceTasks.AsycudaGMDCustomsMessagingServiceTask",
				"Enterprise.Customs.ServiceTasks.CustomsDocumentGeneratorServiceTask",
				"Enterprise.Customs.ServiceTasks.CustomsMessaging.AutoSendCustomsMessagingServiceTask",
				"Enterprise.Customs.CA.ServiceTasks.EHubInterchangeProcessorServiceTask",
				"Enterprise.Customs.CA.ServiceTasks.InterchangeProcessorServiceTask",
				"Enterprise.Customs.CA.ServiceTasks.InterchangeSenderServiceTask",
				"Enterprise.Customs.CA.ServiceTasks.MessageProcessorServiceTask",
				"Enterprise.Customs.US.AIM.ServiceTasks.AIMOutgoingServiceTask",
				"Enterprise.Customs.US.AIM.ServiceTasks.AIMMessageServiceTask",
				"Enterprise.Customs.US.AIM.ServiceTasks.AIMInboundServiceTask",
				"Enterprise.Customs.US.AMS.Messaging.ServiceTasks.AMSServiceTask",
				"Enterprise.Customs.US.eManifest.ServiceTasks.InterchangeProcessorServiceTask",
				"Enterprise.Customs.US.eManifest.ServiceTasks.InterchangeComposerServiceTask",
				"Enterprise.Customs.US.eManifest.ServiceTasks.MessageProcessorServiceTask",
				"Enterprise.Customs.US.ISF.ServiceTasks.ImporterSecurityFilingXMLImporter",
				"Enterprise.Customs.US.ISF.ServiceTasks.ISFOutgoingServiceTask",
				"Enterprise.Customs.US.ISF.ServiceTasks.ISFServiceTask"
			};

			var canRunOnAnyCompanyList = new HashSet<ZString>()
			{
				"Enterprise.Customs.US.ServiceTasks.US1ServiceTask"
			};

			var countriesUnderFrenchCustomsJurisdictionFormattedList = Enterprise.Core.Constants.CountryCodes.France
																	+ "," + Enterprise.Core.Constants.CountryCodes.FrenchGuyana
																	+ "," + Enterprise.Core.Constants.CountryCodes.Guadeloupe
																	+ "," + Enterprise.Core.Constants.CountryCodes.Martinique
																	+ "," + Enterprise.Core.Constants.CountryCodes.Mayotte
																	+ "," + Enterprise.Core.Constants.CountryCodes.Reunion
																	+ "," + Enterprise.Core.Constants.CountryCodes.SaintMartin
																	+ "," + Enterprise.Core.Constants.CountryCodes.SaintBarthelemy;

			var requiresCompanyInCountryDictionary = new Dictionary<string, string>
			{
				{ "Enterprise.Customs.AU.ServiceTasks.CRSMessageProcessorService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.CTLMessageProcessorService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.InterchangeRetrieverService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.MessageProcessorService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.LateAndPendingCargoReportService", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.AU.ServiceTasks.ReferenceFilesServiceTask", Core.Constants.CountryCodes.Australia },
				{ "Enterprise.Customs.CA.ServiceTasks.LVXConsolidateServiceTask", Core.Constants.CountryCodes.Canada },
				{ "Enterprise.Customs.CA.ServiceTasks.CopyOGDToPGAServiceTask", Core.Constants.CountryCodes.Canada },
				{ "Enterprise.Customs.CA.ServiceTasks.RegularProcessingServiceTask", Core.Constants.CountryCodes.Canada },
				{ "Enterprise.Customs.CA.ServiceTasks.AVSQueryServiceTask", Core.Constants.CountryCodes.Canada },
				{ "Enterprise.Customs.CA.ServiceTasks.B3AutoSendingServiceTask", Core.Constants.CountryCodes.Canada },
				{ "Enterprise.Customs.CN.ServiceTasks.MessageRetrieverServiceTask", Core.Constants.CountryCodes.China },
				{ "Enterprise.Customs.DE.EMCS.ServiceTasks.DEMCustomsMessageRetrievingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.DE.EMCS.ServiceTasks.DEMCustomsMessageSendingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.DE.ServiceTasks.DEECustomsMessageRetrievingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.DE.ServiceTasks.DEACustomsMessageRetrievingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.DE.ServiceTasks.DEACustomsMessageSendingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.DE.ServiceTasks.DEECustomsMessageSendingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.DE.ServiceTasks.DEICustomsInterchangeProcessingServiceTask", Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.ES.ServiceTasks.MessageRetrievingService", Core.Constants.CountryCodes.Spain },
				{ "Enterprise.Customs.ES.ServiceTasks.MessageProcessorService", Core.Constants.CountryCodes.Spain },
				{ "Enterprise.Customs.FR.ServiceTasks.FRAutomationDeltaServiceTask", countriesUnderFrenchCustomsJurisdictionFormattedList },
				{ "Enterprise.Customs.FR.ServiceTasks.FRCustomsFallbackProcessingServiceTask", countriesUnderFrenchCustomsJurisdictionFormattedList },
				{ "Enterprise.Customs.FR.ServiceTasks.FRCustomsSenderServiceTask", countriesUnderFrenchCustomsJurisdictionFormattedList },
				{ "Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask", countriesUnderFrenchCustomsJurisdictionFormattedList },
				{ "Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukInterchangePackagerServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukMaintenanceServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.CDS.ServiceTasks.CDSCredentialExpiryTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.CDS.ServiceTasks.CDSMessageRetrieverServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.CDS.ServiceTasks.CDSMessageSenderServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.CNS.CnsCompassServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.Pentant.ServiceTasks.PentantDownloaderServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.MCP.ServiceTasks.PHS11.PHS11ServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.MCP.ServiceTasks.Misc.MiscTextAndIslServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11.McpStatusRetrieverServiceProvider", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.MCP.ServiceTasks.RRA12.RRA12ServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.HK.ServiceTasks.MessageSenderServiceTask", Core.Constants.CountryCodes.HongKong },
				{ "Enterprise.Customs.HK.ServiceTasks.MessageProcessorServiceTask", Core.Constants.CountryCodes.HongKong },
				{ "Enterprise.Customs.IT.ServiceTasks.MessageProcessorServiceTask", Core.Constants.CountryCodes.Italy },
				{ "Enterprise.Customs.IT.ServiceTasks.MessageRetrieverServiceTask", Core.Constants.CountryCodes.Italy },
				{ "Enterprise.Customs.IT.ServiceTasks.MessageSenderServiceTask", Core.Constants.CountryCodes.Italy },
				{ "Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageProcessorService", Core.Constants.CountryCodes.NewZealand },
				{ "Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageRetrieverService", Core.Constants.CountryCodes.NewZealand },
				{ "Enterprise.Customs.NZ.ServiceTasks.CUSMOD.MessageSenderService", Core.Constants.CountryCodes.NewZealand },
				{ "Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.CMDInboundInterchangeProcessorServiceTask", Core.Constants.CountryCodes.Singapore },
				{ "Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.CMDReceiverServiceTask", Core.Constants.CountryCodes.Singapore },
				{ "Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.CMDSenderServiceTask", Core.Constants.CountryCodes.Singapore },
				{ "Enterprise.Customs.SG.V4.ServiceTasks.SGCustomsServiceTask", Core.Constants.CountryCodes.Singapore },
				{ "Enterprise.Customs.TR.ServiceTasks.MessageProcessorService", Core.Constants.CountryCodes.Turkey },
				{ "Enterprise.Customs.TR.ServiceTasks.MessageSenderService", Core.Constants.CountryCodes.Turkey },
				{ "Enterprise.Customs.TR.ServiceTasks.MessageRetrieverService", Core.Constants.CountryCodes.Turkey },
				{ "Enterprise.Customs.TW.ServiceTasks.MessageRetrieverService", Core.Constants.CountryCodes.Taiwan },
				{ "Enterprise.Customs.US.DIS.ServiceTasks.USDISServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.ABIOutgoingServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.BIRDDataTransferServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.ABIServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.EBondServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.AESServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.USRServiceTask", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.AntiDumpingRequester", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.CarrierCodeRequester", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.CountryRequester", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.FIRMSCodeRequester", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.ForeignPortCodeRequester", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.US.ServiceTasks.HTSRequester", Core.Constants.CountryCodes.UnitedStates + "," + Core.Constants.CountryCodes.PuertoRico },
				{ "Enterprise.Customs.UY.Manifest.ServiceTasks.MessageRetrieverService", Core.Constants.CountryCodes.Uruguay },
				{ "Enterprise.Customs.UY.Manifest.ServiceTasks.MessageSenderService", Core.Constants.CountryCodes.Uruguay },
				{ "Enterprise.Customs.ZA.ServiceTasks.MessageRetrieverService", Core.Constants.CountryCodes.SouthAfrica },
				{ "Enterprise.Customs.ZA.ServiceTasks.MessageSenderService", Core.Constants.CountryCodes.SouthAfrica },
				{ "Enterprise.Customs.CustomsWare.ServiceTasks.InterchangeProcessorServiceTask", Core.Constants.CountryCodes.Switzerland + "," + Core.Constants.CountryCodes.Ireland + "," + Core.Constants.CountryCodes.Netherlands + "," + Core.Constants.CountryCodes.UnitedArabEmirates + "," + Core.Constants.CountryCodes.Belgium + "," + Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.CustomsWare.ServiceTasks.SubmissionSenderServiceTask", Core.Constants.CountryCodes.Switzerland + "," + Core.Constants.CountryCodes.Ireland + "," + Core.Constants.CountryCodes.Netherlands + "," + Core.Constants.CountryCodes.UnitedArabEmirates + "," + Core.Constants.CountryCodes.Belgium + "," + Core.Constants.CountryCodes.Germany },
				{ "Enterprise.Customs.KR.ServiceTasks.InBoundServiceTask", Core.Constants.CountryCodes.KoreaSouth },
				{ "Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask", Core.Constants.CountryCodes.KoreaSouth },
				{ "Enterprise.Customs.KR.ServiceTasks.MessageGeneratorServiceTask", Core.Constants.CountryCodes.KoreaSouth },
				{ "Enterprise.Customs.KR.ServiceTasks.PublicKeyUpdateServiceTask", Core.Constants.CountryCodes.KoreaSouth },
				{ "Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
				{ "Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask", Core.Constants.CountryCodes.UnitedKingdom },
			};

			var allCustomsAssemblies = BuildXml.Instance
				.GetAllAssembliesToBuild(false)
				.Where(c => c.StartsWith("Enterprise.Customs.", StringComparison.OrdinalIgnoreCase))
				.ToDictionary(System.IO.Path.GetFileNameWithoutExtension)
				.Keys
				.ToArray();
			var retriever = new SubClassRetriever(allCustomsAssemblies, typeof(ServiceProviderImpl))
			{
				IncludeAbstractClasses = false,
				IncludeAutoGeneratedCode = true,
				IncludeClientDlls = true,
				IncludeNestedClasses = true,
				IncludePrivateNestedClasses = true,
				IncludeTestClasses = false,
				IncludeNonAutoGeneratedCode = true,
			};
			var retrievedTypes = retriever.Retrieve().Where(c => true).ToArray().GroupBy(c => c.Assembly).SelectMany(c => c);

			var exceptionMessages = new ZStringBuilder();
			foreach (var serviceTaskType in retrievedTypes)
			{
				var serviceTaskTypeName = serviceTaskType.FullName;
				var requiresCompanyInCountry = GetHostedServiceAttributes(serviceTaskType)?.SingleOrDefault()?.RequiresCompanyInCountry;
				if (!exceptionsList.Contains(serviceTaskTypeName))
				{
					if (requiresCompanyInCountryDictionary.ContainsKey(serviceTaskTypeName))
					{
						if (requiresCompanyInCountry != requiresCompanyInCountryDictionary[serviceTaskTypeName])
						{
							exceptionMessages.AppendLine(serviceTaskTypeName + "(invalid value: " + requiresCompanyInCountry + ")");
						}

						requiresCompanyInCountryDictionary.Remove(serviceTaskTypeName);
					}
					else if (canRunOnAnyCompanyList.Contains(serviceTaskTypeName))
					{
						if (!string.IsNullOrEmpty(requiresCompanyInCountry))
						{
							exceptionMessages.AppendLine(serviceTaskTypeName + "(should not add requiresCompanyInCountry, invalid value: " + requiresCompanyInCountry + ")");
						}

						canRunOnAnyCompanyList.Remove(serviceTaskTypeName);
					}
				}
			}

			var errors = new ZStringBuilder();
			if (exceptionMessages.Length > 0)
			{
				errors.AppendLine("Please check the following service tasks RequiresCompanyInCountry attribute:");
				errors.Append(exceptionMessages.ToStringWithNewLineBetweenAppends());
			}
			CombineAssertions(() =>
			{
				AssertEquals("Service Tasks exist in requiresCompanyInCountry that no longer exist in production", string.Empty, string.Join(", ", requiresCompanyInCountryDictionary.Keys));
				AssertEquals("Service Tasks exist in canRunOnAnyCompanyList that no longer exist in production", string.Empty, string.Join(", ", canRunOnAnyCompanyList));
				Assert(errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
			});
		}

		HostedServiceAttribute[] GetHostedServiceAttributes(Type customsServiceTaskType)
		{
			var assembly = customsServiceTaskType.Assembly;
			var attributes = Array.ConvertAll(assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false), attribute => (HostedServiceAttribute)attribute);
			attributes = Array.FindAll(attributes, attribute => attribute.TypeName.Equals(customsServiceTaskType.FullName, StringComparison.Ordinal));

			return attributes;
		}
	}
}
