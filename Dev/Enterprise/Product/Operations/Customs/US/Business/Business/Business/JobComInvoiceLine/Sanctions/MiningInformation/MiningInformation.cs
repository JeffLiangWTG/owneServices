using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class MiningInformation : CusCodeData
	{
		public MiningInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : CusCodeData.Schema
		{
			public const int CountryOfMiningMaxLength = 2;
		}

		#endregion

		public new MiningInformationLookups Lookups
		{
			get { return (MiningInformationLookups)base.Lookups; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new MiningInformationLookups(this);
		}

		public new MiningInformationValidation Validation
		{
			get { return (MiningInformationValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new MiningInformationValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.MiningInformationForSanctions;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsDeleted && CY_Data.IsEmpty)
			{
				Delete();
			}
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.MiningInformation|CountryOfMining", Caption = "Country/Region Of Mining")]
		[MaxLength(Schema.CountryOfMiningMaxLength)]
		[List(nameof(Lookups) + "." + nameof(MiningInformationLookups.Countries))]
		public ZString CountryOfMining
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		public ZPropertyInfo CountryOfMiningInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CountryOfMining), o => base.CY_DataInfo); }
		}
	}
}
