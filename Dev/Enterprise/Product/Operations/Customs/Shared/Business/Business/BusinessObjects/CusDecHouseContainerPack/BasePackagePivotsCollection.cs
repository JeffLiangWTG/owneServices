using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BasePackagePivotsCollection<T> : DependentBusinessObjectCollection<T, BasePackage>
		where T : BusinessObject, ICusPackagePivot
	{
		public BasePackagePivotsCollection(BasePackage package)
			: base(package)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(T);
		}
	}
}
