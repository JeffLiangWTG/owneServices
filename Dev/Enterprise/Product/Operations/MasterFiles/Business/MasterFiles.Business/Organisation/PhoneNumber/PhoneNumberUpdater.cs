using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneNumberUpdater : IPhoneNumberUpdater
	{
		#region Constructor

		public PhoneNumberUpdater()
		{
			Factory = new BusinessObjectFactory { NameForDebugging = typeof(PhoneNumberUpdater).FullName };
		}

		#endregion

		#region Interface Implementation

		PhoneNumberUpdateResult[] IPhoneNumberUpdater.Normalize(ILogger logger, ZString phoneNumberColumnName, Tuple<ZGuid, ZString>[] pkAndDefaultCountryCodePairs)
		{
			Argument.NotNullOrEmpty(phoneNumberColumnName, "phoneNumberColumnName");
			Argument.NotNull(pkAndDefaultCountryCodePairs, "pkAndDefaultCountryCodePairs");

			PhoneNumberUpdateResult[] results = null;

			var columnNamePrefix = Schema.GetPrefixFromColumnName(phoneNumberColumnName);
			var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(columnNamePrefix);

			if (tableSchema != null)
			{
				var schemaColumn = tableSchema.GetSchemaColumn(phoneNumberColumnName);

				if (schemaColumn != null && schemaColumn.GetEquivalentZType() == typeof(ZString))
				{
					results = pkAndDefaultCountryCodePairs
						.Select(pkAndDefaultCountryCodePair => NormalizeCore(columnNamePrefix, schemaColumn, pkAndDefaultCountryCodePair.Item1, pkAndDefaultCountryCodePair.Item2))
						.Where(result => result != null)
						.ToArray();
					try
					{
						Factory.Save();
					}
					catch (ZSaveException saveException)
					{
						if (logger != null)
						{
							logger.Log(LogType.Error, saveException.Message);
						}
						results = null;
					}
				}
			}

			return results;
		}

		#endregion

		#region Implementations

		BusinessObjectFactory Factory { get; set; }

		PhoneNumberUpdateResult NormalizeCore(ZString phoneNumberColumnNamePrefix, SchemaColumn phoneNumberSchemaColumn, ZGuid rowPk, ZString defaultCountryCode)
		{
			var phoneNumberFormatter = new PhoneNumberFormatter();
			var businessObject = Factory.Load(phoneNumberColumnNamePrefix, rowPk);
			PhoneNumberUpdateResult result = null;
			if (businessObject != null)
			{
				var phoneNumber = (ZString)businessObject[phoneNumberSchemaColumn];
				var normalizedPhoneNumber = (ZString)phoneNumberFormatter.FormatE164(phoneNumber, defaultCountryCode);
				if (!normalizedPhoneNumber.IsEmpty)
				{
					businessObject[phoneNumberSchemaColumn] = normalizedPhoneNumber;
					result = PhoneNumberUpdateResult.CreateSuccess(rowPk, phoneNumber, normalizedPhoneNumber);
				}
				else
				{
					result = PhoneNumberUpdateResult.CreateFailure(rowPk, phoneNumber);
				}
			}
			return result;
		}

		#endregion
	}
}