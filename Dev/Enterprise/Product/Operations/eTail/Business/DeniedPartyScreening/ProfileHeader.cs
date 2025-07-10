using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class ProfileHeader : NonPersistentBusinessObject
	{
		public ProfileHeader(ProfileHeaderInfo headerInfo, DpsResponse response, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(headerInfo, nameof(headerInfo));
			Argument.NotNull(response, nameof(response));
			Argument.NotNull(factory, nameof(factory));

			ProfileHeaderInfo = headerInfo;
			NameMatchCollection = new NameMatchCollection(headerInfo, response);
			AddressMatchCollection = new AddressMatchCollection(headerInfo, response);
			CountryMatchCollection = new CountryMatchCollection(headerInfo, response);
			RegistrationMatchCollection = new RegistrationMatchCollection(headerInfo, response);
			SourceListNamesModel = new SourceListNamesModel(factory, ProfileHeaderInfo.SourceListCodes?.ToArray() ?? Array.Empty<string>());
		}

		public ZString Name
		{
			get
			{
				var profileNames = ProfileHeaderInfo.ProfileNames;
				var result = profileNames?.FirstOrDefault(x => x.IsPrimaryName)?.FullName
					?? profileNames?.FirstOrDefault()?.FullName
					?? Res.GetString("e9d3a011-85c6-4b10-b474-17173effedb6", "Unknown");

				return result;
			}
		}

		public ZInt Score =>
			MostMatchedName?.Score
			?? MostMatchedAddress?.Score
			?? MostMatchedCountry?.Score
			?? MostMatchedRegistrationCode?.Score
			?? 0;

		public ZString Title
		{
			get
			{
				var result = ZString.Empty;
				switch (Level)
				{
					case RiskLevel.High:
					result = HighRishTitle;
					break;
					case RiskLevel.Medium:
					result = MediumRishTitle;
					break;
					case RiskLevel.Low:
					break;
				}

				return result;
			}
		}

		public ZString LevelOfRisk => Level.ToString();

		public ZBool ShowTitle => Level != RiskLevel.Low;

		public ZString Type => ProfileHeaderInfo.TypeOfEntity;

		static ZString HighRishTitle => Res.GetString("667302a1-78d6-4a62-ac7e-9533823b57e4", "High Risk - Review Required");

		static ZString MediumRishTitle => Res.GetString("d0aff9fa-da0b-4364-b01c-daa54c2f39a6", "Medium Risk - Review Required");

		public RiskLevel Level
		{
			get
			{
				var result = RiskLevel.Low;
				if (NameMatchCollection.OfType<NameMatch>().Any(x => x.Level == RiskLevel.High) ||
					RegistrationMatchCollection.OfType<RegistrationMatch>().Any(x => x.Level == RiskLevel.High) ||
					Type == DeniedPartyConstants.ScreeningNameTypes.Country && (AddressMatchCollection.OfType<AddressMatch>().Any(x => x.Level == RiskLevel.High) || CountryMatchCollection.Count > 0) ||
					NameMatchCollection.OfType<NameMatch>().Any(x => x.Level == RiskLevel.Medium) && AddressMatchCollection.OfType<AddressMatch>().Any(x => x.Level == RiskLevel.High))
				{
					result = RiskLevel.High;
				}
				else if (NameMatchCollection.OfType<NameMatch>().Any(x => x.Level == RiskLevel.Medium) || AddressMatchCollection.OfType<AddressMatch>().Any(x => x.Level == RiskLevel.High))
				{
					result = RiskLevel.Medium;
				}

				return result;
			}
		}

		public bool IsValid => SourceListNamesModel.IncludedLists.Length + SourceListNamesModel.ExcludedLists.Length > 0;

		public bool IsExcluded => SourceListNamesModel.IncludedLists.Length == 0;

		public enum RiskLevel
		{
			Low,
			Medium,
			High
		}

		NameMatch MostMatchedName => NameMatchCollection.OfType<NameMatch>().OrderBy(n => n.Score).LastOrDefault();
		AddressMatch MostMatchedAddress => AddressMatchCollection.OfType<AddressMatch>().OrderBy(n => n.Score).LastOrDefault();
		CountryMatch MostMatchedCountry => CountryMatchCollection.OfType<CountryMatch>().OrderBy(n => n.Score).LastOrDefault();
		RegistrationMatch MostMatchedRegistrationCode => RegistrationMatchCollection.OfType<RegistrationMatch>().OrderBy(n => n.Score).LastOrDefault();

		public ProfileHeaderInfo ProfileHeaderInfo { get; }

		public SourceListNamesModel SourceListNamesModel { get; }

		public NameMatchCollection NameMatchCollection { get; }

		[ResourceStringData("1b9e8f11-4bb0-4d49-bcaa-4ea7a7762ac1", Caption = "Notes")]
		public ZBlob Notes
		{
			get
			{
				if (notes  == null)
				{
					using (var msi = new MemoryStream(ProfileHeaderInfo.ProfileNotes))
					using (var mso = new MemoryStream())
					{
						using (var gs = new GZipStream(msi, CompressionMode.Decompress))
						{
							CopyTo(gs, mso);
						}

						return mso.ToArray();
					}

					static void CopyTo(Stream src, Stream dest)
					{
						var bytes = new byte[4096];
						int cnt;
						while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
						{
							dest.Write(bytes, 0, cnt);
						}
					}
				}

				return notes;
			}
		}
		readonly ZBlob notes;

		public RefComplianceListCollection SourceListNames
		{
			get
			{
				if (sourceListNames == null)
				{
					sourceListNames = new RefComplianceListCollection(Factory, new ZQuery(RefComplianceListSchema.RCL_ListCode, ProfileHeaderInfo.SourceListCodes ?? Array.Empty<string>()));
				}

				return sourceListNames;
			}
		}
		RefComplianceListCollection sourceListNames;

		public AddressMatchCollection AddressMatchCollection { get; }

		public CountryMatchCollection CountryMatchCollection { get; }

		public RegistrationMatchCollection RegistrationMatchCollection { get; }
	}
}
