using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class CertificationCodeMapping : RegistryBusinessObject
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string MainCode = "MainCode";
			public const string SpecialisationCode = "SpecialisationCode";
		}

		#endregion

		public CertificationCodeMapping()
		{ }

		public CertificationCodeMapping(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override bool IsCodeMandatory => false;

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CertificationCodeMapping();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate toClone)
		{
			base.CopyValuesToClone(toClone);

			CertificationCodeMapping clone = (CertificationCodeMapping)toClone;
			clone.CodeMaxLength = CodeMaxLength;
			clone.MainCode = MainCode;
			clone.SpecialisationCode = SpecialisationCode;
		}

		#endregion

		#region Properties

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString MainCode
		{
			get { return mainCode; }
			set
			{
				CheckMaximumLength(MainCodeInfo, value);
				SetNonPersistentPropertyValue(MainCodeInfo, ref mainCode, value);
				if (!IsValidationSuspended)
				{
					ValidateMainCode();
				}
			}
		}
		ZString mainCode;

		public ZPropertyInfo MainCodeInfo
		{
			get { return GetZPropertyInfo(Schema.MainCode); }
		}

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString SpecialisationCode
		{
			get { return specialisationCode; }
			set
			{
				CheckMaximumLength(SpecialisationCodeInfo, value);
				SetNonPersistentPropertyValue(SpecialisationCodeInfo, ref specialisationCode, value);
				if (!IsValidationSuspended)
				{
					ValidateSpecialisationCode();
				}
			}
		}
		ZString specialisationCode;

		public ZPropertyInfo SpecialisationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SpecialisationCode); }
		}

		#endregion

		#region Validation

		static string errorSameCode => Res.GetString("3D27369E-A5F4-4049-8A9A-39E8C5AC84AE", "You cannot have the same code in both cells");
		static string errorDuplicatePair => Res.GetString("D597330F-216F-4B59-8B9B-6B97343C238B", "The pair of codes has duplicates");
		static string errorMultipleLevels => Res.GetString("75B83ACE-4607-43DA-9BB0-5A8454BDAE73", "You cannot have multiple levels of related codes, i.e a single code cannot appear as both Main and Specialization code");
		static string errorSpecCodeNotUnique => Res.GetString("F45164AB-3AAA-4499-BEE6-55A0F5A6DA3B", "Specialization code can have only one parent Main code");

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMainCode();
			ValidateSpecialisationCode();
		}

		public void ValidateMainCode()
		{
			MainCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(MainCodeInfo, MainCodeList);

			if (!MainCode.IsEmpty && MainCode == SpecialisationCode)
			{
				MainCodeInfo.AddError(errorSameCode);
			}
			else if (HasDuplicatePair)
			{
				MainCodeInfo.AddError(errorDuplicatePair);
			}
			else if (HasMultipleLevels)
			{
				MainCodeInfo.AddError(errorMultipleLevels);
			}
		}

		public void ValidateSpecialisationCode()
		{
			SpecialisationCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(SpecialisationCodeInfo, SpecialisationCodeList);
			if (!SpecialisationCode.IsEmpty && MainCode == SpecialisationCode)
			{
				SpecialisationCodeInfo.AddError(errorSameCode);
			}
			else if (HasDuplicatePair)
			{
				SpecialisationCodeInfo.AddError(errorDuplicatePair);
			}
			else if (HasMultipleLevels)
			{
				SpecialisationCodeInfo.AddError(errorMultipleLevels);
			}
			else if (SpecCodeNotUnique)
			{
				SpecialisationCodeInfo.AddError(errorSpecCodeNotUnique);
			}
		}

		bool SpecCodeNotUnique
		{
			get
			{
				foreach (var collection in (this as IBusinessObjectInternals).ParentCollections)
				{
					var col = collection as CertificationCodeMappingCollection;
					if (col != null)
					{
						foreach (CertificationCodeMapping mapping in col)
						{
							if (mapping.PK != PK && mapping.SpecialisationCode == SpecialisationCode)
							{
								return true;
							}
						}

						return false;
					}
				}

				return false;
			}
		}

		bool HasDuplicatePair
		{
			get
			{
				foreach (var collection in (this as IBusinessObjectInternals).ParentCollections)
				{
					var col = collection as CertificationCodeMappingCollection;
					if (col != null)
					{
						foreach (CertificationCodeMapping mapping in col)
						{
							if (mapping.PK != PK && mapping.MainCode == MainCode && mapping.SpecialisationCode == SpecialisationCode)
							{
								return true;
							}
						}

						return false;
					}
				}

				return false;
			}
		}

		bool HasMultipleLevels
		{
			get
			{
				foreach (var collection in (this as IBusinessObjectInternals).ParentCollections)
				{
					var col = collection as CertificationCodeMappingCollection;
					if (col != null)
					{
						foreach (CertificationCodeMapping mapping in col)
						{
							if (mapping.PK != PK && (mapping.MainCode == SpecialisationCode || mapping.SpecialisationCode == MainCode))
							{
								return true;
							}
						}

						return false;
					}
				}

				return false;
			}
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList MainCodeList
		{
			get
			{
				if (mainCodeList == null)
				{
					mainCodeList = new CodeDescriptionPairList();
					FillValuesFromRegistry(mainCodeList);
				}

				return mainCodeList;
			}
		}

		void FillValuesFromRegistry(CodeDescriptionPairList list)
		{
			foreach (CodeDescriptionBool item in RecruiterDataRegistry.Instance.CertificateTypesExtra.Value)
			{
				list.AddPair(item.Code, item.Description);
			}
		}

		CodeDescriptionPairList mainCodeList;

		public CodeDescriptionPairList SpecialisationCodeList
		{
			get
			{
				if (specialisationCodeList == null)
				{
					specialisationCodeList = new CodeDescriptionPairList();
					FillValuesFromRegistry(specialisationCodeList);
				}

				return specialisationCodeList;
			}
		}
		CodeDescriptionPairList specialisationCodeList;

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			MainCode = new ZString(reader.ReadElementString(Schema.MainCode));

			SpecialisationCode = reader.IsStartElement(Schema.SpecialisationCode)
				? new ZString(reader.ReadElementString(Schema.SpecialisationCode))
				: ZString.Empty;
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.MainCode, MainCode.ToString());
			writer.WriteElementString(Schema.SpecialisationCode, SpecialisationCode.ToString());
		}

		#endregion
	}
}
