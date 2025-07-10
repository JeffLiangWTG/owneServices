using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class DeclarationLockConfigLookups : ZLookups
	{
		public DeclarationLockConfigLookups(DeclarationLockConfig parent, BusinessObjectFactory currentFactory)
			: base(parent)
		{
			this.currentFactory = Argument.NotNull(currentFactory, nameof(currentFactory));
		}

		readonly BusinessObjectFactory currentFactory;

		public ICodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				if (declarationTypeList == null)
				{
					var parent = (DeclarationLockConfig)Parent;
					var countryCode = parent.GetCompanyCountry();
					if (string.IsNullOrWhiteSpace(countryCode))
					{
						declarationTypeList = new CodeDescriptionPairList();
					}
					else
					{
						var provider = ObjectFactory.New<Integration.Customs.IDeclarationTypeListProvider>();
						declarationTypeList = (CodeDescriptionPairList)provider.GetListFor(countryCode);
					}
				}

				return declarationTypeList;
			}
		}
		ICodeDescriptionPairList declarationTypeList;

		public CodeDescriptionPairList LockModeList
		{
			get
			{
				return currentFactory.GetCachedValue("DeclarationLockConfigLookups+DeclarationLockModes", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.Customs.DeclarationLockModes.Codes.All, Constants.Customs.DeclarationLockModes.Descriptions.All);
					result.AddPair(Constants.Customs.DeclarationLockModes.Codes.Any, Constants.Customs.DeclarationLockModes.Descriptions.Any);

					return result;
				});
			}
		}
	}
}
