using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class UNDGThresholdLimitNotificationProcessor : IUNDGThresholdLimitNotificationProcessor
	{
		public UNDGThresholdLimitNotificationProcessor(IWhsUNDGLimitValidationHelperFactory whsUNDGLimitValidationHelperFactory)
		{
			WhsUNDGLimitValidationHelperFactory = Argument.NotNull(whsUNDGLimitValidationHelperFactory, nameof(whsUNDGLimitValidationHelperFactory));
		}

		IWhsUNDGLimitValidationHelperFactory WhsUNDGLimitValidationHelperFactory { get; }

		public void NotifyWarehouseManagersOfExceededUNDGLimits(CancellationToken token, WhsWarehouse warehouse, ILogger logger)
		{
			logger.Information(Res.GetString("60b77536-6697-40f9-b743-91626ed33c2c", "Checking DG Limit Thresholds for Warehouse {0}.", warehouse.WW_WarehouseNameMultilingual));

			var contactEmail = warehouse.DGContact?.OC_Email;
			if (string.IsNullOrEmpty(contactEmail))
			{
				logger.Information(Res.GetString("a259a8e6-50ce-40cd-9573-f6ccfe6385f5", "DG Contact with valid email address does not exist for Warehouse {0}. Skipping DG Limit Thresholds check.", warehouse.WW_WarehouseNameMultilingual));
			}
			else
			{
				using (WarehouseUserContextHelper.SetUserContextForWarehouse(warehouse))
				{
					var helper = WhsUNDGLimitValidationHelperFactory.GetWhsUNDGLimitValidationHelper(warehouse.WW_WarehouseType);
					NotifyWarehouseManagerIfDGLimitExceeded(warehouse, contactEmail.Value, helper, token, logger);
				}
			}
		}

		void NotifyWarehouseManagerIfDGLimitExceeded(WhsWarehouse warehouse, ZString contactEmail, IWhsUNDGLimitValidationHelper validationHelper, CancellationToken token, ILogger logger)
		{
			var errors = new StringBuilder();
			var undgLimits = FetchUNDGLimits(warehouse);

			foreach (var limit in undgLimits)
			{
				token.ThrowIfCancellationRequested();

				var limitErrors = validationHelper.GetWhsUNDGLimitValidationMessage(limit);
				if (!limitErrors.IsEmpty)
				{
					errors.AppendLine(limitErrors);
				}
			}

			var errorString = errors.ToString();
			if (errorString.Length > 0)
			{
				logger.Information(Res.GetString("eb889ecd-8d73-4b8e-abb8-e52c5895a9b0", "DG Limit Thresholds were exceeded. Queueing email to DG Contact.", warehouse.WW_WarehouseNameMultilingual));
				SendEmailForExceededLimits(warehouse, contactEmail, errorString);
			}
		}

		IReadOnlyCollection<WhsUNDGLimit> FetchUNDGLimits(WhsWarehouse warehouse)
		{
			var factory = warehouse.Factory;
			var undgLimits = warehouse.UNDGLimits;

			var substancePKs = undgLimits.Select(ul => ul.WWD_DG).Where(fk => !fk.IsEmpty).Distinct().ToArray();
			factory.AddFetchHint(typeof(UNDGSubstance), new ZQuery(UNDGSubstanceSchema.PK, substancePKs));

			var countryRefPKs = undgLimits.Select(ul => ul.WWD_DCR_UNDGCountryReference).Where(fk => !fk.IsEmpty).Distinct().ToArray();
			factory.AddFetchHint(typeof(UNDGCountryReference), new ZQuery(UNDGCountryReferenceSchema.PK, countryRefPKs));

			return undgLimits.Where(x => !x.WWD_DG.IsEmpty).OrderBy(x => x.UNDGSubstance.DG_Code)
				.Union(undgLimits.Where(x => !x.WWD_DCR_UNDGCountryReference.IsEmpty).OrderBy(x => x.CountryReference.DCR_Code))
				.Union(undgLimits.Where(x => !x.WWD_UNDGClass.IsEmpty).OrderBy(x => x.WWD_UNDGClass))
				.ToArray();
		}

		static void SendEmailForExceededLimits(WhsWarehouse warehouse, ZString contactEmail, string errorString)
		{
			var email = new EmailDef();
			email.AddRecipientForSystemCommunication(contactEmail);
			if (email.Recipients.Count > 0)
			{
				email.Subject = Res.GetString("43e1bbb0-1947-4475-a65b-b5583522b4b7", "DG Limit Thresholds Exceeded For Warehouse {0}", warehouse.WW_WarehouseNameMultilingual);

				var stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(Res.GetString("46e2f15a-5c1d-45fd-af05-aeccaf164c14", "The following DG Limit Thresholds have been exceeded in Warehouse {0}:", warehouse.WW_WarehouseNameMultilingual));
				stringBuilder.AppendLine(errorString);
				stringBuilder.AppendLine(Res.GetString("234e1d48-ee42-40b3-95a3-091209385b79", "No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%."));

				email.Body = stringBuilder.ToString();
				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}
	}
}
