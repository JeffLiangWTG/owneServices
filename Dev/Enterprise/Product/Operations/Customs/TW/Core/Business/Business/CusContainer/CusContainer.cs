using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusContainer : AutoTWCusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString CO_FCL_LCL_AIR
		{
			get => base.CO_FCL_LCL_AIR;
			set
			{
				var oldValue = base.CO_FCL_LCL_AIR;
				base.CO_FCL_LCL_AIR = value;
				if (!IsCopying && oldValue != CO_FCL_LCL_AIR)
				{
					switch (CO_FCL_LCL_AIR)
					{
						case TWContainerModesList.Codes.Empty:
							if (!JC_IsEmptyContainer)
							{
								JC_IsEmptyContainer = true;
							}
							break;
						default:
							if (JC_IsEmptyContainer)
							{
								JC_IsEmptyContainer = false;
							}
							break;
					}
					if (CO_IsPart_ReadOnly && CO_IsPart)
					{
						CO_IsPart = false;
					}
				}
			}
		}

		[ResourceStringData("BD6FA4FF-CB50-4135-A75C-6AFC2DE7E3A0", Caption = "Part")]
		[ReadOnlyMember(nameof(CO_IsPart_ReadOnly))]
		public override ZBool CO_IsPart { get => base.CO_IsPart; set => base.CO_IsPart = value; }

		public bool CO_IsPart_ReadOnly
		{
			get
			{
				switch (CO_FCL_LCL_AIR)
				{
					case TWContainerModesList.Codes.FCL:
					case TWContainerModesList.Codes.BCN:
						return false;
					default:
						return true;
				}
			}
		}

		public ZBool JC_IsEmptyContainer
		{
			get { return JobContainer.JC_IsEmptyContainer; }
			set
			{
				var oldValue = JC_IsEmptyContainer;
				JobContainer.JC_IsEmptyContainer = value;
				this.JC_IsEmptyContainerInfo.RefreshBinding(oldValue);
				if (!IsCopying && oldValue != JC_IsEmptyContainer)
				{
					if (JC_IsEmptyContainer)
					{
						if (CO_FCL_LCL_AIR != TWContainerModesList.Codes.Empty)
						{
							CO_FCL_LCL_AIR = TWContainerModesList.Codes.Empty;
						}
					}
					else if (CO_FCL_LCL_AIR == TWContainerModesList.Codes.Empty)
					{
						CO_FCL_LCL_AIR = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo JC_IsEmptyContainerInfo
		{
			get { return GetZPropertyInfo(JobContainerSchema.Constants.JC_IsEmptyContainer); }
		}
	}
}
