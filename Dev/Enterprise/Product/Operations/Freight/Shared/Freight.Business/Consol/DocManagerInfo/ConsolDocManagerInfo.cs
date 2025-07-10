using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// A part of the IDocManagerSupport interface, this returns information about objects related to a consol
	/// </summary>
	public class ConsolDocManagerInfo : DocManagerInfo
	{
		public ConsolDocManagerInfo(CommonConsol parent, ZString docManagerCode) : base(parent, docManagerCode)
		{
		}

		protected CommonConsol Consol
		{
			get { return (CommonConsol)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new ArrayList();
			var consol = Consol;
			list.AddRange(consol.Shipments);
			list.AddRange(consol.Containers);
			list.AddRange((BusinessObject[])consol.GetGlobalManifestHeaders());
			return (BusinessObject[])list.ToArray(typeof(BusinessObject));
		}
	}
}
