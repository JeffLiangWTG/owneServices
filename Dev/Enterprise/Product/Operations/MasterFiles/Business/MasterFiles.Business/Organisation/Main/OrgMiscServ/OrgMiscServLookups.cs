using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMiscServLookups : AutoOrgMiscServLookups
	{
		public OrgMiscServLookups(AutoOrgMiscServ parent)
			: base(parent)
		{
		}

		#region AutoratingDateFilteringList

		public CodeDescriptionPairList AutoratingDateFilteringList
		{
			get
			{
				if (autoratingDateFilteringList == null)
				{
					autoratingDateFilteringList = new CodeDescriptionPairList(OLookUpEditType.DateFilterType);
					autoratingDateFilteringList.Insert(0, new CodeDescriptionPair(RatingDateFilterTypes.Codes.Default, RatingDateFilterTypes.Descriptions.Default));
				}
				return autoratingDateFilteringList;
			}
		}
		CodeDescriptionPairList autoratingDateFilteringList;

		#endregion

		#region ABCAnalysisPeriodsList

		public WhsABCAnalysisPeriodCodeList ABCAnalysisPeriodsList
		{
			get { return Factory.GetCachedValue<WhsABCAnalysisPeriodCodeList>(); }
		}

		#endregion

		#region ABCAnalysisMethodsList

		public WhsABCAnalysisMethodCodeList ABCAnalysisMethodsList
		{
			get { return Factory.GetCachedValue<WhsABCAnalysisMethodCodeList>(); }
		}

		#endregion

		#region ProductAuditActions

		public CodeDescriptionPairList ProductAuditActions { get => Factory.GetCachedValue<ProductAuditActions>(); }

		#endregion

		#region AuthorityToLeaveOptions

		public CodeDescriptionPairList AuthorityToLeaveOptions
		{
			get { return Factory.GetCachedValue<AuthorityToLeaveOptions>(); }
		}

		#endregion

		#region ConsigneeDefaultOptionList

		public CodeDescriptionPairList ConsigneeDefaultOptionList => Factory.GetCachedValue<ConsigneeDefaultOptionList>();

		#endregion

		public CodeDescriptionPairList OM_EXMergeCustomsInvoiceLinesBy_List
		{
			get
			{
				var miscServ = Parent as OrgMiscServ;
				var countryCode = miscServ?.Header?.MainAddress?.OA_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue($"EXMergeCustomsInvoiceLinesBy_List_{countryCode}", () =>
				{
					return OrgCodeLists.GetMergeInvoiceLinesByListByCountryCode(countryCode);
				});
			}
		}

		public CodeDescriptionPairList InvoiceDetailReportSortList1
		{
			get
			{
				CodeDescriptionPairList result = InvoiceDetailReportSortList;
				InsertDefault(result, WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group1);
				return result;
			}
		}

		public CodeDescriptionPairList InvoiceDetailReportSortList2
		{
			get
			{
				CodeDescriptionPairList result = InvoiceDetailReportSortList;
				InsertDefault(result, WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group2);
				return result;
			}
		}

		public CodeDescriptionPairList InvoiceDetailReportSortList3
		{
			get
			{
				CodeDescriptionPairList result = InvoiceDetailReportSortList;
				InsertDefault(result, WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group3);
				return result;
			}
		}

		#region CartonGroups

		public IWhsCartonGroupCollection CartonGroups
		{
			get { return Factory.GetCachedValue("OrgMiscServLookups|CartonGroups", () => ObjectFactory.New<IWhsCartonGroupCollection>(Factory)); }
		}

		#endregion

		public override GlbGroupCollection OrgSecurityGroups
		{
			get
			{
				var collection = new GlbGroupCollectionWithOSMGDescription(Factory, new ZQuery(), new ZQuery());
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgMiscServSchema.Constants.OM_GG_OrgSecurityGroup, "Property0", ZBool.True));
				return collection;
			}
		}

		#region AsAgentOptions

		public CodeDescriptionPairList AsAgentOptions
		{
			get
			{
				if (asAgentOptions == null)
				{
					asAgentOptions = new CodeDescriptionPairList();
					asAgentOptions.AddPair(AsAgentOption.AsCarrier, ResString.GetMultilingualString("1334970C-F682-4DBF-B843-7C18ED500898", "AS CARRIER"));
					asAgentOptions.AddPair(AsAgentOption.AsAgent, ResString.GetMultilingualString("B4105EAE-3768-4ACF-8FEB-3A479FAE9D50", "AS AGENT"));
					asAgentOptions.AddPair(AsAgentOption.AsAgentForCarrier, ResString.GetMultilingualString("5DAA38B6-9D51-4006-AB00-EE9BEAFA73C3", "AS AGENT FOR CARRIER"));
					asAgentOptions.AddPair(AsAgentOption.AsAgentFor, ResString.GetMultilingualString("F271E271-DE1D-4690-83BB-1F0D3F6029EB", "AS AGENT FOR"));
					asAgentOptions.DefaultCode = AsAgentOption.AsCarrier;
				}

				return asAgentOptions;
			}
		}
		CodeDescriptionPairList asAgentOptions;

		public static class AsAgentOption
		{
			public const string AsCarrier = "CAR";
			public const string AsAgent = "AGT";
			public const string AsAgentForCarrier = "ATC";
			public const string AsAgentFor = "ATF";
		}

		#endregion

		#region ProductReceiveWeightOrDimsCheckTypeList

		public CodeDescriptionPairList ProductReceiveWeightOrDimsCheckTypeList
		{
			get { return Factory.GetCachedValue<ProductReceiveWeightOrDimsCheckTypeList>(); }
		}

		#endregion

		#region NumericCodeAirlines

		public NumericCodeRefAirlineCollection NumericCodeAirlines => new NumericCodeRefAirlineCollection(Factory);

		#endregion

		#region EnablePromptToCreateProductsList

		public EnablePromptToCreateProductsList EnablePromptToCreateProductsList
		{
			get { return Factory.GetCachedValue<EnablePromptToCreateProductsList>(); }
		}

		#endregion

		#region Implementation

		CodeDescriptionPairList InvoiceDetailReportSortList
		{
			get
			{
				CodeDescriptionPairList result = new InvoiceDetailReportSortList();
				CodeDescriptionPairList others = new JobChargeAttribTypeList();
				foreach (ICodeDescription item in others)
				{
					if (item.Code != JobChargeAttribTypeList.Codes.DocketLinePK)
					{
						result.Add(item);
					}
				}
				return result;
			}
		}

		void InsertDefault(CodeDescriptionPairList result, ZString groupCode)
		{
			string desc = Res.GetString("29452162-5d45-4b00-9307-aaf6f4bed549", "Default from Registry -") + " " + (groupCode.IsEmpty ? Res.GetString("5bf9c04f-999d-4070-8e4f-63776b02a84f", "Not used") : result.GetDescriptionFromCode(groupCode));
			result.Insert(0, new CodeDescriptionPair("DEF", desc));
		}

		#endregion
	}
}
