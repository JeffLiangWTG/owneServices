using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class RegoNumber : CusCodeData
	{
		public RegoNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(RegoNumberLookups.CY_CodeList))]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set { base.CY_Code = value; }
		}

		#endregion

		#region Lookups / Validation

		public new RegoNumberLookups Lookups
		{
			get { return (RegoNumberLookups)base.Lookups; }
		}

		public new RegoNumberValidation Validation
		{
			get { return (RegoNumberValidation)base.Validation; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new RegoNumberLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new RegoNumberValidation(this);
		}

		#endregion

		#region Related Objects

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				JobComInvoiceLine result = null;
				AIILine aiiLine = AIILine;
				if (aiiLine != null)
				{
					result = aiiLine.Parent;
				}
				return result;
			}
		}

		public AIILine AIILine
		{
			get { return Parent as AIILine; }
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(AIILine)); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.RegoNumber;
		}

		#endregion
	}
}
