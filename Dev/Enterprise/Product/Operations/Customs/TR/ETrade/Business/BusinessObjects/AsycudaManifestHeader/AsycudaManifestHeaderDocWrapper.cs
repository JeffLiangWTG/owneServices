using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaManifestHeaderDocWrapper : ASYCUDA.Business.AsycudaManifestHeaderDocWrapper
	{
		public AsycudaManifestHeaderDocWrapper(AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
		}
		public new AsycudaManifestHeader Manifest => (AsycudaManifestHeader)base.Manifest;
		public AsycudaBillCollection Bills => Manifest.Bills;
		public BusinessObjectCollectionWrapper<AsycudaPackDocWrapper> ETradeDutyReportWrapper
		{
			get
			{
				Manifest.CalculateDuties();
				var packDocWrappers = new List<AsycudaPackDocWrapper>();
				foreach (AsycudaBill bill in Manifest.Bills)
				{
					foreach (AsycudaPack pack in bill.Packs)
					{
						packDocWrappers.Add(new AsycudaPackDocWrapper(pack));
					}
				}
				return new BusinessObjectCollectionWrapper<AsycudaPackDocWrapper>(packDocWrappers);
			}
		}

		#region Properties

		public ZDecimal TotalDutyOfBills
		{
			get
			{
				var result = 0M;
				foreach (AsycudaBill bill in Bills)
				{
					var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
					result += tax.IsNull ? 0 : tax.AET_ChargeAmount;
				}
				return result;
			}
		}
		public ZString JobReference => Manifest.AMA_JobReference;
		public ZString CustomsOffice => Manifest.Lookups.CustomsOfficeList.GetDescriptionFromCode(Manifest.AMA_CustomsOffice);
		public ZDecimal StampTax => Manifest.MasterBill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.StampTax)?.AET_ChargeAmount ?? ZDecimal.Zero;
		public ZDateTime DateAtCustomsOffice => Manifest.DateAtCustomsOffice;
		public ZString RegistrationNumber => Manifest.RegistrationNumber;
		public ZDateTime RegistrationDate => Manifest.RegistrationDate;
		public ZDecimal ExchangeRate => Manifest.ExchangeRate;
		public ZString Currency => "EUR";
		#endregion
	}
}
