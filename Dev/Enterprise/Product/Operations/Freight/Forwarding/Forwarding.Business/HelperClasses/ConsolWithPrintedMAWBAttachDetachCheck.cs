namespace Enterprise.Freight.Forwarding.Business
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.Freight.Business;

	public class ConsolWithPrintedMAWBAttachDetachCheck : IShipmentAttachDetachCheck
	{
		public string Check(
			AttachDetachAction userAction,
			IEnumerable<CommonShipment> shipments,
			IEnumerable<CommonConsol> consols,
			BusinessObject parent,
			ShipmentVsConsolMessageHelper.ListFormatterDelegate listFormatter)
		{
			if (!Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed)
			{
				var finalMAWBPrintedConsols = from consol in consols
											  let forwardingConsol = consol as ForwardingConsol
											  where forwardingConsol != null && forwardingConsol.FinalMAWBPrintedDate.IsValid
											  select forwardingConsol;

				finalMAWBPrintedConsols = finalMAWBPrintedConsols.ToList();

				if (finalMAWBPrintedConsols.Any())
				{
					var parentIsCommonShipment = parent is CommonShipment;
					switch (userAction)
					{
						case AttachDetachAction.New:
							return Res.GetString("383637e7-ce5f-4437-926a-f5b51443d496",
								"Cannot add a new shipment to the {1} as the Master Bill for {2} has already been printed on {3}.{0}{0}{4}",
								System.Environment.NewLine,
								listFormatter(finalMAWBPrintedConsols, 1),
								finalMAWBPrintedConsols.First().JK_UniqueConsignRef,
								finalMAWBPrintedConsols.First().FinalMAWBPrintedDate,
								Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster));

						case AttachDetachAction.Attach:
							return parentIsCommonShipment
								? Res.GetString("31efb964-9297-49d8-b31a-690b7dbc865b",
										"Cannot attach the {1} to the {2} as the Master Bill for {3} has already been printed on {4}.{0}{0}{5}",
										System.Environment.NewLine,
										listFormatter(finalMAWBPrintedConsols, 1),
										listFormatter(shipments),
										finalMAWBPrintedConsols.First().JK_UniqueConsignRef,
										finalMAWBPrintedConsols.First().FinalMAWBPrintedDate,
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster))
								: Res.GetString("30e5f93a-14fb-4184-9306-ab82634f971a",
										"Cannot attach a shipment to the {1} as the Master Bill for {2} has already been printed on {3}.{0}{0}{4}",
										System.Environment.NewLine,
										listFormatter(finalMAWBPrintedConsols, 1),
										finalMAWBPrintedConsols.First().JK_UniqueConsignRef,
										finalMAWBPrintedConsols.First().FinalMAWBPrintedDate,
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster));

						case AttachDetachAction.Detach:
							return parentIsCommonShipment
								? Res.GetString("708b67bb-d39a-4254-a033-536971cef2cb",
										"Cannot detach the {1} from the {2} as the Master Bill for {3} has already been printed on {4}.{0}{0}{5}",
										System.Environment.NewLine,
										listFormatter(finalMAWBPrintedConsols, 1),
										listFormatter(shipments),
										finalMAWBPrintedConsols.First().JK_UniqueConsignRef,
										finalMAWBPrintedConsols.First().FinalMAWBPrintedDate,
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster))
								: Res.GetString("88884983-96b8-40cc-ab60-b102b4b95877",
										"Cannot detach the {1} from the {2} as the Master Bill for {3} has already been printed on {4}.{0}{0}{5}",
										System.Environment.NewLine,
										listFormatter(shipments),
										listFormatter(finalMAWBPrintedConsols, 1),
										finalMAWBPrintedConsols.First().JK_UniqueConsignRef,
										finalMAWBPrintedConsols.First().FinalMAWBPrintedDate,
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster));

						default:
							throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not supported.", userAction));
					}
				}
			}

			return string.Empty;
		}
	}
}
