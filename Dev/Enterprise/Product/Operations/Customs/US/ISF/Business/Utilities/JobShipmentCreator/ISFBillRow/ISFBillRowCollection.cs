
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFBillRowCollection : NonPersistentBusinessObjectCollection<ISFBillRow>
	{
		public ISFBillRowCollection(ISFHeaderRow headerRow)
			: base(headerRow.Factory)
		{
			this.headerRow = headerRow;
		}

		public ISFBillRow this[ZGuid billPK]
		{
			get
			{
				ISFBillRow result = null;
				foreach (ISFBillRow row in this)
				{
					if (row.BillPK == billPK)
					{
						result = row;
						break;
					}
				}
				return result;
			}
		}

		#region Implementation
		readonly ISFHeaderRow headerRow;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ISFBillRow(headerRow);
		}

		protected override bool AllowNewCore
		{
			get { return !headerRow.IsOceanBillData; }
		}

		protected override bool AllowRemoveCore
		{
			get { return !headerRow.IsOceanBillData; }
		}

		#endregion
	}
}

