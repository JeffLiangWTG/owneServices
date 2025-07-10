using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFLineRow : AutoISFLineRow
	{
		public ISFLineRow(CusISFLine line, ISFHeaderRow headerRow)
			: base(line.Factory)
		{
			this.Line = line;
			this.HeaderRow = headerRow;
			SetDefaultData();
		}

		public override ZString HarmonisedNum
		{
			get { return Line.BL_FormattedHarmonisedNum; }
		}

		public override ZString GoodsOrigin
		{
			get { return Line.BL_RN_NKGoodsOrigin; }
		}

		public JobDocAddress ManufacturerAddress
		{
			get { return Line.ManufacturerDocAddress; }
		}

		public ZString ManufacturerAddressText
		{
			get { return ManufacturerAddress != null ? ManufacturerAddress.AddressAsASingleLine : ZString.Empty; }
		}

		[ReadOnlyMember(nameof(ContainerPK_ReadOnly))]
		[List(nameof(Containers))]
		public override ZGuid ContainerPK
		{
			get { return base.ContainerPK; }
			set { base.ContainerPK = value; }
		}

		protected bool ContainerPK_ReadOnly
		{
			get { return Containers.Count == 0; }
		}

		public CusISFEquip Container
		{
			get
			{
				ISFContainerRow containerRow = (ISFContainerRow)Containers.FindByPK(ContainerPK);
				return containerRow == null ? null : containerRow.Container;
			}
		}

		public ISFContainerRowCollection Containers
		{
			get
			{
				if (containersCached == null)
				{
					containersCached = new CachedProperty<ISFContainerRowCollection>(Factory, delegate
					{
						return HeaderRow.Containers.GetContainersToCopy();
					});
				}
				return containersCached.Value;
			}
		}
		CachedProperty<ISFContainerRowCollection> containersCached;

		public readonly ISFHeaderRow HeaderRow;
		public readonly CusISFLine Line;

		void SetDefaultData()
		{
			if (Containers.Count == 1)
			{
				ContainerPK = Containers[0].PK;
			}
			ShouldCopy = true;
		}
	}
}
