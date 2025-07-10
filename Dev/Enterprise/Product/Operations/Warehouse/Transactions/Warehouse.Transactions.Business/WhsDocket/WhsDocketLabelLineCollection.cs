using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ExcludeFromOverriddenAddNewTest]
	public abstract class WhsDocketLabelLineCollection : NonPersistentBusinessObjectCollection<WhsDocketLabelLine>
	{
		#region Constructors

		protected WhsDocketLabelLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		protected void Add(WhsDocketLabelLine line)
		{
			if (line != null && !IsDuplicateItem(line))
			{
				base.Add(line);
			}
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public ZInt TotalNumberOfLabels
		{
			get { return this.Sum(l => ((WhsDocketLabelLine)l).TotalNumberOfLabels); }
		}

		public ZInt NumberOfLabelsToPrint
		{
			get { return this.Sum(l => ((WhsDocketLabelLine)l).NumberOfLabelsToPrint); }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsDocketLabelLine();
		}

		bool IsDuplicateItem(WhsDocketLabelLine lineToCheck)
		{
			bool result = false;
			foreach (WhsDocketLabelLine item in this)
			{
				if (item.OrderNumber == lineToCheck.OrderNumber)
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
