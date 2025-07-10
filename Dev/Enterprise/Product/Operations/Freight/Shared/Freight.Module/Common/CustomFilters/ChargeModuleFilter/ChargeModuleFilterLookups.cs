using System.Diagnostics;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Module
{
	public class ChargeModuleFilterLookups : ZLookups
	{
		public ChargeModuleFilterLookups(ChargeModuleFilter parent)
			: base(parent) { }

		public ChargeCodeGroupList ChargeGroups
		{
			get
			{
				if (chargeGroups == null)
				{
					chargeGroups = new ChargeCodeGroupList();
				}
				return chargeGroups;
			}
		}
		ChargeCodeGroupList chargeGroups;

		#region Implementation

		protected new ChargeModuleFilter Parent
		{
			[DebuggerStepThrough]
			get { return (ChargeModuleFilter)base.Parent; }
		}

		#endregion
	}
}
