using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultOSMG : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string OrgSecurityGroup = "OrgSecurityGroup";
		}

		#endregion

		public DefaultOSMG()
		{
		}

		public DefaultOSMG(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Properties

		#region OrgSecurityGroup

		[List("OrgSecurityGroups")]
		public ZGuid OrgSecurityGroup
		{
			get { return orgSecurityGroup; }
			set
			{
				SetNonPersistentPropertyValue(OrgSecurityGroupInfo, ref orgSecurityGroup, value);
			}
		}
		ZGuid orgSecurityGroup;

		public ZPropertyInfo OrgSecurityGroupInfo
		{
			get { return GetZPropertyInfo(Schema.OrgSecurityGroup); }
		}

		public GlbGroupCollection OrgSecurityGroups
		{
			get { return groupCollection ?? (groupCollection = new GlbGroupCollection(new BusinessObjectFactory())); }
		}
		GlbGroupCollection groupCollection;

		#endregion

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultOSMG(fallbackLevel, factory);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.OrgSecurityGroup, OrgSecurityGroup.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrgSecurityGroup = new ZGuid(reader.ReadElementString(Schema.OrgSecurityGroup));
		}

		#endregion
	}
}
