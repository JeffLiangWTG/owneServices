using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public static class AddInfoGroupOrganizationAddressCollectionUpdator
	{
		internal static void Update(IOrganizationAddressCollectionParent parent, Customs.Business.MultiLineAddInfos.CusAddInfo cusAddInfo, IDataWritingManager writeManager)
		{
			var fda = cusAddInfo as FDA;
			if (fda != null)
			{
				parent.AddOrgAddress(writeManager, fda.FDAFEIAddress, Constants.AddressType.FDAEstablishmentIdentifier);
				parent.AddOrgAddress(writeManager, fda.ManufacturerAddress, nameof(DocAddressType.Manufacturer));
				parent.AddOrgAddress(writeManager, fda.ShipperAddress, Constants.AddressType.FDAShipper);
			}
			else
			{
				var amsLine = cusAddInfo as AMSLine;
				if (amsLine != null)
				{
					parent.AddOrgAddress(writeManager, amsLine.ApplicantAddress, Constants.AddressType.Applicant);
					parent.AddOrgAddress(writeManager, amsLine.GoodsLocationAddress, nameof(DocAddressType.Location));
				}
				else
				{
					var aPHISHeader = cusAddInfo as APHISHeader;
					if (aPHISHeader != null)
					{
						parent.AddOrgAddress(writeManager, aPHISHeader.ApplicantAddress, Constants.AddressType.Applicant);
						parent.AddOrgAddress(writeManager, aPHISHeader.CropGrowerAddress, Constants.AddressType.Grower);
						parent.AddOrgAddress(writeManager, aPHISHeader.ShipperAddress, Constants.AddressType.Shipper);
						parent.AddOrgAddress(writeManager, aPHISHeader.PermittedAddress, Constants.AddressType.Permitted);
					}
					else
					{
						var aceFDA = cusAddInfo as ACEFDA;
						if (aceFDA != null)
						{
							parent.AddOrgAddress(writeManager, aceFDA.DeliverToPartyAddress, Constants.AddressType.DeliverToPartyAddress);
							parent.AddOrgAddress(writeManager, aceFDA.OwnerAddress, Constants.AddressType.Owner);
							parent.AddOrgAddress(writeManager, aceFDA.FDAImporterAddress, Constants.AddressType.FDAImporter);
							parent.AddOrgAddress(writeManager, aceFDA.ShipperAddress, Constants.AddressType.FDAShipper);
							parent.AddOrgAddress(writeManager, aceFDA.ProducerAddress, Constants.AddressType.Producer);
							parent.AddOrgAddress(writeManager, aceFDA.LocationOfGoodsAddress, nameof(DocAddressType.Location));
							parent.AddOrgAddress(writeManager, aceFDA.ManufacturerAddress, nameof(DocAddressType.Manufacturer));
							parent.AddOrgAddress(writeManager, aceFDA.FSVPImporterAddress, Constants.AddressType.FSVPImporter);
						}
						else
						{
							var nhtsa = cusAddInfo as NHTSAHeader;
							if (nhtsa != null)
							{
								parent.AddOrgAddress(writeManager, nhtsa.OwnerAddress, Constants.AddressType.Owner);
								parent.AddOrgAddress(writeManager, nhtsa.RetailerDistributorAddress, nameof(DocAddressType.DistributionCentreAddress));
								parent.AddOrgAddress(writeManager, nhtsa.FabricatingManufacturerAddress, nameof(DocAddressType.Manufacturer));
								parent.AddOrgAddress(writeManager, nhtsa.OriginalVehicleManufacturerAddress, Constants.AddressType.OriginalVehicleManufacturerAddress);
							}
							else
							{
								var pest = cusAddInfo as Pesticide;
								if (pest != null)
								{
									parent.AddOrgAddress(writeManager, pest.ExaminationLocationAddress, nameof(DocAddressType.Location));
								}
								else
								{
									var ttb = cusAddInfo as TTBLine;
									if (ttb != null)
									{
										parent.AddOrgAddress(writeManager, ttb.ConsigneeAddress, nameof(DocAddressType.ConsigneeAddress));
									}
									else
									{
										var pgaVehicle = cusAddInfo as Business.Vehicle;
										if (pgaVehicle != null)
										{
											parent.AddOrgAddress(writeManager, pgaVehicle.OwnerAddress, Constants.AddressType.Owner);
											parent.AddOrgAddress(writeManager, pgaVehicle.StorageLocationAddress, nameof(DocAddressType.Location));
										}
										else
										{
											var constituentElement = cusAddInfo as ConstituentElement;
											if (constituentElement != null)
											{
												parent.AddOrgAddress(writeManager, constituentElement.ProducerAddress, Constants.AddressType.Producer);
											}
											else
											{
												var cpscHeader = cusAddInfo as CPSCHeader;
												if (cpscHeader != null)
												{
													parent.AddOrgAddress(writeManager, cpscHeader.ManufacturerAddress, nameof(DocAddressType.Manufacturer));
												}
												else
												{
													var cpscRule = cusAddInfo as CPSCRule;
													if (cpscRule != null)
													{
														parent.AddOrgAddress(writeManager, cpscRule.SafetyTestLocationAddress, Constants.AddressType.Laboratory);
													}
													else
													{
														var omcHeader = cusAddInfo as OMCHeader;
														if (omcHeader != null)
														{
															parent.AddOrgAddress(writeManager, omcHeader.ExporterAddress, Constants.AddressType.Exporter);
															parent.AddOrgAddress(writeManager, omcHeader.ResponsibleGovernmentOfficialAddress, Constants.AddressType.ResponsibleGovernmentOfficial);
															parent.AddOrgAddress(writeManager, omcHeader.AquacultureFacilityAddress, Constants.AddressType.AquacultureFacility);
														}
														else
														{
															var omcAquacultureFacility = cusAddInfo as USOMCAquacultureFacility;
															if (omcAquacultureFacility != null)
															{
																parent.AddOrgAddress(writeManager, omcAquacultureFacility.AquacultureFacilityAddress, Constants.AddressType.AquacultureFacility);
															}
															else
															{
																var nmfsHarvestingDetail = cusAddInfo as NMFSHarvestingDetail;
																if (nmfsHarvestingDetail != null)
																{
																	parent.AddOrgAddress(writeManager, nmfsHarvestingDetail.ContactParty, Constants.AddressType.ContactParty);
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
