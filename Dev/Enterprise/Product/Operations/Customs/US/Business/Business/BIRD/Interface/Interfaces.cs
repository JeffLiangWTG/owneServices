using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.BIRD.Common
{
	public interface IBIRDRecord
	{
		void Deserialise(string eightyCharacterBlock);
		string Serialise();
	}

	public interface IBIRDHeaderIDRecord : IBIRDRecord
	{
		ZString EntryNumber { get; }
		ZString EntryFilerCode { get; }
	}

	public interface IBIRDBrokerRefernceRecord : IBIRDRecord
	{
		ZString BrokerReferenceNumber { get; }
	}

	public interface IBIRDHeaderRecord : IBIRDRecord
	{
		void Update(JobDeclaration declaration, INotifications notifications);
	}

	public interface IBIRDTariffRecord : IBIRDRecord
	{
		ZString Tariff { get; }
	}
}

namespace Enterprise.Customs.US.Business.BIRD.ACS
{
	public interface IBIRDStatusRecord : IBIRDRecord
	{
		void Update(JobDeclaration declaration, INotifications notifications);
	}

	//ENS40 to ENS62
	public interface IBIRDLineRecord : IBIRDTariffRecord
	{
		void Update(JobComInvoiceLine invoiceLine, INotifications notifications);
	}

	//ENS40
	public interface IBIRDLineIDRecord : IBIRDLineRecord
	{
		ZInt DelimiterInvSequence { get; }

		ZInt LineNumber { get; }
	}

	//ENS70, ENS80, ENS81
	public interface IBIRDSecondaryLineRecord : IBIRDTariffRecord
	{
		void Update(JobComInvoiceLine secondaryLine, INotifications notifications);
	}

	//OI, OA records
	public interface IBIRDOGAStartRecord : IBIRDRecord
	{
		ZString CommercialDesc { get; }

		void SetOGAIndicator(JobComInvoiceLine invoiceLine);
	}

	public interface IBIRDOGALineRecord : IBIRDRecord
	{
		void Update(IOGALine ogaLine, INotifications notifications);
	}

	public interface IBIRDOGALineIDRecord : IBIRDOGALineRecord
	{
		OGAType OGAType { get; }

		void SetOGAIndicator(JobComInvoiceLine invoiceLine);
	}

	public interface IBIRDPGAConstituentRecord : IBIRDRecord
	{
		void Update(ConstituentElement constituentElement, INotifications notifications);
	}

	public interface IBIRDPGAScientificRecord : IBIRDRecord
	{
		void Update(ConstituentElement constituentElement, INotifications notifications);
	}
}

namespace Enterprise.Customs.US.Business.BIRD.ACE
{
	public interface IACEBIRDSecondaryLineRecord : IBIRDTariffRecord
	{
		void Update(JobComInvoiceLine invoiceLine, bool isSupplementaryTariff, INotifications notifications);
	}

	public interface IACEBIRDLineRecord : IBIRDRecord
	{
		void Update(JobComInvoiceLine invoiceLine, INotifications notifications);
	}

	public interface IACEBIRDLineIDRecord : IACEBIRDLineRecord
	{
		ZInt LineNumber { get; }
	}

	public interface IACEBIRDOrgCompanyRecord
	{
		ZString OrganizationType { get; }
		ZString CompanyName { get; }
		ZString CustomsNoType { get; }
		ZString CustomsNumber { get; }
	}

	public interface IACEBIRDOrgAddressRecord
	{
		ZString Address1 { get; }
	}

	public interface IACEBIRDOrgAddress2Record
	{
		ZString Address2 { get; }
	}

	public interface IACEBIRDOrgCountryRecord
	{
		ZString City { get; }
		ZString Country { get; }
		ZString ZipCode { get; }
	}
}

namespace Enterprise.Customs.US.Business
{
	static class IBIRDTariffRecordExtensionMethod
	{
		public static bool IsSupplementaryTariff(this IBIRDTariffRecord record)
		{
			return IBIRDLineRecordHelper.IsSupplementaryTariff(record.Tariff);
		}

		public static void UpdateValue(this IBIRDTariffRecord record, JobComInvoiceLine invoiceLine, ZDecimal value)
		{
			IBIRDLineRecordHelper.UpdateValue(invoiceLine, record.Tariff, value);
		}

		public static void SetTariffs(this IBIRDTariffRecord record, JobComInvoiceLine invoiceLine)
		{
			IBIRDLineRecordHelper.SetTariffs(invoiceLine, record.Tariff);
		}

		public static void SetTariffRelatedDetails(this IBIRDTariffRecord record, JobComInvoiceLine invoiceLine, ZDecimal qty1, ZString uQ1, ZDecimal qty2, ZString uQ2, ZDecimal qty3, ZString uQ3)
		{
			IBIRDLineRecordHelper.SetTariffRelatedDetails(invoiceLine, record.Tariff, qty1, uQ1, qty2, uQ2, qty3, uQ3);
		}
	}

