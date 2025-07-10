using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Rating.Module
{
	public class UrsNamedAccountModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.UrsNamedAccount;

		public override bool AllowDelete => false;
		public override bool AllowEdit => false;
		public override bool AllowNew => false;
		public override bool AllowView => false;
		public override bool SupportsWorkflow => false;
		protected override bool ShowRecentItemsCore() => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		protected override bool IsModuleAllowAsync => false;

		#region Security Checkpoint

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		#endregion

		#region Default security overrides

		public override bool AllowUniversalCopy => false;

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox) =>
			new UrsNamedAccountModuleModuleDecisionProvider(findbox);

		class UrsNamedAccountModuleModuleDecisionProvider : PopupModuleDecisionProvider
		{
			public UrsNamedAccountModuleModuleDecisionProvider(IFindBox findBox) : base(findBox) { }

			public override bool AllowExcelExport => true;
		}

		#endregion

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			return PerformSearchResult.CustomGridLoad(factory, query);
		}

		protected override void OnCustomGridLoad(IBusinessObjectCollection collection, PerformSearchResult searchResult)
		{
			var list = (OrgCarrierNamedAccountCollection)collection;
			list.RemoveAll();

			var allMappedAccounts = Factory.Load<OrgCarrierNamedAccount>(new ZQuery());
			var assignedNamedAccounts = new HashSet<ZString>(allMappedAccounts.Select(n => n.ONA_ForeignName));
			var allNamedAccounts = OrgCarrierNamedAccountLookups.GetCachedNamedAccountsList(Factory);
			var unassignedNamedAccounts = allNamedAccounts.Where(account => !assignedNamedAccounts.Contains(account));

			OrgCarrierNamedAccount bo = null;
			foreach (var account in unassignedNamedAccounts)
			{
				bo ??= list.AddNew();
				bo.ONA_ForeignName = account;

				if (bo.MatchesFilter(searchResult.Query))
				{
					bo = null;
				}
			}

			if (bo != null)
			{
				list.Remove(bo);
			}

			foreach (var account in allMappedAccounts)
			{
				if (account.MatchesFilter(searchResult.Query))
				{
					list.Add(account);
				}
			}

			list.Load(searchResult.Query);
		}

		protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery query)
		{
			return true;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UrsNamedAccountFilterBusinessObject(carrierFilterDisabled: true);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UrsNamedAccountFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgCarrierNamedAccountCollection(Factory);
		}
	}
}
