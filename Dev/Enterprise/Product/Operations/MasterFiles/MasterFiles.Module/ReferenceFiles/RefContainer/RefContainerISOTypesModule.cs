using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefContainerISOTypesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefContainerISOTypes; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefContainerISOTypes);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefContainerISOTypesFilterControl(GridCollection, (RefContainerISOTypesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefContainerISOTypesCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefContainerISOTypesFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Containers; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		public override bool AllowView => false;

		protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			//not required non persistent BO
			return true;
		}

		protected override bool IsModuleAllowAsync => false;

		protected override PerformSearchResult PerformSearchCore(Type type, ZQuery query)
		{
			var factory = SearchManager.GetNewFactory();
			return PerformSearchResult.CustomGridLoad(factory, query);
		}

		protected override void OnCustomGridLoad(IBusinessObjectCollection gridCollection, PerformSearchResult searchResult)
		{
			var fBizoObj = (RefContainerISOTypesFilterBusinessObject)FilterBusinessObject;
			if (!string.IsNullOrEmpty(fBizoObj.InitialCodeOnSearch) && Grid.List != null)
			{
				Grid.UnSelectAll();

				var collection = ((RefContainerISOTypesCollection)GridCollection).ToArray();
				var collectionItem = collection.FirstOrDefault(x => ((ContainerISOType)x).ISOCode == fBizoObj.InitialCodeOnSearch);

				Grid.SelectSingleElement(collectionItem);
			}
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			return Array.Empty<MenuItem>();
		}
	}
}
