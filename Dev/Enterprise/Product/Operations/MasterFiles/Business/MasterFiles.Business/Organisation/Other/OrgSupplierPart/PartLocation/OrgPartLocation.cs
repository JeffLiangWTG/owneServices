using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartLocation : AutoOrgPartLocation, ISupportDataImporting
	{
		public OrgPartLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		bool fIsImportingData;
		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OR_StockTakeCount = 1;
		}

		#endregion
	}
}
