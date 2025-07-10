using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobNumberHelper
	{
		IReadOnlyCollection<(string Prefix, BillCustomisationRegistryItem Registry)> GetJobNumberCustomisationRegistries();

		string GetJobNumberRegEx(string prefix, BillOfLadingNumberCustomisation registryValue);

		string GetGenericJobTypeRegEx();
	}

	public class JobNumberHelper : IJobNumberHelper
	{
		public IReadOnlyCollection<(string Prefix, BillCustomisationRegistryItem Registry)> GetJobNumberCustomisationRegistries()
		{
			return (jobNumberCustomisationRegistries ??= GetInstance());

			IReadOnlyCollection<(string Prefix, BillCustomisationRegistryItem Registry)> GetInstance()
			{
				return new[] {
					(Env.NumberFountains.JobShipmentNumber.Prefix, FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation)
					, ($"({Env.NumberFountains.CustomsJobNo.Prefix}|{Env.NumberFountains.CustomsJobNoByExternalAgent.Prefix})", (BillCustomisationRegistryItem)ObjectFactory.Get<ICustomsDataRegistry>().DeclarationNumberCustomisation)
					, (Env.NumberFountains.JobConsolNumber.Prefix, (BillCustomisationRegistryItem)ObjectFactory.Get<IFreightConfigurationRegistry>().ConsolNumberCustomisation)
				};
			}
		}

		IReadOnlyCollection<(string Prefix, BillCustomisationRegistryItem Registry)> jobNumberCustomisationRegistries;

		public string GetJobNumberRegEx(string prefix, BillOfLadingNumberCustomisation registryValue)
		{
			var customizedRegEx = registryValue
				.Elements.Where(item => item.Include)
				.OrderBy(item => item.Order)
				.Select(item => item.RegExForDataType);
			
			var registryRegEx = string.Join("", customizedRegEx);
			registryRegEx += registryValue.CheckDigitAlgorithm == CheckDigitAlgorithmList.Codes.None
				? (NoResString)""
				: (NoResString)"[0-9a-zA-Z]{1}";

			return RegexWraperForCases(prefix, registryRegEx);
		}

		public string GetGenericJobTypeRegEx()
		{
			return (otherJobTypeRegEx ??= GetInstance());

			string GetInstance()
			{
				var jobNumberFountains = new INumberFountainProxy[] { null
					, Env.NumberFountains.CustomerServiceTicketNumber //CST
					, Env.NumberFountains.CusUnderbondNumberFountain // U,CFS/CTO Air Cargo Outturn Bills
					, Env.NumberFountains.CYDReceiveAdviceJobNumber //PAI
					, Env.NumberFountains.CYDReleaseAdviceJobNumber //RO
					, Env.NumberFountains.CYDDeliveryHeaderJobNumber //DI
					, Env.NumberFountains.CYDTransportationUnitID //TPU
					, Env.NumberFountains.DtbBookingConsolidationMultiJobID //CB
					, Env.NumberFountains.DtbBookingID //TB
					, Env.NumberFountains.DtbConsignmentRunSheetID //CR, Land Transport, Run sheets, Flight Schedule
					, Env.NumberFountains.DtbConsignmentID //CN
					, Env.NumberFountains.SundryChargesNumber //SC
					, Env.NumberFountains.ImporterSecurityFilingReference //ISF
					, Env.NumberFountains.JobCartageNumber //T, Port Transport
					, Env.NumberFountains.JobCartageRunSheetNumber //RS, Run Sheet (Port Transport
					, Env.NumberFountains.JobConsolNumberCFS  //L
					, Env.NumberFountains.JobShipmentNumberAgency //V
					, Env.NumberFountains.JobShipmentNumberCFS //H
					, Env.NumberFountains.TransitWarehouseDispatchID //TD
					, Env.NumberFountains.TransitWarehouseReceiveConsignmentID //RC
					, Env.NumberFountains.TransitWarehouseReceiveID //TR
					, Env.NumberFountains.TransitWarehouseDispatchConsignmentID //DC
					, Env.NumberFountains.WarehouseDocketID //W
					, Env.NumberFountains.WarehouseInvoiceNumber //I
					, Env.NumberFountains.WarehouseStocktakeNumber //SK
					, Env.NumberFountains.WarehouseVASOrderJobID //WV
					, Env.NumberFountains.WorkItemNo //WI, It is used byWhsAdHocServiceJob
					, Env.NumberFountains.VoyageAccountingNumber(GlbCompany.CurrentCompany.PK.ToGuid()) //VA
				}.Skip(1);
				var prefixPart = string.Join("|", jobNumberFountains.Select(x => x.Prefix).OrderBy(x => x));
				return RegexWraperForCases($"({prefixPart})", "([0-9]{8})");
			}
		}
		string otherJobTypeRegEx;

		string RegexWraperForCases(string prefix, string regex)
		{
			/*********
			 * We need to check cases that
			 * 1) "S00001001"
			 * 2) "\tS00001001"
			 * 3) "S00001001\t"
			 * 4) "\tS00001001\t"
			 * 5) "The line is end after S00001001.\r\nAnd this is second line."
			 *********/
			var wrappedRegEx = regex + @"[\.\,]?";
			return $"(^{prefix}|[\\s]{prefix})(({wrappedRegEx}[\\s])|({wrappedRegEx})$)";
		}
	}
}