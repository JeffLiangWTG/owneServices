using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => Parent;

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeList>();

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				var transportMode = Parent.JE_TransportMode;
				return Factory.GetCachedValue("TWDeclarationCargoIdTypeList" + transportMode, () =>
				{
					var result = new ContainerModeList();
					switch (transportMode)
					{
						case TW.Business.TransportTypeList.Codes.Sea:
							result.RemoveCode(ContainerModeList.Codes.Loose);
							break;
						case TW.Business.TransportTypeList.Codes.Air:
							result.RemoveCode(ContainerModeList.Codes.Containerized);
							result.RemoveCode(ContainerModeList.Codes.Bulk);
							result.RemoveCode(ContainerModeList.Codes.BreakBulk);
							break;
						default:
							break;
					}
					return result;
				});
			}
		}

		public override ICodeDescriptionPairList CustomsOfficeList => TWRefCusCodeListTypes.GetCustomsOfficeList(Factory, Declaration);

		public override IBusinessObjectCollection LocationOfGoodsCollection
		{
			get
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.Taiwan,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
					ZDateTime.Today);

				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", new ZString(RefCusCodeListAttributeTypes.Codes.CustomsOffice)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", Declaration.JE_CustomsOffice));

				return collection;
			}
		}

		public TWGlbStaffCollection CusAgentsTWList => new TWGlbStaffCollection(Factory);

		protected override CodeDescriptionPairList PackingUnitTypesListCore => JE_TotalNoOfPacksPackType_List;

		public override CodeDescriptionPairList JE_TotalNoOfPacksPackType_List => TWRefCusCodeListTypes.GetCommercialPackUnitsList(Factory);

		public CodeDescriptionPairList CustomsProfileList => Parent.CusAgent?.GetCustomsProfile() ?? new CodeDescriptionPairList();

		public CodeDescriptionPairList PaymentMethodsList
		{
			get
			{
				if (Parent.IsImport)
				{
					return Factory.GetCachedValue<IMPPaymentMethod>();
				}
				else
				{
					return Factory.GetCachedValue<EXPPaymentMethod>();
				}
			}
		}

		public CodeDescriptionPairList CaseNoList
		{
			get
			{
				var declarantOrgHeaderPK = Parent.DeclarantAddress?.OA_OH ?? ZGuid.Empty;
				var importerOrgHeaderPK = Parent.JE_OH_Importer;
				string cacheKey = string.Format(System.Globalization.CultureInfo.InvariantCulture, "CaseNoList{0}{1}{2}{3}", Core.Constants.CountryCodes.Taiwan, OrgCusCode.TaiwanCodeTypes.PBR, declarantOrgHeaderPK, importerOrgHeaderPK);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();

					if (!declarantOrgHeaderPK.IsEmpty || !importerOrgHeaderPK.IsEmpty)
					{
						var zQuery = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Taiwan);
						zQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.TaiwanCodeTypes.PBR);

						var nestedQuery = new ZQuery();
						if (!declarantOrgHeaderPK.IsEmpty)
						{
							nestedQuery.AddToFilter(OrgCusCodeSchema.OK_OH, declarantOrgHeaderPK);
						}
						if (!importerOrgHeaderPK.IsEmpty)
						{
							nestedQuery.AddToFilter(nestedQuery.IsEmpty ? nestedQuery.DefaultJoinCondition : JoinCondition.Or, OrgCusCodeSchema.OK_OH, Parent.JE_OH_Importer);
						}

						zQuery.AddToFilter(nestedQuery);
						result.AddRange(Factory.Load<OrgCusCode>(zQuery));
					}
					return result;
				});
			}
		}

		protected override CodeDescriptionPairList EntryStatusListForDefaultFallBack => Factory.GetCachedValue("Enterprise.Customs.TW.Business.EntryStatusCodeList", () =>
		{
			var list = new UntranslatableCodeDescriptionPairList((NoResString)"Entry Status Code List");
			list.AddRange(new EntryStatusCodeList());
			return list;
		});

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<JobDeclarationMessageStatusList>();

		public override CodeDescriptionPairList MergeByList => Factory.GetCachedValue<MergeByCodeList>();

		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<EntryStatusCodeList>();

		public CodeDescriptionPairList ClearanceStatusList => Factory.GetCachedValue<ClearanceStatusCodeList>();

		#region PortFilter
		protected override ZQuery OriginPortFilter()
		{
			ZQuery result;
			if (ValidationHelper.IsOriginWithForeignPortIsRequired(Declaration))
			{
				result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.Taiwan);
			}
			else if (ValidationHelper.IsOriginWithTWPortCodeIsRequired(Declaration))
			{
				result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Taiwan);
			}
			else if (ValidationHelper.IsExportWarehouse(Declaration) || ValidationHelper.IsOriginWithAllPortCodeIsRequired(Declaration))
			{
				result = PortQuery(string.Empty, PortLocation.All);
			}
			else
			{
				result = base.OriginPortFilter();
			}
			return result;
		}

		protected override ZQuery FinalDestinationPortFilter()
		{
			ZQuery result;
			if (ValidationHelper.IsFinalDestinationWithForeignPortIsRequired(Declaration))
			{
				result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.Taiwan);
			}
			else if (ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(Declaration))
			{
				result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Taiwan);
			}
			else if (ValidationHelper.IsImportWarehouse2(Declaration) || ValidationHelper.IsFinalDestinationWithAllPortCodeIsRequired(Declaration))
			{
				result = PortQuery(string.Empty, PortLocation.All);
			}
			else
			{
				var baseFilter = base.FinalDestinationPortFilter();
				result = ValidationHelper.IsImportTransportMode(Declaration) ? baseFilter.AddToFilter(PortQuery(Declaration.JE_TransportMode, PortLocation.Local), JoinCondition.And) : baseFilter;
			}
			return result;
		}

		protected override ZQuery LoadingPortFilter() => PortQuery(string.Empty, PortLocation.All);

		protected override ZQuery DischargePortFilter() => PortQuery(string.Empty, PortLocation.All);

		#endregion

		public OrganisationsFindBoxCollection DeclarantOrganisationsFindBoxCollection => Factory.GetCachedValue("Enterprise.Customs.TW.Business.JobDeclarationLookups.DeclarantOrganisationsFindBoxCollection", () => new OrganisationsFindBoxCollection(Parent.Factory));

		public new TWRefVesselCollection Vessels => new TWRefVesselCollection(Factory);

		public CodeDescriptionPairList TWTransportCodeList => Factory.GetCachedValue<TransportCodeList>();

		public CodeDescriptionPairList DeclDocTypeList
		{
			get
			{
				var messageType = Parent?.JE_MessageType ?? ZString.Empty;
				return Factory.GetCachedValue(ZString.Format((NoResString)"Enterprise.Customs.TW.Business.DeclDocTypeList_{0}", messageType), () =>
				{
					CodeDescriptionPairList result = null;
					if (messageType == Customs.Business.JobMessageTypeList.Codes.Export)
					{
						result = Factory.GetCachedValue<ExportDeclDocTypeList>();
					}
					else if (messageType == Customs.Business.JobMessageTypeList.Codes.Import)
					{
						result = Factory.GetCachedValue<ImportDeclDocTypeList>();
					}
					else
					{
						result = new CodeDescriptionPairList();
					}
					return result;
				});
			}
		}
	}
}
