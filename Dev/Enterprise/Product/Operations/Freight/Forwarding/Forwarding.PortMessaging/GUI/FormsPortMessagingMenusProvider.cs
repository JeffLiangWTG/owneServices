using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	sealed class FormsPortMessagingMenusProvider : IPortMessagingMenusProvider
	{
		public FormsPortMessagingMenusProvider(IBusiness hostEntity)
		{
			this.hostEntity = hostEntity;
		}

		readonly IBusiness hostEntity;

		ForwardingConsol Consol => hostEntity as ForwardingConsol;

		public bool Enabled => Consol != null && (Consol.IsSea || Consol.IsAir);

		public void AddMenuItems(ZMenuItem parentMenu)
		{
			if (Consol != null)
			{
				parentMenu.AddFormsMenuItems(Consol, ModuleIDs.JobConsol, CreatePortMessagingMenuItemInfos());
			}
		}

		IEnumerable<IMenuItemInfo> CreatePortMessagingMenuItemInfos()
		{
			return new IMenuItemInfo[]
			{
				CreateImportMenuItemInfo(),
				CreateExportMenuItemInfo(),
				CreatePortbaseMenuItemInfo(),
				CreateCargoDuesMenuItemInfo(),
				CreateServiceInstructionMenuItemInfo(),
				new SystemMenuItemInfo
				{
					ID = ConsolSystemFormMenuItems.DocumentMenuETerminalReleaseManifestCNPK
				},
				new SystemMenuItemInfo
				{
					ID = ConsolSystemFormMenuItems.DocumentMenuContainerLoadPlanCNPK
				}
			};
		}

		ParentMenuItemInfo CreatePortbaseMenuItemInfo()
		{
			return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("6aaad96a-275e-4c93-a710-1d2f77e34cbb", "Portbase (NL)"),
				SubMenus = new[]
				{
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuExportNotificationPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuImportNotificationPK
					}
				}
			};
		}

		ParentMenuItemInfo CreateImportMenuItemInfo()
		{
			return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("0c89e26b-5d35-4815-b88c-1e2f40109028", "Import"),
				SubMenus = new IMenuItemInfo[]
				{
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuAufTragDEAdvancedLogisticsPortOrderImportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuIFTDGNBEDangerousGoodsNotificationImportPK
					},
					new ParentMenuItemInfo
					{
						Name = ResString.GetMultilingualString("495e5669-a9b6-49e1-9a95-df4dcddf6a9c", "Certified Pick up (BE)"),
						SubMenus = new IMenuItemInfo[]
						{
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCertifiedPickupBEAcceptDeclinePK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCertifiedPickupBETransferPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCertifiedPickupBERevokePK
							},
						}
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuDTIFRPortMessagingImportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuImportManifestLPDPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuOutturnReportCDMPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuDOSRequestFRPortMessagingImportPK
					},
					new CustomMenuItemInfo
					{
						Name = TracingRequestMenuName,
						OnClick = (bizObj) => SendExportTracingRequest(bizObj, DemandeDeTracingDirection.Import),
						IsApplicable = (bizObj) => IsTracingRequestApplicable(bizObj, DemandeDeTracingDirection.Import)
					},
					new ParentMenuItemInfo
					{
						Name = ResString.GetMultilingualString("372C9EFE-3702-4E49-BEA4-A919F5A6C7EB", "Secure Cont. Release"),
						SubMenus = new IMenuItemInfo[]
						{
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuTMiningSecureContainerReleaseTransfer
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuTMiningSecureContainerReleaseRevoke
							}
						}
					}
				}
			};
		}

		ParentMenuItemInfo CreateExportMenuItemInfo()
		{
			return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("695ea24e-bc34-46fe-a390-0ddab6c27bf8", "Export"),
				SubMenus = new IMenuItemInfo[]
				{
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuAufTragDEAdvancedLogisticsPortOrderExportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuEBADECBEExportNotificationPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuIFTDGNBEDangerousGoodsNotificationExportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuFCLBookingAMQPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuDTEFRPortMessagingExportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuLDEFRPortMessagingExportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuDOSRequestFRPortMessagingExportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuExportNotificationCargonautNLPK
					},
					new CustomMenuItemInfo
					{
						Name = TracingRequestMenuName,
						OnClick = (bizObj) => SendExportTracingRequest(bizObj, DemandeDeTracingDirection.Export),
						IsApplicable = (bizObj) => IsTracingRequestApplicable(bizObj, DemandeDeTracingDirection.Export)
					},
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.DocumentMenuXFZBRequestMXPortMessagingExportPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuExportPreAdviceNotificationPK
					}
				}
			};
		}

		string TracingRequestMenuName => tracingRequestMenuName ?? (tracingRequestMenuName = Res.GetString("10816326-c061-428a-b50c-3f3d8b2143f9", "Tracing Request (TRC) (FR)"));
		string tracingRequestMenuName;

		void SendExportTracingRequest(BusinessObject bizObj, DemandeDeTracingDirection direction)
		{
			if (bizObj is ForwardingConsol consol)
			{
				if (consol.Containers.Count == 0)
				{
					Globals.Message.Show(Res.GetString("8f3600bb-595b-4e7f-9368-ebd10f95fce9", "Tracing Request (TRC) could not be send because this consol has no containers."));
					return;
				}

				if (consol.Containers.Count == 1)
				{
					var confirmationDialogResult = Globals.Message.Show(Res.GetString("4feb08b1-aaae-4274-88ea-27834053c1a3", "Are you sure you want to send the TRC container tracing message to the port system?"), Res.GetString("a67eb89f-6b81-4ada-8266-064f3aa2c6da", "Tracing Request (TRC)"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (confirmationDialogResult == DialogResult.No)
					{
						return;
					}
				}

				var res = DemandeDeTracingMessageSender.SendMessage(new ForwardingConsolTRCDetailsProvider(consol), direction, out var notifications);

				var message = string.Empty;

				if (res)
				{
					message = Res.GetString("1c72d736-2bdb-4e92-9593-bb6b4096d346", "Tracing Request (TRC) has been sent.");
				}
				else if (notifications.Count > 0)
				{
					message = string.Join(System.Environment.NewLine, notifications.Select(n => n.Message));
				}

				if (!string.IsNullOrWhiteSpace(message))
				{
					var caption = Res.GetString("6fd9ddbf-2eb7-49d3-9062-672155969227", "Information");
					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		bool IsTracingRequestApplicable(BusinessObject bizObj, DemandeDeTracingDirection direction)
		{
			if (bizObj is ForwardingConsol consol)
			{
				var currentCountry = (RefCountry)Env.CurrentCompany.Country;
				return DemandeDeTracingHelper.IsTracingRequestApplicable(consol, direction)
					&& (currentCountry.IsFranceOrTerritory || currentCountry.IsPartOfEuropeanUnion || currentCountry.IsInEFTA);
			}

			return false;
		}

		ParentMenuItemInfo CreateCargoDuesMenuItemInfo()
		{
			return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("e64b3594-630b-4ad9-bdc6-f388993f16fe", "Cargo Dues (ZA)"),
				SubMenus = new[]
				{
					new ParentMenuItemInfo
					{
						Name = ResString.GetMultilingualString("f32f9f74-1a06-438b-a498-207e97a228e3", "Cargo Dues Order"),
						SubMenus = new[]
						{
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesOrder2ImportZAPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesOrder2ExportZAPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesOrder2LoadCoastwiseZAPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesOrder2DischargeCoastwiseZAPK
							}
						}
					},
					new ParentMenuItemInfo
					{
						Name = ResString.GetMultilingualString("ff2ba58c-5954-4632-abb3-8692663d504a", "Cargo Dues Quotation"),
						SubMenus = new[]
						{
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesQuotation2ImportZAPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesQuotation2ExportZAPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesQuotation2LoadCoastwiseZAPK
							},
							new SystemMenuItemInfo
							{
								ID = ConsolSystemFormMenuItems.DocumentMenuCargoDuesQuotation2DischargeCoastwiseZAPK
							}
						}
					}
				}
			};
		}

		ParentMenuItemInfo CreateServiceInstructionMenuItemInfo()
		{
			return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("EA920CA1-807D-43EB-B1F6-17F006C00355", "Service Instruction (ZA)"),
				SubMenus = new[]
				{
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuTPTShippingOrderExportsZAPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuTPTLandingOrderImportsZAPK
					}
				},
				OnPopup = menu =>
				{
					if (Consol.TransportMode == Core.Constants.TransportModes.Sea
					&& Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.SouthAfrica
					&& (Consol.JK_ConsolMode == Core.Constants.ContainerModes.BreakBulk || Consol.JK_ConsolMode == Core.Constants.ContainerModes.RollOnRollOff))
					{
						CreateTransShipOrderMenuItems(menu);
					}

					menu.Visible = menu.MenuItems.Count > 0;
				}
			};
		}

		void CreateTransShipOrderMenuItems(ZMenuItem parentMenuItem)
		{
			const string transShipOrderMenuTag = "transShipOrder";

			var existingTransShipmentMenuItems = parentMenuItem
				.MenuItems
				.OfType<ZMenuItem>()
				.Where(m => string.CompareOrdinal(Convert.ToString(m.Tag, CultureInfo.InvariantCulture), transShipOrderMenuTag) == 0)
				.ToArray();

			foreach (var menuItem in existingTransShipmentMenuItems)
			{
				parentMenuItem.MenuItems.Remove(menuItem);
			}

			var provider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();

			foreach (var portCode in GetTPTTranshpTransportPorts())
			{
				var caption = Res.GetString("1E34ED2F-B525-4BD6-9060-0C74FA8EBF0E", "Transhipment Order ({0})", portCode);

				var tptMenuItem = new TPTTransShipOrderMenuItem(Consol.Factory, portCode, caption);

				EventHandler clickHandler = (s, e) =>
				{
					var command = provider?.GetCommand(Consol, tptMenuItem, ModuleIDs.JobConsol);
					command?.Execute();
				};

				var menuItem = new ZMenuItem(caption, clickHandler);
				menuItem.Tag = transShipOrderMenuTag;

				parentMenuItem.MenuItems.Add(menuItem);
			}
		}

		IEnumerable<ZString> GetTPTTranshpTransportPorts()
		{
			var dischargePort = ZString.Empty;

			foreach (var transport in Consol.Transports.OfType<Transport>().OrderBy(t => t.JW_LegOrder))
			{
				if (!transport.IsSea)
				{
					continue;
				}

				if (dischargePort == ZString.Empty)
				{
					dischargePort = transport.JW_RL_NKDiscPort;
					continue;
				}

				if (transport.JW_RL_NKLoadPort == dischargePort
					&& transport.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.SouthAfrica, StringComparison.InvariantCultureIgnoreCase))
				{
					yield return dischargePort;
				}

				dischargePort = transport.JW_RL_NKDiscPort;
			}
		}
	}
}
