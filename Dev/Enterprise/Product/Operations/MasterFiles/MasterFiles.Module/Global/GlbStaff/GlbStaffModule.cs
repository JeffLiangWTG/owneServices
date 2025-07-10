using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module.General;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffModule : ZFilterGridModule, IImportCollectionInfoProvider, IOperationalActionSupportable, IGlbStaffModule
	{
		public GlbStaffModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Actions Menu

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems().ToList();
			if (AllowNew)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Staff.NewStaff", "New Staff"), NewStaff_Click));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Staff.NewResource", "New Resource"), NewResource_Click));
			}
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var items = base.GetNewActionMenuItems().ToList();
			var menuItem = ObjectFactory.Get<IADActionsMenuItemProvider>().GetModuleMenuItem(this, Factory);
			if (menuItem != null)
			{
				items.Add((MenuItem)menuItem);
			}

			return items.ToArray();
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Staff; }
		}

		#endregion

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbStaff; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.GlbStaffDescriptorCode; }
		}

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.GlbStaff };

		#endregion

		#region Forms / Controller

		void NewResource_Click(object sender, EventArgs e)
		{
			ShowNewResourceForm();
		}

		void NewStaff_Click(object sender, EventArgs e)
		{
			ShowNewForm();
		}

		void ShowNewResourceForm()
		{
			ZControllerFactory.Create(ControllerIDs.GlbResource).ShowNewForm();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			GlbStaff staff = (GlbStaff)selectedBusinessObject;
			if (staff != null && staff.GS_IsResource)
			{
				return ZControllerFactory.Create(ControllerIDs.GlbResource);
			}
			return ZControllerFactory.Create(ControllerIDs.GlbStaff);
		}

		#endregion

		#region Filter

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbStaffFilterControl(GridCollection, (GlbStaffFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbStaffAndResourceCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbStaffFilterBusinessObject();
		}

		protected override bool IsModuleAllowAsync => false;

		protected override FilteredGridLoader CreateSearchManager()
			=> new GlbStaffFilteredGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements, Factory);

		#endregion

		#region IImportCollectionInfoProvider Members

		string IImportCollectionInfoProvider.ContextKey
		{
			get { return "{2270F081-AD9D-45d9-8FF2-0E30944CB12E}"; }
		}

		public IImportCollectionInfo ImportCollectionInfo
		{
			get
			{
				var importCollectionInfo = new ValidateAndSaveImportCollectionInfo<GlbStaff>();
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_LoginName);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Code, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Title);

				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_NameTitle);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_FullName);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_GivenName);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_MiddleName);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Surname);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_FriendlyName);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_PreferredSurname);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_FullNameInMotherLanguage);

				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_NextOfKin);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_NextOfKinHomePhone);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_NextOfKinEmail);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_NextOfKinRelationship, ZCharacterCasing.Upper);

				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmergencyContactName);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmergencyHomePhone);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmergencyContactEmail);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmergencyContactRelationship, ZCharacterCasing.Upper);

				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_RN_NKNationalityCode, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_ResidencyStatus, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_ResidencyExpiry);

				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Birthdate);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Gender);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmploymentDate);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmploymentBasis);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Pager);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_UserAddress1);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_UserAddress2);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_RN_NKCountryCode, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_City);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_State);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_Postcode);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_WorkingLanguage, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_GB_HomeBranch, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_GE_HomeDepartment, ZCharacterCasing.Upper);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_WorkPhone);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_WorkExtension);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_FaxNum);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_HomePhone);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_MobilePhone);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_EmailAddress);
				importCollectionInfo.Add(GlbStaff.Schema.StaffPlainTextPasswordForImportOnly);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_ChangePasswordAtNextLogin);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_CanLogin);

				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_DepartureDate);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_IsSalesRep);
				importCollectionInfo.Add(GlbStaffSchema.Constants.GS_SavePersonalDataToActiveDirectory);

				return importCollectionInfo;
			}
		}

		#endregion

		#region IOperationalActionSupportable

		public OperationalActionSupporter OperationalActionSupporter => new GlbStaffOperationalActionSupporter();

		#endregion

		public void HideRecentItems()
		{
			showRecentItems = false;
		}

		bool showRecentItems = true;

		protected override bool ShowRecentItemsCore()
		{
			return showRecentItems;
		}
	}

	class GlbStaffFilteredGridLoader : FilteredGridLoader
	{
		public GlbStaffFilteredGridLoader(
			FilterStripBusinessObject filterBusinessObject,
			ResultCountMessage handler,
			IModuleDecisionProvider provider,
			ModuleIdentifier moduleId,
			Func<BusinessObjectFactory> createFactory,
			Type typeOfElements,
			BusinessObjectFactory securityItemsFactory)
		: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
		{
			this.securityItemsFactory = securityItemsFactory;
		}

		protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var staffFilterBusinessObject = (GlbStaffFilterBusinessObject)filterBusinessObject;
			var securityRightFilters = staffFilterBusinessObject.ActiveModuleFilters.OfType<StaffSecurityModuleFilter>()
				.Where(filter => !filter.SecurityFilterContainer.LookupKey.IsEmpty)
				.ToList();

			if (securityRightFilters.Count > 0)
			{
				query.MaximumRows = null;
			}
			var result = factory.Load(type, query);

			if (securityRightFilters.Count > 0)
			{
				var deniedStaff = new HashSet<ZGuid>();

				// We load all security rows related to the checkpoints being queried (and their parents etc).
				// We could further refine the filter to only the staff members (and their groups) in the rest of the filter, but we don't know what those are yet.

				var allGlbSecurity = new GlbSecurityCollection(securityItemsFactory);
				var securityQuery = new ZQuery(GlbSecuritySchema.GU_SecurityRight, GlbStaffGroupHelper.KeysForSecurityRightFilters(securityRightFilters));
				allGlbSecurity.Load(securityQuery);
				GlbStaffGroupHelper.LoadCollectionWithCategory(result, securityRightFilters, allGlbSecurity, deniedStaff);
				result = result.Where(r => !deniedStaff.Contains(r.PK)).ToArray();
			}

			return result;
		}

		readonly BusinessObjectFactory securityItemsFactory;
	}
}
