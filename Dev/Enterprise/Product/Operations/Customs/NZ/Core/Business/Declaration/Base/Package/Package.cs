using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class Package : BasePackage, IPackingInformation, Integration.Customs.NZ.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override CodeDescriptionPairList PackTypeList
		{
			get { return UniversalReferenceHelper.GetUNEPackageTypeList(Factory); }
		}

		protected override CusDecHouseContainerPackValidation GetNewValidation()
		{
			return new PackageValidation(this);
		}

		public override ZInt CW_PackQty
		{
			get { return base.CW_PackQty; }
			set
			{
				bool hasChanged = base.CW_PackQty != value;
				base.CW_PackQty = value;
				MarkPackagesAsNeedingValidation(hasChanged);
			}
		}

		protected override ZString FreightPackageTypeCore
		{
			get { return PackageTypeConverter.GetFreightPackageType(CW_PackType); }
		}

		public override ZGuid CW_CR_HouseContainer
		{
			get { return base.CW_CR_HouseContainer; }
			set
			{
				bool hasChanged = base.CW_CR_HouseContainer != value;
				base.CW_CR_HouseContainer = value;
				MarkPackagesAsNeedingValidation(hasChanged);
			}
		}

		void MarkPackagesAsNeedingValidation(bool shouldMarkIt)
		{
			if (shouldMarkIt)
			{
				if (Bill != null && Bill.Declaration != null)
				{
					Bill.Declaration.MarkAsNeedingValidation();
				}

				if (PackingGroup != null)
				{
					PackingGroup.Packages.MarkAsNeedingValidation();
				}
			}
		}

		#region IPackingInformation Members

		bool IPackingInformation.SupportMarksAndNumbers
		{
			get { return false; }
		}

		ZString IPackingInformation.MarksAndNumbers
		{
			get { return ZString.Empty; }
			set { }//NZ does not need Marks and numbers at package level
		}

		#endregion

		protected override bool ShouldDeleteIfPackQtyIsEmpty
		{
			get { return CW_ContainerNoOrEquipmentNo.IsEmpty; }
		}

		public new PackingGroup PackingGroup
		{
			get { return (PackingGroup)base.PackingGroup; }
		}
	}
}
