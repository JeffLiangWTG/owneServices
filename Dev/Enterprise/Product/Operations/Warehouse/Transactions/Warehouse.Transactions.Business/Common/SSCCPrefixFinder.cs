using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class SSCCPrefixFinder : ISSCCPrefixFinder
	{
		public ZString GetSSCCPrefix(SSCCGenerationContext context, Func<OrgHeader> getClient, Func<WhsWarehouse> getWarehouse, INotifications notify, bool shouldPrompt)
		{
			Argument.NotNull(notify, nameof(notify));
			Argument.NotNull(getClient, nameof(getClient));
			Argument.NotNull(getWarehouse, nameof(getWarehouse));

			ZString result;

			if ((context == SSCCGenerationContext.GeneratingIDsViaUser || context == SSCCGenerationContext.ScanPacking)
				&& SSCCPrefix.HasValue)
			{
				result = SSCCPrefix.Value;
			}
			else
			{
				var client = getClient();
				var customCodes = (OrgCusCode[])client.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.GS1));
				if (customCodes.Length > 0)
				{
					result = customCodes[0].OK_CustomsRegNo;
				}
				else
				{
					bool cachePrefix;
					(cachePrefix, result) = GetSSCCPrefixFromWarehouse(notify, getWarehouse, context, client, shouldPrompt);

					if (cachePrefix)
					{
						SSCCPrefix = result;
					}
				}
			}

			return result;
		}

		(bool CachePrefix, ZString Prefix) GetSSCCPrefixFromWarehouse(INotifications notify, Func<WhsWarehouse> getWarehouse, SSCCGenerationContext context, OrgHeader client, bool shouldPrompt)
		{
			(bool CachePrefix, ZString Prefix) result;

			const bool CachePrefix = true;

			var warehouse = getWarehouse();
			var ssccPrefix = warehouse?.GetSSCCPrefix() ?? ZString.Empty;
			if (!ssccPrefix.IsEmpty)
			{
				// always fallback to warehouse if option is ticked
				if (warehouse.WW_UseGS1PrefixFallback
					// it's ok to automatically fallback to the Warehouse Prefix to simply check if a barcode is SSCC
					|| context == SSCCGenerationContext.CheckIfBarcodeIsSSCC)
				{
					result = (!CachePrefix, ssccPrefix);
				}
				// ScanPacking, ClosingPackage & GeneratingIDsOnSave all cannot prompt the user for a question.
				// Only a user initiated Action will prompt the user.
				else if (context == SSCCGenerationContext.GeneratingIDsViaUser)
				{
					if (shouldPrompt)
					{
						var usePrefix = GetUserResponse(notify, client);

						// we don't want to ask a second time.
						result = (CachePrefix, usePrefix ? ssccPrefix : ZString.Empty);
					}
					else
					{
						result = default;
					}
				}
				// Scan Packing can cache the result of no question asked, therefore no fallback
				else if (context == SSCCGenerationContext.ScanPacking)
				{
					result = (CachePrefix, "");
				}
				// don't fallback, we don't know what the user wants, safer not to use the Warehouse's prefix
				else if (context == SSCCGenerationContext.AutoClosingPackage || context == SSCCGenerationContext.GeneratingIDsOnSave)
				{
					result = default;
				}
				else
				{
					throw new ArgumentException($"Unhandled SSCC Context: {context}.");
				}
			}
			else
			{
				result = default;
			}

			return result;
		}

		static bool GetUserResponse(INotifications notify, OrgHeader client)
		{
			var message = Res.GetString("d7afeaf9-2855-4d54-9607-5ae178cb1468", "Client {0} does not have a GS1 company prefix, would you like to use the warehouse GS1 company prefix?", client.OH_FullNameTruncated);
			var caption = Res.GetString("27d5e318-00a4-43b7-adb2-be454a1847db", "Generate SSCC Number");
			var e = new QueryUserYesNoEventArgs(caption, message, defaultResponse: false);

			notify.QueryUser(e);

			return e.Response;
		}

		ZString? SSCCPrefix { get; set; }
	}
}
