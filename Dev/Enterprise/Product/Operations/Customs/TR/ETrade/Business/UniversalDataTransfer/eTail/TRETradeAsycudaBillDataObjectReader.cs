using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.TR.ETrade.Business.AsycudaPack;

namespace Enterprise.Customs.TR.ETrade.Business
{
	sealed class TRETradeHVLVAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public TRETradeHVLVAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override void PopulateBillForSpecificRules(ASYCUDA.Business.AsycudaBill bill)
		{
			var billRow = GetColumnIndexer(bill);

			SetValue(billRow, AsycudaBillSchema.ABL_ManifestUQ, GetQuantityUnitFromHVLVItemWithFallback());
			SetValue(billRow, AsycudaBillSchema.ABL_NetWeight, dataObject.ManifestedWeight);
			SetValue(billRow, AsycudaBillSchema.ABL_NetWeightUQ, dataObject.TotalWeightUnit);

			var genAddOnData = new[]
				{
					new AddInfo() { Key = AsycudaBill.Schema.ArrivalCountry, Value = CountryCodes.Turkey },
				};

			var genAddOnColumnInfo = new[]
			{
					new GenAddOnDetail
					{
						TypeCode = AddOnColumnDataType.GetCodeFromType(typeof(ZString)),
						AddInfoKey = AsycudaBill.Schema.ArrivalCountry,
						GenAddOnColumnName = AsycudaBill.Schema.ArrivalCountry,
						PropertyName = AsycudaBill.Schema.ArrivalCountry
					},
				};

			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(genAddOnData, genAddOnColumnInfo, bill);
		}

		protected override void FillOrganizationsForSpecificRules(IColumnIndexer billRow)
		{
			SetValue(billRow, AsycudaBillSchema.ABL_ShipperRegNo, GetOrgVATCodeFromAddress(DocAddressType.ConsignorDocumentaryAddress));
			SetValue(billRow, AsycudaBillSchema.ABL_ConsigneeRegNo, GetOrgVATCodeFromAddress(DocAddressType.ConsigneeDocumentaryAddress));
		}

		protected override AsycudaPackDataObjectReader GetAsycudaPackDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return new TRETradeHVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, factory, bill, helper, isUpdateEnabled, dataObject);
		}

		ZString GetQuantityUnitFromHVLVItemWithFallback()
		{
			var packType = dataObject.PackingLineCollection.FirstOrDefault()?.PackType?.Code;

			if (packType.HasValue && !packType.Value.IsEmpty)
			{
				return packType.Value;
			}

			return PackTypes.Bin;
		}

		ZString GetOrgVATCodeFromAddress(DocAddressType addressType)
		{
			var result = ZString.Empty;

			var orgAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType.ToString());

			if (orgAddress != null)
			{
				if (orgAddress.GovRegNum.HasValue && orgAddress.GovRegNumType.Code.Equals(OrgCusCode.CodeTypes.VATCode))
				{
					result = orgAddress.GovRegNum.Value;
				}
				else
				{
					var vatCode = orgAddress.RegistrationNumberCollection?.FirstOrDefault(c => c.Type.Code.Equals(OrgCusCode.CodeTypes.VATCode));
					if (vatCode != null && vatCode.Value.HasValue)
					{
						result = vatCode.Value.Value;
					}
				}
			}

			return result;
		}
	}
}
