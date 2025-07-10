using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusMiscRequestLineCollection : ActiveBusinessObjectCollection<CusMiscRequestLine>
	{
		public CusMiscRequestLineCollection(CusMiscRequestHeader requestHeader)
			: base(requestHeader)
		{
		}
	}
}
