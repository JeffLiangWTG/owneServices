using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class CensusWarningOverride : Customs.Business.CusCodeData, ICensusWarningOverride, Integration.Customs.US.ICensusWarningOverride
	{
		public class Comparer : IComparer<CensusWarningOverride>
		{
			public int Compare(CensusWarningOverride x, CensusWarningOverride y)
			{
				int result = x.CY_Code.CompareTo(y.CY_Code);

				if (result == 0)
				{
					result = x.CY_Data.CompareTo(y.CY_Data);
				}
				return result;
			}
		}

		public CensusWarningOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CensusWarningOverrideLookup Lookups
		{
			get { return (CensusWarningOverrideLookup)base.Lookups; }
		}

		[List(nameof(Lookups) + "." + nameof(CensusWarningOverrideLookup.CensusOverrideList))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new CensusWarningOverrideValidation(this);
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CensusWarningOverrideLookup(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CY_Type = CusCodeDataTypeList.Codes.CensusWarningOverride;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot)); }
		}

		#region ICensusWarningOverride Members

		ZString ICensusWarningOverride.ConditionCode
		{
			get { return CY_Code; }
		}

		ZString ICensusWarningOverride.OverrideCode
		{
			get { return CY_Data; }
		}

		#endregion
	}
}
