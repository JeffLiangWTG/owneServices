using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class CertificateLookups : CusCodeDataLookups
	{
		public CertificateLookups(CertificateCusCodeData parent)
			: base(parent)
		{
		}

		protected abstract ZString[] PermitTypes { get; }

		new CertificateCusCodeData Parent => (CertificateCusCodeData)base.Parent;

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				return Factory.GetCachedValue($"{nameof(CY_CodeList)} for CertificateLookups", delegate
				{
					var result = new CodeDescriptionPairList();

					foreach (var permit in Certificates)
					{
						result.AddPair(permit.CPH_Number, permit.HumanReadableName);
					}

					return result;
				});
			}
		}

		public PermitFindBoxCollection Certificates
		{
			get
			{
				var permitHolder = Parent.EntryInstruction?.JobDeclaration?.Importer;
				var permitTypes = PermitTypes;
				var qtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
				var assessmentDate = CusEntryInstruction.GetEffectiveAssessmentDate(Parent.EntryInstruction, Factory);

				var filterRules = new Dictionary<CodeDescriptionPair, ZString>();

				return PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, permitHolder, permitTypes, Parent.CY_Code, filterRules, assessmentDate.Date, qtyValIndicator, true);
			}
		}
	}
}
