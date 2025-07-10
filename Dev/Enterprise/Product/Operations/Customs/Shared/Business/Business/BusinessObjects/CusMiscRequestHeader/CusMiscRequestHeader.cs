using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusMiscRequestHeader : AutoCusMiscRequestHeader
	{
		public CusMiscRequestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusMiscRequestHeaderTypeDecider TypeDecider = new CusMiscRequestHeaderTypeDecider();

		[ChildEditable(false)]
		public CusMiscRequestLineCollection RequestLines
		{
			get
			{
				if (requestLines == null)
				{
					requestLines = GetRequestLines();
					RegisterEditableChildObject(requestLines);
				}
				return requestLines;
			}
		}
		CusMiscRequestLineCollection requestLines;

		protected virtual CusMiscRequestLineCollection GetRequestLines() => new CusMiscRequestLineCollection(this);
	}
}
