
namespace Enterprise.MasterFiles.Business
{
	using Enterprise.Integration;

	public class EquipmentCertificatesValidation : GenRegCertAccredMaintListValidation
	{
		public EquipmentCertificatesValidation(GenRegCertAccredMaintList parent)
			: base(parent)
		{
		}

		protected override void CheckXZ_Type()
		{
			base.CheckXZ_Type();

			if (!Parent.XZ_Type.IsEmpty)
			{
				var registryItem = ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes;
				var registryPath = ((IRegistryItemInternals)registryItem).Location;

				var type = registryItem.Value.Find(Parent.XZ_Type);
				if (type == null)
				{
					Parent.XZ_TypeInfo.AddError(Res.GetString("b2d6eae4-93d2-453e-8c3c-bba44a6daaa2", "Unknown type '{0}'. Check the Certificate Type in the following Registry: {1}", Parent.XZ_Type, registryPath));
				}
				else
				{
					if (type.IsUnique)
					{
						var equipment = (RefEquipment)Parent.MasterParent;
						if (equipment.Certificates.IsDuplicated(Parent.XZ_Type))
						{
							Parent.XZ_TypeInfo.AddError(Res.GetString("646c8574-91dc-45bc-ac7c-371336af43f4", "Equipment Certificate Type '{0} ({1})' must be unique according to the Registry: {2}", type.Code, type.Description, registryPath));
						}
					}
					if (type.IsMandatory)
					{
						if (Parent.XZ_RefNumber.IsEmpty)
						{
							Parent.XZ_TypeInfo.AddError(Res.GetString("e6276c62-c68f-4a04-a1c0-2a813e1c68df", "Equipment Certificate Type '{0} ({1})' is Mandatory according to the registry and thus must have its Reference Number filled out: {2}", type.Code, type.Description, registryPath));
						}
					}
				}
			}
		}

		protected override void CheckXZ_RefNumber()
		{
		}

		protected override void CheckTypeCodeIsInList()
		{
		}

		#region Implementation

		protected new GenRegCertAccredMaintList Parent
		{
			get { return (GenRegCertAccredMaintList)base.Parent; }
		}

		#endregion
	}
}
