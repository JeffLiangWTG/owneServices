using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainerInvoiceLinePivotTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			if (bizO is CusContainerInvoiceLinePivot pivot)
			{
				pivot.Container.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			var container = declaration.CusContainers.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			invoiceLineContainer.IsForInvoiceLine = true;
			return invoiceLineContainer.Pivot;
		}

		protected override Type BaseTypeDecidedType => typeof(CusContainerInvoiceLinePivot);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia,    ObjectFactory.GetType<Integration.Customs.AU.ICusContainerInvoiceLinePivot>() },
				{ Core.Constants.CountryGuids.NewZealand,   ObjectFactory.GetType<Integration.Customs.NZ.ICusContainerInvoiceLinePivot>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusContainerInvoiceLinePivot>() }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia,    ObjectFactory.GetType<Integration.Customs.AU.ICusContainerInvoiceLinePivot>() },
				{ Core.Constants.CountryCodes.NewZealand,   ObjectFactory.GetType<Integration.Customs.NZ.ICusContainerInvoiceLinePivot>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusContainerInvoiceLinePivot>() }
			};
		}
	}
}
