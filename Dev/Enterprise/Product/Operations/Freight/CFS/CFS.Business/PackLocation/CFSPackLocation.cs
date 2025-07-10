
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLocation : PackLocation
	{
		public CFSPackLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		public new CFSPackLine PackLine
		{
			get { return (CFSPackLine)GetParentPackLine(); }
		}

		protected override PackLine GetParentPackLine()
		{
			return Factory.Load<CFSPackLine>(JQ_JL);
		}

		#endregion
	}
}
