using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class RelatedPartiesHelper
	{
		public static (ZBool WasSuccessful, ZString Log) RecalculateRelatedParties(ForwardingConsol consol)
		{
			if (!consol.IsExport() && !consol.IsImport())
			{
				return (false,
					Res.GetString("f7b384b5-1b7c-4359-a793-6e5e8d4be66a",
						"You cannot Recalculate Related Parties for this company because the company does not match Pickup or Delivery direction of this job."));
			}

			consol.RecalculateExportParties();
			consol.RecalculateImportParties();

			return (true, ZString.Empty);
		}

		public static ZString TryRedefaultCreditor(ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));

			consol.RedefaultCreditor();

			foreach (var transport in consol.Transports.Cast<Transport>())
			{
				transport.RedefaultCreditor(shouldOverrideExistingCreditor: true);
			}

			var isExport = consol.IsExport();
			if (consol.Containers.Count > 0 && (isExport || consol.IsImport()))
			{
				var penalties = isExport
					? consol.Containers.Cast<ForwardingContainer>().SelectMany(x => x.ExportPenalties)
					: consol.Containers.Cast<ForwardingContainer>().SelectMany(x => x.ImportPenalties);

				foreach (var penalty in penalties)
				{
					penalty.DefaultCreditor();
				}
			}

			return GetReasonsIfRelatedPartiesCannotBeDefaulted(consol);
		}

		static ZString GetReasonsIfRelatedPartiesCannotBeDefaulted(ForwardingConsol consol)
		{
			var reasons = new ZStringBuilder();
			var direction = consol.JobDirection;
			var orgCode = string.Empty;

			if (consol.IsCoLoad)
			{
				if ((direction == Directions.Export && consol.CarrierExportCreditorAddress.IsEmpty) || (direction == Directions.Import && consol.CarrierImportCreditorAddress.IsEmpty))
				{
					if (consol.JK_OA_CreditorAddress.IsEmpty)
					{
						reasons.Append(Res.GetString("0B5D2E0E-E253-4028-A887-5D9A678B9A8E", "• Co-Load's address is not valid."));
					}
					else
					{
						orgCode = consol.Creditor.OH_Code;
					}
				}
			}
			else if (consol.JK_OA_CreditorAddress.IsEmpty)
			{
				if (consol.JK_OA_ShippingLineAddress.IsEmpty)
				{
					reasons.Append(Res.GetString("802EEB1A-B809-4338-9AA0-67B7C11A2BF8", "• Shipping Line's address is not valid."));
				}
				else
				{
					orgCode = consol.ShippingLine.OH_Code;
				}
			}

			if (!string.IsNullOrWhiteSpace(orgCode) && (direction == Directions.Export || direction == Directions.Import))
			{
				reasons.Append(Res.GetString("A6C1359F-2F06-4F62-9546-4D3E58ECE3D2",
					"• Unable to default {0} Creditor Address of Organization <{1}> as no matching Related Parties with type of 'SPC' found or the Related Party is NOT an AP Organization.", direction.ToString(), orgCode));
			}

			return reasons.ToStringWithNewLineBetweenAppends();
		}
	}
}
