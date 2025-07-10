using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	/// <summary>
	/// Module for EntryLine.
	/// </summary>
	public class EntryLineModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.EntryLine; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EntryLineFilterControl(GridCollection, (EntryLineFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlobalCusEntryLineCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EntryLineFilterBusinessObject(GridCollection);
		}

		public override ZBool HasActions
		{
			get { return false; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomsFiles; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}
	}
}
