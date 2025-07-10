// Julien
function init() 
{
	if (isUserAgentWith("Windows CE")) {// Windows CE basis or Windows Mobile
		if (isUserAgentWith("IEMobile")) {// Only for Windows Mobile
			document.getElementById('wm').style.display = "block";
		} else {
			document.getElementById('ce').style.display = "block";
		}
	} else {

		document.getElementById('wm').style.display = "block";
		document.getElementById('ce').style.display = "block";

		document.getElementById('error').style.display = "block";
		document.getElementById('error-message').innerHTML = "Your device is not compatible for this installation. You can still download the installers in order to copy them to a compatible device.";

		document.getElementById('error-details-message').innerHTML = "User Agent: " + navigator.userAgent;
		document.getElementById('error-details-title').onclick = function () {
			toggleDisplay('error-details-message');
		};
	}
}

function isUserAgentWith(text) 
{
	return navigator.userAgent.indexOf(text) > -1;
}

function toggleDisplay(eltId) 
{
	var elt = document.getElementById(eltId);
	elt.style.display = elt.style.display == "block" ? "none" : "block";
}