	static class IACEBIRDSecondaryLineRecordExtensionMethod
	{
		public static void UpdateValue(this IACEBIRDSecondaryLineRecord record, JobComInvoiceLine invoiceLine, ZDecimal value)
		{
			IBIRDLineRecordHelper.UpdateValue(invoiceLine, record.Tariff, value);
		}

		public static void SetTariffRelatedDetails(this IACEBIRDSecondaryLineRecord record, JobComInvoiceLine invoiceLine, ZDecimal qty1, ZString uQ1, ZDecimal qty2, ZString uQ2, ZDecimal qty3, ZString uQ3)
		{
			IBIRDLineRecordHelper.SetTariffRelatedDetails(invoiceLine, record.Tariff, qty1, uQ1, qty2, uQ2, qty3, uQ3);
		}
	}

	static class IBIRDLineRecordHelper
	{
		public static bool IsSupplementaryTariff(ZString tariff)
		{
			return tariff.StartsWith("98") || tariff.StartsWith("99");
		}

		public static void UpdateValue(JobComInvoiceLine invoiceLine, ZString tariff, ZDecimal value)
		{
			if (CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(tariff))
			{
				invoiceLine.US_98GoodsValue += value;
			}
			else
			{
				invoiceLine.JI_LinePrice += value;
			}
		}

		public static void SetTariffs(JobComInvoiceLine invoiceLine, ZString tariff)
		{
			if (!tariff.IsEmpty && invoiceLine != null)
			{
				if (IsSupplementaryTariff(tariff))
				{
					invoiceLine.US_SupTariff = tariff;
				}
				else
				{
					var declaration = invoiceLine.Declaration;
					using (declaration.SuspendDefaultingSecondaryTariffLines())
					{
						invoiceLine.JI_Tariff = tariff;
					}
				}
			}
		}

		public static void SetTariffRelatedDetails(JobComInvoiceLine invoiceLine, ZString tariff, ZDecimal qty1, ZString uQ1, ZDecimal qty2, ZString uQ2, ZDecimal qty3, ZString uQ3)
		{
			SetTariffs(invoiceLine, tariff);

			if (IsSupplementaryTariff(tariff))
			{
				invoiceLine.US_SupUQ1 = uQ1;
				invoiceLine.US_SupQty1 = qty1;
				invoiceLine.US_SupUQ2 = uQ2;
				invoiceLine.US_SupQty2 = qty2;
				invoiceLine.US_SupUQ3 = uQ3;
				invoiceLine.US_SupQty3 = qty3;
			}
			else
			{
				invoiceLine.JI_CustomsUnitQty = uQ1;
				invoiceLine.JI_CustomsQuantity = qty1;
				invoiceLine.JI_CustomsSecondUnitQty = uQ2;
				invoiceLine.JI_CustomsSecondQuantity = qty2;
				invoiceLine.JI_CustomsThirdUnitQty = uQ3;
				invoiceLine.JI_CustomsThirdQuantity = qty3;
			}
		}
	}

	public class BIRDUpdateHeaderHelperTool
	{
		public void UpdateOrWarn(BusinessObject header, string propertyNameInHeader, IZType valueToSet, string valueToAdviseToUsers, string nameOfField, INotifications notifications)
		{
			IZType existingValue = (IZType)header[propertyNameInHeader];

			if (existingValue.IsEmpty)
			{
				header[propertyNameInHeader] = valueToSet;
			}
			else if (existingValue.IsValid && valueToSet.IsValid)
			{
				var valueMatches = existingValue.Equals(valueToSet);
				if (valueToSet.BaseDataType == typeof(string) || valueToSet.BaseDataType == typeof(ZString))
				{
					valueMatches = ((ZString)existingValue).EqualsIgnoringCase((ZString)valueToSet);
				}

				if (!valueMatches)
				{
					notifications.AddWarning(string.Format(Warning, nameOfField, valueToAdviseToUsers));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string Warning = "There is more than one value for {0} in this BIRD transaction and CargoWise One has only one place to store this value. '{1}' cannot be set anywhere.";

		public void UpdateAtFallbacksIfPossible(IEnumerable<ZPropertyInfo> fallbackHeaderInfos, IZType valueToSet, ZPropertyInfo targetInfo)
		{
			ZPropertyInfo previousFallBackInfo = null;

			foreach (ZPropertyInfo fallbackInfo in fallbackHeaderInfos)
			{
				IZType fallBackValue = fallbackInfo.Value;

				if (fallBackValue.IsEmpty || previousFallBackInfo != null && previousFallBackInfo.Value.Equals(fallBackValue))
				{
					fallbackInfo.Value = valueToSet;
					break;
				}

				previousFallBackInfo = fallbackInfo;
			}

			targetInfo.Value = valueToSet;
		}
	}
}
