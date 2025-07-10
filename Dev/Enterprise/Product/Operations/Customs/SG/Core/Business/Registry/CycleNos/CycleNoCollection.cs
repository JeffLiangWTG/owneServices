using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.SG.V4.Business.XmlSerializers")]
	public class CycleNoCollection : RegistryBusinessObjectCollectionTemplate, ICodeDescriptionPairList
	{
		public CycleNoCollection()
			: base()
		{
		}

		public CycleNoCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new CycleNo this[int i]
		{
			get { return (CycleNo)Elements[i]; }
		}

		public new CycleNo AddNew()
		{
			return (CycleNo)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CycleNo(CurrentFallbackLevel, CurrentFactory, this);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CycleNoCollection(fallbackLevel, factory);
		}

		#region Default Cycle Number

		public CycleNoCollection GetDefaultCollection
		{
			get
			{
				if (fDefaultCycleNoCollection == null)
				{
					fDefaultCycleNoCollection = new CycleNoCollection()
					{
						new CycleNo() { CycleNum = 1, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0)) },
						new CycleNo() { CycleNum = 2, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(0, 25, 0)) },
						new CycleNo() { CycleNum = 3, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(1, 10, 0)) },
						new CycleNo() { CycleNum = 4, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(1, 55, 0)) },
						new CycleNo() { CycleNum = 5, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(2, 40, 0)) },
						new CycleNo() { CycleNum = 6, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(3, 25, 0)) },
						new CycleNo() { CycleNum = 7, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(4, 10, 0)) },
						new CycleNo() { CycleNum = 8, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(4, 55, 0)) },
						new CycleNo() { CycleNum = 9, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(5, 40, 0)) },
						new CycleNo() { CycleNum = 10, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(6, 25, 0)) },
						new CycleNo() { CycleNum = 11, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(7, 10, 0)) },
						new CycleNo() { CycleNum = 12, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(7, 55, 0)) },
						new CycleNo() { CycleNum = 13, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(8, 40, 0)) },
						new CycleNo() { CycleNum = 14, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(9, 25, 0)) },
						new CycleNo() { CycleNum = 15, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(10, 10, 0)) },
						new CycleNo() { CycleNum = 16, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(10, 55, 0)) },
						new CycleNo() { CycleNum = 17, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(11, 40, 0)) },
						new CycleNo() { CycleNum = 18, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(12, 25, 0)) },
						new CycleNo() { CycleNum = 19, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(13, 10, 0)) },
						new CycleNo() { CycleNum = 20, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(13, 55, 0)) },
						new CycleNo() { CycleNum = 21, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(14, 40, 0)) },
						new CycleNo() { CycleNum = 22, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(15, 25, 0)) },
						new CycleNo() { CycleNum = 23, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(16, 10, 0)) },
						new CycleNo() { CycleNum = 24, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(16, 55, 0)) },
						new CycleNo() { CycleNum = 25, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(17, 40, 0)) },
						new CycleNo() { CycleNum = 26, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(18, 25, 0)) },
						new CycleNo() { CycleNum = 27, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(19, 10, 0)) },
						new CycleNo() { CycleNum = 28, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(19, 55, 0)) },
						new CycleNo() { CycleNum = 29, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(20, 40, 0)) },
						new CycleNo() { CycleNum = 30, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(21, 25, 0)) },
						new CycleNo() { CycleNum = 31, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(22, 10, 0)) },
						new CycleNo() { CycleNum = 32, SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(22, 55, 0)) },
					};
				}
				return fDefaultCycleNoCollection;
			}
		}
		CycleNoCollection fDefaultCycleNoCollection;

		#endregion

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return this.Cast<CycleNo>().Any(x => x.CycleNumStr == code.ToString());
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			return this.Cast<CycleNo>().FirstOrDefault(x => x.CycleNum.ToString() == code)?.SubmissionTimeStr ?? "";
		}
	}
}
