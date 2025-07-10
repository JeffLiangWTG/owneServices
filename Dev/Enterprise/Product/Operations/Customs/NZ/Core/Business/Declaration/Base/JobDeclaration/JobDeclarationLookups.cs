using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public CodeDescriptionPairList TSWEntryStatusList
		{
			get
			{
				if (declaration.IsECIWriteoff)
				{
					return Factory.GetCachedValue<LowValueConsignmentStatusList>();
				}
				else
				{
					return Factory.GetCachedValue<TSWEntryStatusList>();
				}
			}
		}

		public CodeDescriptionPairList StatusList
		{
			get
			{
				if (declaration.IsECIWriteoff)
				{
					return Factory.GetCachedValue<LowValueConsignmentStatusList>();
				}
				else
				{
					return Factory.GetCachedValue<StatusList>();
				}
			}
		}

		public override CodeDescriptionPairList EntryStatusList
		{
			get
			{
				if (declaration.IsECIWriteoff)
				{
					return Factory.GetCachedValue<LowValueConsignmentStatusList>();
				}
				else
				{
					return Factory.GetCachedValue("EntryStatusList for" + declaration.JE_ApplicationCode, delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(Factory.GetCachedValue<FormalEntryStatusList>());

						var consolidationStatusList = Factory.GetCachedValue<ConsolidatedEntryStatusList>();
						foreach (CodeDescriptionPair consolidationStatus in consolidationStatusList)
						{
							result.AddPairIfNotExist(consolidationStatus.Code, consolidationStatus.Description);
						}

						result.Sort();
						return result;
					});
				}
			}
		}

		public override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				var declaration = this.declaration;
				switch (declaration.JE_MessageType)
				{
					case JobMessageTypeList.Codes.Import:
						return Factory.GetCachedValue<CodeDescriptionPairList>("MessageSubList for" + declaration.JE_ApplicationCode, delegate
						{
							return new JobMessageSubTypeForImportList(declaration.IsTSWDeclaration);
						});
					case JobMessageTypeList.Codes.Export:
						return Factory.GetCachedValue<JobMessageSubTypeForExportList>();
					case JobMessageTypeList.Codes.Excise:
						return Factory.GetCachedValue<JobMessageSubTypeForExciseList>();
					default:
						return Factory.GetCachedValue<JobMessageSubTypeList>();
				}
			}
		}

		public CodeDescriptionPairList OriginalEntryTypeList
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.Declaration.OriginalEntryTypeList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(JobMessageSubTypeList.Codes.Sight, JobMessageSubTypeList.Descriptions.Sight);
						result.AddPair(JobMessageSubTypeList.Codes.Temporary, JobMessageSubTypeList.Descriptions.Temporary);
						return result;
					});
			}
		}

		public CodeDescriptionPairList ContainerModeList
		{
			get { return Factory.GetCachedValue<ContainerModeList>(); }
		}

		public CodeDescriptionPairList ProcessingPortList
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.Declaration.ProcessingPortList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair("NZAKL", "Auckland");
						result.AddPair("NZCHC", "Christchurch/Lyttelton");
						result.AddPair("NZDUD", "Dunedin/Port Chalmers");
						result.AddPair("NZIVC", "Invercargill");
						result.AddPair("NZNSN", "Nelson");
						result.AddPair("NZNPL", "New Plymouth/Port Taranaki");
						result.AddPair("NZTRG", "Tauranga");
						result.AddPair("NZNPE", "Napier");
						result.AddPair("NZWLG", "Wellington");
						return result;
					});
			}
		}

		public override CodeDescriptionPairList PaymentPartyList
		{
			get
			{
				if (declaration.IsTSWCREWriteOff || declaration.IsTSWICRWriteOff)
				{
					return Factory.GetCachedValue<FreightPaymentMethodList>();
				}
				else
				{
					return Factory.GetCachedValue<PaymentMethodList>();
				}
			}
		}

		protected override ZQuery OriginPortFilter()
		{
			ZQuery result = new ZQuery();
			if (declaration.IsImport)
			{
				result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
			else if (declaration.IsExport && !declaration.IsECIWriteoff)
			{
				result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
			return result;
		}

		public override CodeDescriptionPairList JE_TotalNoOfPacksPackType_List
		{
			get { return UniversalReferenceHelper.GetUNEPackageTypeList(Factory); }
		}

		public CodeDescriptionPairList JE_SoldOrConsigned_List
		{
			get { return Factory.GetCachedValue<TermsOfSaleList>(); }
		}

		public override CodeDescriptionPairList IncoTermList
		{
			get { return Factory.GetCachedValue<IncoTermList>(); }
		}

		public CodeDescriptionPairList ContainerSizeList
		{
			get { return Factory.GetCachedValue<ContainerSizeList>(); }
		}

		public override CodeDescriptionPairList TransportTypeList
		{
			get
			{
				if (declaration.IsECIWriteoff)
				{
					return Factory.GetCachedValue(".NZ.Business.EciWriteOffDeclaration.TransportTypeList",
						() =>
						{
							var result = new CodeDescriptionPairList();
							result.AddPair(JobTransportModeList.Codes.Air, JobTransportModeList.Descriptions.Air);
							result.AddPair(JobTransportModeList.Codes.Sea, JobTransportModeList.Descriptions.Sea);
							return result;
						});
				}
				else
				{
					return Factory.GetCachedValue<JobTransportModeList>();
				}
			}
		}

		protected internal CustomsAcceptableCurrencyList AcceptableNZCustomCurrencyList
		{
			get { return Factory.GetCachedValue<CustomsAcceptableCurrencyList>(); }
		}

		public CodeDescriptionPairList YesNoList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}

		public CodeDescriptionPairList LocationList
		{
			get
			{
				var isSea = declaration.IsSea;
				var isImport = declaration.IsImport;
				var isExport = declaration.IsExport;
				if (isSea && isImport)
				{
					return Factory.GetCachedValue<GoodsLocatedAtListForSeaImport>();
				}
				else if (isSea && isExport)
				{
					return Factory.GetCachedValue<GoodsLocatedAtListForSeaExport>();
				}
				else
				{
					return Factory.GetCachedValue<GoodsLocatedAtList>();
				}
			}
		}

		public CodeDescriptionPairList MsgTransModeList
		{
			get { return Factory.GetCachedValue<MsgTransportList>(); }
		}

		public MessagingStatusList MessagingStatuses
		{
			get { return Factory.GetCachedValue<MessagingStatusList>(); }
		}

		public override CodeDescriptionPairList EFTModeList
		{
			get { return Factory.GetCachedValue<LowValueConsignmentStatusList>(); }
		}

		#region NatureOfTransaction

		public virtual CodeDescriptionPairList TransactionNatureList
		{
			get { return Factory.GetCachedValue<NatureOfTransactionList>(); }
		}

		#endregion

		public override CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				var submissionType = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.Value.SubmissionType;
				var cacheKey = "NZ.JobDeclarationLookups.ApplicationCodeList-" + submissionType;

				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new JobApplicationCodeList();
					if (!submissionType.IsEmpty && submissionType != DeclarationApplicationCodeList.Codes.Builtin)
					{
						result.RemoveCode(JobApplicationCodeList.Codes.CUS);
						result.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
						result.Sort();
					}
					return result;
				});
			}
		}

		public OrgHeaderCollection NotifyPartyOrganisations => fNotifyPartyOrganisations ?? (fNotifyPartyOrganisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection fNotifyPartyOrganisations;

		public OrgHeaderCollection DeliveryDestinationPartyOrganisations => fDeliveryDestinationPartyOrganisations ?? (fDeliveryDestinationPartyOrganisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection fDeliveryDestinationPartyOrganisations;

		public OrgHeaderCollection DeliveryNotificationCCPATFOrganisations => deliveryNotificationCCPATFOrganisations ?? (deliveryNotificationCCPATFOrganisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection deliveryNotificationCCPATFOrganisations;

		public CodeDescriptionPairList MPIPaymentMethods
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.EciWriteOffDeclaration.MPIPaymentMethods",
						() =>
						{
							var result = new CodeDescriptionPairList();
							result.AddPair(MAFPaymentMethodList.Codes.Account, MAFPaymentMethodList.Descriptions.Account);
							result.AddPair(MAFPaymentMethodList.Codes.Cash, MAFPaymentMethodList.Descriptions.Cash);
							return result;
						});
			}
		}

		public RefUNLOCOCollection BarrierPortList
		{
			get
			{
				return declaration.IsImport ? base.PortOfArrivals : base.PortOfLoadings;
			}
		}

		public RefUNLOCOCollection DeliveryNotifyPortList
		{
			get
			{
				return Factory.GetCachedValue("NZ.JobDeclarationLookups.DeliveryNotifyPortList", () =>
				{
					var filter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.NewZealand);
					return new RefUNLOCOCollection(declaration.Factory, filter);
				});
			}
		}

		public CodeDescriptionPairList GoodsLocationList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				switch (declaration.JE_GoodsLocatedAt)
				{
					case GoodsLocatedAtListForSeaImport.Codes.DES:
						{
							result = LocationFinalDestination(result);
							break;
						}

					case GoodsLocatedAtListForSeaImport.Codes.DIS:
						{
							result = LocationPortOfDischarge(result);
							break;
						}

					case GoodsLocatedAtListForSeaExport.Codes.PC:
						{
							result = LocationPortOfLoading(result);
							break;
						}

					case GoodsLocatedAtList.Codes.BW:
					case GoodsLocatedAtList.Codes.C:
					case GoodsLocatedAtList.Codes.CTO:
					case GoodsLocatedAtList.Codes.CY:
					case GoodsLocatedAtList.Codes.D:
					case GoodsLocatedAtList.Codes.FW:
						{
							var organisation = declaration.GetGoodsLocationOrg();
							if (organisation != null)
							{
								result.AddRange(GetCachedCodes(organisation, OrgCusCode.CodeTypes.ControlledPremisesID));
								result.AddRange(GetCachedCodes(organisation, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility));
								result.AddRange(GetCachedCodes(organisation, OrgCusCode.CodeTypes.CustomsClientCode));
							}
							break;
						}
				}

				return result;
			}
		}

		CodeDescriptionPairList LocationFinalDestination(CodeDescriptionPairList result)
		{
			var finalDestination = declaration.JE_RL_NKFinalDestination;
			if (!finalDestination.IsEmpty)
			{
				result.AddPair(finalDestination, finalDestination);
			}

			return result;
		}

		CodeDescriptionPairList LocationPortOfDischarge(CodeDescriptionPairList result)
		{
			var portOfDischarge = declaration.JE_RL_NKPortOfArrival;
			if (!portOfDischarge.IsEmpty)
			{
				result.AddPair(portOfDischarge, portOfDischarge);
			}

			return result;
		}

		CodeDescriptionPairList LocationPortOfLoading(CodeDescriptionPairList result)
		{
			var portOfLoading = declaration.JE_RL_NKPortOfLoading;
			if (!portOfLoading.IsEmpty)
			{
				result.AddPair(portOfLoading, portOfLoading);
			}

			return result;
		}

		CodeDescriptionPairList GetCachedCodes(OrgHeader organisation, string codeType)
		{
			return organisation != null
				? Factory.GetCachedValue(string.Concat(nameof(GetCachedCodes), codeType, organisation.PK), () =>
				{
					var result = new CodeDescriptionPairList();

					var cusCodes = organisation.CustomsCodes
					.GetOrgCusCodesForCodeAndCountry(codeType, Core.Constants.CountryCodes.NewZealand)
					.OrderBy(c => c.OK_OA_PremisesAddress);

					foreach (var cusCode in cusCodes)
					{
						var humanReadableName = !cusCode.OK_OA_PremisesAddress.IsEmpty && cusCode.OK_OA_PremisesAddress.IsValid
							? Factory.Load<OrgAddress>(cusCode.OK_OA_PremisesAddress)?.HumanReadableName ?? ZString.Empty
							: organisation.HumanReadableName;

						result.AddPair(cusCode.OK_CustomsRegNo, string.Concat(codeType, separator, humanReadableName));
					}

					return result;
				}, CacheStalenessPolicy.StaleOnFactorySave)
				: new CodeDescriptionPairList();
		}

		const string separator = " - ";
	}
}
