function ExecuteTrackingServiceMethod(methodName, parameters) {
    try {
        Enterprise.Tracking.Web.ServerServices.TrackingWebService.Execute(methodName, parameters, HandleServiceMethodResponse);
    }
    catch (ex) { alert(ex); }
}