using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// Summary description for CFSUnallocatedPackLinesView.
	/// </summary>
	public class CFSUnallocatedPackLinesView : ConsolUnAllocatedPackLinesView
	{
		public CFSUnallocatedPackLinesView(CFSLoadListConsol loadList, PackLineNonDependentCollection packLines)
			: base(loadList, packLines)
		{
			this.LoadList = loadList;
		}

		public new CFSPackLine this[int index]
		{
			get { return (CFSPackLine)base[index]; }
		}

		public new CFSPackLine AddNew()
		{
			return (CFSPackLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CFSPackLine);
		}

		#region Implementation

		protected CFSLoadListConsol LoadList;

		#endregion
	}
}
