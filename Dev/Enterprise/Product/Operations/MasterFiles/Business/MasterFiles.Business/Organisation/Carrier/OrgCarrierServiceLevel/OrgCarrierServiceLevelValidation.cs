using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierServiceLevelValidation : AutoOrgCarrierServiceLevelValidation
	{
		public OrgCarrierServiceLevelValidation(AutoOrgCarrierServiceLevel parent) : base(parent)
		{
		}

		public new OrgCarrierServiceLevel Parent
		{
			get { return (OrgCarrierServiceLevel)base.Parent; }
		}

		#region PL_CarrierServiceLevelDescription

		protected override void CheckPL_CarrierServiceLevelDescription()
		{
			base.CheckPL_CarrierServiceLevelDescription();
			MandatoryValidation.CheckEntered(Parent.PL_CarrierServiceLevelDescriptionInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PL_CarrierServiceLevelDescriptionInfo, GetAllCarrierServiceLevels());
		}

		#endregion

		#region PL_Code

		protected override void CheckPL_Code()
		{
			base.CheckPL_Code();
			MandatoryValidation.CheckEntered(Parent.PL_CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PL_CodeInfo, GetAllCarrierServiceLevels());
		}

		#endregion

		protected override void CheckPL_ProductCode()
		{
			base.CheckPL_ProductCode();
			if (!Parent.PL_ProductCode.IsEmpty)
			{
				ValidateCsvField(Parent.PL_ProductCodeInfo, csl => csl.ProductCodes);
			}
		}

		protected override void CheckPL_CarrierServiceCode()
		{
			base.CheckPL_CarrierServiceCode();
			if (!Parent.PL_CarrierServiceCode.IsEmpty)
			{
				ValidateCsvField(Parent.PL_CarrierServiceCodeInfo, csl => csl.CarrierServiceCodes);
			}
		}

		void ValidateCsvField(ZPropertyInfo propertyInfo, Func<OrgCarrierServiceLevel, ZString[]> getProperty)
		{
			var thisServiceCodes = getProperty(Parent);
			var allServiceCodes = GetAllCarrierServiceLevels()
				.Where(sl => sl.PK != Parent.PK)
				.SelectMany(getProperty);

			if (thisServiceCodes.Any(segment => string.IsNullOrWhiteSpace(segment)))
			{
				propertyInfo.AddError(Res.GetString("cd7dcf59-f3e7-41a4-90e8-1066bd2724b4", "The {0} must contain 1 value, or a series of values each separated by a comma.", propertyInfo.HumanReadableName));
			}

			if (allServiceCodes.Any(apc => thisServiceCodes.Any(tpc => tpc.EqualsIgnoringCase(apc))))
			{
				propertyInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(propertyInfo.HumanReadableName));
			}
		}

		OrgCarrierServiceLevel[] GetAllCarrierServiceLevels()
			=> Parent.Factory.Load<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_OM, Parent.PL_OM));
	}
}
