using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.Business
{
	public class ConvertToStandAloneDeclarationService : IConvertToStandAloneDeclarationService
	{
		public IConvertToStandAloneDeclarationResponse ConvertToStandAloneDeclaration(Guid consignmentPK, BusinessObjectFactory factory)
		{
			var consignment = factory.Load<HVLVConsignment>(consignmentPK);
			if (consignment != null)
			{
				return ConvertToStandAloneDeclaration(consignment);
			}
			else
			{
				return new ConvertToStandAloneDeclarationConsignmentNotFoundResponse();
			}
		}

		public IConvertToStandAloneDeclarationResponse ConvertToStandAloneDeclaration(IHVLVConsignment consignment)
		{
			return ConvertToStandAloneDeclaration(consignment, Enumerable.Empty<IHVLVConsignment>());
		}

		public IConvertToStandAloneDeclarationResponse ConvertToStandAloneDeclaration(IHVLVConsignment consignment, IEnumerable<IHVLVConsignment> consignmentsToMerge)
		{
			if (consignment is HVLVConsignment c)
			{
				return ConvertToStandAloneDeclarationCore(c, consignmentsToMerge.OfType<HVLVConsignment>());
			}
			else
			{
				return new ConvertToStandAloneDeclarationConsignmentNotFoundResponse();
			}
		}

		static List<string> ConvertToStandAloneDeclarationSupportedCountries => new List<string>
		{
			CountryCodes.Australia,
			CountryCodes.Canada,
			CountryCodes.NewZealand,
			CountryCodes.UnitedStates,
			CountryCodes.Singapore,
			CountryCodes.Taiwan,
			CountryCodes.Turkey,
			CountryCodes.SouthAfrica,
			CountryCodes.Belgium,
			CountryCodes.Switzerland,
			CountryCodes.Germany,
			CountryCodes.Spain,
			CountryCodes.France,
			CountryCodes.UnitedKingdom,
			CountryCodes.Ireland,
			CountryCodes.Italy,
			CountryCodes.Netherlands,
			CountryCodes.Sweden,
			CountryCodes.Brazil,
			CountryCodes.China,
			CountryCodes.Poland,
			CountryCodes.UnitedArabEmirates
		};

		IConvertToStandAloneDeclarationResponse ConvertToStandAloneDeclarationCore(HVLVConsignment consignment, IEnumerable<HVLVConsignment> consignmentsToMerge)
		{
			var response = default(IConvertToStandAloneDeclarationResponse);
			var declaration = default(BaseJobDeclaration);
			var forwardingShipment = consignment.ManifestedOnShipment;

			if (forwardingShipment == null)
			{
				response = new ConvertToStandAloneDeclarationConsignmentNotAttachedToShipmentResponse();
			}
			else
			{
				if (forwardingShipment.Consols.Count == 0)
				{
					response = new ConvertToStandAloneDeclarationConsignmentMissingTransportDetailsResponse();
				}
				else if (consignment.IsImport && consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.Cleared)
				{
					response = new ConvertToStandAloneDeclarationConsignmentHasClearedImportCustomsStatusResponse();
				}
				else if (consignment.IsExport && consignment.HVC_ExportReleaseStatus == HVLVReleaseStatus.Cleared)
				{
					response = new ConvertToStandAloneDeclarationConsignmentHasClearedExportCustomsStatusResponse();
				}
				else if (!ConvertToStandAloneDeclarationSupportedForDestinationCountry(consignment.ManifestedOnShipment?.Destination?.Country?.Code))
				{
					response = new ConvertToStandAloneDeclarationShipmentDestinationNotSupportedResponse();
				}
				else if (consignment.HasDeclarationForCurrentDirection)
				{
					response = new ConvertToStandAloneDeclarationConsignmentHasExistingDeclarationResponse();
				}
				else
				{
					var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, consignment));
					var dataExportStrategy = ObjectFactory.Get<IHVLVConsignmentDataExportStrategy>("HVLVConsignmentToDeclarationDataExportStrategy");
					var consignmentWriter = ObjectFactory.Get<IHVLVConsignmentDataObjectWriter>("HVLVConsignmentDataObjectWriter", writeManager, dataExportStrategy);
					consignmentWriter.SetConsignmentsToMerge(consignmentsToMerge);

					try
					{
						if (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUStandAloneDeclaration.Value)
						{
							forwardingShipment.Factory.ServiceContainer.AddService(new NonWesternEuropeanCharactersRemovalService());
						}

						var writer = consignmentWriter as ITopLevelDataObjectWriter;
						var dataObject = writer.GetDataObject(consignment);
						var declarationContextManager = DataContextType.CustomsDeclaration.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal;
						var universalFactory = new UniversalObjectFactory(consignment.Factory);
						var logger = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
						var declarationReader = declarationContextManager.GetShipmentDataObjectReader(dataObject, logger, universalFactory);
						declaration = declarationReader.ReadIntoTopLevelBusinessObject() as BaseJobDeclaration;

						if (declaration != null)
						{
							AddSaveHooksForDeclarationFactory();

							using (consignment.ManagingShipment?.SuspendSettingHasChangesIncludingChildren())
							{
								response = new ConvertToStandAloneDeclarationSuccessfulResponse(declaration.PK.ToGuid());

								if (Completed != null)
								{
									var cancelEventArgs = new CancelEventArgs();
									Completed.Invoke(declaration, cancelEventArgs);
									if (cancelEventArgs.Cancel)
									{
										if (!declaration.IsInDatabase)
										{
											declaration.Delete();
										}
										DeleteSavingHooks(declaration.Factory);

										response =  null;
									}
								}
							}
						}
						else
						{
							response = new ConvertToStandAloneDeclarationUnexpectedErrorResponse(logger.ToString());
						}
					}
					finally
					{
						forwardingShipment.Factory.ServiceContainer.RemoveService<NonWesternEuropeanCharactersRemovalService>();
					}
				}
			}

			return response;

			void AddSaveHooksForDeclarationFactory()
			{
				var declarationFactory = declaration.Factory;

				declarationFactory.Saving -= SavingHooks;
				declarationFactory.Saving += SavingHooks;
				declarationFactory.Saved -= DeleteSavingHooksAfterSaved;
				declarationFactory.Saved += DeleteSavingHooksAfterSaved;
			}

			void DeleteSavingHooksAfterSaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					DeleteSavingHooks(factory);
				}
			}

			void SavingHooks(BusinessObjectFactory factory)
			{
				AddTransferredLog();
				UpdateConsignments();
			}

			void AddTransferredLog()
			{
				var eventParameters = new[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, consignment.HVC_ConsignmentId),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, GlobalModuleNamesConstants.HVLV)
				};
				declaration.Logs.CreateOrRecreateEventLog(AutoEvents.Transferred, EstimateActual.Actual, ZDateTimeOffset.UtcNow, ZString.Empty, eventParameters.ToArray());
			}

			void UpdateConsignments()
			{
				UpdateConsignment(consignment, declaration.PK);
				consignmentsToMerge?.ForEach(x => UpdateConsignment(x, declaration.PK));

				AddMasterIssuerSCACCodeIfNeeded(consignment, declaration);
			}

			void UpdateConsignment(HVLVConsignment consignment, ZGuid declarationPK)
			{
				if (consignment.IsImport)
				{
					consignment.HVC_ImportCustomsClearanceStatus = string.Empty;
					consignment.HVC_JE_ImportDeclaration = declarationPK;
					consignment.SetLastUsageCodeForAllItems(UsageCodes.ImportStandAloneDeclaration);
				}
				else
				{
					consignment.HVC_ExportCustomsClearanceStatus = string.Empty;
					consignment.HVC_JE_ExportDeclaration = declarationPK;
					consignment.SetLastUsageCodeForAllItems(UsageCodes.ExportStandAloneDeclaration);
				}
			}

			void DeleteSavingHooks(BusinessObjectFactory factory)
			{
				factory.Saving -= SavingHooks;
				factory.Saved -= DeleteSavingHooksAfterSaved;
			}
		}

		public static bool ConvertToStandAloneDeclarationSupportedForDestinationCountry(string countryCode) => ConvertToStandAloneDeclarationSupportedCountries.Contains(countryCode);

		void AddMasterIssuerSCACCodeIfNeeded(HVLVConsignment consignment, BaseJobDeclaration declaration)
		{
			var shipment = consignment.ManifestedOnShipment;

			if (shipment.TransportMode == TransportModes.Sea && shipment.Destination != null && shipment.Destination.Country.Code == CountryCodes.UnitedStates)
			{
				var consolBO = shipment.ArrivalConsol;
				var scacCode = consolBO?.ShippingLine?.SCACCode;
				var usDeclaration = declaration as Customs.US.Business.JobDeclaration;

				if (scacCode.HasValue && usDeclaration != null)
				{
					usDeclaration.JE_MasterBillIssuerSCAC = scacCode.Value;
				}
			}
		}

		public event CancelEventHandler Completed;
	}
}
