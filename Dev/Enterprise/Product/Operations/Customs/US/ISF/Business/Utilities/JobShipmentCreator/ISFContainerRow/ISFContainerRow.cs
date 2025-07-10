using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	[CodeProperty(ISFContainerRow.Schema.ContainerNumber), DescriptionProperty("ContainerDescription")]
	public class ISFContainerRow : AutoISFContainerRow
	{
		public ISFContainerRow(CusISFEquip container, ISFHeaderRow headerRow)
			: base(container.Factory)
		{
			this.Container = container;
			this.headerRow = headerRow;
			SetDefaultData();
		}

		public readonly CusISFEquip Container;

		public override ZString ContainerNumber
		{
			get
			{
				CusISFEquip container = Container;
				return container == null ? ZString.Empty : container.BE_ContainerNum;
			}
		}

		public override ZString ContainerISOType
		{
			get
			{
				CusISFEquip container = Container;
				return container == null ? ZString.Empty : container.BE_ContainerISO;
			}
		}

		public override ZString ContainerUSCode
		{
			get
			{
				CusISFEquip container = Container;
				return container == null ? ZString.Empty : container.BE_EquipCode;
			}
		}

		public ZString ContainerDescription
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder(ContainerNumber);
				if (!ContainerISOType.IsEmpty)
				{
					builder.Append("ISOType:" + ContainerISOType);
				}
				if (!ContainerUSCode.IsEmpty)
				{
					builder.Append("USCode:" + ContainerUSCode);
				}
				return builder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public override ZBool ShouldCopy
		{
			get { return base.ShouldCopy; }
			set
			{
				ZBool oldValue = ShouldCopy;
				base.ShouldCopy = value;
				if (!IsCopying && oldValue != ShouldCopy)
				{
					headerRow.Bills.RefreshBindingIncludingChildren();
				}
			}
		}

		#region Implementation

		void SetDefaultData()
		{
			base.ShouldCopy = true;
		}

		readonly ISFHeaderRow headerRow;
		#endregion
	}
}
