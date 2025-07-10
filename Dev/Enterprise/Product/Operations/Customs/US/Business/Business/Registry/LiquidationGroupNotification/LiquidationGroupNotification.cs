using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class LiquidationGroupNotification : GroupNotification
	{
		#region Schema

		public new class Schema : GroupNotification.Schema
		{
			public const string SuppressNoChangeLiquidations = "SuppressNoChangeLiquidations";
		}

		#endregion

		public LiquidationGroupNotification()
		{
		}

		public LiquidationGroupNotification(ZString sendMode, ZGuid sendGroupPK, ZBool suppressNoChangeLiquidations)
			: base(sendMode, sendGroupPK)
		{
			this.SuppressNoChangeLiquidations = suppressNoChangeLiquidations;
		}

		public LiquidationGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region SuppressNoChangeLiquidations

		public ZBool SuppressNoChangeLiquidations
		{
			get { return suppressNoChangeLiquidations; }
			set { SetNonPersistentPropertyValue(SuppressNoChangeLiquidationsInfo, ref suppressNoChangeLiquidations, value); }
		}
		ZBool suppressNoChangeLiquidations;

		public ZPropertyInfo SuppressNoChangeLiquidationsInfo
		{
			get { return GetZPropertyInfo(Schema.SuppressNoChangeLiquidations); }
		}

		#endregion

		public new static LiquidationGroupNotification Default
		{
			get { return new LiquidationGroupNotification(GroupNotification.StaffMemberOrNominatedGroup, Core.Constants.Groups.PostMastersGroupPK, false); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LiquidationGroupNotification(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			SuppressNoChangeLiquidations = reader.ReadElementStringAsZBool(Schema.SuppressNoChangeLiquidations);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SuppressNoChangeLiquidations, SuppressNoChangeLiquidations.ToString());
		}
	}
}
