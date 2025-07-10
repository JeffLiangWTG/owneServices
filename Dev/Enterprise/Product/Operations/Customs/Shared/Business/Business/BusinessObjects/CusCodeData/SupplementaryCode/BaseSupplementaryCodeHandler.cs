using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseSupplementaryCodeHandler<TSupplementaryCode> : ISupplementaryCodeHandler<TSupplementaryCode>
		where TSupplementaryCode : BaseSupplementaryCode
	{
		public TSupplementaryCode LoadWithOrder<TParent>(TParent parent, ZShort order)
			where TParent : BusinessObject, ISupplementaryCodeSupporter
		{
			var loader = new BaseSupplementaryCode.Loader(parent.Factory);
			return loader.Load<TSupplementaryCode, TParent>(parent, order);
		}

		public TSupplementaryCode LoadOrCreate<TParent>(ZString value, TParent parent, ZShort order, ZPropertyInfo targetPropertyInfo)
			where TParent : BusinessObject, ISupplementaryCodeSupporter
		{
			var loader = new BaseSupplementaryCode.Loader(parent.Factory);
			var oldValue = ZString.Empty;
			var bizObj = loader.Load<TSupplementaryCode, TParent>(parent, order);
			if (!value.IsEmpty)
			{
				if (bizObj == null)
				{
					bizObj = loader.LoadOrCreate<TSupplementaryCode, TParent>(parent, order);
					parent.RegisterEditableChildObject(bizObj);
				}
				else
				{
					oldValue = bizObj.CY_Code;
				}

				bizObj.CY_Code = value;
			}
			else if (bizObj != null)
			{
				oldValue = bizObj.CY_Code;
				bizObj.SuspendValidation();
				bizObj.CY_Code = ZString.Empty;
				bizObj.Delete();
			}

			targetPropertyInfo.RefreshBinding(oldValue);

			return bizObj;
		}
	}
}
