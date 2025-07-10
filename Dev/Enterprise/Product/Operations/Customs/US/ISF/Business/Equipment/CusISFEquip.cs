using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Customs.US.ISF.Business
{
	[CodeProperty(CusISFEquip.Schema.BE_ContainerNum), DescriptionProperty(CusISFEquip.Schema.BE_ContainerNum)]
	[DependentBusinessObject(typeof(CusISFHeader), "Equipment")]
	[SingleObjectAroundARow]
	public class CusISFEquip : AutoCusISFEquip, IContainerData, ICusISFEquip
	{
		public CusISFEquip(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusISFHeader Header
		{
			get { return Factory.Load<CusISFHeader>(BE_BF); }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFEquipLookups.EquipmentDescriptionCodes))]
		public override ZString BE_EquipCode
		{
			get { return base.BE_EquipCode; }
			set { base.BE_EquipCode = value; }
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region IContainerData Members

		ZString IContainerData.EquipmentDescriptionCode
		{
			get { return BE_EquipCode; }
		}

		ZString IContainerData.EquipmentInitial
		{
			get { return BE_ContainerNum.Left(4); }
		}

		ZString IContainerData.EquipmentNumber
		{
			get
			{
				ZString equipmentNumberWithCheckDigit = EquipmentNumberWithCheckDigit;
				int numberLength = (equipmentNumberWithCheckDigit.Length > 7) ? 7 : 6;
				return equipmentNumberWithCheckDigit.Left(numberLength);
			}
		}

		ZString EquipmentNumberWithCheckDigit
		{
			get { return BE_ContainerNum.SubstringSafe(4); }
		}

		ZString IContainerData.EquipmentNumberCheckDigit
		{
			get
			{
				ZString equipmentNumberWithCheckDigit = EquipmentNumberWithCheckDigit;
				return (equipmentNumberWithCheckDigit.Length > 6) ? equipmentNumberWithCheckDigit.Right(1) : ZString.Empty;
			}
		}

		ZString IContainerData.EquipmentSizeTypeCode
		{
			get { return BE_ContainerISO; }
		}

		#endregion
	}
}
