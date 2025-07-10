using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class TariffViewAsCodeDescription : ICodeDescription
	{
		public TariffViewAsCodeDescription(ZString tariff, ZString description, bool isMandatory)
		{
			this.tariff = tariff;
			this.description = description;
			this.isMandatory = isMandatory;
		}

		readonly ZString tariff;
		readonly ZString description;
		readonly bool isMandatory;

		public ZString Tariff => tariff;
		public bool IsMandatory => isMandatory;

		public const string NotApplicableCode = "N/A";
		public const string NotApplicableDescription = "Not Applicable";
		public const string NotApplicableDescriptionForSection232 = "Section 232 Not Applicable";

		public override bool Equals(object obj) => ((ICodeDescription)this).Code.Equals(((ICodeDescription)obj)?.Code);

		public override int GetHashCode() => ((ICodeDescription)this).Code.GetHashCode();

		#region ICodeDescription

		string ICodeDescription.Code
		{
			get { return new TariffFormatter().DisplayFormat(Tariff); }
		}

		string ICodeDescription.Description
		{
			get { return description; }
		}

		object ICodeDescription.PK
		{
			get { return null; }
		}

		#endregion

	}
}
