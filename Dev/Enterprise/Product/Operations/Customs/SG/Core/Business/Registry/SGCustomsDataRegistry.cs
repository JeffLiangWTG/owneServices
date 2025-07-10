using System;
using Enterprise.Core;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.SG.V4.Business.ResString;

namespace Enterprise.Customs.SG.Registry
{
	public sealed class SGCustomsDataRegistry : RegistryItemSet, Integration.Customs.SG.ISGCustomsRegistry
	{
		public static SGCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new SGCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static SGCustomsDataRegistry instance;

		SGCustomsDataRegistry()
		{
		}

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = CountryFilterPKs.Singapore;
		}

		public override bool IsForProductivityWise => false;

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Singapore => CombineCategories(Customs_CountryOrRegion, (NoResString)"Singapore");
			public static MultilingualString Customs_Singapore_CertificateofOrigin => CombineCategories(Customs_Singapore, (NoResString)"Certificate of Origin");
			public static MultilingualString Customs_Singapore_CMD => CombineCategories(Customs_Singapore, (NoResString)"CMD");
			public static MultilingualString Customs_Singapore_Emails => CombineCategories(Customs_Singapore, (NoResString)"Emails");
			public static MultilingualString Customs_Singapore_Tradenetv4 => CombineCategories(Customs_Singapore, (NoResString)"Tradenet v4");
			public static MultilingualString Customs_Singapore_ACCESS => CombineCategories(Customs_Singapore, (NoResString)"ACCESS");
			public static MultilingualString Customs_Singapore_Tradenetv4_MHUB => CombineCategories(Customs_Singapore_Tradenetv4, (NoResString)"MHUB");
			public static MultilingualString Customs_Singapore_Tradenetv4_MHUB_Mhaccess4 => CombineCategories(Customs_Singapore_Tradenetv4_MHUB, (NoResString)"Mhaccess4");
			public static MultilingualString Customs_Singapore_NTP => CombineCategories(Customs_Singapore, (NoResString)"NTP");
		}

		public CodeDescriptionPairListRegistryItem OriginCriterion
		{
			get
			{
				return GetItem("OriginCriterion", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"OriginCriterion",
						Categories.Customs_Singapore_CertificateofOrigin,
						(NoResString)"Origin Criterion",
						(NoResString)"List of Origin Criterion required for Certificate of Origin requests. \r\n\r\nNote: If editing this list, Origin Criterion is specified over three distinct fields. \r\nEach field can have 25 characters maximum.\r\nUse the comma (,) separator to split the value into fields 2 and 3 if required.",
						75,
						RegistryStorageFlags.Company,
						true,
						GetDefaultOriginCriterionList()
						);
				});
			}
		}

		CodeDescriptionPairList GetDefaultOriginCriterionList()
		{
			return new OriginCriterionCodeList();
		}

		public BinaryRegistryItem PermitPrintingCertificate
		{
			get
			{
				return GetItem("PermitPrintingCertificate", delegate
				{
					return new BinaryRegistryItem(
						"PermitPrintingCertificate",
						Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Permit Printing Certificate", // Support only registry item
						(NoResString)"Specify the Permit Printing Certificate", // Support only registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport,
						SingatureRepository.Instance.DefaultPermitPrintCertificateData)
					{
						EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(PermitPrintingCertificatePassword)
					};
				});
			}
		}

		public StringRegistryItem PermitPrintingCertificatePassword
		{
			get
			{
				return GetItem("PermitPrintingCertificatePassword", delegate
				{
					return new StringRegistryItem
					(
						"PermitPrintingCertificatePassword",
						Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Permit Printing Certificate Password", // Support only registry item
						(NoResString)"Specify the Password for Permit Printing Certificate",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport,
						SingatureRepository.Instance.DefaultPermitPrintCertificatePassword)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
					};
				});
			}
		}

		#region Send Impediment Emails

		public CodePairRegistryItem SendImpediments
		{
			get
			{
				return GetItem("SendImpediments", delegate
				{
					return new CodePairRegistryItem(
						"SendImpediments",
						Categories.Customs_Singapore_Emails,
						(NoResString)"Send Impediment Queries",
						(NoResString)"Send Customs or Controlling Agency query instructions to staff member, nominated group or combination of both",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendImpedimentsToGroup
		{
			get
			{
				return GetItem("SendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendImpedimentsToGroup",
						Categories.Customs_Singapore_Emails,
						(NoResString)"Send Impediment Queries To Group",
						(NoResString)"Send Customs or Controlling Agency query instructions to selected group",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		public IntRegistryItem MaximumMessageSize
		{
			get
			{
				return GetItem("MaximumMessageSize", delegate
				{
					return new IntRegistryItem(
						"MaximumMessageSize",
						SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Maximum Message Size",
						(NoResString)"Maximum Message Size.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						10, 10, 100);
				});
			}
		}

		public BooleanRegistryItem VerboseLogging
		{
			get
			{
				return GetItem("VerboseLogging", delegate
				{
					return new BooleanRegistryItem(
						"VerboseLogging",
						SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Verbose Logging - TradeNet Service Task - Complex message transmission logging",
						(NoResString)"Setting this registry item on will enable verbose logging of the TradeNet Messaging Service Task, to assist when investigating communication issues.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem MessageNumberOffset
		{
			get
			{
				return GetItem("MessageNumberOffset", delegate
				{
					return new IntRegistryItem(
						"MessageNumberOffset",
						Categories.Customs_Singapore,
						(NoResString)"Message Number Offset",
						(NoResString)"Offset value used to generate a different URN number range.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						0,
						0,
						9999);
				});
			}
		}

		#region CMD

		public StringRegistryItem CargoAgentCode
		{
			get
			{
				return GetItem("CargoAgentCode", delegate
				{
					return new StringRegistryItem(
						"CargoAgentCode",
						Categories.Customs_Singapore_CMD,
						(NoResString)"Cargo Agent Code",
						(NoResString)"Cargo Agent Code required for sending CMD Messages",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						string.Empty);
				});
			}
		}

		public StringRegistryItem CargoAgentRef
		{
			get
			{
				return GetItem("CargoAgentRef", delegate
				{
					return new StringRegistryItem(
						"CargoAgentRef",
						Categories.Customs_Singapore_CMD,
						(NoResString)"Cargo Agent Reference",
						(NoResString)"Cargo Agent Reference required for sending CMD Messages",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						string.Empty);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem GHAList
		{
			get
			{
				return GetItem("GHAList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"GHAList",
						Categories.Customs_Singapore_CMD,
						ResString.GetMultilingualString("F79EB1D7-7922-4B5F-9047-257D3BB34233", "GHA List"),
						ResString.GetMultilingualString("EC82F252-76DB-40ED-A655-2AD9D93A45E8", "List of Ground Handling Agents operating in Singapore"),
						4,
						RegistryStorageFlags.System,
						true,
						RegistryOptions.PreserveTestValue,
						DefaultGHAList);
				});
			}
		}

		CodeDescriptionPairList DefaultGHAList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Constants.SGCustoms.GHA.DNAT, ResString.GetMultilingualString("8ACD138B-435F-4217-B213-97A00771A6B0", "DNATA GHA"));
				result.AddPair(Constants.SGCustoms.GHA.SATS, ResString.GetMultilingualString("62D5C573-F175-47D0-98B9-C39FE73ACF51", "SATS GHA"));
				return result;
			}
		}

		public BooleanRegistryItem SendViaEHub
		{
			get
			{
				return GetItem("SendViaEHub", delegate
				{
					return new BooleanRegistryItem(
						"SendViaEHub",
						Categories.Customs_Singapore_CMD,
						(NoResString)"Send via eHub",
						(NoResString)"Send CMD Messages through eHub",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						true);
				});
			}
		}

		#endregion

		#region Tradenet All Versions

		public CodePairRegistryItem SendAcknowledgements
		{
			get
			{
				return GetItem("SendAcknowledgements", delegate
				{
					return new CodePairRegistryItem(
						"SendAcknowledgements",
						Categories.Customs_Singapore_Emails,
						(NoResString)"Send Declaration Acknowledgements",
						(NoResString)"Send message acknowledgements to staff member, nominated group or combination of both",
						new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("SendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendAcknowledgementsToGroup",
						Categories.Customs_Singapore_Emails,
						(NoResString)"Send Declaration Acknowledgements to Group",
						(NoResString)"Send message acknowledgements to selected group",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem SendErrors
		{
			get
			{
				return GetItem("SendErrors", delegate
				{
					return new CodePairRegistryItem(
						"SendErrors",
						Categories.Customs_Singapore_Emails,
						(NoResString)"Send Declaration Errors",
						(NoResString)"Send message errors to staff member, nominated group or combination of both",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendErrorsToGroup
		{
			get
			{
				return GetItem("SendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendErrorsToGroup",
						Categories.Customs_Singapore_Emails,
						(NoResString)"Send Declaration Errors To Group",
						(NoResString)"Send message errors to selected group",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Tradenet 4

		public BooleanRegistryItem AttachPermitToAcknowledgementEmail
		{
			get
			{
				return GetItem("AttachPermitToAcknowledgementEmail", delegate
				{
					return new BooleanRegistryItem(
						"AttachPermitToAcknowledgementEmail",
						Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Attach Permit To Acknowledgement Email",
						(NoResString)"A PDF version of the permit is attached to acknowledgement emails",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public DateTimeRegistryItem LastSentEmailWithBrokerErrors
		{
			get
			{
				return GetItem("LastSentEmailWithBrokerErrors", delegate
				{
					return new DateTimeRegistryItem(
						"LastSentEmailWithBrokerErrors",
						Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Last Sent Email With Broker Errors",
						(NoResString)"Date of last sent email with broker errors",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsHidden,
						DateTime.MinValue);
				});
			}
		}

		#region MHUB

		public BooleanRegistryItem IsolateMhxFilesPerProcessController
		{
			get
			{
				return GetItem("IsolateMhxFilesPerProcessController", delegate
				{
					return new BooleanRegistryItem(
						"IsolateMhxFilesPerProcessController",
						Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Isolate MHX files per task",
						(NoResString)"Install and consume MHX java assemblies in a shared area (off) or once per process controller (on, default)",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public StringEffectiveDateRegistryItem WebAddress
		{
			get
			{
				return GetItem("WebAddress", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "https://www.tradexchange.gov.sg";
					stringEffectiveDateDefault.NewValue = "https://www.tradenet.gov.sg";
					stringEffectiveDateDefault.EffectiveDate = MHUBAccessUpgradeEffectiveDate.Value;
					return new StringEffectiveDateRegistryItem(
						"WebAddress",
						Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Web Address",
						(NoResString)"The web address of the TradeNet applet server",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						stringEffectiveDateDefault);
				});
			}
		}

		#region MHAccess MHX Four 4 

		public DateTimeRegistryItem MHUBAccessUpgradeEffectiveDate
		{
			get
			{
				return GetItem("MHUBAccessUpgradeEffectiveDate", delegate
				{
					return new DateTimeRegistryItem(
						"MHUBAccessUpgradeEffectiveDate",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"MHAccess upgrade effective date",
						(NoResString)"Effective Date when the MHAccess upgrade comes into effect.\r\nSG Customs are upgrading their MHAccess software to utilise JRE 1.8 and improved GCM (Galois Counter Mode) cipher.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						new DateTime(2017, 07, 27));
				});
			}
		}

		public StringEffectiveDateRegistryItem JREVersion
		{
			get
			{
				return GetItem("JREVersion", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "1.6.0_121";
					stringEffectiveDateDefault.NewValue = "1.8.0_131";
					stringEffectiveDateDefault.EffectiveDate = MHUBAccessUpgradeEffectiveDate.Value;
					return new StringEffectiveDateRegistryItem(
						"JREVersion",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Java version",
						(NoResString)"The current Java version for MHAccess software to utilise.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						stringEffectiveDateDefault);
				});
			}
		}

		public StringEffectiveDateRegistryItem WebAddressTrial
		{
			get
			{
				return GetItem("WebAddressTrial", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "https://trial.tradexchange.gov.sg";
					stringEffectiveDateDefault.NewValue = "https://trial.tradenet.gov.sg";
					stringEffectiveDateDefault.EffectiveDate = MHUBAccessUpgradeEffectiveDate.Value;
					return new StringEffectiveDateRegistryItem(
						"WebAddressTrial",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Web Address (trial MHX4)",
						(NoResString)"The web address of the TRIAL TradeNet applet server. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						stringEffectiveDateDefault);
				});
			}
		}

		public StringRegistryItem WebProxyAddress
		{
			get
			{
				return GetItem("WebProxyAddress", delegate
				{
					return new StringRegistryItem(
						"WebProxyAddress",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Web Proxy",
						(NoResString)"The web address (server.com:port) of the proxy through which MHAccess should connect",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"");
				});
			}
		}

		public StringRegistryItem MhaccessTrustStore_Trial
		{
			get
			{
				return GetItem("MhaccessTrustStore_Trial", delegate
				{
					return new StringRegistryItem(
						"MhaccessTrustStore_Trial",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Trust store (trial)",
						(NoResString)"Trust store (trial). Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						@"MHX_TrialTruststore.db");
				});
			}
		}

		public StringRegistryItem MhaccessTrustStore_Prod
		{
			get
			{
				return GetItem("MhaccessTrustStore_Prod", delegate
				{
					return new StringRegistryItem(
						"MhaccessTrustStore_Prod",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Trust store (prod)",
						(NoResString)"Trust store (prod). Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						@"MHX_ProdTruststore.db");
				});
			}
		}

		public StringRegistryItem MhaccessExePathFallback
		{
			get
			{
				return GetItem("MhaccessExePathFallback", delegate
				{
					return new StringRegistryItem(
						"MhaccessExePathFallback",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"MHAccess.exe",
						(NoResString)"Directory in which MHAccess.exe is found. First the program will look in the Windows Registry to find the install location, but if this Windows Registry node is not given or the named value does not work, try here instead.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						@"C" + @":\Program Files (x86)\MHAccess");
				});
			}
		}

		public BooleanRegistryItem UseMhxDirectWebServicesInsteadOfScripting
		{
			get
			{
				return GetItem("SGUseMhxDirectWebServicesInsteadOfScripting", delegate
				{
					return new BooleanRegistryItem(
						"SGUseMhxDirectWebServicesInsteadOfScripting",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Use direct web service access for MHX4",
						(NoResString)"Use direct webservice access? If no, MHAccess script automation is used instead. This option is dependent o the option 'Use version 4 of MHX' and does nothing if that is not enabled.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public StringEffectiveDateRegistryItem MhaccessVersionString
		{
			get
			{
				return GetItem("MhaccessVersionString", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "4.0.3";
					stringEffectiveDateDefault.NewValue = "4.0.4";
					stringEffectiveDateDefault.EffectiveDate = MHUBAccessUpgradeEffectiveDate.Value;
					return new StringEffectiveDateRegistryItem(
						"MhaccessVersionString",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Version Number",
						(NoResString)"Version of MHAccess. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						stringEffectiveDateDefault);
				});
			}
		}

		public StringRegistryItem VendorID
		{
			get
			{
				return GetItem("VendorID", delegate
				{
					return new StringRegistryItem(
						"VendorID",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Vendor ID",
						(NoResString)"Vendor ID. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"A7");
				});
			}
		}
		public StringRegistryItem EncryptionKey
		{
			get
			{
				return GetItem("EncryptionKey", delegate
				{
					return new StringRegistryItem(
						"EncryptionKey",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Encryption Key",
						(NoResString)"Encryption key (content of key.txt). Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"UdJCd/zLFmybL6HgjSD08TkJKA9tGcis");
				});
			}
		}

		public StringEffectiveDateRegistryItem DigestForLibs
		{
			get
			{
				return GetItem("DigestForLibs", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "6DB57667814ABDD96DE73FED486095F0F0947F1E7FB64DA8538FCAA50CA40BDB";
					stringEffectiveDateDefault.NewValue = "E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D";
					stringEffectiveDateDefault.EffectiveDate = MHUBAccessUpgradeEffectiveDate.Value;
					return new StringEffectiveDateRegistryItem(
						"DigestForLibs",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Digest",
						(NoResString)"Digest of MHAccess libraries. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						stringEffectiveDateDefault);
				});
			}
		}

		public StringRegistryItem Mhx4EndpointPartialPathForUpload
		{
			get
			{
				return GetItem("Mhx4EndpointPartialPathForUpload", delegate
				{
					return new StringRegistryItem(
						"Mhx4EndpointPartialPathForUpload",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Endpoint - upload",
						(NoResString)"Relative URI for uploads endpoint. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						"/txmhbweb/upload/uploadService");
				});
			}
		}

		public StringRegistryItem Mhx4EndpointPartialPathForDownload
		{
			get
			{
				return GetItem("Mhx4EndpointPartialPathForDownload", delegate
				{
					return new StringRegistryItem(
						"Mhx4EndpointPartialPathForDownload",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Endpoint - download",
						(NoResString)"Relative URI for downloads endpoint. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						"/txmhbweb/download/downloadService");
				});
			}
		}

		public StringRegistryItem Mhx4EndpointPartialPathForServlet
		{
			get
			{
				return GetItem("Mhx4EndpointPartialPathForServlet", delegate
				{
					return new StringRegistryItem(
						"Mhx4EndpointPartialPathForServlet",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Endpoint - servlet",
						(NoResString)"Relative URI for servlet endpoint. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						"/txmhbweb/mhb/EDIServlet");
				});
			}
		}

		public StringRegistryItem Mhx4EndpointPartialPathForTestConnectionServlet
		{
			get
			{
				return GetItem("Mhx4EndpointPartialPathForTestConnectionServlet", delegate
				{
					return new StringRegistryItem(
						"Mhx4EndpointPartialPathForTestConnectionServlet",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Endpoint - Test Connection Servlet",
						(NoResString)"Relative URI for Test Connection Servlet endpoint. Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						"/txmhbweb/mhb/TestConnectionServlet");
				});
			}
		}

		public StringRegistryItem RecipientIDLive
		{
			get
			{
				return GetItem("RecipientIDLive", delegate
				{
					return new StringRegistryItem(
						"RecipientIDLive",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Recipient ID (live)",
						(NoResString)"Recipient ID (live). Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"DCS4001");
				});
			}
		}

		public StringRegistryItem RecipientIDTrial
		{
			get
			{
				return GetItem("RecipientIDTrial", delegate
				{
					return new StringRegistryItem(
						"RecipientIDTrial",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Recipient ID (trial)",
						(NoResString)"Recipient ID (trial). Do not edit this without instruction from WTG.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"DCST401");
				});
			}
		}

		public StringRegistryItem SoapDownloadDirectory
		{
			get
			{
				return GetItem("SoapDownloadDirectory", delegate
				{
					return new StringRegistryItem(
						"SoapDownloadDirectory",
						Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
						(NoResString)"Soap Download Directory",
						(NoResString)"The downloads directory on the TradeNet server. Supported macros are <<LOGIN>> meaning the username.",
						RegistryStorageFlags.System,
						"/fshome/sftphome/<<LOGIN>>/mhxdownload/");
				});
			}
		}

		#endregion

		public StringRegistryItem HomeDirectory
		{
			get
			{
				return GetItem("HomeDirectory", delegate
				{
					return new StringRegistryItem(
						"HomeDirectory",
						Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Server Home Directory",
						(NoResString)"The home directory on the TradeNet server",
						RegistryStorageFlags.System,
						"/fshome/sftp/cwise");
				});
			}
		}

		public StringRegistryItem RecipientMailbox
		{
			get
			{
				return GetItem("RecipientMailbox", delegate
				{
					return new StringRegistryItem(
						"RecipientMailbox",
						Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Recipient Mailbox",
						(NoResString)"The mailbox that TradeNet receive/send messages to",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"DCS4001");
				});
			}
		}

		public IntRegistryItem SynchronousMHUBTimeout
		{
			get
			{
				return GetItem("SynchronousMHUBTimeout", delegate
				{
					return new IntRegistryItem(
							"SynchronousMHUBTimeout",
							Categories.Customs_Singapore_Tradenetv4_MHUB,
							(NoResString)"Synchronous Timeout",
							(NoResString)"How many milliseconds should be waited before initial connection is cancelled",
							RegistryStorageFlags.System,
							RegistryOptions.PreserveTestValue,
							300000);
				});
			}
		}

		public IntRegistryItem SynchronousMHUBReadWriteTimeout
		{
			get
			{
				return GetItem("SynchronousMHUBReadWriteTimeout", delegate
				{
					return new IntRegistryItem(
							"SynchronousMHUBReadWriteTimeout",
							Categories.Customs_Singapore_Tradenetv4_MHUB,
							(NoResString)"Synchronous ReadWrite Timeout",
							(NoResString)"How many milliseconds should be waited before between packets before connection is cancelled",
							RegistryStorageFlags.System,
							RegistryOptions.PreserveTestValue,
							600000);
				});
			}
		}
		public IntRegistryItem FTPThreadTimeout
		{
			get
			{
				return GetItem("FTPThreadTimeout", delegate
				{
					return new IntRegistryItem(
							"FTPThreadTimeout",
							Categories.Customs_Singapore_Tradenetv4_MHUB,
							(NoResString)"FTP Thread Timeout",
							(NoResString)"FTP Thread Timeout",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							0);
				});
			}
		}

		public IntRegistryItem FTPProcessTimeout
		{
			get
			{
				return GetItem("FTPProcessTimeout", delegate
				{
					return new IntRegistryItem(
									"FTPProcessTimeout",
									Categories.Customs_Singapore_Tradenetv4_MHUB,
									(NoResString)"FTP Process Timeout",
									(NoResString)"FTP Process Timeout",
									RegistryStorageFlags.System,
									RegistryOptions.Default,
									200000);
				});
			}
		}

		#region Test System

		public BooleanRegistryItem SendTestMessages
		{
			get
			{
				return GetItem("SendTestMessages", delegate
				{
					return new BooleanRegistryItem(
						"SendTestMessages",
						Categories.Customs_Singapore_Tradenetv4_MHUB,
						(NoResString)"Send Test Messages to TradeNet Version 4",
						(NoResString)"Setting this flag on will mean TradeNet messages generated will be sent into the MHub Version 4 Test System.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region ACCESS > Enable
		public BooleanRegistryItem ACCESSEnable
		{
			get
			{
				return GetItem("ACCESSEnable", delegate
				{
					return new BooleanRegistryItem(
						"ACCESSEnable",
						Categories.Customs_Singapore_ACCESS,
						(NoResString)"Enabled",
						(NoResString)"Company listed is enabled for ACCESS system.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		#endregion

		#region ACCESS > Cycle Numbers

		public CycleNoRegistryItem CycleNumbers
		{
			get
			{
				const string hint = "This is a list of Cycle Numbers applicable for the ACCESS system.";

				return GetItem("CycleNumbers", delegate
				{
					return new CycleNoRegistryItem(
						"CycleNumbers",
						Categories.Customs_Singapore_ACCESS,
						"Cycle Numbers",
						hint,
						RegistryStorageFlags.System,
						new CycleNoCollection().GetDefaultCollection);
				});
			}
		}

		#endregion

		#region Send XML Message

		public BooleanRegistryItem EnableXMLTradeNetMessaging
		{
			get
			{
				return GetItem("EnableXML", delegate
				{
					return new BooleanRegistryItem(
						"EnableXML",
						SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Enable XML TradeNet Messaging",
						(NoResString)"The system will send and receive XML TradeNet messages when enabled.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						true);
				});
			}
		}

		#endregion

		#region Message Submission Retry

		public IntRegistryItem SubmissionRetryLimit
		{
			get
			{
				return GetItem("SubmissionRetryLimit", delegate
				{
					return new IntRegistryItem(
						"SubmissionRetryLimit",
						SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Submission Retry Limit",
						(NoResString)"Please enter the number of submission retries before failing a TradeNet interchange.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						3, 1, 5);
				});
			}
		}

		public IntRegistryItem SubmissionFinalRetryDelay
		{
			get
			{
				return GetItem("SubmissionFinalRetryDelay", delegate
				{
					return new IntRegistryItem(
						"SubmissionFinalRetryDelay",
						SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4,
						(NoResString)"Submission Final Retry Delay",
						(NoResString)"Please enter the number of minutes to delay the final submission retry to TradeNet. Maximum of 60 minutes.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						30, 0, 60);
				});
			}
		}

		#endregion

		#region ISGCustomsRegistry Members
		IRegistryItem Integration.Customs.SG.ISGCustomsRegistry.ACCESSEnable => ACCESSEnable;

		IRegistryItem Integration.Customs.SG.ISGCustomsRegistry.CycleNumbers => CycleNumbers;
		#endregion
	}
}
