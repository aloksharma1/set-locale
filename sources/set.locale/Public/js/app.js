﻿var textBtnDanger = "btn-danger",
	textBtnSuccess = "btn-success",
	textId = "id",
	textIsActive = "isactive",
	allLanguages = [{ id: 'tr', text: 'Türkçe' },
		{ id: 'en', text: 'English' },
		{ id: 'sp', text: 'Español' },
		{ id: 'cn', text: '中文 (zhōngwén)' },
		{ id: 'ru', text: 'Русский язык' },
		{ id: 'fr', text: 'Français' },
		{ id: 'gr', text: 'Deutsch' },
		{ id: 'it', text: 'Italiano' },
		{ id: 'az', text: 'Azərbaycan dili' },
		{ id: 'tk', text: 'түркmенче (türkmençe)' },
		{ id: 'kz', text: 'Қазақ тілі' }];

$(function () {
    $("a.btnAction").click(function () {
        var $self = $(this),
			id = $self.data(textId),
			isActive = $self.data(textIsActive);

        $("input#btnModalAction").data(textId, id).data(textIsActive, isActive);
    });
});


// FEEDBACK
$(function () {
	var fbRetMsg = $("#feedbackReturnMessage");
    $('#btnSaveFeedback').click(function () {
        var message = $("#FeedbackMessage").val();
        
        if (message.length < 1) { fbRetMsg.html('<label class="error">*</label>'); return; }

        fbRetMsg.html(null);

        $.post('/Feedback/New', { message: message }, function (result) {
            if (result.IsOk) {
                fbRetMsg.html('<div class="alert alert-success alert-dismissable"><span>Thanks for feedback.</span></div>');
                setTimeout(function () {
                    $("#modalFeedback").modal('hide');
                    $("#FeedbackMessage").val(null);
                }, 2000);
            } else {
                fbRetMsg.html('<div class="alert alert-warning alert-dismissable">' +
                    '<button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' +
                    '<strong>Ups! </strong> ' + result.Msg + '</div>');
            }
        });
    });
    $('#modalFeedback').on('hidden.bs.modal', function () { fbRetMsg.html(null); $("#Feedback").val(''); $("label.error").remove(); });
});

// SEARCH
$(function () {
    $("#txtSearch").focusout(function () {
        if ($('.popopver').length < 1) {
            $('#txtSearch').popover('hide');
        }
    });

    var queryString, highlightRow;
    $('#txtSearch').keyup(function (key) {
        if (key.which == 37 || key.which == 39) return;

        if (key.which == 40) {
            highlightRow = $('.popover-content').find('.row.highlight');
            if (highlightRow.length == 0) {
                $('.popover-content>.row:first').addClass('highlight');
            } else {
                var nextHighlight = $('.highlight').next('.row');
                if (nextHighlight.length > 0) {
                    $('.highlight').removeClass('highlight');
                    nextHighlight.addClass('highlight');
                }
            }
        } else if (key.which == 38) {
            highlightRow = $('.popover-content').find('.row.highlight');
            if (highlightRow.length == 0) {
                $('.popover-content>.row:last').addClass('highlight');
            } else {
                var prevHighlight = $('.highlight').prev('.row');
                if (prevHighlight.length > 0) {
                    $('.highlight').removeClass('highlight');
                    prevHighlight.addClass('highlight');
                }
            }
        } else if (key.which == 13) {
            highlightRow = $('.highlight');
            if (highlightRow.length > 0) {
                location.href = highlightRow.find('a').attr('href');

                if (queryString != undefined) {
                    queryString.abort();
                }
            }
        } else {
            queryString = $(this).val();
            if (queryString.length > 1) {
                $('.popover-content:visible').html(null);
                $('#txtSearch').popover({ content: window.textPleaseWait, title: "", placement: "bottom", container: "body", html: "true" }).popover('show');

                $.get('/search/query', { text: queryString }, function (r) {
                    $('.popover-content:visible').html(null);
                    if (r.IsOk) {
                        if (r.Result.length == 0) {
                            showNoResultForSearch();
                        } else {
                            $.each(r.Result, function () {
                                var row = $('<div class="row" onclick="location.href=' + "'" + this.Url + "'" + '" style="padding:5px;margin-left:-5px;margin-right:-5px;margin-top:-8px;cursor:pointer;border-bottom: solid 1px #2B3E50;"></div>');
                                var li = $('<div class="col-sm-12"></div>').append('<a href="' + this.Url + '"><span class="label label-warning">' + this.Tag + '</span></br> ' + this.Name + '</a>');
                                row.append(li);

                                $('.popover-content:visible').append(row);
                            });

                            $('.popover-content .row').mouseover(function () {
                                $(this).addClass('highlight');
                            });
                            $('.popover-content .row').mouseout(function () {
                                $(this).removeClass('highlight');
                            });
                        }
                    } else {
                        showNoResultForSearch();
                    }
                });
            }
        }
    });

    function showNoResultForSearch() {
        $('.popover-content:visible').html(window.textNoResult);
    }
});
