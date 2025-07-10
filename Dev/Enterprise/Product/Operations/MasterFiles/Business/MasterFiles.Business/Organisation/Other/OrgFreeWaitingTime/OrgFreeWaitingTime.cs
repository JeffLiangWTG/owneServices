using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgFreeWaitingTime : AutoOrgFreeWaitingTime, IOrgFreeWaitingTime
	{
		public OrgFreeWaitingTime(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CodeDescriptionPairList DropModes
		{
			get
			{
				return new CombinedEquipmentNeededList();
			}
		}

		public IRefContainerCollection ContainerTypes
		{
			get { return new RefContainerCollection(Factory); }
		}

		#region IOrgFreeWaitingTime

		public ZDateTime CFS
		{
			get { return OY_CFSFreeWaitingTime; }
			set { OY_CFSFreeWaitingTime = value; }
		}

		public ZDateTime CNE
		{
			get { return OY_CNEFreeWaitingTime; }
			set { OY_CNEFreeWaitingTime = value; }
		}

		public ZDateTime CNR
		{
			get { return OY_CNRFreeWaitingTime; }
			set { OY_CNRFreeWaitingTime = value; }
		}

		public ZDateTime CTO
		{
			get { return OY_CTOFreeWaitingTime; }
			set { OY_CTOFreeWaitingTime = value; }
		}

		public ZDateTime CYD
		{
			get { return OY_CYDFreeWaitingTime; }
			set { OY_CYDFreeWaitingTime = value; }
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OY_CFSFreeWaitingTime
		{
			get => base.OY_CFSFreeWaitingTime;
			set => base.OY_CFSFreeWaitingTime = value.ConvertToDurationBasedDate(OY_CFSFreeWaitingTimeInfo);
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OY_CNEFreeWaitingTime
		{
			get => base.OY_CNEFreeWaitingTime;
			set => base.OY_CNEFreeWaitingTime = value.ConvertToDurationBasedDate(OY_CNEFreeWaitingTimeInfo);
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OY_CNRFreeWaitingTime
		{
			get => base.OY_CNRFreeWaitingTime;
			set => base.OY_CNRFreeWaitingTime = value.ConvertToDurationBasedDate(OY_CNRFreeWaitingTimeInfo);
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OY_CTOFreeWaitingTime
		{
			get => base.OY_CTOFreeWaitingTime;
			set => base.OY_CTOFreeWaitingTime = value.ConvertToDurationBasedDate(OY_CTOFreeWaitingTimeInfo);
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OY_CYDFreeWaitingTime
		{
			get => base.OY_CYDFreeWaitingTime;
			set => base.OY_CYDFreeWaitingTime = value.ConvertToDurationBasedDate(OY_CYDFreeWaitingTimeInfo);
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OY_OtherFreeWaitingTime
		{
			get => base.OY_OtherFreeWaitingTime;
			set => base.OY_OtherFreeWaitingTime = value.ConvertToDurationBasedDate(OY_OtherFreeWaitingTimeInfo);
		}

		[List("ContainerTypes")]
		public ZGuid CNTType
		{
			get { return OY_RC_ContainerType; }
			set { OY_RC_ContainerType = value; }
		}

		[List("DropModes")]
		public ZString DropMode
		{
			get { return OY_DropMode; }
			set { OY_DropMode = value; }
		}

		public ZDateTime Other
		{
			get { return OY_OtherFreeWaitingTime; }
			set { OY_OtherFreeWaitingTime = value; }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (Address != null && Address.Header != null)
			{
				shouldBeReadOnly = !Address.Header.SecurityProvider.HasModifyAddressCapabilitiesSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
