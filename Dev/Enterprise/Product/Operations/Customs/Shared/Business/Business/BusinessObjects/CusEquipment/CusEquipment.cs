using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(BaseJobDeclaration), nameof(BaseJobDeclaration.Equipments)), CodeProperty(CusEquipment.Schema.CEQ_IdentificationNumber), DescriptionProperty(CusEquipment.Schema.CEQ_IdentificationNumber)]
	public class CusEquipment : AutoCusEquipment, Integration.Customs.Shared.ICusEquipment, ITypeDeciderContext
		, IClusterKeyWorker
	{
		public CusEquipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString CEQ_IdentificationNumber
		{
			get => base.CEQ_IdentificationNumber;
			set
			{
				var oldValue = CEQ_IdentificationNumber;
				base.CEQ_IdentificationNumber = value;
				if (!IsCopying && oldValue != CEQ_IdentificationNumber)
				{
					if (Declaration is BaseJobDeclaration declaration && declaration.EquipmentsRequired)
					{
						declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(this);
						declaration.AddToContainersAndEquipmentsOnDeclaration_ListIfNeeded(CEQ_IdentificationNumberInfo, true);
					}
				}
			}
		}

		public override void Delete()
		{
			if (Declaration is BaseJobDeclaration declaration)
			{
				if (declaration.SupportEquipments && !CEQ_IdentificationNumber.IsEmpty)
				{
					var list = declaration.ContainersAndEquipmentsOnDeclaration_List;
					if (list.Cast<BusinessObjectElement>().FirstOrDefault(x => object.ReferenceEquals(x.BizObject, this)) is BusinessObjectElement item)
					{
						list.Remove(item);
					}

					var packingGroups = declaration.PackingGroups.Where(x => x.CR_CEQ_Equipment == PK).Select(x => x.PK).ToList();

					foreach (BasePackage package in declaration.Packages.Where(x => packingGroups.Contains(x.CW_CR_HouseContainer)))
					{
						package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
					}
				}
			}

			base.Delete();
		}

		public static readonly CusEquipmentTypeDecider TypeDecider = new CusEquipmentTypeDecider();

		public BaseJobDeclaration Declaration => Factory.Load<BaseJobDeclaration>(CEQ_JE_Declaration);

		[RelatedBusinessObject(nameof(Declaration))]
		public override ZGuid CEQ_JE_Declaration { get => base.CEQ_JE_Declaration; set => base.CEQ_JE_Declaration = value; }

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CEQ_JE_DeclarationInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CEQ_ClusterKeyInfo;

		#endregion
	}
}
