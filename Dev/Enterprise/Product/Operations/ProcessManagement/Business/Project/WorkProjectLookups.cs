//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWorkProjectLookups
//
//    This class should be used for overriding collections in AutoWorkProjectLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkProjectLookups : AutoWorkProjectLookups
	{
		public WorkProjectLookups(AutoWorkProject parent)
			: base(parent)
		{ }

		public WorkProjectLookups(BusinessObjectFactory factory)
			: base(null)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public new Project Parent
		{
			get { return (Project)base.Parent; }
		}

		protected override BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? base.Factory;
			}
		}

		#region ProjectType

		public CodeDescriptionPairList ActiveTypes
		{
			get { return GetActiveTypes(Factory); }
		}

		static public CodeDescriptionPairList GetActiveTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WorkProjectLookups.ActiveTypes",
					delegate
					{
						return CreateTypeList(true);
					});
		}

		public CodeDescriptionPairList AllTypes
		{
			get { return GetAllTypes(Factory); }
		}

		static public CodeDescriptionPairList GetAllTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WorkProjectLookups.AllTypes",
				delegate
				{
					return CreateTypeList(false);
				});
		}

		public static CodeDescriptionPairList CreateTypeList(bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.ProjectTypeTree.Value;
			return tree.GetParents(activeOnly);
		}

		#endregion

		#region ProjectSubtype

		public CodeDescriptionPairList ActiveSubtypes
		{
			get
			{
				ZString parentType = Parent != null ? Parent.WKP_Type : ZString.Empty;
				return GetSubtypeList(parentType, true);
			}
		}

		public CodeDescriptionPairList AllSubtypes
		{
			get
			{
				ZString parentType = Parent != null ? Parent.WKP_Type : ZString.Empty;
				return GetSubtypeList(parentType, false);
			}
		}

		public CodeDescriptionPairList GetSubtypeList(ZString parentType, bool activeOnly)
		{
			return GetSubtypes(Factory, parentType, activeOnly);
		}

		public static CodeDescriptionPairList GetSubtypes(BusinessObjectFactory factory, ZString parentType, bool activeOnly)
		{
			return factory.GetCachedValue(
				"WorkProjectLookups.Subtypes:" + parentType + ":" + (activeOnly ? 'A' : 'X'),
					delegate
					{
						return CreateSubtypeList(parentType, activeOnly);
					});
		}

		public static CodeDescriptionPairList CreateSubtypeList(ZString parentType, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.ProjectTypeTree.Value;
			return tree.GetChildren(activeOnly, parentType);
		}

		#endregion

		#region ProjectModule

		public CodeDescriptionPairList ActiveModules
		{
			get
			{
				return GetModuleList(
					Parent != null ? Parent.WKP_Type : ZString.Empty,
					Parent != null ? Parent.WKP_SubType : ZString.Empty,
					true);
			}
		}

		public CodeDescriptionPairList AllModules
		{
			get
			{
				return GetModuleList(
					Parent != null ? Parent.WKP_Type : ZString.Empty,
					Parent != null ? Parent.WKP_SubType : ZString.Empty,
					false);
			}
		}

		public CodeDescriptionPairList GetModuleList(ZString parentType, ZString parentSubtype, bool activeOnly)
		{
			return GetModules(Factory, parentType, parentSubtype, activeOnly);
		}

		public static CodeDescriptionPairList GetModules(BusinessObjectFactory factory, ZString parentType, ZString parentSubtype, bool activeOnly)
		{
			return factory.GetCachedValue(
				"WorkProjectLookups.Modules:" + parentType + ":" + parentSubtype + ":" + (activeOnly ? 'A' : 'X'),
					delegate
					{
						return CreateModuleList(parentType, parentSubtype, activeOnly);
					});
		}

		public static CodeDescriptionPairList CreateModuleList(ZString parentType, ZString parentSubtype, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.ProjectTypeTree.Value;
			return tree.GetChildren(parentType, parentSubtype, activeOnly);
		}

		#endregion

		#region Priority

		public CodeDescriptionPairList ActivePriorities
		{
			get
			{
				return GetPriorityList(
					Parent != null ? Parent.WKP_Type : ZString.Empty,
					Parent != null ? Parent.WKP_SubType : ZString.Empty,
					Parent != null ? Parent.WKP_Module : ZString.Empty,
					true);
			}
		}

		public CodeDescriptionPairList AllPriorities
		{
			get
			{
				return GetPriorityList(
					Parent != null ? Parent.WKP_Type : ZString.Empty,
					Parent != null ? Parent.WKP_SubType : ZString.Empty,
					Parent != null ? Parent.WKP_Module : ZString.Empty,
					false);
			}
		}

		public CodeDescriptionPairList GetPriorityList(ZString parentType, ZString parentSubtype, ZString parentModule, bool activeOnly)
		{
			return GetPriorities(Factory, parentType, parentSubtype, parentModule, activeOnly);
		}

		public static CodeDescriptionPairList GetPriorities(BusinessObjectFactory factory, ZString parentType, ZString parentSubtype, ZString parentModule, bool activeOnly)
		{
			return factory.GetCachedValue(
				"WorkProjectLookups.Priorities:" + parentType + ":" + parentSubtype + ":" + parentModule + ":" + (activeOnly ? 'A' : 'X'),
					delegate
					{
						return CreatePriorityList(parentType, parentSubtype, parentModule, activeOnly);
					});
		}

		public static CodeDescriptionPairList CreatePriorityList(ZString parentType, ZString parentSubtype, ZString parentModule, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.ProjectTypeTree.Value;
			return tree.GetChildren(activeOnly, parentType, parentSubtype, parentModule);
		}

		#endregion

		#region Clients

		public OrganisationsFindBoxCollection Clients
		{
			get
			{
				if (clients == null)
				{
					ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsActive, ZBool.True);
					clients = new OrganisationsFindBoxCollection(Factory, query);
				}

				return clients;
			}
		}
		OrganisationsFindBoxCollection clients;

		#endregion

		#region Contacts

		public OrgContactDependentCollection ContactList
		{
			get
			{
				if (contactList == null || contactList.Master != Parent.ClientOrganisation)
				{
					if (Parent.ClientOrganisation != null)
					{
						ZQuery activeContactQuery = new ZQuery(OrgContactSchema.OC_IsActive, true);
						contactList = new OrgContactDependentCollection(Parent.ClientOrganisation, activeContactQuery);
					}
					else
					{
						contactList = new OrgContactDependentCollection(Factory);
					}
				}

				return contactList;
			}
		}

		OrgContactDependentCollection contactList;

		#endregion

		#region Technical Contacts

		public new OrgContactCollection TechnicalContacts
		{
			get
			{
				ZQuery query = new ZQuery(OrgContactSchema.OC_OH, SQLComparisonOperator.Equal, Parent.TechnicianOrganisationPK);

				OrgContactCollection result = new OrgContactCollection(Factory, query);

				if (!Parent.TechnicianOrganisationPK.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", Parent.TechnicianOrganisationPK));
				}

				return result;
			}
		}

		#endregion

		#region ProjectManagers

		public override GlbStaffCollection ProjectManagers
		{
			get
			{
				if (projectManagers == null)
				{
					ZQuery query = new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True);
					projectManagers = new GlbStaffCollection(Factory, query);
				}
				return projectManagers;
			}
		}
		GlbStaffCollection projectManagers;

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region Opportunities

		public override OrgOpportunityCollection Opportunities
		{
			get
			{
				ZGuid orgPK = Parent.ClientOrganisation != null ? Parent.ClientOrganisation.PK : ZGuid.Empty;
				ZQuery query = new ZQuery(OrgOpportunitySchema.P8_OH, SQLComparisonOperator.Equal, orgPK);
				OrgOpportunityCollection result = new OrgOpportunityCollection(Factory, query);

				if (!orgPK.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", orgPK));
				}
				return result;
			}
		}

		#endregion

		#region Status

		public CodeDescriptionPairList StatusList
		{
			get { return Factory.GetCachedValue<ProcessTaskStatusCodeList>(); }
		}

		#endregion

		public CodeDescriptionPairList CloseList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Descriptions.Closed);
				list.AddPair(ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Descriptions.Cancelled);
				return list;
			}
		}
	}
}
