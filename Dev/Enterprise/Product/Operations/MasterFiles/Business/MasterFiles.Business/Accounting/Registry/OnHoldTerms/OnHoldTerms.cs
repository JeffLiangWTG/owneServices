using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OnHoldTerms : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Terms = "Terms";
			public const string TermDays = "TermDays";
		}

		#endregion

		#region Bound Properties

		#region Terms

		[List(nameof(ARTerms))]
		public ZString Terms
		{
			get { return terms; }
			set
			{
				if (value != terms)
				{
					TermDays = 0;
				}
				SetNonPersistentPropertyValue(TermsInfo, ref terms, value);
				if (!IsValidationSuspended)
				{
					ValidateTerms();
				}
			}
		}

		public ZPropertyInfo TermsInfo
		{
			get { return GetZPropertyInfo(Schema.Terms, "Terms"); }
		}

		public void ValidateTerms()
		{
			TermsInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(TermsInfo);
		}

		ZString terms;

		public CodeDescriptionPairList ARTerms => new ARInvoiceTermsList();

		#endregion

		#region Term Days

		[ReadOnlyMember(nameof(TermDaysIsReadOnly))]
		public ZByte TermDays
		{
			get { return termDays; }
			set
			{
				SetNonPersistentPropertyValue(TermDaysInfo, ref termDays, value);
				if (!IsValidationSuspended)
				{
					ValidateTermDaysInfo();
				}
			}
		}

		public ZPropertyInfo TermDaysInfo
		{
			get { return GetZPropertyInfo(Schema.TermDays, "Term Days"); }
		}

		public void ValidateTermDaysInfo()
		{
			TermDaysInfo.ClearAllNotifications();
		}

		ZByte termDays;

		bool TermDaysIsReadOnly => InvoiceTerm.GetIsTermWithoutDays(Terms);

		#endregion

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Terms = ARInvoiceTermsList.CashOnDelivery.Code;
			TermDays = 0;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OnHoldTerms();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Terms, Terms);
			writer.WriteElementString(Schema.TermDays, TermDays.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Terms = reader.ReadElementString(Schema.Terms);
			TermDays = ZByte.ParseSafe(reader.ReadElementString(Schema.TermDays), 0);
		}

		#endregion
	}
}
