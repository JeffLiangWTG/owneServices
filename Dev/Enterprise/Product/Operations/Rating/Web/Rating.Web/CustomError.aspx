<%@ Page Language="C#" %>
<%
	// This custom error page is loaded for requests that land within the
	// ASP.NET pipeline but outside the OWIN request pipeline, eg:
	//
	// * Missing .aspx file (returns 404)
	// * Potentially dangerous request eg: "/asterisk/*/path" (returns 400)

	int statusCode;
	if (!int.TryParse(Request.QueryString["statusCode"], out statusCode)) {
		statusCode = 500;
	}
	Response.StatusCode = statusCode;

	Response.ContentType = "text/plain";
	var message = Request.QueryString["message"] ?? "Status code " + statusCode.ToString();
	Response.Write(message);
	Response.End();
%>
