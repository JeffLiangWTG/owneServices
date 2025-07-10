using System;
using System.ComponentModel;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class OrgTypeModuleFilter : ModuleFlagsFilter
	{
		#region Construction

		public OrgTypeModuleFilter(ZString description)
			: base(description, dummyFlags, new GetFlagsQuery[] { DummyQuery })
		{
		}

		protected override bool ShouldCheckMaximumFlags
		{
			get { return false; }
		}

		protected override bool ShouldCheckFlagAmountEqualsDelegateAmount
		{
			get { return false; }
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("AndJoinCondition", AndJoinCondition.ToString());
			writer.WriteElementString("OrJoinCondition", OrJoinCondition.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "AndJoinCondition")
			{
				ZBool valueAnd = new ZBool(reader.ReadElementString("AndJoinCondition"));
				AndJoinCondition = valueAnd;
			}

			if (reader.Name == "OrJoinCondition")
			{
				ZBool valueOr = new ZBool(reader.ReadElementString("OrJoinCondition"));
				OrJoinCondition = valueOr;
			}
		}

		#endregion

		#region Property10

		public ZBool Property10
		{
			get { return property10; }
			set
			{
				if (property10 != value)
				{
					InvalidateCachedQuery();
					SetNonPersistentPropertyValue(Property10Info, ref property10, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty10();
					}
				}
			}
		}

		public ZPropertyInfo Property10Info
		{
			get { return GetZPropertyInfo(nameof(Property10)); }
		}

		ZBool property10;

		#endregion

		#region Property11

		public ZBool Property11
		{
			get { return property11; }
			set
			{
				if (property11 != value)
				{
					InvalidateCachedQuery();
					SetNonPersistentPropertyValue(Property11Info, ref property11, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty11();
					}
				}
			}
		}

		public ZPropertyInfo Property11Info
		{
			get { return GetZPropertyInfo(nameof(Property11)); }
		}

		ZBool property11;

		#endregion

		#region Property12

		public ZBool Property12
		{
			get { return property12; }
			set
			{
				if (property12 != value)
				{
					InvalidateCachedQuery();
					SetNonPersistentPropertyValue(Property12Info, ref property12, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty12();
					}
				}
			}
		}

		public ZPropertyInfo Property12Info
		{
			get { return GetZPropertyInfo(nameof(Property12)); }
		}

		ZBool property12;

		#endregion

		#region Property13

		public ZBool Property13
		{
			get { return property13; }
			set
			{
				if (property13 != value)
				{
					InvalidateCachedQuery();
					SetNonPersistentPropertyValue(Property13Info, ref property13, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty13();
					}
				}
			}
		}

		public ZPropertyInfo Property13Info
		{
			get { return GetZPropertyInfo(nameof(Property13)); }
		}

		ZBool property13;

		#endregion

		#region Validation

		public new ModuleOrgTypeFilterValidation Validation
		{
			get { return (ModuleOrgTypeFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleOrgTypeFilterValidation(this);
		}

		#endregion

		#region Dummies

		static ZQuery DummyQuery(ZBool value)
		{
			return new ZQuery();
		}

		static readonly string[] dummyFlags = { "test0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13" };

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			return GetOrgTypeFilter();
		}

		protected ZQuery GetOrgTypeFilter()
		{
			ZBool oB_IsDebtor = Property0;
			ZBool oB_IsCreditor = Property1;
			ZBool oH_IsConsignee = Property2;
			ZBool oH_IsConsignor = Property3;
			ZBool oH_IsShippingProvider = Property4;
			ZBool oH_IsForwarder = Property5;
			ZBool oH_IsTransportClient = Property6;
			ZBool oH_IsWarehouseClient = Property7;
			ZBool oH_IsBroker = Property8;
			ZBool oH_IsMiscFreightServices = Property9;

			ZBool oH_IsCompetitor = Property10;
			ZBool oH_IsSalesLead = Property11;

			ZBool oH_IsControllingAgent = Property12;
			ZBool oH_IsControllingCustomer = Property13;

			ZQuery query = new ZQuery();

			JoinCondition orgTypeJoinCondition = (OrJoinCondition) ? JoinCondition.Or : JoinCondition.And;

			if (oH_IsCompetitor)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsCompetitor, SQLComparisonOperator.Equal, oH_IsCompetitor);
			}

			if (oH_IsSalesLead)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsSalesLead, SQLComparisonOperator.Equal, oH_IsSalesLead);
			}

			if (oH_IsMiscFreightServices)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsMiscFreightServices, SQLComparisonOperator.Equal, oH_IsMiscFreightServices);
			}

			if (oH_IsBroker)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsBroker, SQLComparisonOperator.Equal, oH_IsBroker);
			}

			if (oH_IsWarehouseClient)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsWarehouseClient, SQLComparisonOperator.Equal, oH_IsWarehouseClient);
			}

			if (oH_IsTransportClient)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsTransportClient, SQLComparisonOperator.Equal, oH_IsTransportClient);
			}

			if (oH_IsForwarder)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, oH_IsForwarder);
			}

			if (oH_IsShippingProvider)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsShippingProvider, SQLComparisonOperator.Equal, oH_IsShippingProvider);
			}

			if (oH_IsConsignor)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsConsignor, SQLComparisonOperator.Equal, oH_IsConsignor);
			}

			if (oH_IsConsignee)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsConsignee, SQLComparisonOperator.Equal, oH_IsConsignee);
			}

			if (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value && oH_IsControllingAgent)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsControllingAgent, SQLComparisonOperator.Equal, oH_IsControllingAgent);
			}

			if (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value && oH_IsControllingCustomer)
			{
				query.AddToFilter(orgTypeJoinCondition, OrgHeaderSchema.OH_IsControllingCustomer, SQLComparisonOperator.Equal, oH_IsControllingCustomer);
			}

			if (oB_IsCreditor || oB_IsDebtor)
			{
				ZDBOnlySubQuery debtorCreditorSubquery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

				if (oB_IsCreditor)
				{
					debtorCreditorSubquery.AddToFilter(orgTypeJoinCondition, OrgCompanyDataSchema.OB_IsCreditor, SQLComparisonOperator.Equal, oB_IsCreditor);
				}

				if (oB_IsDebtor)
				{
					debtorCreditorSubquery.AddToFilter(orgTypeJoinCondition, OrgCompanyDataSchema.OB_IsDebtor, SQLComparisonOperator.Equal, oB_IsDebtor);
				}

				debtorCreditorSubquery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				ZDBOnlyQuery debCredDBOnlyqueryAddition = new ZDBOnlyQuery(typeof(OrgHeader));
				debCredDBOnlyqueryAddition.AddSubQuery(debtorCreditorSubquery, JoinCondition.And);
				query.AddToFilter(debCredDBOnlyqueryAddition, orgTypeJoinCondition);
			}

			return query;
		}

		#endregion

		#region ReadOnly calculation

		public OrgModuleType ModuleType
		{
			get { return moduleType; }
			set
			{
				if (moduleType != value)
				{
					moduleType = value;
					SetFlagsByModuleId(value);
				}
			}
		}

		OrgModuleType moduleType;

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			bool result = property.IsReadOnly;

			if (ModuleType == OrgModuleType.CompetitorIntelligence || ModuleType == OrgModuleType.ClientIntelligence)
			{
				bool orgShowARTab = Env.Registry.OrgShowARTab;
				bool orgShowConsigneeConsignorTab = Env.Registry.OrgShowConsigneeConsignorTab;

				switch (property.Name)
				{
					case "Property1":
					case "Property4":
					case "Property5":
					case "Property6":
					case "Property7":
					case "Property8":
					case "Property9":
					case "Property10":
					case "Property11":
					case "Property12":
					case "Property13":
						result = true;
						break;

					case "Property0":
						result = (ModuleType == OrgModuleType.CompetitorIntelligence || !orgShowARTab);
						break;

					case "Property2":
					case "Property3":
						result = (ModuleType == OrgModuleType.CompetitorIntelligence || !orgShowConsigneeConsignorTab);
						break;

					case "AndJoinCondition":
					case "OrJoinCondition":
						result = (ModuleType == OrgModuleType.CompetitorIntelligence || (!orgShowConsigneeConsignorTab && !orgShowARTab));
						break;
				}
			}

			return result;
		}

		void SetFlagsByModuleId(OrgModuleType orgModuleType)
		{
			bool orgShowARTab = Env.Registry.OrgShowARTab;
			bool orgShowConsigneeConsignorTab = Env.Registry.OrgShowConsigneeConsignorTab;

			if (orgModuleType == OrgModuleType.CompetitorIntelligence)
			{
				Property10 = true;
			}
			else if (orgModuleType == OrgModuleType.ClientIntelligence)
			{
				Property11 = true;

				if (!orgShowARTab)
				{
					Property0 = false;
				}

				if (!orgShowConsigneeConsignorTab)
				{
					Property2 = false;
					Property3 = false;
				}
			}

			RefreshBinding();
		}

		#endregion

	}

	#region class Validation

	public class ModuleOrgTypeFilterValidation : ModuleFlagsFilterValidation
	{
		public ModuleOrgTypeFilterValidation(OrgTypeModuleFilter parent)
			: base(parent)
		{
		}

		#region Validate Properties

		public void ValidateProperty10()
		{
			// run user delegate here
		}

		public void ValidateProperty11()
		{
			// run user delegate here
		}

		public void ValidateProperty12()
		{
			// run user delegate here
		}

		public void ValidateProperty13()
		{
			// run user delegate here
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProperty10();
			ValidateProperty11();
			ValidateProperty12();
			ValidateProperty13();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		#endregion
	}

	#endregion
}
