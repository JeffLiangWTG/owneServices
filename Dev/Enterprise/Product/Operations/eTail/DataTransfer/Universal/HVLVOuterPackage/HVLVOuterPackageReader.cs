using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.DataTransfer.Universal
{
	class HVLVOuterPackageReader : DataObjectReader<PackingLine, HVLVOuterPackage>
	{
		public HVLVOuterPackageReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, HVLVOuterPackage packageBOToPopulate = null)
			: base(dataObject, logger, factory)
		{
			this.packageBOToPopulate = packageBOToPopulate;
		}

		HVLVOuterPackage packageBOToPopulate;

		protected override HVLVOuterPackage GetExistingBusinessObject()
		{
			var existingOuterPackage = default(HVLVOuterPackage);
			var reference = dataObject.ReferenceNumber.GetValueOrDefault();
			if (!reference.IsEmpty)
			{
				var query = new ZQuery(HVLVOuterPackageSchema.HVO_PackageReference, reference);
				existingOuterPackage = factory.Load<HVLVOuterPackage>(query).FirstOrDefault();
			}

			return existingOuterPackage;
		}

		protected override void PopulateBusinessObject(HVLVOuterPackage packageBO)
		{
			if (packageBOToPopulate == null)
			{
				packageBOToPopulate = packageBO;
			}

			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_PackageReference, dataObject.ReferenceNumber);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_Status, dataObject.Status);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_PackageBarcode, dataObject.Barcode);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_ContainerNumber, dataObject.ContainerNumber);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_F3_NKPackageType, dataObject.PackType?.Code);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_RH_NKCommodityCode, dataObject.Commodity?.Code);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_Volume, dataObject.Volume);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_Weight, dataObject.TareWeight);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_Length, dataObject.Length);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_Height, dataObject.Height);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_Width, dataObject.Width);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_WeightUQ, dataObject.WeightUnit?.Code);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_VolumeUQ, dataObject.VolumeUnit?.Code);
			SetValue(packageBOToPopulate, HVLVOuterPackageSchema.HVO_UnitOfDimension, dataObject.LengthUnit?.Code);

			PopulateOrgAddresses(dataObject, packageBOToPopulate);

			packageBOToPopulate.AttachMatchingItems(dataObject, factory);
		}

		void PopulateOrgAddresses(PackingLine outerPackageDataObject, HVLVOuterPackage outerPackageBO)
		{
			var destinationDepot = GetMatchingOrgAddress(outerPackageDataObject, nameof(DocAddressType.CustomsDepotAddress));

			if (destinationDepot != null)
			{
				SetValue(outerPackageBO, HVLVOuterPackageSchema.HVO_OA_DestinationDepot, destinationDepot.PK);
			}

			var lastMileCarrier = GetMatchingOrgAddress(outerPackageDataObject, AddressTypes.DeliveryLocalCartage);

			if (lastMileCarrier != null && !lastMileCarrier.OA_OH.IsEmpty)
			{
				SetValue(outerPackageBO, HVLVOuterPackageSchema.HVO_OH_LastMileCarrier, lastMileCarrier.OA_OH);
			}
		}

		OrgAddress GetMatchingOrgAddress(PackingLine outerPackageDataObject, string addressType)
		{
			var addressDataObject = outerPackageDataObject.OrganizationAddressCollection?.FirstOrDefault(addressType);
			return addressDataObject != null ? new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched() : null;
		}

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(HVLVOuterPackage targetBO)
		{
			if (targetBO != null)
			{
				if (targetBO.HVO_Status == HVLVOuterPackageStatus.Codes.Consolidated)
				{
					return Res.GetString("472ebcc6-0eab-425f-b924-08297e9839f6", "Outer Package {0} cannot be updated via XUS as it has already been consolidated.", targetBO.HVO_PackageReference);
				}
			}
			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		#endregion
	}
}
