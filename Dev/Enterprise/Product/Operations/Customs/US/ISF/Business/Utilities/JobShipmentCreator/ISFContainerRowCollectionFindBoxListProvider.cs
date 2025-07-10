using CargoWise.EntityFramework.Business.Internal;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFContainerRowCollectionFindBoxListProvider : NonPersistentBusinessObjectFindBoxListProvider
	{
		public ISFContainerRowCollectionFindBoxListProvider(ISFContainerRowCollection collection)
			: base(collection)
		{
		}

		public override string DescriptionFromPrimaryKey(ZGuid pK)
		{
			var container = List.Factory.Load<ISFContainerRow>(pK);
			return container != null ? container.ContainerDescription : ZString.Empty;
		}

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			var result = ZGuid.Empty;
			if (!string.IsNullOrEmpty(code))
			{
				var bizObj = GetBusinessObjectFromCode(code);
				result = bizObj != null ? bizObj.PK : ZGuid.Invalid;
			}

			return result;
		}

		public override string CodeFromPrimaryKey(ZGuid pK)
		{
			var container = List.Factory.Load<ISFContainerRow>(pK);
			return container != null ? container.ContainerNumber : ZString.Empty;
		}
	}
}

