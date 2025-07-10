using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefExchangeRateController : ZController, IBusinessObjectLoader
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.ExchangeRate;

		public override ModuleIdentifier ModuleID => ModuleIDs.ExchangeRate;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefExchangeRate);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.LoadTop1<RefExchangeRate>(RefExchangeRate.Loader.GetFilterByPK(sourceEntityPK));
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var exRate = (RefExchangeRate)businessEntity;
			return new RefExchangeRateForm(exRate);
		}

		BusinessObject IBusinessObjectLoader.Load(BusinessObjectFactory factory, ZGuid pk)
		{
			RefExchangeRate result = null;
			var moduleResultsPKCollection = ModuleResultsPKCollection;
			if (moduleResultsPKCollection != null)
			{
				PKData? foundData = null;
				foreach (var data in moduleResultsPKCollection)
				{
					if (data.PK == pk)
					{
						foundData = data;
						break;
					}
				}
				if (foundData.HasValue)
				{
					result = factory.LoadTop1<RefExchangeRate>(RefExchangeRate.Loader.GetFilterByPK(pk));
				}
			}
			return result;
		}
	}
}
