using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCACCaseRate : AutoUSCACCaseRate
	{
		public USCACCaseRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		internal bool RemoveOnFactorySaving;

		protected override void OnFactorySaving()
		{
			if (RemoveOnFactorySaving && !IsDeleted)
			{
				Delete();
			}
			base.OnFactorySaving();
		}
	}
}
