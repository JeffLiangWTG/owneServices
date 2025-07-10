using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Customs
{
	partial class XtCredentialLicenceType
	{
		public static string GetCode()
		{
			var databaseType = ObjectFactory.Get<IProductRegistration>()?.Key.DatabaseType ?? string.Empty;
			switch (databaseType)
			{
				case DatabaseTypes.Codes.Production:
					return Codes.Product;
				case DatabaseTypes.Codes.Training:
					return Codes.Training;
				default:
					return Codes.Test;
			}
		}
	}
}
