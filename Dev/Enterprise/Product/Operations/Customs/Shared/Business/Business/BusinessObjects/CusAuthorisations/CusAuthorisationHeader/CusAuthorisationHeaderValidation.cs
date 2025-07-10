using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusAuthorisationHeaderValidation : CusPermitHeaderValidation
	{
		public CusAuthorisationHeaderValidation(CusAuthorisationHeader parent)
			: base(parent)
		{
		}

		public new CusAuthorisationHeader Parent => (CusAuthorisationHeader)base.Parent;

		protected override void CheckCPH_Type()
		{
			base.CheckCPH_Type();
			MandatoryValidation.CheckEntered(Parent.CPH_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CPH_TypeInfo);
		}

		protected override void CheckCPH_Number()
		{
			base.CheckCPH_Number();
			MandatoryValidation.CheckEntered(Parent.CPH_NumberInfo);
			CheckCPH_NumberIsUnique();
			CheckCPH_NumberFormat();
			Parent.Provider.CheckRuleMinRequirement(Parent, Parent.CPH_NumberInfo);
		}

		protected override void CheckCPH_StartDate()
		{
			base.CheckCPH_StartDate();
			MandatoryValidation.CheckEntered(Parent.CPH_StartDateInfo);
		}

		protected override void CheckCPH_EndDate()
		{
			base.CheckCPH_EndDate();
			MandatoryValidation.CheckEntered(Parent.CPH_EndDateInfo);
			if (Parent.CPH_StartDate >= Parent.CPH_EndDate)
			{
				Parent.CPH_EndDateInfo.AddError(Res.GetString("D9CDA5FD-AC23-4002-9943-D429CB62C63C", "Start date should be earlier than end date."));
			}
		}

		protected override void CheckCPH_EndDateIsValidZDateRange()
		{
		}

		protected override void CheckCPH_OA_AppliesTo()
		{
			var parent = Parent;

			var addressAuthorizationTypes = parent.Provider.AuthorizationTypesNeedAddress;
			if (addressAuthorizationTypes.Contains(parent.CPH_Type))
			{
				if (parent.CPH_OA_AppliesTo.IsEmpty)
				{
					parent.CPH_OA_AppliesToInfo.AddError(Res.GetString("C3DAD2CC-911D-47AE-BFE7-A990676C3D4A", "Please enter an Authorization Address when Authorization Type is {0}.", string.Join(", ", addressAuthorizationTypes)));
				}
			}

			var authorizationTypesNeedWarehouse = parent.Provider.AuthorizationTypesNeedWarehouse;
			if (authorizationTypesNeedWarehouse.Contains(parent.CPH_Type) && !parent.CPH_OA_AppliesTo.IsEmpty)
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, parent.CPH_OA_AppliesTo);
				query.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, SQLComparisonOperator.Equal, authorizationTypesNeedWarehouse);

				var authorisationHeader = parent.Factory.LoadTop1<CusAuthorisationHeader>(query);
				if (authorisationHeader != null)
				{
					parent.CPH_OA_AppliesToInfo.AddError(Res.GetString("84300F8D-7607-44D0-AA8D-16FECEE19912", "There is already a Customs Warehousing authorization {0} for this address.", authorisationHeader.CPH_Number));
				}
			}
		}

		protected override void CheckCPH_OH_PermitHolder()
		{
			base.CheckCPH_OH_PermitHolder();

			var parent = Parent;
			if (parent.CPH_ApplicationCode != CusPermitHeaderApplicationCodeList.Codes.Rule)
			{
				MandatoryValidation.CheckEntered(parent.CPH_OH_PermitHolderInfo);
			}
		}

		void CheckCPH_NumberIsUnique()
		{
			var parent = Parent;
			if (CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(parent))
			{
				parent.CPH_NumberInfo.AddError(Res.GetString("5D276680-FD9A-4A29-9357-A6F5BF3FC7CD", "Authorization holder has another authorization with the same type and number"));
			}
		}

		protected virtual void CheckCPH_NumberFormat()
		{
			var parent = Parent;
			var provider = parent.Provider;
			if (!parent.CPH_Number.IsEmpty && !provider.IsAuthorisationNumberValid(parent))
			{
				parent.CPH_NumberInfo.AddMessageError(provider.GetAuthorisationNumberInvalidFormatMessage(parent));
			}
		}
	}
}
