using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefNMFC : AutoRefNMFC, Integration.IRefNMFC
	{
		public RefNMFC(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ReadOnly(true)]
		public override ZString FN_Code
		{
			get { return base.FN_Code; }
			set { base.FN_Code = value; }
		}

		public override ZString FN_Class
		{
			get
			{
				return base.FN_Class;
			}
			set
			{
				base.FN_Class = value;
				FN_Code = FN_ItemNo + "|" + value;
				RefreshBinding();
			}
		}

		public override ZString FN_ItemNo
		{
			get
			{
				return base.FN_ItemNo;
			}
			set
			{
				base.FN_ItemNo = value;
				FN_Code = value + "|" + FN_Class;
				RefreshBinding();
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("DC0DE941-FDA8-4A8F-AA12-42C895328A06", "NMFC - {0}", CalculateShortcutName());

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
