/*
** nopCommerce custom js functions
*/



function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function setLocation(url) {
    window.location.href = url;
}

function displayAjaxLoading(display) {
    if (display) {
        $('.ajax-loading-block-window').show();
    }
    else {
        $('.ajax-loading-block-window').hide('slow');
    }
}

function displayPopupNotification(message, messagetype, modal) {
    //types: success, error, warning
    var container;
    if (messagetype == 'success') {
        //success
        container = $('#dialog-notifications-success');
    }
    else if (messagetype == 'error') {
        //error
        container = $('#dialog-notifications-error');
    }
    else if (messagetype == 'warning') {
        //warning
        container = $('#dialog-notifications-warning');
    }
    else {
        //other
        container = $('#dialog-notifications-success');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }

    container.html(htmlcode);

    var isModal = (modal ? true : false);
    container.dialog({
        modal: isModal,
        width: 350
    });
}
function displayJoinedPopupNotifications(notes) {
    if (Object.keys(notes).length === 0) return;

    var container = $('#dialog-notifications-success');
    var htmlcode = document.createElement('div');

    for (var note in notes) {
        if (notes.hasOwnProperty(note)) {
            var messages = notes[note];

            for (var i = 0; i < messages.length; ++i) {
                var elem = document.createElement("div");
                elem.innerHTML = messages[i];
                elem.classList.add('popup-notification');
                elem.classList.add(note);

                htmlcode.append(elem);
            }
        }
    }

    container.html(htmlcode);
    container.dialog({
        width: 350,
        modal: true
    });
}
function displayPopupContentFromUrl(url, title, modal, width) {
    var isModal = (modal ? true : false);
    var targetWidth = (width ? width : 550);
    var maxHeight = $(window).height() - 20;

    $('<div></div>').load(url)
        .dialog({
            modal: isModal,
            width: targetWidth,
            maxHeight: maxHeight,
            title: title,
            close: function (event, ui) {
                $(this).dialog('destroy').remove();
            }
        });
}

function displayBarNotification(message, messagetype, timeout) {
    var notificationTimeout;

    var messages = typeof message === 'string' ? [message] : message;
    if (messages.length === 0)
        return;

    //types: success, error, warning
    var cssclass = ['success', 'error', 'warning'].indexOf(messagetype) !== -1 ? messagetype : 'success';

    //remove previous CSS classes and notifications
    $('#bar-notification')
        .removeClass('success')
        .removeClass('error')
        .removeClass('warning');
    $('.bar-notification').remove();

    //add new notifications
    var htmlcode = document.createElement('div');

    //IE11 Does not support miltiple parameters for the add() & remove() methods
    htmlcode.classList.add('bar-notification', cssclass);
    htmlcode.classList.add(cssclass);

    //add close button for notification
    var close = document.createElement('span');
    close.classList.add('close');
    close.setAttribute('title', document.getElementById('bar-notification').dataset.close);

    for (var i = 0; i < messages.length; i++) {
        var content = document.createElement('p');
        content.classList.add('content');
        content.innerHTML = messages[i];

        htmlcode.appendChild(content);
    }

    htmlcode.appendChild(close);

    $('#bar-notification')
        .append(htmlcode);

    $(htmlcode)
        .fadeIn('slow')
        .on('mouseenter', function () {
            clearTimeout(notificationTimeout);
        });

    //callback for notification removing
    var removeNoteItem = function () {
        $(htmlcode).remove();
    };

    $(close).on('click', function () {
        $(htmlcode).fadeOut('slow', removeNoteItem);
    });

    //timeout (if set)
    if (timeout > 0) {
        notificationTimeout = setTimeout(function () {
            $(htmlcode).fadeOut('slow', removeNoteItem);
        }, timeout);
    }
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}


// CSRF (XSRF) security
function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};

function AddReview(url) {
    $.post(url, $("#product-details-form").serialize(), function (data) {
        $(".main-product-review-page").html(data);
    });
}

function EditReview(productId, reviewId, reviewText, rating) {
    //$("#AddProductReview_ReviewText").val(reviewText);
    //$("#addproductrating_" + rating).prop('checked', 'checked');
    //$("#AddProductReview_ReviewId").val(reviewId);
    //$("#AddProductReview_ReviewProductId").val(productId);
    //$("#add-review").hide();
    //$("#edit-review").show();
    //$("#cancel-review").show();
    //$(".update-review-module").show();
    //$(".write-review-module").hide();

    $.get('/product/ProductReviewsList', { productId: productId, reviewId: reviewId }, function (data) {
        $(".main-product-review-page").html(data);
    });
}

function CancelEditReview() {
    $("#AddProductReview_ReviewText").val('');
    $("#addproductrating_5").prop('checked', 'checked');
    $("#AddProductReview_ReviewId").val(0);
    $("#add-reviews").show();
    $("#edit-review").hide();
    $("#cancel-review").hide();
    $(".update-review-module").hide();
    $(".write-review-module").show();
}

function SubmitReviewEdit() {
    var productId = $("#AddProductReview_ReviewProductId").val();
    var reviewId = $("#AddProductReview_ReviewId").val();
    var reviewText = $("#AddProductReview_ReviewText").val();
    var rating = $('input[name="AddProductReview.Rating"]:checked').val();

    $.post('/product/editreview', $("#product-details-form").serialize(), function (data) {
        $(".main-product-review-page").html(data);
        CancelEditReview();
    });
}

function DeleteReview(reviewId) {
    if (confirm("Are you sure want to remove this record?")) {
        $.post('/product/deletereview', { reviewId: reviewId }, function (data) {
            $(".main-product-review-page").html(data);
            CancelEditReview();
        });
    }
}

function addQuestion(productId) {
    var question = $("#Question").val();
    if (question == '') {
        $("#Question").focus();
        return;
    }

    $.post('/product/AddQuestion', { productId: productId, question: question }, function (data) {
        if (data.success) {
            $("#Question").val('');
            $(".submitQuestionResult").text(data.message);
        } else {
            $("#Question").val('');
            $(".submitQuestionResult").text(data.message);
        }
    });
}