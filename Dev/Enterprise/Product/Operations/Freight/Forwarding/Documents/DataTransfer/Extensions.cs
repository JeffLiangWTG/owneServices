using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using ContainerType = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerType;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using EZC = Enterprise.ZArchitecture.Core;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	static class Extensions
	{
		#region AddDataProvider

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext AddDataProvider(this UniversalDataBuss.DataObjects.Universal._2012_11.DataContext dataContext)
		{
			if (dataContext == null)
			{
				return null;
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			var dataProvider = new UniversalDataBuss.DataObjects.Universal._2012_11.DataProvider()
			{
				Code = enterpriseCode + serverCode + GlbCompany.CurrentCompany.GC_Code,
				Type = UniversalDataBuss.DataObjects.Universal._2012_11.DataProviderType.EnterpriseID
			};

			if (dataContext.DataSource == null)
			{
				dataContext.DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource();
			}

			dataContext.DataSource.DataProvider = dataProvider;

			if (dataContext is IDataContextDataObject dataContextDataObject)
			{
				dataContextDataObject.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			}

			return dataContext;
		}

		#endregion

		#region AddUserBranchAndDepartment

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext AddUserBranchAndDepartment(this UniversalDataBuss.DataObjects.Universal._2012_11.DataContext dataContext)
		{
			if (dataContext.Workflow == null)
			{
				dataContext.Workflow = new UniversalDataBuss.DataObjects.Universal._2012_11.Workflow();
			}

			dataContext.Workflow.EventUser = new Staff() { Code = GlbStaff.CurrentUser.GS_Code, Name = GlbStaff.CurrentUser.GS_FullName };
			dataContext.Workflow.EventBranch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code, Name = GlbBranch.CurrentBranch.GB_BranchName };
			dataContext.Workflow.EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code, Name = GlbDepartment.CurrentDepartment.HumanReadableName };

			return dataContext;
		}

		#endregion

		#region CreateUXmlDataContext

		public static UniversalDataBuss.DataObjects.Universal._2012_11.DataContext CreateUXmlDataContext(this IDataSourceProvider dataSourceProvider)
		{
			var res = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = dataSourceProvider.SourceType,
					Key = dataSourceProvider.SourceID
				}
			};

			return res;
		}

		#endregion

		#region ToUXmlOrganizationAddress

		public static OrganizationAddress ToUXmlOrganizationAddress(this IAddress source, string addressType, IDataObjectWriterStrategy strategy, IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> overridenRegistrationNumbers = null, bool isUpperCase = false)
		{
			if (source == null || string.IsNullOrWhiteSpace(source.CompanyName))
			{
				return null;
			}

			var address = new OrganizationAddress(strategy);
			address.CompanyName = isUpperCase ? source.CompanyName.ToUpperInvariant() : source.CompanyName;
			address.AddressType = addressType;
			address.Address1 = isUpperCase ? source.AddressLine1.ToUpperInvariant() : source.AddressLine1;
			address.Address2 = isUpperCase ? source.AddressLine2.ToUpperInvariant() : source.AddressLine2;
			address.AdditionalAddressInformation = source.AdditionalAddressInformation;
			address.GovRegNum = source.TaxNumber;

			if (source.TaxNumberType != null
				&& !source.TaxNumberType.Code.IsEmpty)
			{
				address.GovRegNumType = new RegistrationNumberType
				{
					Code = source.TaxNumberType.Code,
					Description = source.TaxNumberType.Description
				};
			}

			address.AddressOverride = ZBool.False;
			address.City = isUpperCase ? source.City.ToUpperInvariant() : source.City;
			address.Postcode = isUpperCase ? source.Postcode.ToUpperInvariant() : source.Postcode;
			address.State = isUpperCase ? source.State.ToUpperInvariant() : source.State;
			address.Country = source.Country?.ToUXmlCountry(isUpperCase);
			address.Port = source.Unloco?.ToUXmlUnloco();

			address.Contact = source.Contact;
			address.Email = source.Email;
			address.Fax = source.Fax;
			address.Phone = source.Phone;

			if (overridenRegistrationNumbers != null)
			{
				if (overridenRegistrationNumbers.Any())
				{
					address.SetRegistrationNumberCollection(() => overridenRegistrationNumbers.ToList());
				}
			}
			else if (source.RegistrationNumbers?.Count > 0)
			{
				var regNumbers = source
					.RegistrationNumbers
					.Select(ToUXmlRegistrationNumber)
					.ToList();

				address.SetRegistrationNumberCollection(() => regNumbers);
			}

			return address;
		}

		#endregion

		#region ToUXmlContainer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key")]
		public static Container ToUXmlContainer(this IContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.ContainerCount = source.ContainerCount;
			container.IsShipperOwned = source.IsShipperOwned;
			container.IsEmptyContainer = source.IsEmpty;
			container.IsControlledAtmosphere = source.HasControlledAtmosphere;
			container.TempRecorderSerialNo = source.TemperatureRecorderSerialNumber;
			container.SetPointTemp = source.SetTemperature?.Value;
			container.SetPointTempUnit = source.SetTemperature?.Unit?.Code;
			container.HumidityPercent = Convert.ToByte((decimal)(source.Humidity?.Value ?? 0), CultureInfo.CurrentCulture);
			container.AirVentFlow = source.AirVentFlow?.Value;
			container.AirVentFlowRateUnit = source.AirVentFlow?.Unit?.ToUXmlCodeDescriptionPair();
			container.Seal = source.Seal;
			container.SealPartyType = source.SealPartyType.ToUXmlCodeDescriptionPair();
			container.SecondSeal = source.SecondSeal;
			container.SecondSealPartyType = source.SecondSealPartyType.ToUXmlCodeDescriptionPair();
			container.ThirdSeal = source.ThirdSeal;
			container.ThirdSealPartyType = source.ThirdSealPartyType.ToUXmlCodeDescriptionPair();
			container.GoodsWeight = source.GoodsWeight?.Value;
			container.TareWeight = source.TareWeight?.Value;
			container.DunnageWeight = source.Dunnage?.Value;
			container.GrossWeight = source.GrossWeight?.Value;
			container.WeightUnit = source.GoodsWeight?.Unit.ToUXmlUnitOfWeight();
			container.VolumeUnit = source.VolumeCapacity?.Unit.ToUXmlUnitOfVolume();
			container.GrossWeightVerificationDateTime = source.VerifiedDate;
			container.GrossWeightVerificationType = source.VerifiedMethod.ToUXmlCodeDescriptionPair();
			container.ContainerType = source.Type.ToUXmlContainerType();
			container.NonOperatingReefer = source.IsNonOperativeReefer;
			container.EmptyRequired = source.EmptyRequired;
			container.DepartureEstimatedPickup = source.DepartureEstimatedPickup;
			container.ArrivalDeliveryRequiredBy = source.ArrivalDeliveryRequiredBy;
			container.OverhangBack = source.OverhangBack?.Value;
			container.OverhangFront = source.OverhangFront?.Value;
			container.OverhangHeight = source.OverhangHeight?.Value;
			container.OverhangLeft = source.OverhangLeft?.Value;
			container.OverhangRight = source.OverhangRight?.Value;
			container.ExportDepotCustomsReference = source.ExportDepotCustomsReference;
			container.ImportDepotCustomsReference = source.ImportDepotCustomsReference;
			container.ContainerQuality = source.ContainerQuality.ToUXmlCodeDescriptionPair();
			container.LengthUnit = new UnitOfLength()
			{
				Code = Core.Constants.Length.Feet,
				Description = Core.Constants.Length.GetDescription(Core.Constants.Length.Feet, Core.Constants.PluralState.Plural)
			};
			if (source?.Commodity != null)
			{
				container.Commodity = new Commodity() { Code = source.Commodity.Code, Description = source.Commodity.Description };
			}

			container.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				source.VerifiedByAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.GrossWeightVerifiedBy), writerStrategy),
				source.DepartureContainerYard.ToUXmlOrganizationAddress(nameof(DocAddressType.ContainerYardEmptyPickupAddress), writerStrategy)
			});

			var numbers = source
				.Numbers
				?.Select(ToUXmlAdditionalReference)
				.ToArray();

			container.SetAdditionalReferenceCollection(() => (numbers?.Any() ?? false) ? new DataObjectList<AdditionalReference>(numbers) : null);

			container.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>
					{
						new AddInfo
						{
							Key = "Genset",
							Value = source.Genset ? "true" : "false"
						}
					};

				return addInfos.Any() ? addInfos : null;
			});

			return container;
		}

		#endregion

		#region ToUXmlContainer

		public static Container ToUXmlContainer(this ContainerLoadPlanContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.GrossWeight = source.GrossWeight?.Value;
			container.TareWeight = source.TareWeight?.Value;
			container.WeightUnit = source.GrossWeight?.Unit.ToUXmlUnitOfWeight();
			container.FCL_LCL_AIR = source.Mode != null ? new ContainerMode() { Code = source.Mode.Code, Description = source.Mode.Description } : null;
			container.ContainerType = source.ContainerType.ToUXmlContainerType();
			container.Seal = source.Seal;
			container.SecondSeal = source.SecondSeal;
			container.ThirdSeal = source.ThirdSeal;
			container.PackDate = source.PackDate;
			container.GoodsDescription = source.GoodsDetails;
			container.SetPointTemp = source.SetTemperature?.Value;
			container.SetPointTempUnit = source.SetTemperature?.Unit?.Code;
			container.NonOperatingReefer = source.IsNonOperativeReefer;

			return container;
		}

		#endregion

		#region ToUXmlShipment

		public static UniversalShipment ToUXmlShipment(this ContainerLoadPlanSOGrouping source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var shipment = new UniversalShipment(writerStrategy);
			shipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = DocDataConstants.DataSources.Booking,
					Key = source.SONumber
				}
			};

			shipment.SetPackingLineCollection(() =>
			{
				var collection = new DataObjectList<PackingLine>();
				var packline = new PackingLine(writerStrategy);
				packline.PackQty = new ZLong(source.Quantity);
				packline.PackType = source.PackageType != null ? new PackageType() { Code = source.PackageType.Code, Description = source.PackageType.Description } : null;
				packline.Weight = source.Weight?.Value;
				packline.WeightUnit = source.Weight?.Unit.ToUXmlUnitOfWeight();
				packline.Volume = source.Volume?.Value;
				packline.VolumeUnit = source.Volume?.Unit.ToUXmlUnitOfVolume();
				packline.DetailedDescription = source.GoodsDescription;
				packline.MarksAndNos = source.MarksAndNumbers;

				packline.SetUNDGCollection(() =>
				{
					List<UNDG> result = null;
					if (source.DangerousGoods != null && source.DangerousGoods.Any())
					{
						result = new List<UNDG>();
						result.AddRange(source.DangerousGoods.Select(x => x.ToUXmlUNDG(writerStrategy, false)));
					}
					return result;
				});

				collection.Add(packline);
				return collection;
			});

			return shipment;
		}

		#endregion

		#region ToUXmlUNDG

		public static UNDG ToUXmlUNDG(this IDangerousGood source, IDataObjectWriterStrategy writerStrategy, bool includeVariant)
		{
			if (source == null)
			{
				return null;
			}

			var undg = new UNDG(writerStrategy);
			undg.UNDGCode = includeVariant ? source.Code : source.Unno;
			undg.Contact = source.Contact.ToUXmlContact();
			undg.ProperShippingName = source.ProperShippingName;
			undg.TechicalName = source.TechnicalName;
			undg.PackQty = source.Quantity;
			undg.PackedInLimitedQuantity = source.PackedInLimitedQuantity;
			undg.IMOClass = source.IMOClass;
			undg.PackingGroup = source.PackingGroup;
			undg.SubLabel1 = source.SubLabel1;
			undg.SubLabel2 = source.SubLabel2;
			undg.Weight = source.Weight?.Value;
			undg.State = new UNDGStateConverter().ToEnumValue(source.State);
			undg.WeightUQ = new UnitOfWeight { Code = source.Weight?.Unit?.Code, Description = source.Weight?.Unit?.Description };
			undg.NetExplosiveWeight = source.NetExplosiveWeight?.Value;
			undg.NetExplosiveWeightUQ = new UnitOfWeight { Code = source.NetExplosiveWeight?.Unit?.Code, Description = source.NetExplosiveWeight?.Unit?.Description };
			undg.Volume = source.Volume?.Value;
			undg.VolumeUQ = new UnitOfVolume { Code = source.Volume?.Unit?.Code, Description = source.Volume?.Unit?.Description };
			undg.PackType = new PackageType { Code = source.PackageType?.Code, Description = source.PackageType?.Description };
			undg.MarinePollutant = new UNDGMarinePollutant { Code = source.MarinePollutant?.Code, Description = source.MarinePollutant?.Description };
			undg.ExceptedQuantityCode = source.ExceptedQuantityCode?.ToUXmlCodeDescriptionPair();
			undg.EmergencyScheduleFire = source.EmergencyScheduleFire?.ToUXmlCodeDescriptionPair();
			undg.EmergencyScheduleSpillage = source.EmergencyScheduleSpillage?.ToUXmlCodeDescriptionPair();
			undg.Standard = source.Standard;

			if (source.FlashPoint != null)
			{
				undg.FlashPoint = source.FlashPoint?.Value.ToString();
			}

			return undg;
		}

		public static UNDG ToUXmlUNDG(this DGRestriction source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var undg = new UNDG(writerStrategy);
			undg.EmergencyScheduleFire = source.EmergencyScheduleFire.ToUXmlCodeDescriptionPair();
			undg.EmergencyScheduleSpillage = source.EmergencyScheduleSpillage.ToUXmlCodeDescriptionPair();
			undg.ExceptedQuantityCode = new CodeDescriptionPair { Code = source.ExceptedQuantityCode };
			undg.FlashPoint = source.FlashPoint;
			undg.IMOClass = source.IMOClass;
			undg.MarinePollutant = new UNDGMarinePollutant { Code = source.MarinePollutantCode };
			undg.PackedInLimitedQuantity = source.PackedInLimitedQuantity;
			undg.PackingGroup = source.PackingGroup;
			undg.ProperShippingName = source.ProperShippingName;
			undg.Standard = source.Standard;
			undg.State = new UNDGStateConverter().ToEnumValue(source.State);
			undg.SubLabel1 = source.SubLabel1;
			undg.SubLabel2 = source.SubLabel2;
			undg.UNDGCode = source.Code;

			return undg;
		}

		#endregion

		#region ToUXmlClassification

		public static Classification ToUXmlClassification(this IHarmonizedCode source)
		{
			if (source == null)
			{
				return null;
			}

			var classification = new Classification
			{
				Code = source.Code,
				Country = source.Country?.ToUXmlCountry(),
				Type = new CodeDescriptionPair
				{
					Code = Enterprise.Freight.Business.FreightConstants.Classification.Codes.HarmonizedCode,
					Description = Enterprise.Freight.Business.FreightConstants.Classification.Description.HarmonizedCode
				}
			};

			return classification;
		}

		#endregion

		#region ToUXmlContact

		public static OrganizationContact ToUXmlContact(this IContact source)
		{
			if (source == null)
			{
				return null;
			}

			var contact = new OrganizationContact();
			contact.FullName = source.FullName;
			contact.Phone = source.Phone;
			contact.Email = source.Email;

			return contact;
		}

		#endregion

		#region ToUXmlContainerType

		public static ContainerType ToUXmlContainerType(this IContainerType type)
		{
			if (type == null)
			{
				return null;
			}

			return new ContainerType
			{
				ISOCode = type.ISOCode,
				Code = type.Code,
				Description = type.Description,
				Category = type.Type != null
					? new ContainerTypeCategory() { Code = type.Type.Code, Description = type.Type.Description }
					: null
			};
		}

		#endregion

		#region ToUXmlPackingLine

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key")]
		public static PackingLine ToUXmlPackingLine(this IPackingLine source, IDataObjectWriterStrategy writerStrategy, bool includeVariant = false, Dictionary<ZGuid, int> packingLineContainerLinkMap = null,
			string itnNumbers = "", string dueNumbers = "", string ucrNumbers = "", string ctkNumbers = "", bool splitHarmonisedCode = false, string ctnNumbers = "")
		{
			if (source == null)
			{
				return null;
			}

			var packingLine = new PackingLine(writerStrategy);
			packingLine.ContainerNumber = source.ContainerNumber;
			packingLine.GoodsDescription = source.GoodsDescription;
			packingLine.DetailedDescription = source.GoodsDescription;
			packingLine.PackQty = new ZLong(source.Quantity);
			packingLine.PackingLineID = source.PackingLineID;
			packingLine.MarksAndNos = source.MarksAndNumbers;
			packingLine.HarmonisedCode = source.GetHarmonisedCode(splitHarmonisedCode);
			packingLine.PackType = source.PackageType.ToUXmlPackType();
			packingLine.Weight = source.Weight?.Value;
			packingLine.WeightUnit = source.Weight?.Unit.ToUXmlUnitOfWeight();
			packingLine.Volume = source.Volume?.Value;
			packingLine.VolumeUnit = source.Volume?.Unit.ToUXmlUnitOfVolume();
			packingLine.Height = source.Height?.Value;
			packingLine.Width = source.Width?.Value;
			packingLine.Length = source.Length?.Value;
			packingLine.LengthUnit = source.Length?.Unit.ToUXmlUnitOfLength();
			packingLine.ReferenceNumber = source.ReferenceNumber;
			packingLine.ImportReferenceNumber = source.ImportReferenceNumber;
			packingLine.ExportReferenceNumber = source.ExportReferenceNumber;
			packingLine.RequiresTemperatureControl = source.RequiresTemperatureControl;
			packingLine.RequiredTemperatureMinimum = source.TemperatureMinimum?.Value;
			packingLine.RequiredTemperatureMaximum = source.TemperatureMaximum?.Value;

			var requiredTemperatureUnit = source.TemperatureMinimum?.Unit;
			packingLine.RequiredTemperatureUnit = requiredTemperatureUnit == null
				? null
				: new CodeDescriptionPair1Char { Code = requiredTemperatureUnit.Code, Description = requiredTemperatureUnit.Description };
			packingLine.SetUNDGCollection(() => source.DangerousGoods?.Select(u => u.ToUXmlUNDG(writerStrategy, includeVariant)).ToList());
			packingLine.SetClassificationCollection(() => source.GetClassificationCollection(splitHarmonisedCode));
			packingLine.OutturnComment = source.OutturnComment;
			if (source?.Commodity != null)
			{
				packingLine.Commodity = new Commodity() { Code = source.Commodity.Code, Description = source.Commodity.Description };
			}

			if (packingLineContainerLinkMap != null && packingLineContainerLinkMap.TryGetValue((ZGuid)((DocDataObject)source).Identifier, out int containerLink))
			{
				packingLine.ContainerLink = containerLink;
			}

			packingLine.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				if (!string.IsNullOrEmpty(itnNumbers))
				{
					addInfos.Add(new AddInfo
					{
						Key = Customs.Common.US.CusEntryNumberTypeList.Codes.ITN,
						Value = itnNumbers
					});
				}
				if (!string.IsNullOrEmpty(ctkNumbers))
				{
					addInfos.Add(new AddInfo
					{
						Key = Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote,
						Value = ctkNumbers
					});
				}
				if (!string.IsNullOrEmpty(dueNumbers))
				{
					addInfos.Add(new AddInfo
					{
						Key = CusEntryNumberTypes.Brazil.DUE,
						Value = dueNumbers
					});
				}
				if (!string.IsNullOrEmpty(ucrNumbers))
				{
					addInfos.Add(new AddInfo
					{
						Key = CusEntryNumberTypes.Standard.UniqueConsignementReference,
						Value = ucrNumbers
					});
				}
				if (!string.IsNullOrEmpty(ctnNumbers))
				{
					addInfos.Add(new AddInfo
					{
						Key = CanadaAdditionalReferenceNumberTypes.Codes.CTN,
						Value = ctnNumbers
					});
				}
				if (source.EntryType != null)
				{
					addInfos.Add(new AddInfo()
					{
						Key = "EntryType",
						Value = source.EntryType.Code
					});

					if (source.EntryType.Code == EntryTypes.Codes.EntryTypes_AES || source.EntryType.Code == EntryTypes.Codes.EntryTypes_AE1)
					{
						addInfos.Add(new AddInfo()
						{
							Key = "CustomsStatusComplete",
							Value = source.Complete.ToString()
						});
						addInfos.Add(new AddInfo()
						{
							Key = "Shortage",
							Value = source.Shortage.ToString()
						});
						addInfos.Add(new AddInfo()
						{
							Key = "CargoItem",
							Value = source.CargoItem
						});
						addInfos.Add(new AddInfo()
						{
							Key = "PackageNumber",
							Value = source.PackageNumber
						});
					}
				}

				return addInfos.Any() ? addInfos : null;
			});

			return packingLine;
		}

		static ZString? GetHarmonisedCode(this IPackingLine source, bool splitHarmonisedCode)
		{
			if (splitHarmonisedCode && source.HarmonizedCode != null && source.HarmonizedCode.Code.Length > 0)
			{
				return source.HarmonizedCode.Code.Split(", ").First();
			}

			return source.HarmonizedCode?.Code;
		}

		static DataObjectList<Classification> GetClassificationCollection(this IPackingLine source, bool splitHarmonisedCode)
		{
			DataObjectList<Classification> classificationCollection = null;

			if (splitHarmonisedCode && source.HarmonizedCode != null && source.HarmonizedCode.Code.Length > 0)
			{
				classificationCollection = classificationCollection ?? new DataObjectList<Classification>();

				var noCountryHarmonizedCodes = source.HarmonizedCode.Code.Split(", ");
				foreach (var noCountryHarmonizedCode in noCountryHarmonizedCodes)
				{
					classificationCollection.Add(new Classification
					{
						Code = noCountryHarmonizedCode,
						Country = new Country
						{
							Code = ZString.Empty
						},
						Type = new CodeDescriptionPair
						{
							Code = Enterprise.Freight.Business.FreightConstants.Classification.Codes.HarmonizedCode,
							Description = Enterprise.Freight.Business.FreightConstants.Classification.Description.HarmonizedCode
						}
					});
				}
			}

			if (source.HarmonizedCodes != null)
			{
				classificationCollection = classificationCollection ?? new DataObjectList<Classification>();

				classificationCollection.AddRange(source.HarmonizedCodes.Select(c => c.ToUXmlClassification()));
			}

			return classificationCollection;
		}

		public static PackingLine ToUXmlPackingLine(this SeaShipmentBookingRequestPackLine source, IDataObjectWriterStrategy writerStrategy, bool includeVariant = false, Dictionary<ZGuid, int> packingLineContainerLinkMap = null)
		{
			if (source == null)
			{
				return null;
			}

			var packingLine = new PackingLine(writerStrategy);

			packingLine.GoodsDescription = source.GoodsDescription;
			packingLine.DetailedDescription = source.GoodsDescription;
			packingLine.PackQty = new ZLong(source.PacksQuantity);
			packingLine.MarksAndNos = source.MarksAndNumbersOnPackages;
			packingLine.HarmonisedCode = string.Join(", ", source.HarmonizedCodesCollection.Where(x => !x.Code.IsEmpty).Select(x => x.Code).ToList());
			packingLine.PackType = source.PackType.ToUXmlPackType();
			packingLine.Weight = source.CargoWeight?.Value;
			packingLine.WeightUnit = source.CargoWeight?.Unit.ToUXmlUnitOfWeight();
			packingLine.Volume = source.CargoVolume?.Value;
			packingLine.VolumeUnit = source.CargoVolume?.Unit.ToUXmlUnitOfVolume();

			packingLine.SetUNDGCollection(
				() => source.DangerousGoods?.Select(dgItem => dgItem.ToUXmlUNDG(writerStrategy, includeVariant)).ToList()
			);

			packingLine.SetClassificationCollection(
				() =>
				{
					return source.HarmonizedCodesCollection != null
						? new DataObjectList<Classification>(source.HarmonizedCodesCollection.Select(ToUXmlClassification))
						: null;
				}
			);
			return packingLine;
		}

		#endregion

		#region ToUXmlUnitOfWeight

		public static UnitOfWeight ToUXmlUnitOfWeight(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			var weightUnit = new UnitOfWeight();
			weightUnit.Code = source.Code;
			weightUnit.Description = source.Description;

			return weightUnit;
		}

		#endregion

		#region ToUXmlUnitOfVolume

		public static UnitOfVolume ToUXmlUnitOfVolume(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			var volumeUnit = new UnitOfVolume();
			volumeUnit.Code = source.Code;
			volumeUnit.Description = source.Description;

			return volumeUnit;
		}

		#endregion

		#region ToUXmlUnitOfLength

		public static UnitOfLength ToUXmlUnitOfLength(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			var lengthUnit = new UnitOfLength
			{
				Code = source.Code,
				Description = source.Description
			};

			return lengthUnit;
		}

		#endregion

		#region ToUXmlPackType

		public static PackageType ToUXmlPackType(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			var packType = new PackageType();
			packType.Code = source.Code;
			packType.Description = source.Description;

			return packType;
		}

		#endregion

		#region ToUXmlCodeDescriptionPair

		public static CodeDescriptionPair ToUXmlCodeDescriptionPair(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			var codeDescription = new CodeDescriptionPair();
			codeDescription.Code = source.Code;
			codeDescription.Description = source.Description;
			return codeDescription;
		}

		#endregion

		#region ToUXmlCurrency

		public static Currency ToUXmlCurrency(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			return new Currency()
			{
				Code = source.Code.ToUpperInvariant(),
				Description = source.Description
			};
		}

		#endregion

		#region ToUXmlContainerMode

		public static ContainerMode ToUXmlContainerMode(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			return new ContainerMode()
			{
				Code = source.Code,
				Description = source.Description
			};
		}

		#endregion

		#region ToUXmlRegistrationNumber

		public static UniversalDataBuss.DataObjects.Universal.RegistrationNumber ToUXmlRegistrationNumber(this IRegistrationNumber source)
		{
			if (source == null)
			{
				return null;
			}

			var regNumber = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regNumber.Type = new RegistrationNumberType
			{
				Code = source.Type?.Code,
				Description = source.Type?.Description
			};
			regNumber.CountryOfIssue = new Country
			{
				Code = source.CountryOfIssue?.Code,
				Name = source.CountryOfIssue?.Name
			};
			regNumber.Value = source.Value;

			return regNumber;
		}

		public static UniversalDataBuss.DataObjects.Universal.RegistrationNumber ToUXmlRegistrationNumber(this TaxInfo source, bool shouldPopulateRegulatingCountry = false)
		{
			if (source == null)
			{
				return null;
			}

			var regNumber = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regNumber.Type = new RegistrationNumberType
			{
				Code = source.Code,
				Description = string.Concat(source.ShortLabel, '|', source.LongLabel)
			};
			regNumber.CountryOfIssue = new Country
			{
				Code = source.Country?.Code,
				Name = source.Country?.Name
			};
			regNumber.Value = source.Number;
			if (shouldPopulateRegulatingCountry && !string.IsNullOrEmpty(regNumber.Value) && !string.IsNullOrEmpty(source.RegulatingCountry?.Code))
			{
				regNumber.Value += "|" + source.RegulatingCountry.Code;
			}

			return regNumber;
		}

		#endregion

		#region ToUXmlTransportLeg

		public static TransportLeg ToUXmlTransportLeg(this ITransport source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var transportLeg = new TransportLeg(writerStrategy);
			transportLeg.LegOrder = (byte)source.LegOrder;
			transportLeg.VoyageFlightNo = source.VoyageFlightNumber;
			transportLeg.EstimatedDeparture = source.ETD;
			transportLeg.EstimatedArrival = source.ETA;
			transportLeg.ActualDeparture = source.ATD;
			transportLeg.ActualArrival = source.ATA;
			transportLeg.LCLCutOff = source.LCLCutOff;
			transportLeg.LCLReceivalCommences = source.LCLReceivalCommences;
			transportLeg.TransportMode = source.Mode?.Code.ToUXmlTransportMode();
			transportLeg.LegType = source.Type.Code.ToUXmlLegType();
			transportLeg.VesselName = source.Vessel?.Name;
			transportLeg.VesselLloydsIMO = source.Vessel?.LloydsIMO;
			transportLeg.PortOfLoading = source.PortOfLoading.ToUXmlUnloco();
			transportLeg.PortOfDischarge = source.PortOfDischarge.ToUXmlUnloco();
			transportLeg.Carrier = source.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.Carrier), writerStrategy);

			transportLeg.SetAdditionalTransportModeCollection(() =>
			{
				List<AdditionalTransportMode> result = null;

				var transportMode = source.AdditionalTransportMode?.Code.ToUXmlAdditionalTransportMode();

				if (transportMode != null)
				{
					result = new List<AdditionalTransportMode>();
					var additionalTransportMode = new AdditionalTransportMode();
					additionalTransportMode.TransportMode = transportMode;
					result.Add(additionalTransportMode);
				}
				return result;
			});

			return transportLeg;
		}

		public static TransportMode? ToUXmlTransportMode(this ZString code)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return null;
			}

			switch (code)
			{
				case Core.Constants.TransportModes.Air:
					return TransportMode.Air;

				case Core.Constants.TransportModes.Sea:
					return TransportMode.Sea;

				case Core.Constants.TransportModes.Road:
					return TransportMode.Road;

				case Core.Constants.TransportModes.Rail:
					return TransportMode.Rail;

				case Core.Constants.TransportModes.Storage:
					return TransportMode.Storage;

				case Core.Constants.TransportModes.InlandWaterwayTransport:
					return TransportMode.InlandWaterway;
			}

			return null;
		}

		public static TransportMode? ToUXmlAdditionalTransportMode(this ZString code)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return null;
			}

			switch (code)
			{
				case Core.Constants.TransportModes.Road:
					return TransportMode.Road;

				case Core.Constants.TransportModes.Rail:
					return TransportMode.Rail;
			}

			return null;
		}

		static LegType? ToUXmlLegType(this ZString code)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return null;
			}

			switch (code)
			{
				case Core.Constants.TransportPlanningType.Flight1:
					return LegType.Flight1;

				case Core.Constants.TransportPlanningType.Flight2:
					return LegType.Flight2;

				case Core.Constants.TransportPlanningType.Flight3:
					return LegType.Flight3;

				case Core.Constants.TransportPlanningType.MainVessel:
					return LegType.Main;

				case Core.Constants.TransportPlanningType.PreCarriage:
					return LegType.PreCarriage;

				case Core.Constants.TransportPlanningType.OnForwarding:
					return LegType.OnForwarding;

				case Core.Constants.TransportPlanningType.Other:
					return LegType.Other;
			}

			return null;
		}

		#endregion

		#region ToUXmlCountry

		public static Country ToUXmlCountry(this ICountry source, bool isUpperCase = false)
		{
			if (source == null)
			{
				return null;
			}

			return new Country
			{
				Code = isUpperCase ? source.Code.ToUpperInvariant() : source.Code,
				Name = source.Name
			};
		}

		#endregion

		#region ToUXmlUnloco

		public static UNLOCO ToUXmlUnloco(this IUnloco source)
		{
			if (source == null)
			{
				return null;
			}

			return new UNLOCO
			{
				Code = source.Code,
				Name = source.Name
			};
		}

		#endregion

		#region AddDateToUXmlDateCollection

		public static void Add(this List<Date> dates, ZDateTime dateTimeToBeAdded, DateType dateType, ZBool? isEstimate = null)
		{
			if (!dateTimeToBeAdded.IsEmpty)
			{
				dates.Add(new Date
				{
					Type = dateType,
					Value = dateTimeToBeAdded,
					IsEstimate = isEstimate
				});
			}
		}

		#endregion

		#region CreateUXmlDeliveryMode

		public static CodeDescriptionPair CreateUXmlDeliveryMode(bool isDoorPickup, bool isDoorDelivery)
		{
			var code = string.Format(CultureInfo.InvariantCulture, "{0}T{1}", isDoorPickup ? "D" : "P", isDoorDelivery ? "D" : "P"); // hardcoded constant
			var description = string.Format(CultureInfo.InvariantCulture, (EZC.NoResString)"{0} To {1}", isDoorPickup ? (EZC.NoResString)"Door" : (EZC.NoResString)"Peer", isDoorDelivery ? (EZC.NoResString)"Door" : (EZC.NoResString)"Peer"); // hardcoded constant

			return new CodeDescriptionPair
			{
				Code = code,
				Description = description
			};
		}

		#endregion

		#region ToUXmlAddInfos

		public static IEnumerable<AddInfo> ToUXmlAddInfos(this IUnloco unloco, string propertyName)
		{
			if (!string.IsNullOrEmpty(propertyName)
				&& unloco != null)
			{
				yield return new AddInfo
				{
					Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", propertyName, nameof(unloco.Code)),
					Value = unloco.Code
				};

				yield return new AddInfo
				{
					Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", propertyName, nameof(unloco.Name)),
					Value = unloco.Name
				};
			}
		}

		#endregion

		#region ToUXmlAdditionalReference

		public static AdditionalReference ToUXmlAdditionalReference(this IReferenceNumber number)
		{
			if (number == null)
			{
				return null;
			}

			return new AdditionalReference
			{
				Type = new EntryType
				{
					Code = number.Type?.Code,
					Description = number.Type?.Description
				},
				ReferenceNumber = number.Value
			};
		}

		#endregion

		#region ToUXmlShipment

		static string GetMergeSubNumbers(IShipment shipment, string numberType, string numbers = "")
		{
			if (shipment.ShipmentType.Code == Constants.ShipmentTypes.AssemblyMaster || shipment.ShipmentType.Code == Constants.ShipmentTypes.BlindCoLoadMaster || shipment.ShipmentType.Code == Constants.ShipmentTypes.CoLoadMaster)
			{
				if (shipment.Shipments != null)
				{
					foreach (var subShipment in shipment.Shipments)
					{
						numbers += GetMergeSubNumbers(subShipment, numberType, numbers) + ", ";
					}
				}
			}
			else
			{
				switch (numberType)
				{
					case Customs.Common.US.CusEntryNumberTypeList.Codes.ITN:
						if (shipment.ITNNumber.IsEmpty)
						{
							numbers = string.Join(", ", new[] { shipment.ExportStatement, shipment.ExportStatementField1Code, shipment.ExportStatementField2Code });
						}
						else
						{
							numbers = shipment.ITNNumber;
						}
						break;
					case CusEntryNumberTypes.Brazil.DUE:
						numbers = shipment.DUENumber;
						break;
					case CusEntryNumberTypes.Standard.UniqueConsignementReference:
						numbers = shipment.UCRNumber;
						break;
					case Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote:
						numbers = shipment.CTKNumber;
						break;
					case CanadaAdditionalReferenceNumberTypes.Codes.CTN:
						numbers = shipment.CTNNumber;
						break;
				}
			}

			return numbers;
		}

		public static string GetDistinctNumbers(this IShipment shipment, string numberType)
		{
			var numbers = GetMergeSubNumbers(shipment, numberType);

			if (!string.IsNullOrEmpty(numbers))
			{
				numbers = string.Join(", ", numbers.Split(new string[] { @", " }, StringSplitOptions.RemoveEmptyEntries).Distinct());
			}

			return numbers;
		}

		public static IShipment GetMatchedShipmentForPackLine(this IShipment shipment, string shipmentID)
		{
			IShipment matchedShipment;

			if (shipment.ShipmentID == shipmentID)
			{
				return shipment;
			}
			else if (shipment.Shipments != null)
			{
				foreach (var subShipment in shipment.Shipments)
				{
					matchedShipment = subShipment.GetMatchedShipmentForPackLine(shipmentID);
					if (matchedShipment != null)
					{
						return matchedShipment;
					}
				}
			}

			return null;
		}

		public static UniversalShipment ToUXmlShipment(this IShipment shipment, Dictionary<ZGuid, int> packingLineContainerLinkMap, IDataObjectWriterStrategy writerStrategy,
			bool hasITNNumbers = false, bool hasDUENumbers = false, bool hasUCRNumbers = false, bool isUpperCaseOrg = false, Func<DataObjectList<PackingLine>> getPackingLineCollectionFunc = null, CodeDescriptionPair houseBillPaymentType = null,
			bool hasCTNNumbers = false)
		{
			if (shipment == null)
			{
				return null;
			}

			var universalShipment = new UniversalShipment(writerStrategy);
			universalShipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = DocDataConstants.DataSources.ForwardingShipment,
					Key = shipment.ShipmentID
				}
			};

			universalShipment.WayBillNumber = shipment.HouseBillNumber;
			universalShipment.HBLContainerPackModeOverride = shipment.ContainerPackingMode?.Code;
			universalShipment.ShipmentType = shipment.ShipmentType.ToUXmlForOCMShipmentType(); // to match the mapping logic
			universalShipment.BookingConfirmationReference = shipment.ShipperReference;
			universalShipment.LocalProcessing = new LocalProcessing
			{
				PickupRequiredBy = shipment.PickRequestedByDate,
				DeliveryRequiredBy = shipment.DeliveryRequiredByDate
			};

			universalShipment.PortOfOrigin = shipment.Origin?.ToUXmlUnloco();
			universalShipment.PortOfDestination = shipment.Destination?.ToUXmlUnloco();

			if (shipment.GoodsValue != null)
			{
				universalShipment.GoodsValue = shipment.GoodsValue?.Amount;
				universalShipment.GoodsValueCurrency = new Currency() { Code = shipment.GoodsValue.Currency.Code, Description = shipment.GoodsValue.Currency.Description };
			}

			var organizationAddressList = new List<OrganizationAddress>
			{
				shipment.Consignor.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.PickupFrom.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorPickupDeliveryAddress), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.PickupCFS.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCFSAddress), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.DeliveryTo.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneePickupDeliveryAddress), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.DeliveryCFS.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCFSAddress), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2), writerStrategy, isUpperCase: isUpperCaseOrg),
				shipment.NotifyParty3.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty3), writerStrategy, isUpperCase: isUpperCaseOrg)
			};

			if (!shipment.Buyer.IsEmpty())
			{
				organizationAddressList.Add(shipment.Buyer.ToUXmlOrganizationAddress(nameof(DocAddressType.BuyerDocumentaryAddress), writerStrategy, isUpperCase: isUpperCaseOrg));
			}

			if (!shipment.Supplier.IsEmpty())
			{
				organizationAddressList.Add(shipment.Supplier.ToUXmlOrganizationAddress(nameof(DocAddressType.SupplierDocumentaryAddress), writerStrategy, isUpperCase: isUpperCaseOrg));
			}

			universalShipment.SetOrganizationAddressCollection(() => organizationAddressList);

			if (getPackingLineCollectionFunc == null)
			{
				getPackingLineCollectionFunc = () =>
				{
					return new DataObjectList<PackingLine>(((DocDataObjects.Shipment)shipment).AllPackingLinesIncludeCoLoad
						.Select(x => x.ToUXmlPackingLine
						(
							writerStrategy,
							true,
							packingLineContainerLinkMap,
							itnNumbers: hasITNNumbers ? shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(Customs.Common.US.CusEntryNumberTypeList.Codes.ITN) : string.Empty,
							dueNumbers: hasDUENumbers ? shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Brazil.DUE) : string.Empty,
							ucrNumbers: hasUCRNumbers ? shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Standard.UniqueConsignementReference) : string.Empty,
							ctnNumbers: hasCTNNumbers ? shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CanadaAdditionalReferenceNumberTypes.Codes.CTN) : string.Empty
						)));
				};
			}
			universalShipment.SetPackingLineCollection(() => getPackingLineCollectionFunc());

			universalShipment.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber()
				{
					Number = hasITNNumbers ? shipment.GetDistinctNumbers(Customs.Common.US.CusEntryNumberTypeList.Codes.ITN) : shipment.ITNNumber.ToString(),
					Type = new EntryType()
					{
						Code = Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN,
						Description = Enterprise.Customs.Common.US.CusEntryNumberTypeList.Descriptions.ITN
					},
					EntryIsSystemGenerated = false
				},
				new EntryNumber()
				{
					Number = hasDUENumbers ? shipment.GetDistinctNumbers(CusEntryNumberTypes.Brazil.DUE) : shipment.DUENumber.ToString(),
					Type = new EntryType()
					{
						Code = CusEntryNumberTypes.Brazil.DUE,
						Description = (EZC.NoResString)"Declaração Única de Exportação" // Valid Code and should not be localized
					},
					EntryIsSystemGenerated = false
				},
				new EntryNumber()
				{
					Number = hasUCRNumbers ? shipment.GetDistinctNumbers(CusEntryNumberTypes.Standard.UniqueConsignementReference) : shipment.UCRNumber.ToString(),
					Type = new EntryType()
					{
						Code = CusEntryNumberTypes.Standard.UniqueConsignementReference,
						Description = nameof(CusEntryNumberTypes.Standard.UniqueConsignementReference)
					},
					EntryIsSystemGenerated = false
				}
			});

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				var countriesOfRouting = GetCountriesOfRouting(shipment.Transports, shipment.Origin?.Country.Code, shipment.Destination?.Country.Code);
				if (!countriesOfRouting.IsEmpty)
				{
					addInfos.Add(new AddInfo
					{
						Key = "CountriesOfRouting",
						Value = countriesOfRouting
					});
				}

				return addInfos.Any() ? addInfos : null;
			});

			if (houseBillPaymentType != null)
			{
				var paymentInstruction = new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = "HPT",
						Description = (EZC.NoResString)"HBL Payment Type"
					},
					PaymentMethod = houseBillPaymentType
				};
				universalShipment.SetPaymentHandlingInstructionCollection(() => new List<PaymentHandlingInstruction> { paymentInstruction });
			}

			return universalShipment;
		}

		public static CodeDescriptionPair ToUXmlForOCMShipmentType(this ICodeDescription source)
		{
			if (source == null)
			{
				return null;
			}

			var codeDescription = new CodeDescriptionPair
			{
				Code = source.Code == Constants.ShipmentTypes.ThirdPartyOwnershipHouse ? source.Code.ToString() : Constants.ShipmentTypes.StandardHouse,
				Description = source.Description
			};

			return codeDescription;
		}

		#endregion

		#region Additional References

		#region SuppressResourceStringsCheckRegion

		static class AdditionalReferenceCodes
		{
			public const string BillOfLading = "BOL";
			public const string ShipperReference = "SHP";
			public const string FreightForwarderReference = "FFW";
			public const string CarrierContractNumber = "CON";
			public const string QuotationNumber = "QUO";
		}

		static class AdditionalReferenceDescriptions
		{
			public const string BillOfLading = "Bill Of Lading Number";
			public const string ShipperReference = "Shipper Reference";
			public const string FreightForwarderReference = "Freight Forwarder Reference";
			public const string CarrierContractNumber = "Carrier Contract Number";
			public const string QuotationNumber = "Quotation Number";
		}

		#endregion

		public static IEnumerable<AdditionalReference> ToUXmlAdditionalReferences(this IAdditionalReferenceProvider provider)
		{
			if (!provider.ShipperReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = provider.ShipperReference,
					Type = new EntryType
					{
						Code = AdditionalReferenceCodes.ShipperReference,
						Description = AdditionalReferenceDescriptions.ShipperReference
					}
				};
			}

			if (!provider.FreightForwarderReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = provider.FreightForwarderReference,
					Type = new EntryType
					{
						Code = AdditionalReferenceCodes.FreightForwarderReference,
						Description = AdditionalReferenceDescriptions.FreightForwarderReference
					}
				};
			}

			if (!provider.CarrierContractNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = provider.CarrierContractNumber,
					Type = new EntryType
					{
						Code = AdditionalReferenceCodes.CarrierContractNumber,
						Description = AdditionalReferenceDescriptions.CarrierContractNumber
					}
				};
			}

			if (!provider.QuotationNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = provider.QuotationNumber,
					Type = new EntryType
					{
						Code = AdditionalReferenceCodes.QuotationNumber,
						Description = AdditionalReferenceDescriptions.QuotationNumber
					}
				};
			}
		}

		#endregion

		#region Additional Service

		public static UniversalDataBuss.DataObjects.Universal.AdditionalService ToUXmlAdditionalService(this IAdditionalService service, IDataObjectWriterStrategy writerStrategy)
		{
			if (service == null)
			{
				return null;
			}

			return new UniversalDataBuss.DataObjects.Universal.AdditionalService
			{
				ServiceCode = service.ServiceCode?.ToUXmlCodeDescriptionPair(),
				Booked = service.Booked,
				Completed = service.Completed,
				Duration = service.Duration,
				ServiceCount = service.ServiceCount,
				ServiceNote = service.ServiceNote,
				References = service.References,
				Contractor = service.Contractor?.ToUXmlOrganizationAddress(nameof(DocAddressType.Contractor), writerStrategy),
				Location = service.Location?.ToUXmlOrganizationAddress(nameof(DocAddressType.Location), writerStrategy)
			};
		}

		#endregion

		#region GetUnknownAddress

		public static OrganizationAddress GetUnknownAddress(this DocAddressType addressType, IDataObjectWriterStrategy strategy)
		{
			var address = new OrganizationAddress(strategy);
			address.CompanyName = DocDataConstants.CertificateAddressOptions.Unknown;
			address.AddressType = addressType.ToString();
			return address;
		}

		#endregion

		#region GetCountriesOfRouting

		public static ZString GetCountriesOfRouting(ITransports transports, string origin, string destination)
		{
			if (transports != null)
			{
				void AppendNonDuplicateCountryCode(List<string> countriesOfRouting, string appendCountry)
				{
					if (!string.IsNullOrEmpty(appendCountry) && (countriesOfRouting.Count == 0 || countriesOfRouting.Last() != appendCountry))
					{
						countriesOfRouting.Add(appendCountry);
					}
				}

				var countriesOfRouting = new List<string>();
				AppendNonDuplicateCountryCode(countriesOfRouting, origin);

				foreach (var t in transports)
				{
					AppendNonDuplicateCountryCode(countriesOfRouting, t.PortOfLoading?.Country?.Code);
					AppendNonDuplicateCountryCode(countriesOfRouting, t.PortOfDischarge?.Country?.Code);
				}

				AppendNonDuplicateCountryCode(countriesOfRouting, destination);

				return string.Join("|", countriesOfRouting);
			}

			return string.Empty;
		}

		#endregion
	}
}
