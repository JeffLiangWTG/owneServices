using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class EIDOMessagingHeader : AutoEIDOMessagingHeader
	{
		public EIDOMessagingHeader() { }

		#region Properties

		public override void ValidateEmail()
		{
			base.ValidateEmail();
			if (Identities.Count > 0)
			{
				MandatoryValidation.CheckEntered(EmailInfo);
			}

			if (!EmailAddressValidation.IsEmailAddressValid(Email))
			{
				EmailInfo.AddError(Res.GetString("{2B86BF7A-FEF1-4c9b-BDB0-B1D1E1089F78}", "Please enter a valid email address."));
			}
		}

		public new BusinessObjectFactory CurrentFactory
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CurrentFactory; }
		}

		#endregion

		#region Related BusinessObjects

		public EIDOMessagingIdentityCollection Identities
		{
			get
			{
				if (identities == null)
				{
					identities = new EIDOMessagingIdentityCollection(this);
					RegisterEditableChildObject(identities);
				}
				return identities;
			}
		}
		EIDOMessagingIdentityCollection identities;

		#endregion

		#region Operations

		public EIDOMessagingIdentity GetIdentity(ZGuid principalPK)
		{
			foreach (EIDOMessagingIdentity identity in Identities)
			{
				if (identity.PrincipalPK == principalPK)
				{
					return identity;
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Testing = true;
			Email = "stop20@test.1-stop.biz";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EIDOMessagingHeader();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			EIDOMessagingHeader newHeader = (EIDOMessagingHeader)clone;
			base.CopyValuesToClone(newHeader);

			newHeader.identities = (identities == null ? null : identities.Clone(newHeader));
		}

		protected override void ReadIdentities(XmlReader reader)
		{
			Identities.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("Identity")) // Xml Element Name
				{
					EIDOMessagingIdentity identity = Identities.AddNew();
					((IXmlSerializable)identity).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WriteIdentities(XmlWriter writer)
		{
			foreach (EIDOMessagingIdentity identity in Identities)
			{
				writer.WriteStartElement("Identity"); // This is XML text
				((IXmlSerializable)identity).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion
	}
}


