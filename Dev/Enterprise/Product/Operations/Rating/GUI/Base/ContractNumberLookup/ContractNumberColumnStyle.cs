using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Rating.GUI
{
	internal class ContractNumberFindBoxColumnStyle : ZBaseFindBoxColumnStyle
	{
		public ContractNumberFindBoxColumnStyle(ContractNumberFindBoxColumnStyleInfo columnInfo)
			: base(() => new ContractNumberGridFindBox(), columnInfo)
		{
		}

		protected override void InitialiseEditControlForNewPosition(BusinessObject bizObj, bool readOnly)
		{
			base.InitialiseEditControlForNewPosition(bizObj, readOnly);
			FindBox.RateEntry = bizObj as IRateEntry;
		}

		internal new ContractNumberGridFindBox FindBox => base.FindBox as ContractNumberGridFindBox;
	}

	internal class ContractNumberFindBoxColumnStyleInfo : ZBaseFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ContractNumberFindBoxColumnStyle); }
		}
	}
}
