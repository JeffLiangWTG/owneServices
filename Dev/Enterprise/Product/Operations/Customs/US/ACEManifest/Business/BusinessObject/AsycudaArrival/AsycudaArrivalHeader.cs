using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaArrivalHeader : ASYCUDA.Business.AsycudaArrivalHeader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : ASYCUDA.Business.AsycudaArrivalHeader.Schema
		{
			public const int ACEManifestATH_ArrivalReferenceMaxLength = 1;
			public const int ACEManifestATH_VoyageFlightNoMaxLength = 8;
		}

		public AsycudaArrivalHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public override ZGuid ATH_AMA_ManifestHeader
		{
			get => base.ATH_AMA_ManifestHeader;
			set
			{
				var oldValue = ATH_AMA_ManifestHeader;
				base.ATH_AMA_ManifestHeader = value;
				if (ATH_AMA_ManifestHeader != oldValue && !IsCopying)
				{
					TransferHeaders.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		[MaxLength(Schema.ACEManifestATH_ArrivalReferenceMaxLength)]
		public override ZString ATH_Reference
		{
			get => base.ATH_Reference;
			set => base.ATH_Reference = value.ToUpper();
		}

		[MaxLength(Schema.ACEManifestATH_VoyageFlightNoMaxLength)]
		public override ZString ATH_VoyageFlightNo
		{
			get => base.ATH_VoyageFlightNo;
			set => base.ATH_VoyageFlightNo = value.ToUpper();
		}

		public void MarkTransferBillsAsNeedingValidation()
		{
			foreach (var transferHeader in TransferHeaders)
			{
				transferHeader.TransferBills.MarkAsNeedingValidation();
			}
		}

		public new IBusinessObjectCollection<AsycudaTransferHeader> TransferHeaders => (IBusinessObjectCollection<AsycudaTransferHeader>)base.TransferHeaders;
		protected override ManifestBase.IAsycudaTransferHeaderCollection<ManifestBase.AsycudaTransferHeader> CreateNewAsycudaTransferHeaderCollection() => new ManifestBase.AsycudaTransferHeaderCollection<AsycudaTransferHeader>(this);

		public new AsycudaArrivalHeaderValidation Validation => (AsycudaArrivalHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaArrivalHeaderValidation GetNewValidation() => new AsycudaArrivalHeaderValidation(this);
	}
}
