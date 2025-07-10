using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class UniversalDataObjectReaderHelper : Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper
	{
		public UniversalDataObjectReaderHelper(UniversalObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica)
		{
		}

		public IEnumerable<GenAddOnDetail> GetAsycudaManifestHeaderGenAddOnColumnList(AsycudaManifestHeader headerBO)
		{
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.ExcessIndicator), AddInfoKey = AddInfoConstants.AsycudaManifestHeader.ExcessIndicator, GenAddOnColumnName = AsycudaManifestHeader.Schema.ExcessIndicator, PropertyName = nameof(headerBO.ExcessIndicator) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.FullyLoadedUnloadedDate), AddInfoKey = AddInfoConstants.AsycudaManifestHeader.FullyLoadedUnloadedDate, GenAddOnColumnName = AsycudaManifestHeader.Schema.FullyLoadedUnloadedDate, PropertyName = nameof(headerBO.FullyLoadedUnloadedDate) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.GateInOutMessageType), AddInfoKey = AddInfoConstants.AsycudaManifestHeader.GateInOutMessageType, GenAddOnColumnName = AsycudaManifestHeader.Schema.GateInOutMessageType, PropertyName = nameof(headerBO.GateInOutMessageType) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.GateInOutDate), AddInfoKey = AddInfoConstants.AsycudaManifestHeader.GateInOutDate, GenAddOnColumnName = AsycudaManifestHeader.Schema.GateInOutDate, PropertyName = nameof(headerBO.GateInOutDate) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.ParentBill), AddInfoKey = AddInfoConstants.AsycudaManifestHeader.ParentBill, GenAddOnColumnName = AsycudaManifestHeader.Schema.ParentBill, PropertyName = nameof(headerBO.ParentBill) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.UnpackedDate), AddInfoKey = AddInfoConstants.AsycudaManifestHeader.UnpackedDate, GenAddOnColumnName = AsycudaManifestHeader.Schema.UnpackedDate, PropertyName = nameof(headerBO.UnpackedDate) };
		}

		public IEnumerable<GenAddOnDetail> GetAsycudaContainerGenAddOnColumnList(AsycudaContainer containerBO)
		{
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(containerBO.ContUnpackTime), AddInfoKey = AddInfoConstants.AsycudaContainer.ContUnpackTime, GenAddOnColumnName = AsycudaContainer.Schema.ContUnpackTime, PropertyName = AsycudaContainer.Schema.ContUnpackTime };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(containerBO.GateInOutDate), AddInfoKey = AddInfoConstants.AsycudaContainer.GateInOutDate, GenAddOnColumnName = AsycudaContainer.Schema.GateInOutDate, PropertyName = AsycudaContainer.Schema.GateInOutDate };
		}

		public IEnumerable<GenAddOnDetail> GetAsycudaBillGenAddOnColumnList(AsycudaBill billBO)
		{
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(billBO.CustomsCPC), AddInfoKey = AddInfoConstants.AsycudaBill.CustomsCPC, GenAddOnColumnName = AsycudaBill.Schema.CustomsCPC, PropertyName = AsycudaBill.Schema.CustomsCPC };
		}

		public IEnumerable<GenAddOnDetail> GetCusOutturnGenAddOnColumnList(CusOutturn outturnBO)
		{
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(outturnBO.ExcessShortInd), AddInfoKey = AddInfoConstants.AsycudePack.ExcessShortInd, GenAddOnColumnName = CusOutturn.Schema.ExcessShortInd, PropertyName = nameof(CusOutturn.Schema.ExcessShortInd) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(outturnBO.PackCondDesc), AddInfoKey = AddInfoConstants.AsycudePack.PackCondDesc, GenAddOnColumnName = CusOutturn.Schema.PackCondDesc, PropertyName = nameof(CusOutturn.Schema.PackCondDesc) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(outturnBO.ContShouldBe), AddInfoKey = AddInfoConstants.AsycudePack.ContShouldBe, GenAddOnColumnName = CusOutturn.Schema.ContShouldBe, PropertyName = nameof(CusOutturn.Schema.ContShouldBe) };
		}

		public AsycudaBillsReaderHelper BillsReaderHelper
		{
			get { return billsReaderHelper ?? (billsReaderHelper = new AsycudaBillsReaderHelper()); }
		}
		AsycudaBillsReaderHelper billsReaderHelper;

		public AyscudaContainersReaderHelper ContainersReaderHelper
		{
			get { return containersReaderHelper ?? (containersReaderHelper = new AyscudaContainersReaderHelper()); }
		}
		AyscudaContainersReaderHelper containersReaderHelper;

		public AyscudaPacksReaderHelper PacksReaderHelper
		{
			get { return packsReaderHelper ?? (packsReaderHelper = new AyscudaPacksReaderHelper()); }
		}
		AyscudaPacksReaderHelper packsReaderHelper;
	}

	public class AsycudaBillsReaderHelper : CollectionReaderHelper<AsycudaBill>
	{
		public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, AsycudaManifestHeader header)
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, header.PK);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			query.FetchOnlyFromLocalCache = !header.IsInDatabase;
			base.MarkUnprocessedExistingObjectFor(factory, query);
		}

		protected override ZGuid GetParentBOPK(AsycudaBill bill)
		{
			return bill.ABL_AMA;
		}
	}

	public class AyscudaContainersReaderHelper : CollectionReaderHelper<AsycudaContainer>
	{
		public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, AsycudaManifestHeader header)
		{
			var query = new ZQuery(AsycudaContainerSchema.ACN_AMA_Manifest, header.PK);
			query.FetchOnlyFromLocalCache = !header.IsInDatabase;
			base.MarkUnprocessedExistingObjectFor(factory, query);
		}

		protected override ZGuid GetParentBOPK(AsycudaContainer container)
		{
			return container.ACN_AMA_Manifest;
		}
	}

	public class AyscudaPacksReaderHelper : CollectionReaderHelper<AsycudaPack>
	{
		public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, AsycudaBill bill)
		{
			var query = new ZQuery(AsycudaPackSchema.APA_ABL_Bill, bill.PK);
			query.FetchOnlyFromLocalCache = !bill.IsInDatabase;
			base.MarkUnprocessedExistingObjectFor(factory, query);
		}
		protected override ZGuid GetParentBOPK(AsycudaPack pack)
		{
			return pack.APA_ABL_Bill;
		}
	}
}
