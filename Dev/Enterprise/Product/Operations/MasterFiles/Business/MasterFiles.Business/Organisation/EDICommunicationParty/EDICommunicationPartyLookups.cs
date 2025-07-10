using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationPartyLookups : AutoEDICommunicationPartyLookups
	{
		public EDICommunicationPartyLookups(AutoEDICommunicationParty parent)
			: base(parent)
		{
		}

		public new EDICommunicationParty Parent
		{
			get { return (EDICommunicationParty)base.Parent; }
		}

		[List("ApplicationCodes")]
		public CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				return Factory.GetCachedValue("EDICommunicationPartyLookups.ApplicationCodeList", () => CreateApplicationList());
			}
		}

		public static CodeDescriptionPairList CreateApplicationList()
		{
			var list = new CodeDescriptionPairList();
			var applicationDescriptorList = new EDIClientApplicationDescriptorList();
			foreach (CodeDescriptionPair providerCodePair in applicationDescriptorList)
			{
				if (ObjectFactory.Get<IEDIClientApplicationDescriptors>().GetValue(providerCodePair.Code) != null)
				{
					list.Add(providerCodePair);
				}
			}
			return list;
		}

		#region TechnicalContacts

		public override OrgContactCollection TechnicalContacts
		{
			get { return new EDICommunicationPartyOrgContactCollection(Factory); }
		}

		internal class EDICommunicationPartyOrgContactCollection : OrgContactCollection
		{
			public EDICommunicationPartyOrgContactCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ZQuery CreateAdditionalFilter()
			{
				return new ZQuery(OrgContactSchema.OC_Email, SQLComparisonOperator.IsNotBlank, string.Empty);
			}

			protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

				var orgContact = (OrgContact)selectedBusinessObject;

				if (orgContact.OC_Email.IsEmpty)
				{
					errors.Add(Res.GetString("7477DB38-A927-44FF-A735-7A07955C3546", "An Organization Contact selected from here must have an email."));
				}
			}
		}
		#endregion
	}
}
