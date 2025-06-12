/* 
* eHub v1.0.0 - eHub Framework 
* Date:2014-10-20 
*/
(function ($) {
	$.widget("ehub.combobox", {
		_create: function () {
			this.wrapper = $("<span>")
          .addClass("ehub-combobox")
          .insertAfter(this.element);

			this._createAutocomplete();
			this._createShowAllButton();
		},

		_createAutocomplete: function () {
			this.element.appendTo(this.wrapper).addClass("ehub-combobox-input");
		},

		_createShowAllButton: function () {
			var input = this.element;

			$("<div>").insertAfter(this.element).attr("tabIndex", -1).attr("title", "Show All Items").button({
				icons: { primary: "ui-icon-triangle-1-s" },
				text: false
			}).removeClass("ui-corner-all").addClass("ehub-combobox-toggle ui-corner-right").attr("id", input.attr('id') + '_button')
			.click(function () {
				var wasOpen = input.autocomplete("widget").is(":visible");
				// Close if already visible
				if (wasOpen) {
					input.blur();
					return;
				}
				// Pass empty string as value to search for, displaying all results
				input.focus();
				input.autocomplete("search", "");
			});
		}
	});
})(jQuery);

(function () {
	$.ehubToast = function (message, type, options) {
		options = $.extend(true, {}, $.ehubToast.defaultOptions, options);

		var html = '<div class="ehub-toast alert alert-' + (type ? type : options.type) + ' ' + (options.customClass ? options.customClass : '') + '">';
		if (options.allowDismiss)
			html += '<span class="close" data-dismiss="alert">&times;</span>';
		html += message;
		html += '</div>';

		var offsetSum = options.offset.amount;
		if ($('.ehub-toast').length >= options.offset.amount) {
			for(var i = 0; i < $('.ehub-toast').length - options.offset.amount; i++) {
				$.ehubToast.remove($('.ehub-toast').first());
			}
		}

		$('.ehub-toast').each(function () {
			return offsetSum = Math.max(offsetSum, parseInt($(this).css(options.offset.from)) + this.offsetHeight + options.spacing);
		});

		var css =
		{
			'position': (options.appendTo === 'body' ? 'fixed' : 'absolute'),
			'margin': 0,
			'z-index': '9999',
			'display': 'none',
			'min-width': options.minWidth,
			'max-width': options.maxWidth
		};

		css[options.offset.from] = offsetSum + 'px';

		var $alert = $(html).css(css)
							.appendTo(options.appendTo);

		switch (options.align) {
			case "center":
				$alert.css(
				{
					"left": "50%",
					"margin-left": "-" + ($alert.outerWidth() / 2) + "px"
				});
				break;
			case "left":
				$alert.css("left", "20px");
				break;
			default:
				$alert.css("right", "20px");
		}

		if ($alert.fadeIn) $alert.fadeIn();
		else $alert.css({ display: 'block', opacity: 1 });

		function removeAlert() {
			$.ehubToast.remove($alert);
		}

		if (options.delay > 0) {
			setTimeout(removeAlert, options.delay);
		}

		$alert.find("[data-dismiss=\"alert\"]").removeAttr('data-dismiss').click(removeAlert);

		return $alert;
	};

	$.ehubToast.remove = function ($alert) {
		if ($alert.fadeOut) {
			$alert.fadeOut(function () {
				$alert.remove();
				if (parseInt($('.ehub-toast').first().css($.ehubToast.defaultOptions.offset.from)) > 4) {
					$('.ehub-toast').each(function () {
						$(this).css($.ehubToast.defaultOptions.offset.from, parseInt($(this).css($.ehubToast.defaultOptions.offset.from)) - $.ehubToast.defaultOptions.distance - $.ehubToast.defaultOptions.spacing);
					});
				}
			});
		}
		else {
			$alert.remove();
			if (parseInt($('.ehub-toast').first().css($.ehubToast.defaultOptions.offset.from)) > $.ehubToast.defaultOptions.distance) {
			$('.ehub-toast').each(function () {
				$(this).css($.ehubToast.defaultOptions.offset.from, parseInt($(this).css($.ehubToast.defaultOptions.offset.from)) - $.ehubToast.defaultOptions.distance - $.ehubToast.defaultOptions.spacing);
			});
		}
		}
	};

	$.ehubToast.defaultOptions = {
		appendTo: "body",
		customClass: false,
		type: "black",
		offset:
		{
			from: "bottom",
			amount: 5
		},
		align: "center",
		delay: 10000,
		allowDismiss: true,
		spacing: 3,
		distance: 31
	};
})();


// Will upgade in eHub.class to get ajax update when scrolling ***************** IMPORTANT ***********************
//source: function (request, response) {
//	if (lastClient == clientElement.val()) {
//		response(data);
//		return;
//	}
//	$.ajax({
//		type: "POST",
//		url: '<%: Url.Action("ProvidersDropDown") %>',
//		dataType: "json",
//		data: {
//			CC_ID: clientElement.val()
//		},
//		success: function (d) {
//			//if (lastClient == clientElement.val()) return true;
//			//lastClient = clientElement.val();
//			console.log('ajax');
//			data = d;
//			lastClient = clientElement.val();
//			response(d);
//		}
//	});
//}