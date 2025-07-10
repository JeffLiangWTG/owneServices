using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefShippingLineFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagsFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", RefShippingLineSchema.RSL_CarrierName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|Name", "Name");
			filters.AddTextFilter("SCAC", RefShippingLineSchema.RSL_StandardCarrierAlphaCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|SCAC", "SCAC");
			filters.AddTextFilter("C1C", RefShippingLineSchema.RSL_CargoWiseOneCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|C1C", "C1C");

			var scacOrC1CFilter = filters.AddTextFilter("SCAC or C1C", GetSCACorC1CQuery);
			scacOrC1CFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|SCACorC1C", "SCAC or C1C");
			scacOrC1CFilter.MaxLength = Math.Max(RefShippingLineSchema.RSL_StandardCarrierAlphaCode.MaxLength, RefShippingLineSchema.RSL_CargoWiseOneCode.MaxLength);
		}

		internal static ZQuery GetSCACorC1CQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, comparisonOperator, value.SubstringSafe(0, RefShippingLineSchema.RSL_StandardCarrierAlphaCode.MaxLength));
			query.AddToFilter(JoinCondition.Or, RefShippingLineSchema.RSL_CargoWiseOneCode, comparisonOperator, value.SubstringSafe(0, RefShippingLineSchema.RSL_CargoWiseOneCode.MaxLength));

			return query;
		}

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter("Integrations Enabled", GetFlagsNames(), GetFlagColumns()).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|IntegrationsEnabled", "Integrations Enabled");

			var nvoStatusFilter = filters.AddTextFilter("NVO Status", GetIsNVOQuery, GetIsNVOList);
			nvoStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|NVOStatus", "Non Vessel Operator Status");
			nvoStatusFilter.DefaultProperty = RefShippLineFilterCode.AllStatusForNVO;
			nvoStatusFilter.Category = FilterCategories.StatusAndFlags;

			var messagingRequirementsFilter = filters.AddTextFilter("Messaging Requirements", GetMessagingRequirementsQuery, MessagingRequirementList);
			messagingRequirementsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefShippingLineFilter|MessagingRequirements", "Messaging Requirements");
			messagingRequirementsFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetIsNVOQuery(ZString value)
		{
			var query = new ZQuery();

			if (value == RefShippLineFilterCode.NVO)
			{
				query.AddToFilter(RefShippingLineSchema.RSL_IsNVO, ZBool.True);
			}
			else if (value == RefShippLineFilterCode.NotNVO)
			{
				query.AddToFilter(RefShippingLineSchema.RSL_IsShippingLine, ZBool.True);
			}

			return query;
		}

		IList GetIsNVOList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(RefShippLineFilterCode.NVO, Res.GetString("07997EDB-78B4-41D9-8A35-CF35A4C59578", "Non Vessel Operator"));
			result.AddPair(RefShippLineFilterCode.NotNVO, Res.GetString("7574050D-46FD-4599-8450-80546ED6F2C7", "Not Non Vessel Operator"));
			result.AddPair(RefShippLineFilterCode.AllStatusForNVO, Res.GetString("13ED6367-BDDB-4AD1-A81A-55C2E6A21422", "Show all records"));

			return result;
		}

		internal static string[] GetFlagsNames()
		{
			return new[]
			{
				Res.GetString("3CC3DBB8-0D83-4AA1-B704-33F64F2448F0", "Ocean Carrier Messaging"),
				Res.GetString("81B57AA9-D493-4795-8719-4D860C06C5EB", "Booking Request"),
				Res.GetString("80E4480B-70F7-4CDB-A775-1A6A611B8D0C", "Shipping Instruction"),
				Res.GetString("7FA5AC13-4BDD-458D-BEAA-A9CE532025BE", "Verified Gross Container Weight"),
				Res.GetString("EDF33D87-F4B9-484E-BBDD-1FED3BCECD3F", "Shipping Order (China)"),
				Res.GetString("F6258E2A-6102-4FE2-8FF5-4BCA11317230", "eManifest (China)"),
				Res.GetString("CA55A373-7ED5-4293-8520-0431E1F97E1F", "Global Sailing Schedule"),
				Res.GetString("7DBD18D4-D356-43F8-BD1B-AE29EFE2C8D5", "Container Automation"),
				Res.GetString("45AF4B26-5C6E-48D0-A845-1CF9C1B46356", "Cargo Sphere Rates"),
				Res.GetString("725CCE98-09B6-4745-90E8-146C94B6D6CE", "Invoice")
			};
		}

		internal static SchemaBoolColumn[] GetFlagColumns()
		{
			return new[]
			{
				RefShippingLineSchema.RSL_OceanCarrierMessagingAvailable,
				RefShippingLineSchema.RSL_BookingRequestAvailable,
				RefShippingLineSchema.RSL_ShippingInstructionAvailable,
				RefShippingLineSchema.RSL_VerifiedGrossContainerWeightAvailable,
				RefShippingLineSchema.RSL_ShippingOrderAvailable,
				RefShippingLineSchema.RSL_EManifestAvailable,
				RefShippingLineSchema.RSL_GlobalSailingScheduleAvailable,
				RefShippingLineSchema.RSL_ContainerAutomationAvailable,
				RefShippingLineSchema.RSL_CargoSphereRatesAvailable,
				RefShippingLineSchema.RSL_InvoiceAvailable
			};
		}

		#region Messaging Requirements

		ZQuery GetMessagingRequirementsQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(RefShippingLine));

			var refShippingLineMessagingRequirementQuery = new ZDBOnlySubQuery(typeof(RefShippingLineMessagingRequirement), RefShippingLineMessagingRequirementSchema.RSR_RSL_ShippingLine);
			refShippingLineMessagingRequirementQuery.AddToFilter(RefShippingLineMessagingRequirementSchema.RSR_RST_NKType, value);

			result.AddSubQuery(refShippingLineMessagingRequirementQuery, JoinCondition.And);

			return result;
		}

		public CodeDescriptionPairList MessagingRequirementList
		{
			get
			{
				return Factory.GetCachedValue("RefShippingLineFilterBusinessObject.MessagingRequirementList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.ContractNumberMandatory, Core.Constants.ShippingLineMessagingRequirement.Description.ContractNumberMandatory);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.NamedAccountMandatory, Core.Constants.ShippingLineMessagingRequirement.Description.NamedAccountMandatory);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.DGNetWeightMandatory, Core.Constants.ShippingLineMessagingRequirement.Description.DGNetWeightMandatory);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.AcceptEitherAirflowOrHumidity, Core.Constants.ShippingLineMessagingRequirement.Description.AcceptEitherAirflowOrHumidity);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.DimensionsMandatoryForOOG, Core.Constants.ShippingLineMessagingRequirement.Description.DimensionsMandatoryForOOG);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.HarmonisedCode, Core.Constants.ShippingLineMessagingRequirement.Description.HarmonisedCode);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.BillOfLadingProvider, Core.Constants.ShippingLineMessagingRequirement.Description.BillOfLadingProvider);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.AttachFormAsPDFInMessage, Core.Constants.ShippingLineMessagingRequirement.Description.AttachFormAsPDFInMessage);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.SealNumberMandatory, Core.Constants.ShippingLineMessagingRequirement.Description.SealNumberMandatory);
					result.AddPair(Core.Constants.ShippingLineMessagingRequirement.Code.IntegrationViaEmailToCarrierLocalOffice, Core.Constants.ShippingLineMessagingRequirement.Description.IntegrationViaEmailToCarrierLocalOffice);

					return result;
				});
			}
		}

		#endregion
	}
}
