using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class DOT : AutoDOT
		, IDOT
		, ICusAddInfoTypeSupporter
	{
		public DOT(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "DOT"; }
		}

		public override ZString US_DOTBoxNo
		{
			get { return base.US_DOTBoxNo; }
			set
			{
				base.US_DOTBoxNo = value;
				if (US_DOTCountryOfOrigin.IsEmpty)
				{
					if (US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._05 || US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._06 || US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._12)
					{
						if (InvoiceLine != null)
						{
							US_DOTCountryOfOrigin = InvoiceLine.US_UC_NKCountryOfOrigin;
						}
					}
				}
			}
		}

		#endregion

		#region IDOT Members

		ZString IDOT.CommercialDescription
		{
			get { return US_DOTCommercialDesc; }
		}

		ZString IDOT.BoxNumber
		{
			get { return US_DOTBoxNo; }
		}

		ZString IDOT.BoxCertification
		{
			get { return "Y"; }
		}

		ZString IDOT.PassportNumber
		{
			get { return US_DOTPassport; }
		}

		ZString IDOT.CountryISO
		{
			get { return US_DOTCountryOfOrigin; }
		}

		ZString IDOT.DOTBondSuretyCode
		{
			get { return US_DOTBondSuretyCode; }
		}

		ZBool IDOT.NHTSAPermissionLetterOfficialOrdersCertification
		{
			get { return US_DOTPriorApproval; }
		}

		ZBool IDOT.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter
		{
			get { return US_DOTImpSubstStatement; }
		}

		ZString IDOT.ClarificationCode
		{
			get { return US_DOTClarCode; }
		}

		ZString IDOT.TireManufacturerIDCode
		{
			get { return US_DOTTireID; }
		}

		ZString IDOT.TireManufacturerBrandName
		{
			get { return US_DOTTireBrandName; }
		}

		IEnumerable<IDOTVIN> IDOT.VINs
		{
			get
			{
				foreach (IDOTVIN dotvin in DOTVINs)
				{
					yield return dotvin;
				}
			}
		}

		ZString IOGALine.CommercialDesc
		{
			get { return US_DOTCommercialDesc; }
			set { US_DOTCommercialDesc = value; }
		}

		#endregion

		#region DOTVINs

		[ChildEditable(true)]
		public DOTVINCollection DOTVINs
		{
			get
			{
				if (dotvins == null)
				{
					dotvins = new DOTVINCollection(this);
					dotvins.Load();
					RegisterEditableChildObject(dotvins);
				}
				return dotvins;
			}
		}
		DOTVINCollection dotvins;

		#endregion

		#region Override

		public override void Delete()
		{
			base.Delete();
			DOTVINs.RemoveAndDeleteAll();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			DOT result = (DOT)base.CloneInternal(args);

			foreach (DOTVIN dotvin in DOTVINs)
			{
				result.DOTVINs.Add((DOTVIN)dotvin.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(DOTVIN), false)));
			}

			return result;
		}

		#endregion

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDOTVIN, typeof(DOTVIN));
			return result;
		}

		#endregion
	}
}
