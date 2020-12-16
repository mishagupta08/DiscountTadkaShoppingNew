$(document).ready(function () {
    $("#offline").hide();
    $("#dtcard").hide();
    $("#walletPaymentModes").hide();
    $("#cardPaymentMode").hide();

    $(".preloader").hide();
    $("button[name=saveButton]").attr("disabled", true);
    $("button[name=saveButton]").addClass("disabled");

    $('#stateList').unbind();
    $('#stateList').change(function (e) {
        GetCityByState();
    });

    $("#payWithPayu").click(function (e) {
        window.location = "/Manage/GetPaymentWindow?netPayment=" + $("#Amount").val();
    });

    //$('.ps-slider').unbind();
    //$('.ui-state-default').mouseup(function (e) {
    //    alert("Id: " + $(this).attr("id") + " Value: " + $(this).val());
    //    window.location = $("#PointsUrl").val() + "&FilterFromPoint=" + $('#minPointValue').html() + "&FilterToPoint=" + $('#maxPointValue').html();
    //});

    $('a[name=deleteFunction]').unbind();
    $('a[name=deleteFunction]').click(function (e) {
        DeleteDetail(this);
    });

    $('input[name=pointsCheckBox]').click(function () {
        var points = "";
        var idList = "";
        $("input:checkbox[name=pointsCheckBox]:checked").each(function () {
            //alert("Id: " + $(this).attr("id") + " Value: " + $(this).val());
            points += $(this).val() + ",";
            idList += $(this).attr("id") + ",";
        });

        window.location = $("#PointsUrl").val() + points + "&searchString=" + idList;
    });

    $('input[name=termsCondition]').change(function () {
        $("button[name=saveButton]").attr("disabled", true);
        $("button[name=saveButton]").addClass("disabled");
        if ($(this).is(":checked")) {
            var btnId = $(this).val();
            $("#" + btnId).attr("disabled", false);
            $("#" + btnId).removeClass("disabled");
        }
    });

    $('a[name=updateQuantity]').unbind();
    $('a[name=updateQuantity]').click(function (e) {
        debugger;
        UpdateQuantityDetail(this);
    });

    $('#lnkOtp').click(function (e) {
        GenerateOtp(this);
    });
    $('a[name=lnkOtp]').click(function (e) {
        GenerateOtpCommonFunction(this);
    });

    $('#walletPaymodeDropDown').change(function (e) {
        $("#Paytm").hide();
        $("#phonePay").hide();
        $(".Bank").hide();
        var paymode = $(this).val();
        if (paymode == "7") {
            $("#Paytm").show();
        }
        else if (paymode == "8") {
            $("#phonePay").show();
        }
        else {
            $(".Bank").show();
        }
    });

    $('#pointSaveBtn').click(function (e) {
        SaveOrderDetailWithPoint();
    });

    $('#ConfirmPassword').click(function (e) {
        ConfirmPassword();
    });

    //$('input:radio[name=walletTypeBtn]')[0].checked = true;

    //$('input[name=paymentmethod]').unbind();
    //$('input[name=paymentmethod]').click(function (e) {
    //    LoadPaymentPage(this);
    //});

    $('input[name = paymentmethod]').bind('change', function () {
        var value = $(this).val();
        $("#offline").hide();
        $("#dtcard").hide();
        $("#dtcardOTP").hide();
        $("#walletPaymentModes").hide();
        $("#cardPaymentMode").hide();

        $("#" + value).show();
    });

    $('#Pay1').bind('click', function () {
        var isChecked = document.getElementById("Pay1").checked;
        if (isChecked) {
            var totalCharge = parseInt($("#totalCharge").val());
            var userPoints = parseInt($("#userPoints").val());
            if (totalCharge <= userPoints) {
                $("#paymentOptions").hide();
            }
            else {

                $("input[name=pointAdjusted1]").val(userPoints);
                $("input[name=pointAdjusted2]").val(userPoints);
                $("#paymentOptions").show();
            }
        }
        else {
            $("#paymentOptions").show();
        }
    });
});

