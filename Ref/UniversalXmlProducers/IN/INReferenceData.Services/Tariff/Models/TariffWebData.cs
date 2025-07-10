using System;

namespace CargoWise.RefDbRepo.INReferenceData.Services.Tariff.Models
{
	public class TariffWebData
	{
		public int id { get; set; }
		public string isActive { get; set; }
		public int orderId { get; set; }
		public DateTime createdDt { get; set; }
		public int versionNo { get; set; }
		public string isPublished { get; set; }
		public string publishDt { get; set; }
		public string titleEn { get; set; }
		public string titleHi { get; set; }
		public string path { get; set; }
		public DateTime contentDt { get; set; }
		public int year { get; set; }
		public int viewType { get; set; }
		public Contentviewconfigs contentViewConfigs { get; set; }
		public Tableviewconfig[] tableViewConfigs { get; set; }
		public Contenttype contentType { get; set; }
		public Childcontentlist[] childContentList { get; set; }
		public Cbicdocmst[] cbicDocMsts { get; set; }
	}

	public class Contentviewconfigs
	{
		public int id { get; set; }
		public int contentId { get; set; }
		public int tableViewConfigId { get; set; }
		public string dataOrderedBy { get; set; }
		public int isOrderReversed { get; set; }
	}

	public class Contenttype
	{
		public int id { get; set; }
		public object isActive { get; set; }
		public object orderId { get; set; }
		public object createdDt { get; set; }
		public object updatedDt { get; set; }
		public object updateBy { get; set; }
		public object versionNo { get; set; }
		public object parentId { get; set; }
		public object contentType { get; set; }
		public object titleEn { get; set; }
		public object titleHi { get; set; }
		public object infoEn { get; set; }
		public object infoHi { get; set; }
		public object path { get; set; }
	}

	public class Tableviewconfig
	{
		public int id { get; set; }
		public object contentId { get; set; }
		public string titleEn { get; set; }
		public string titleHi { get; set; }
		public string valColumnEn { get; set; }
		public string valColumnHi { get; set; }
		public string isActive { get; set; }
		public int orderId { get; set; }
		public object colStyle { get; set; }
		public object displayTextLimit { get; set; }
	}

	public class Childcontentlist
	{
		public int id { get; set; }
		public string isActive { get; set; }
		public int orderId { get; set; }
		public DateTime createdDt { get; set; }
		public DateTime updatedDt { get; set; }
		public string publishDt { get; set; }
		public DateTime archiveDt { get; set; }
		public int parentId { get; set; }
		public string titleEn { get; set; }
		public string path { get; set; }
		public DateTime contentDt { get; set; }
		public DateTime refDt1 { get; set; }
		public DateTime refDt2 { get; set; }
		public DateTime expiryDt { get; set; }
		public int year { get; set; }
		public int viewType { get; set; }
		public int childContentCount { get; set; }
		public Contenttype contentType { get; set; }
		public Cbicdocmst[] cbicDocMsts { get; set; }
		public int updateBy { get; set; }
		public int versionNo { get; set; }
		public int taxId { get; set; }
		public int locationId { get; set; }
	}

	public class Cbicdocmst
	{
		public int id { get; set; }
		public string isActive { get; set; }
		public int orderId { get; set; }
		public string docType { get; set; }
		public string docTitleEn { get; set; }
		public string filePathEn { get; set; }
	}
}
