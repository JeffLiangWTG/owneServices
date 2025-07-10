using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AdditionalAddInfoGroupCollectionDataObjectReaderForInvoiceLineWHSPackLine : DataObjectReader, IAdditionalAddInfoGroupCollectionDataObjectReader
	{
		public AdditionalAddInfoGroupCollectionDataObjectReaderForInvoiceLineWHSPackLine(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger)
		{
			this.helper = Argument.NotNull(helper, "helper");
		}
		readonly UniversalDataObjectReaderHelper helper;

		#region IAdditionalAddInfoGroupCollectionDataObjectReader Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Process(IEnumerable<UniversalCustoms.AddInfoGroup> addInfoGroupCollection, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase)
		{
			var invoiceLine = parentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? helper.Load<JobComInvoiceLine>(parentPK) : null;
			var invoiceLineRow = GetColumnIndexer(invoiceLine);
			var invoiceRow = GetColumnIndexer(invoiceLineRow == null ? null : helper.Load<JobComInvoiceHeader>(invoiceLineRow, JobComInvoiceLineSchema.JI_JZ));
			var declaration = invoiceRow == null ? null : helper.Load<JobDeclaration>(invoiceRow, JobComInvoiceHeaderSchema.JZ_JE);
			if (declaration != null)
			{
				var declarationPK = declaration.PK;
				var declarationIsInDatabase = declaration.IsInDatabase;
				foreach (var addInfoGroup in addInfoGroupCollection)
				{
					var packageID = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, logger);
					if (packageID.HasValue)
					{
						var query = new ZQuery(CusAddInfoSchema.B7_ParentID, declarationPK);
						query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USWHSPack);
						query.FetchOnlyFromLocalCache = !declarationIsInDatabase;
						var matchingPackageReference = string.Format(CultureInfo.InvariantCulture, "*{0}={1}*", USWHSPackAddInfo.Schema.US_PackageReference.Substring(3), packageID.Value);
						var packRow = GetColumnIndexer(helper.Load<WHSPack>(query).FirstOrDefault(x => ("*" + x.B7_AddInfoData + "*").Contains(matchingPackageReference)));
						if (packRow == null)
						{
							logger.Log(Integration.LogType.Warning, ResString.GetMultilingualString("{2ECE51D8-5D1D-4B5B-BDD3-197168174B6D}", "Cannot create WHS Pack Line for Invoice Line (Product:{0}, {4}:{1}, {5}:{2}) as no matching WHS Package ({3}) was found.", invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_PartNo), invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceNumber), invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK), packageID.Value, "InvoiceNumber", "LineNo"));
						}
						else
						{
							var packPK = packRow.GetValue(CusAddInfoSchema.PK);
							var invoiceLinePK = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
							var packLineQuery = new ZQuery(CusAddInfoSchema.B7_ParentID, declarationPK);
							packLineQuery.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USWHSPackLine);
							packLineQuery.FetchOnlyFromLocalCache = !declarationIsInDatabase;
							var packLineRows = helper.Load<WHSPackLine>(packLineQuery).Select(x => GetColumnIndexer(x)).Where(x =>
								{
									var addInfos = x.GetAddInfos(CusAddInfoSchema.B7_AddInfoData);
									return addInfos.GetValue(USWHSPackLineAddInfoSchema.US_JI_InvoiceLine) == invoiceLinePK && addInfos.GetValue(USWHSPackLineAddInfoSchema.US_B7_WHSPack) == packPK;
								}).ToArray();
							var packLineRow = packLineRows.Length > 0 ? packLineRows[0] : null;
							var isDefaultingEnabled = IsDefaultingEnabled;
							if (packLineRow == null)
							{
								packLineRow = GetColumnIndexer(helper.Factory.New<WHSPackLine>());
								packLineRow.SetValue(CusAddInfoSchema.B7_ParentID, declarationPK);
								packLineRow.SetValue(CusAddInfoSchema.B7_ParentTableCode, (ZString)JobDeclarationSchema.Constants.Prefix);
								packLineRow.SetValue(CusAddInfoSchema.B7_Type, (ZString)CusAddInfoTypeAttribute.Codes.USWHSPackLine);
								if (isDefaultingEnabled)
								{
									SetValue(packLineRow, USWHSPackLineAddInfoSchema.US_B7_WHSPack, packPK);
								}
								else
								{
									packLineRow.SetValue(CusAddInfoSchema.B7_AddInfoData, (ZString)(USWHSPackLineAddInfo.Schema.US_B7_WHSPack.Substring(3) + "=" + packPK.ToString()));
								}
							}
							var packedQty = addInfoGroup.AddInfoCollection.GetZDecimalValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty, logger);
							if (IsDefaultingEnabled)
							{
								SetValue(packLineRow, USWHSPackLineAddInfoSchema.US_JI_InvoiceLine, invoiceLinePK);
								SetValue(packLineRow, USWHSPackLineAddInfoSchema.US_PackedQty, packedQty);
							}
							else
							{
								var addInfos = packLineRow.GetAddInfos(CusAddInfoSchema.B7_AddInfoData);
								addInfos.Update(USWHSPackLineAddInfoSchema.Constants.US_JI_InvoiceLine.Substring(3), invoiceLinePK);
								if (packedQty.HasValue)
								{
									addInfos.Update(USWHSPackLineAddInfoSchema.Constants.US_PackedQty.Substring(3), packedQty.Value);
								}
								SetValue(packLineRow, CusAddInfoSchema.B7_AddInfoData, AddInfoParser.Serialise(addInfos));
							}
						}
					}
					else
					{
						logger.Log(Integration.LogType.Warning, ResString.GetMultilingualString("21E1F750-47EF-40B9-800B-E69BE314F89D", "Cannot create WHS Pack Line for Invoice Line (Product:{0}, {3}:{1}, {4}:{2}) as no Package ID was specified.", invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_PartNo), invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceNumber), invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK), "InvoiceNumber", "LineNo"));
					}
				}
			}
		}

		#endregion
	}
}