function GenerateOtpCommonFunction(thisvar) {
    $("#loginError").html("");
    //var passwordDetail = $('#lnkOtp').val();
    var elementId = $(thisvar).attr("data-value");
    $(".preloader").show();
    $.ajax({
        url: '/Manage/GenerateOtpDetail',
        type: 'Post',
        datatype: 'Json',
        data: {}
    }).done(function (result) {
        $("#" + elementId).html(result);
        $(".preloader").hide();

    }).fail(function (error) {
        $("#loginError").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}

function GenerateOtp(thisvar) {
    $("#loginError").html("");
    //var passwordDetail = $('#lnkOtp').val();
    $(".preloader").show();
    $.ajax({
        url: '/Manage/GenerateOtpDetail',
        type: 'Post',
        datatype: 'Json',
        data: {}
    }).done(function (result) {
        $("#otpMessage").html(result);
        $(".preloader").hide();

    }).fail(function (error) {
        $("#loginError").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}

function SaveDetailFormOtp() {
    $("#loginError").html("");
    var loginDetail = $('#addFormOTP').serialize();
    $(".preloader").show();
    $.ajax({
        url: '/Manage/SaveDetailFormOtp',
        type: 'Post',
        datatype: 'Json',
        data: loginDetail
    }).done(function (result) {
        if (result == "ok") {
            window.location = "/Manage/thankYouPage";
        }
        else {
            $("#loginError2").html(result);
        }
        $(".preloader").hide();

    }).fail(function (error) {
        $("#loginError1").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}


function ConfirmPassword() {
    $(".preloader").show();
    $("#loginError").html("");
    var loginDetail = $('#addForm1').serialize();
    $.ajax({
        url: '/Manage/ConfirmPassword',
        type: 'Post',
        async: false,
        datatype: 'Json',
        data: loginDetail
    }).done(function (result) {
        if (result.Data == "ok") {
            window.location = "/Manage/thankYouPage";
        }
        else {
            var walletAmount = 0;
            if (result.indexOf(':') > 0) {
                var dataArray = result.split(':');
                walletAmount = dataArray[1];
                result = dataArray[0];
            }

            document.getElementById("walletAmount").innerHTML = walletAmount;
            //$("#WalletTypeHidden").val($("input[name='walletTypeBtn']:checked").val());
            if (result == "Not Found") {
                $("#loginError1").html("Password does not match.");
            }
            else if (result == "ok") {
                window.location = "/Manage/thankYouPage";
            }
            else if (result == "Sufficient") {
                $("#dtcard").hide();
                $("#dtcardOTP").show();
            }
            else if (result == "InSufficient") {
                $("#loginError1").html("User Exists but does not have sufficient balance in wallet.<br/> Current Wallet Balance : " + walletAmount);
            }
            else {
                $("#loginError1").html(result);
            }
        }
        $(".preloader").hide();

    }).fail(function (error) {
        $("#loginError1").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}



function SaveOrderDetailWithPoint() {
    $("#loginError").html("");
    var loginDetail = $('#addForm').serialize();
    $(".preloader").show();
    $.ajax({
        url: '/Manage/SaveOrderDetailWithPoints',
        type: 'Post',
        datatype: 'Json',
        data: loginDetail
    }).done(function (result) {
        $("#loginError").html(result);
        $(".preloader").hide();

    }).fail(function (error) {
        $("#loginError").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}

function SaveDetailForm(isTrue) {
    $("#loginError").html("");
    var loginDetail = $('#addForm').serialize();

    if (isTrue) {
        loginDetail = $('#addFormWallet').serialize();
    }

    $(".preloader").show();
    $.ajax({
        url: '/Manage/UpdateOrderDetail',
        type: 'Post',
        datatype: 'Json',
        data: loginDetail
    }).done(function (result) {
        if (result == "ok") {
            window.location = "/Manage/thankYouPage";
        }
        else {
            if (isTrue) {
                $("#loginErrorW").html(result);
            }
            else {
                $("#loginError").html(result);
            }
            $(".preloader").hide();
        }

    }).fail(function (error) {
        $("#loginError").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}

function UpdateShippingCharge(dlivryTpe) {

    $(".preloader").show();
    window.location.href = "/Manage/GetCartProductList?isWithPayment=false&deliveryType=" + dlivryTpe;
    //$.ajax({
    //    url: '/Manage/UpdateShippingChargeDetail',
    //    type: 'Post',
    //    datatype: 'Json',
    //    data: { deliveryType: dlivryTpe }
    //}).done(function (result) {
    //    $(".preloader").hide();
    //    //location.reload();
    //    window.location.href = "/Manage/GetCartProductList?isWithPayment=false&deliveryType=" + dlivryTpe;

    //}).fail(function (error) {
    //    $("#error").html(error.statusText);
    //    $(".preloader").hide();
    //});

    return false;
}

function UpdateQuantityDetail(thisVar) {
    debugger;
    $(".preloader").show();
    var idVal = $(thisVar).attr("data-id");
    var id = "#productQuantity_" + idVal;
    var radioVal = $("input[type=radio]:checked").val();
    var qty = $(id).val();
    //var qty = $("#quantityID").val();
    $.ajax({
        url: '/Manage/UpdateProductQuantityDetail',
        type: 'Post',
        datatype: 'Json',
        data: { prodId: idVal, quantity: qty, deliveryType: radioVal }
    }).done(function (result) {
        $(".preloader").hide();
        alert(result);
        location.reload();
    }).fail(function (error) {
        $("#error").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}

function GetCityByState() {
    var id = $("#stateList").val();
    $(".preloader").show();
    $.ajax({
        url: '/Home/GetCityListByState',
        type: 'Post',
        datatype: 'Json',
        data: { Id: id }
    }).done(function (result) {
        if (result == null || result == undefined || result == "") {
            $("#error").html(result);
        }
        else {

            $("#cityList").html("");
            $.each(result, function (key, value) {
                $("#cityList").append($("<option></option>").val(value.cityID).html(value.cityName));
            });
        }

        $(".preloader").hide();
    });
}


function DeleteDetail(thisVar) {

    $(".preloader").show();
    var idVal = $(thisVar).attr("data-id");

    $.ajax({
        url: '/Manage/DeleteCartProduct',
        type: 'Post',
        datatype: 'Json',
        data: { prodId: idVal }
    }).done(function (result) {
        $(".preloader").hide();
        alert(result);
        location.reload();
    }).fail(function (error) {
        $("#error").html(error.statusText);
        $(".preloader").hide();
    });

    return false;
}