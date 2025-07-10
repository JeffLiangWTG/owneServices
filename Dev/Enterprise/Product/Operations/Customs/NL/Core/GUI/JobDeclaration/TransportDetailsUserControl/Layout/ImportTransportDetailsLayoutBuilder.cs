using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI;

public class ImportTransportDetailsLayoutBuilder : TransportDetailsLayoutBuilder<JobDeclaration>
{
	protected override int MaxColumns => 1;

	NLTransportDetailsControlBag nlBag { get; } = NLTransportDetailsControlBag.Instance;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		var transportModeDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInfo };
		var transportModeInlandDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_MessageTypeInfo, x => x.JE_TransportModeInlandInfo };

		SetVisibility(CommonBag.TransportIDAndNationalityUserControl, jobDec => !jobDec.IsAir && !jobDec.IsSea, jobDec => jobDec.JE_MessageTypeInfo);
		SetVisibility(nlBag.FlightAndNationalityUserControl, x => x.IsAir, transportModeDependency);
		SetVisibility(CommonBag.VoyageAndNationalityUserControl, x => x.IsSea, transportModeDependency);
		SetVisibility(CommonBag.TransportInlandIDAndNationalityUserControl, x => (x.JE_TransportModeInland.IsEmpty || (!x.IsRoadInland && !x.IsSeaInland && !IsMailOrFixedInstallationInland(x))), transportModeInlandDependency);
		SetVisibility(CommonBag.TransportInlandModeAndTypeOfIdUserControl, x => !IsMailOrFixedInstallationInland(x), transportModeInlandDependency);
		SetVisibility(NLTransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl, x => IsMailOrFixedInstallationInland(x), transportModeInlandDependency);
		SetVisibility(NLTransportDetailsControlBag.Instance.ImportTransportInlandRoadUserControl, x => x.IsRoadInland, transportModeInlandDependency);
		SetVisibility(CommonBag.TransportInlandSeaUserControl, x => x.IsSeaInland, transportModeInlandDependency);
	}

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();

		SetTransportInlandModeAndTypeOfIdUserControlCaptions();
		SetInlandUserControlsCaptions();
		SetVoyageAndNationalityUserControlCaptions();
		SetTransportAndNationalityUserControlCaptions();

		SetCaption(CommonBag.MasterBillTextBox, x => JobDeclarationInlandTransportResDataHelper.MasterBillResString, x => x.JE_TransportModeInfo);
		SetCaption(CommonBag.OceanBillTextBox, x => JobDeclarationInlandTransportResDataHelper.OceanBillResString, x => x.JE_TransportModeInfo);
	}

	void SetInlandUserControlsCaptions()
	{
		SetCaptions(
			NLTransportDetailsControlBag.Instance.ImportTransportInlandRoadUserControl,
			j => GetInlandCaptionDictionary(j, ImportTransportInlandRoadUserControl.ControlNames.TransportIDTextBox, ImportTransportInlandRoadUserControl.ControlNames.TransportNationalityCodeFindBox),
			j => j.JE_TransportMeansInfo
		);
		SetCaptions(
			CommonBag.TransportInlandSeaUserControl,
			j => GetInlandCaptionDictionary(j, TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox, TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox),
			j => j.JE_TransportMeansInfo
		);
		SetCaptions(
			CommonBag.TransportInlandIDAndNationalityUserControl,
			j => GetInlandCaptionDictionary(j, TransportInlandIDAndNationalityUserControl.ControlNames.TransportIDTextBox, TransportInlandIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox),
			j => j.JE_TransportMeansInfo
		);
	}

	Dictionary<string, ResourceStringData> GetInlandCaptionDictionary(JobDeclaration declaration, string idControlName, string nationalityControlName)
	{
		var result = new Dictionary<string, ResourceStringData>();
		if (!string.IsNullOrEmpty(idControlName))
		{
			result.Add(idControlName, JobDeclarationInlandTransportResDataHelper.GetInlandTransactionIDCaption(declaration));
		}

		if (!string.IsNullOrEmpty(nationalityControlName))
		{
			result.Add(nationalityControlName, JobDeclarationInlandTransportResDataHelper.InlandTransportNationalityCodeFindBoxResString);
		}

		return result;
	}

	void SetTransportInlandModeAndTypeOfIdUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit, JobDeclarationInlandTransportResDataHelper.InlandModeOfTransportResString },
			{ Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.TypeOfIDDropEdit, JobDeclarationInlandTransportResDataHelper.InlandTransportCodeResString },
		};

		SetCaptions(CommonBag.TransportInlandModeAndTypeOfIdUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	void SetVoyageAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.VoyageAndNationalityUserControl.ControlNames.VoyageNumberTextBox, JobDeclarationInlandTransportResDataHelper.VoyageResString },
		};

		SetCaptions(CommonBag.VoyageAndNationalityUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	void SetTransportAndNationalityUserControlCaptions()
	{
		var caption = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox, JobDeclarationInlandTransportResDataHelper.TransportIdResString },
		};

		SetCaptions(CommonBag.TransportIDAndNationalityUserControl, _ => caption, x => x.JE_TransportModeInfo);
	}

	bool IsMailOrFixedInstallationInland(JobDeclaration jobDeclaration)
	{
		return jobDeclaration.IsMailInland || jobDeclaration.IsFixedInstallationInland;
	}
}
