using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBWAAddInfoCollection : NonPersistentBusinessObjectCollection<WhsBWAAddInfo>
	{
		public WhsBWAAddInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected internal void Add(WhsBWAAddInfo addInfo)
		{
			if (addInfo != null && !IsDuplicateItem(addInfo))
			{
				base.Add(addInfo);
			}
		}

		bool IsDuplicateItem(WhsBWAAddInfo addInfo)
		{
			bool result = false;
			foreach (WhsBWAAddInfo item in this)
			{
				if (item.KeyString == addInfo.KeyString)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsBWAAddInfo("key1", "value1");
		}

		public ZString Serialize()
		{
			var stringBuilder = new ZStringBuilder(this.Cast<WhsBWAAddInfo>().Select(x => $"{x.KeyString}={x.ValueString}"));
			return stringBuilder.ToStringWithDelimiterBetweenAppends("*");
		}
	}
}
