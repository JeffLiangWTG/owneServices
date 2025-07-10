using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ItemPackaging : AutoItemPackaging, ICusCodeDataTypeSupporter
	{
		public ItemPackaging(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public ItemPackagingDataCollection ItemPackages
		{
			get
			{
				if (itemPackages == null)
				{
					itemPackages = new ItemPackagingDataCollection(this);
					itemPackages.Load();
					RegisterEditableChildObject(itemPackages);
				}

				return itemPackages;
			}
		}
		ItemPackagingDataCollection itemPackages;

		JobComInvoiceLine InvoiceLine
		{
			get { return Parent as JobComInvoiceLine; }
		}

		#region Property Overrides

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.ItemPackaging|NZ_NumberOfPackages", Caption = "Pkg. Qty")]
		public override ZInt NZ_NumberOfPackages
		{
			get { return base.NZ_NumberOfPackages; }
			set
			{
				base.NZ_NumberOfPackages = value;
				if (InvoiceLine != null)
				{
					InvoiceLine.NumberOfPackages1Info.RefreshBinding();
					InvoiceLine.ItemPackages.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.ItemPackaging|NZ_PackageUQ", Caption = "Pkg. UQ")]
		public override ZString NZ_PackageUQ
		{
			get { return base.NZ_PackageUQ.IsEmpty ? PackageTypeConverter.GetCustomsTSWPackagingType(Factory, Constants.PkgUnit.Package) : base.NZ_PackageUQ; }
			set
			{
				base.NZ_PackageUQ = value;
				if (InvoiceLine != null)
				{
					InvoiceLine.Packages1UQInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.ItemPackaging|NZ_PackageVolume", Caption = "Volume")]
		public override ZDecimal NZ_PackageVolume
		{
			get { return base.NZ_PackageVolume; }
			set
			{
				bool hasChanges = value != base.NZ_PackageVolume;
				base.NZ_PackageVolume = value;
				if (hasChanges && !IsCopying)
				{
					if (InvoiceLine != null)
					{
						InvoiceLine.PackagesVolume1Info.RefreshBinding();
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.ItemPackaging|NZ_VolumeUQ", Caption = "Vol. UQ")]
		public override ZString NZ_VolumeUQ
		{
			get { return base.NZ_VolumeUQ.IsEmpty ? (ZString)cubicMetres : base.NZ_VolumeUQ; }
			set
			{
				base.NZ_VolumeUQ = value;
			}
		}
		const string cubicMetres = "MTQ";

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.ItemPackaging|NZ_ShippingMarks", Caption = "Marks and Numbers")]
		public override ZString NZ_ShippingMarks
		{
			get { return base.NZ_ShippingMarks.IsEmpty ? (ZString)"Unknown" : base.NZ_ShippingMarks; }
			set
			{
				base.NZ_ShippingMarks = value;
				if (InvoiceLine != null)
				{
					InvoiceLine.PackagingMarks1Info.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.ItemPackaging|NZ_PackingMaterial", Caption = "Packing Material")]
		public override ZString NZ_PackingMaterial
		{
			get { return base.NZ_PackingMaterial; }
			set
			{
				base.NZ_PackingMaterial = value;
				if (InvoiceLine != null)
				{
					InvoiceLine.PackagingMaterial1Info.RefreshBinding();
				}
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZItemPackaging, typeof(ItemPackagingData));
			return result;
		}

		#endregion

		#region Packaging Defaulting

		public void SetPackagingDefaultQtyFromInvoiceQty()
		{
			if (NZ_NumberOfPackages.IsEmpty)
			{
				if (InvoiceLine.JI_InvoiceQuantity > 0 && !InvoiceLine.JI_InvoiceUQ.IsEmpty)
				{
					var invoiceQuantity = InvoiceLine.JI_InvoiceQuantity;
					if (IsPackageTypeUQ(InvoiceLine.JI_InvoiceUQ))
					{
						NZ_PackageUQ = GetTSWPackingUnitForThisPackType(InvoiceLine.JI_InvoiceUQ);
						SetNumberOfPackages(invoiceQuantity);
					}
					else if (InvoiceLine.Part != null)
					{
						// If the Invoice Line UQ is not a Package Type and the User has entered a Product.
						// Then please check the Unit Conversion table for the Product to locate a Package Type conversion and use the converted value with the selected UQ as the Packaging Quantity and Packaging UQ.
						var part = InvoiceLine.Part;
						foreach (OrgPartUnit partUnit in part.PartUnits)
						{
							if (partUnit.OF_PackType == InvoiceLine.JI_InvoiceUQ)
							{
								if (IsPackageTypeUQ(partUnit.OF_ParentPackType) && partUnit.OF_QuantityInParent > 0)
								{
									var convertedQty = invoiceQuantity / partUnit.OF_QuantityInParent;
									SetNumberOfPackages(convertedQty);
									NZ_PackageUQ = GetTSWPackingUnitForThisPackType(partUnit.OF_ParentPackType);
								}
							}
						}
					}

					if (NZ_NumberOfPackages.IsEmpty)
					{
						// if a specific freight package type is not used & a conversion cannot be found from the product - default invoice details with Pack Type of PK - Package
						NZ_PackageUQ = PackageTypeConverter.GetCustomsTSWPackagingType(Factory, Constants.PkgUnit.Package);
						SetNumberOfPackages(invoiceQuantity);
					}
				}
			}

			if (NZ_PackageUQ.IsEmpty)
			{
				NZ_PackageUQ = PackageTypeConverter.GetCustomsTSWPackagingType(Factory, Constants.PkgUnit.Package);
			}
		}

		void SetNumberOfPackages(ZDecimal decimalValue)
		{
			if (decimalValue <= int.MaxValue)
			{
				NZ_NumberOfPackages = decimalValue.Round(0).ToZInt();
			}
		}

		public void SetPackagingVolumeFromInvoiceProduct(bool hasChanges)
		{
			if (NZ_PackageVolume.IsEmpty || hasChanges)
			{
				var volume = InvoiceLine.JI_Volume;
				var volumeUQ = InvoiceLine.JI_VolumeUQ;
				if (volume > 0 && !volumeUQ.IsEmpty)
				{
					if (InvoiceLine.JI_VolumeUQ == Enterprise.Core.Constants.Volume.CubicMetres)
					{
						NZ_PackageVolume = volume;
					}
					else
					{
						NZ_PackageVolume = Enterprise.Core.Constants.Volume.ConvertSafe(volume, volumeUQ, Enterprise.Core.Constants.Volume.CubicMetres);
					}
				}
			}
		}

		protected ZString GetTSWPackingUnitForThisPackType(ZString freightPackType)
		{
			return PackageTypeConverter.GetCustomsTSWPackagingType(Factory, freightPackType);
		}

		static bool IsPackageTypeUQ(string unitOfQty)
		{
			bool result = false;
			switch (unitOfQty)
			{
				case Constants.PkgUnit.Bag:
				case Constants.PkgUnit.BulkBag:
				case Constants.PkgUnit.BreakBulk:
				case Constants.PkgUnit.BaleCompressed:
				case Constants.PkgUnit.BaleUncompressed:
				case Constants.PkgUnit.Bundle:
				case Constants.PkgUnit.Bottle:
				case Constants.PkgUnit.Box:
				case Constants.PkgUnit.Basket:
				case Constants.PkgUnit.Case:
				case "CS":
				case Constants.PkgUnit.Container:
				case Constants.PkgUnit.Coil:
				case Constants.PkgUnit.Crate:
				case Constants.PkgUnit.Carton:
				case Constants.PkgUnit.Cylinder:
				case Constants.PkgUnit.Drum:
				case Constants.PkgUnit.Envelope:
				case "GOH":
				case Constants.PkgUnit.Keg:
				case Constants.PkgUnit.Mix:
				case Constants.PkgUnit.Pail:
				case Constants.PkgUnit.Package:
				case "PAC":
				case Constants.PkgUnit.Pallet:
				case Constants.PkgUnit.Reel:
				case Constants.PkgUnit.Roll:
				case Constants.PkgUnit.Sheet:
				case Constants.PkgUnit.Skid:
				case Constants.PkgUnit.Spool:
				case Constants.PkgUnit.Tube:
				case Constants.PkgUnit.Unit:
				case Constants.PkgUnit.Piece:
				case "EA":
				case "NMB":
				case "NO":
				case "PCS":
				case "TE":
					result = true;
					break;
			}

			return result;
		}

		#endregion
	}
}
