using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.eManifest.Business
{
	class EquipmentCertificateTypesProvider : IDefaultCertificateTypesProvider
	{
		#region Implementation of IDefaultCertificateTypesProvider

		public void AddDefaultCertificateTypes(ICertificateTypeCollection defaultTypes)
		{
			defaultTypes.AddNew(ConveyanceReferences.Codes.ACEId, ConveyanceReferences.Descriptions.ACEId.GetUnresolvedString(), false, true, true, AlertTypeList.Codes.NoAlert);
			defaultTypes.AddNew(ConveyanceReferences.Codes.CarrierId, ConveyanceReferences.Descriptions.CarrierId.GetUnresolvedString(), false, true, true, AlertTypeList.Codes.NoAlert);
		}

		public ZValidation GetDefaultTypesSpecificValidation(BusinessObject parent)
		{
			return new EquipmentCertificateTypesValidation((AutoGenRegCertAccredMaintList)parent);
		}

		#endregion

		#region EquipmentCertificateTypesValidation

		class EquipmentCertificateTypesValidation : AutoGenRegCertAccredMaintListValidation
		{
			public EquipmentCertificateTypesValidation(AutoGenRegCertAccredMaintList parent)
				: base(parent)
			{
			}

			protected override void CheckXZ_RefNumber()
			{
				base.CheckXZ_RefNumber();
				if (!Parent.XZ_RefNumberInfo.HasNotifications())
				{
					switch (Parent.XZ_Type)
					{
						case ConveyanceReferences.Codes.ACEId:
							RelatedObjectValidation.MaxLengthValidation(Parent.XZ_RefNumberInfo, 10, null, ConveyanceReferences.Descriptions.ACEId);
							break;
						case ConveyanceReferences.Codes.CarrierId:
							RelatedObjectValidation.MaxLengthValidation(Parent.XZ_RefNumberInfo, 23, null, ConveyanceReferences.Descriptions.CarrierId);
							break;
					}
				}
			}
		}

		#endregion
	}
}
