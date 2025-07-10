using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Used for Rate Line Item Mappers.
	/// This object is NEVER persisted.
	/// </summary>
	public class DummyRateLineItem : RateLineItem
	{
		public DummyRateLineItem(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; } // dummy should never be saved
		}

		[ReadOnly(true)]
		public override ZDecimal TM_Value
		{
			get { return base.TM_Value; }
			set { base.TM_Value = value; }
		}

		[ReadOnly(true)]
		public override ZString TM_Text
		{
			get { return base.TM_Text; }
			set { base.TM_Text = value; }
		}
	}
}

