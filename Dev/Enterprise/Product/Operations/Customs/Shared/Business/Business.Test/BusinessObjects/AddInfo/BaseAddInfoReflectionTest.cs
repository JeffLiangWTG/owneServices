using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseAddInfoReflectionTest : TestCaseWithFactory
	{
		[SnailTest]
		public void TestCustomsBusinessAssembliesSpecifyUniversalCopyAddInfoPropertyDefinitionAttribute()
		{
			var addInfoManagerSubClasses = GetIAddInfoManagerTypes();
			var retrievedTypesByAssembly = addInfoManagerSubClasses
				.GroupBy(c => c.Assembly);

			CombineAssertions(() =>
			{
				foreach (var assemblyGroup in retrievedTypesByAssembly)
				{
					var definitionAttribute = assemblyGroup.Key.GetCustomAttribute<UniversalCopyAddInfoPropertyDefinitionAttribute>();
					AssertNotNull($@"Assembly {assemblyGroup.Key} should specify Attribute UniversalCopyAddInfoPropertyDefinition in AssemblyInfo.cs.
Because it has IAddInfoManager sub classes:
{string.Join("\r\n", assemblyGroup.Select(x => x.FullName))}'.", definitionAttribute);
				}
			});
		}

		[SnailTest]
		public void TestPrefixOfWrappedAddInfoProperty()
		{
			var businessObjectType = typeof(BusinessObject);

			var retrievedTypes = GetIAddInfoManagerTypes()
				.GroupBy(c => c.Assembly)
				.Where(c => c.Key.GetCustomAttribute<UniversalCopyAddInfoPropertyDefinitionAttribute>() != null)
				.SelectMany(c => c);

			var notMatchedName = new List<(Type type, string message)>();
			var exceptionMessages = new ZStringBuilder();

			foreach (var retrievedType in retrievedTypes)
			{
				if (retrievedType.IsSubclassOf(businessObjectType) && retrievedType.GetCustomAttribute<UniversalCopyAddInfoAttribute>() != null)
				{
					var assembly = retrievedType.Assembly;

					using (ClientHookLoader.Instance.OverrideClientAssemblyForTestIfNeeded(assembly) ?? DisposableAction.NoAction)
					{
						try
						{
							var countryCode = assembly.GetCustomAttribute<UniversalCopyAddInfoPropertyDefinitionAttribute>().CountryCode;
							if (string.IsNullOrEmpty(countryCode))
							{
								countryCode = assembly.GetCustomAttribute<CountrySpecificTestAttribute>().CountryCode;
							}
							using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
							{
								var newfactory = NewFactory();

								var master = newfactory.New(retrievedType);
								var addInfo = ((IAddInfoManager)master).AddInfo as BusinessObject;

								if (master != null && addInfo != null)
								{
									var topLevelAttribute = retrievedType.GetCustomAttributes(typeof(UniversalCopyAddInfoAttribute), true).FirstOrDefault() as UniversalCopyAddInfoAttribute;

									var addInfoProperties = addInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().ToArray();

									var wrappedInfoGroups = master.ZPropertyInfoHash
										.GetPropertyInfos(PropertyInfoTypes.Wrapping)
										.Cast<ZWrappedPropertyInfo>()
										.Where(c => c.InnerInfo != null && retrievedType.GetProperty(c.Name) != null)
										.GroupBy(c => c.InnerInfo);

									string GetMappingName(ZPropertyInfo info)
									{
										var mappingAttribute = info.PropertyDescriptor.Attributes[typeof(UniversalCopyAddInfoPropertyMappingAttribute)] as UniversalCopyAddInfoPropertyMappingAttribute;
										var result = mappingAttribute?.AddInfoPropertyInfoName ?? string.Empty;

										return result.IsNullOrEmpty() && topLevelAttribute != null && topLevelAttribute.HasMapping
											? string.Concat(topLevelAttribute.AddInfoPrefix, info.Name.Remove(0, topLevelAttribute.PropertyPrefix.Length))
											: result;
									}

									foreach (var grouping in wrappedInfoGroups)
									{
										var wrapperInnerInfo = grouping.Key;
										var addInfoPropertyInfo = addInfoProperties.FirstOrDefault(c => c == wrapperInnerInfo);

										if (addInfoPropertyInfo != null)
										{
											var wrappedInfoNames = grouping.Select(c => c.Name);
											var wrappedMappingInfoNames = grouping.Select(GetMappingName).Distinct();

											var addInfoName = addInfoPropertyInfo.Name;

											if (wrappedInfoNames.All(c => c != addInfoName) && wrappedMappingInfoNames.All(c => c != addInfoName))
											{
												notMatchedName.Add((retrievedType, $"{addInfoName} => {string.Join(", ", wrappedInfoNames)}"));
											}
										}
									}
								}
							}
						}
						catch (Exception exception)
						{
							exceptionMessages.AppendLine($"{retrievedType.FullName} - {GetInnerExceptionMessage(exception)}");
						}
					}
				}
			}

			var errors = new ZStringBuilder();

			if (notMatchedName.Any())
			{
				errors.Append("Please follow one of below rules on an AddInfo property and its Wrappered property. You can use UniversalCopyAddInfoAttribute to define the top level prefix mapping on the top of type.");
				errors.Append("a) Same Name -> [UniversalCopyAddInfo]");
				errors.Append("b) Same suffix with diferent prefix -> [UniversalCopyAddInfo('XX', 'YY')], 'XX' is a property prefix,'YY' is an addInfo prefix");
				errors.Append("c) Use UniversalCopyAddInfoPropertyMappingAttribute for a property level mapping -> [UniversalCopyAddInfoPropertyMapping('YY_123')], 'YY_123' is an addInfo property name");
				errors.AppendLine();
				errors.AppendLine("These incorrect properties are:");

				var groups = notMatchedName.GroupBy(c => c.type.FullName).OrderBy(c => c.Key);
				errors.Append(string.Join(System.Environment.NewLine, groups.Select(c => $"Type: {c.Key}{System.Environment.NewLine}{System.Environment.NewLine}{string.Join(System.Environment.NewLine, c.Select(d => d.message))}{System.Environment.NewLine}")));
			}

			if (exceptionMessages.Length > 0)
			{
				errors.AppendLine();
				errors.Append("There are some exceptions when the system is finding matched property infos on these types:");
				errors.AppendLine();

				errors.Append(exceptionMessages.ToStringWithNewLineBetweenAppends());
			}

			Assert(errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		[SnailTest]
		public void TestNoNewColumnsToColumnsForFastSearch()
		{
			var exceptionsList = new HashSet<string>()
			{
				"column:SG_OutwardHAWB, class:Enterprise.Customs.SG.V4.Business.AddInfoJobDeclaration",
				"column:SG_OutwardMAWB, class:Enterprise.Customs.SG.V4.Business.AddInfoJobDeclaration",
				"column:SG_OutwardVesselName, class:Enterprise.Customs.SG.V4.Business.AddInfoJobDeclaration",
				"column:SG_OutwardVoyageFlightNo, class:Enterprise.Customs.SG.V4.Business.AddInfoJobDeclaration",
				"column:SG_OutwardTransportMode, class:Enterprise.Customs.SG.V4.Business.AddInfoJobDeclaration",
				"column:ZG_Parallel, class:Enterprise.Customs.ES.Business.Declaration.AddInfoCusEntryHeader",
				"column:US_PackageReference, class:Enterprise.Customs.US.Business.USWHSPackAddInfo",
				"column:US_JI_InvoiceLine, class:Enterprise.Customs.US.Business.USWHSPackLineAddInfo",
				"column:US_B7_WHSPack, class:Enterprise.Customs.US.Business.USWHSPackLineAddInfo",
				"column:US_ITDate, class:Enterprise.Customs.US.Business.AddInfoBill",
				"column:US_TIBExpiryDate, class:Enterprise.Customs.US.Business.AddInfoCusEntryHeader",
				"column:US_ALDate, class:Enterprise.Customs.US.Business.AddInfoCusEntryHeader",
				"column:US_F_AdmissionType, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_US_NKLocationOfGoods, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_OtherReconIndicator, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_NAFTAReconIndicator, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_SuretyCode, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EntryType, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_InbondType, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EntryMode, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_SchDEntry, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_TIBExpiryDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_FilingDDPP, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_PeriodBaseDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_Assoc514ProtestNo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_Assoc520PetitionNo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_ProtestantType, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_ApplicationFurtherReview, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_AcceleratedDispositionInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EntryFilerCode, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_BRDRefNo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PaymentDueDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_IsAIIRequested, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PreliminaryStatementPrintDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PaymentType, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EstimatedEntryDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_SchDLoading, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_SchDArrival, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PaperlessEntry, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_IssueCode, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_IsAggregate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EntryDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_TeamNo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_NAFTADrawbackCountry, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EntryFilerCode, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PreparerDistrictPort, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_ClaimPort, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_ExporterSummaryInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PreInspectionInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_NAFTAClaimInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_AcceleratedClaimInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_WaiverNoticeInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PetroleumClaimInd, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_EarliestExportDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_DRWDatePeriodFrom, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_DRWDatePeriodTo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PresentationDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_PSC, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_SchDExport, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_DateOfExport, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_RN_NKCountryOfDestination, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_TransportReference, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_UI_NKCarrierSCAC, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_P_StatusDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_DeferredTaxDueDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_BondProducerAccNo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_ALDate, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_BondType, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_BondDispositionCode, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_BondDispositionCode2, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_InsuranceDisposition, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_FTZNo, class:Enterprise.Customs.US.Business.AddInfoJobDeclaration",
				"column:US_SPI, class:Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine",
				"column:US_ITNumber, class:Enterprise.Customs.US.Business.USITNumberAddInfo",
				"column:UZ_RelPrintInd, class:Enterprise.Customs.ZA.Business.AddInfoCusEntryHeader",
				"column:ZG_EidrType, class:Enterprise.Customs.GB.Business.Declaration.AddInfoJobDeclaration",
				"column:ZG_SuppDecDueDate, class:Enterprise.Customs.GB.Business.Declaration.AddInfoJobDeclaration",
				"column:ZG_NorthernIrelandMode, class:Enterprise.Customs.GB.Business.Declaration.AddInfoJobDeclaration",
				"column:ZG_ClaimEuSubsidy, class:Enterprise.Customs.GB.Business.Declaration.AddInfoJobDeclaration",
				"column:ZG_NiGoodsAtRiskOfMovingToROI, class:Enterprise.Customs.GB.Business.Declaration.AddInfoJobDeclaration",
				"column:ZG_DeferredSubmission, class:Enterprise.Customs.GB.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_GuarantorType, class:Enterprise.Customs.GB.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_OriginType, class:Enterprise.Customs.GB.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_TransportArrangement, class:Enterprise.Customs.GB.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_DeferredSubmission, class:Enterprise.Customs.DE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_GuarantorType, class:Enterprise.Customs.DE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_OriginType, class:Enterprise.Customs.DE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_TransportArrangement, class:Enterprise.Customs.DE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_DeferredSubmission, class:Enterprise.Customs.EU.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_GuarantorType, class:Enterprise.Customs.EU.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_OriginType, class:Enterprise.Customs.EU.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_TransportArrangement, class:Enterprise.Customs.EU.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_DeferredSubmission, class:Enterprise.Customs.IE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_GuarantorType, class:Enterprise.Customs.IE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_OriginType, class:Enterprise.Customs.IE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:ZG_TransportArrangement, class:Enterprise.Customs.IE.EMCS.Business.EMCSAddInfoJobDeclaration",
				"column:CA_CCNInfoNumber, class:Enterprise.Customs.CA.Business.CACargoControlNumberAddInfo",
				"column:CA_K84AccountingDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_PlaceOfReport, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_PortOfExit, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_LVSCloseDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_UnladingOffice, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_CarrierCode, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_ServiceOption, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_B2Type, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_OriginalTransactionNo, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_OGDStatus, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_EstimatedPaymentDueDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_IsOurFault, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_InitiatedBy, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_ChequeNo, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_ChequeDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_B2Total, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_DeclarationException, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_B2SubmissionDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_ConfirmedDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_B2AcceptedDate, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_AccountingAge, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:CA_CSAEntry, class:Enterprise.Customs.CA.Business.AddInfoJobDeclaration",
				"column:ZG_Parallel, class:Enterprise.Customs.IT.Business.Declaration.AddInfoCusEntryHeader",
			};

			var exceptionClass = new HashSet<string>()
			{
				"Enterprise.Customs.US.Business.USDispositionDataAddInfo"
			};

			var allCustomsAssemblies = BuildXml.Instance
					.GetAllAssembliesToBuild(false)
					.Where(c => c.StartsWith("Enterprise.Customs.", StringComparison.OrdinalIgnoreCase))
					.ToDictionary(c => System.IO.Path.GetFileNameWithoutExtension(c))
					.Keys
					.ToArray();
			var retriever = new SubClassRetriever(allCustomsAssemblies, typeof(BaseAddInfo))
			{
				IncludeAbstractClasses = true,
				IncludeAutoGeneratedCode = false,
				IncludeClientDlls = false,
				IncludeNestedClasses = true,
				IncludePrivateNestedClasses = true,
				IncludeTestClasses = false,
				IncludeNonAutoGeneratedCode = true,
			};
			var retrievedTypes = retriever.Retrieve().Where(c => !exceptionClass.Contains(c.FullName) && c.IsSubclassOfRawGeneric(typeof(BaseAddInfo)) && !c.IsAbstract).ToArray().GroupBy(c => c.Assembly).SelectMany(c => c);

			var exceptionMessages = new ZStringBuilder();
			foreach (var retrievedType in retrievedTypes)
			{
				var columnsForFastSearchProperty = retrievedType.GetProperty("ColumnsForFastSearch", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (columnsForFastSearchProperty != null)
				{
#pragma warning disable SYSLIB0050 // 'FormatterServices' is obsolete: 'Formatter-based serialization is obsolete and should not be used'
					var columnsForFastSearch = (SchemaColumn[])columnsForFastSearchProperty.GetMethod.Invoke(FormatterServices.GetUninitializedObject(retrievedType), null);
#pragma warning restore SYSLIB0050
					foreach (var columnForFastSearch in columnsForFastSearch)
					{
						var colName = $"column:{columnForFastSearch.Name}, class:{retrievedType.FullName}";
						if (!exceptionsList.Contains(colName))
						{
							exceptionMessages.AppendLine(colName);
						}
					}
				}
			}

			var errors = new ZStringBuilder();
			if (exceptionMessages.Length > 0)
			{
				errors.AppendLine("All new columns to be added to ColumnsForFastSearch should now be moved to AddInfoLeafTables. i.e. JobKRComInvoiceLine. Please check the following column(s):");
				errors.Append(exceptionMessages.ToStringWithNewLineBetweenAppends());
			}

			Assert(errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		[SnailTest]
		public void TestNoNewColumnsToColumnsForFastSearch_IDispositionCodeColumnsForFastSearchProvider()
		{
			var exceptionsList = new HashSet<string>()
			{
				"column:US_Code, class:Enterprise.Customs.US.AMS.Business.CusInBondBill",
				"column:US_Code, class:Enterprise.Customs.US.AMS.Business.CusInBondHeader",
				"column:US_Code, class:Enterprise.Customs.US.InBond.Business.CusInBondMoveDetail"
			};

			var usCustomsAssemblies = BuildXml.Instance
					.GetAllAssembliesToBuild(false)
					.Where(c => c.StartsWith("Enterprise.Customs.US.", StringComparison.OrdinalIgnoreCase))
					.ToDictionary(c => System.IO.Path.GetFileNameWithoutExtension(c))
					.Keys
					.ToArray();

			var retriever = new SubClassRetriever(usCustomsAssemblies, typeof(BusinessObject))
			{
				IncludeAbstractClasses = true,
				IncludeAutoGeneratedCode = false,
				IncludeClientDlls = false,
				IncludeNestedClasses = true,
				IncludePrivateNestedClasses = true,
				IncludeTestClasses = false,
				IncludeNonAutoGeneratedCode = true,
			};
			var retrievedTypes = retriever.Retrieve().Where(c => c.GetInterface("IDispositionCodeColumnsForFastSearchProvider") != null && !c.IsAbstract).ToArray().GroupBy(c => c.Assembly).SelectMany(c => c);

			var exceptionMessages = new ZStringBuilder();
			foreach (var retrievedType in retrievedTypes)
			{
#pragma warning disable SYSLIB0050 // 'FormatterServices' is obsolete: 'Formatter-based serialization is obsolete and should not be used'
				var columnsForFastSearch = (SchemaColumn[])retrievedType.InvokeMember("Enterprise.Customs.US.Business.IDispositionCodeColumnsForFastSearchProvider.GetColumnsForFastSearch", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, FormatterServices.GetUninitializedObject(retrievedType), null);
#pragma warning restore SYSLIB0050
				foreach (var columnForFastSearch in columnsForFastSearch)
				{
					var colName = $"column:{columnForFastSearch.Name}, class:{retrievedType.FullName}";
					if (!exceptionsList.Contains(colName))
					{
						exceptionMessages.AppendLine(colName);
					}
				}
			}

			var errors = new ZStringBuilder();
			if (exceptionMessages.Length > 0)
			{
				errors.AppendLine("All new columns to be added to ColumnsForFastSearch should now be moved to AddInfoLeafTables. i.e. JobKRComInvoiceLine. Please check GetColumnsForFastSearch in following class :");
				errors.Append(exceptionMessages.ToStringWithNewLineBetweenAppends());
			}

			Assert(errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		Type[] GetIAddInfoManagerTypes()
		{
			var allCustomsAssemblies = BuildXml.Instance
				.GetAllAssembliesToBuild(false)
				.Where(c => c.StartsWith("Enterprise.Customs.", StringComparison.OrdinalIgnoreCase))
				.ToDictionary(c => System.IO.Path.GetFileNameWithoutExtension(c))
				.Keys
				.ToArray();

			var retriever = new SubClassRetriever(allCustomsAssemblies, Type.GetType("Enterprise.Customs.Business.IAddInfoManager, Enterprise.Customs.Business"))
			{
				IncludeAbstractClasses = false,
				IncludeAutoGeneratedCode = true,
				IncludeClientDlls = false,
				IncludeNestedClasses = false,
				IncludePrivateNestedClasses = false,
				IncludeTestClasses = false,
				IncludeNonAutoGeneratedCode = true,
			};

			return retriever.Retrieve()
				.Where(x => !x.IsSubclassOfRawGeneric(typeof(CusAddInfo<>)))
				.Where(x => !typeof(IAddInfoSchemaProvider).IsAssignableFrom(x))
				.ToArray();
		}

		string GetInnerExceptionMessage(Exception exception)
		{
			return exception.InnerException == null ? exception.ToString() : GetInnerExceptionMessage(exception.InnerException);
		}
	}
}
