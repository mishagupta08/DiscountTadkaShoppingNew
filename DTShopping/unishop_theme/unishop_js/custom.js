/*
function TopSlider(){
    var k1=0.5;
    var k2=0.6;
    var w0=jQuery(window).width();
    if(w0 > 1600) { k2=0.55}
    var w2= w0*k1;
    var w1= w0*k2;
    var w3= (w0-jQuery(".container").width())*0.5;
    var h1=w2/1.9;
    if(h1 < 235) {h1=235;}

    jQuery("#slider_top").css({"height":h1+"px"});
    jQuery("#slider_top").css({"width":w0+"px"});
    jQuery("#carousel1 li").css({"width":w0+"px"});
    jQuery(".overlap_widget_wrapper").css({"width":w0+"px"});

    jQuery("#slider_top a.callbacks_nav.next").css({"right":w3+"px","top":h1*0.5-jQuery("#slider_top a.callbacks_nav.next").height()*0.5+"px"});
    jQuery("#slider_top a.callbacks_nav.prev").css({"left":w3+"px","top":h1*0.5-jQuery("#slider_top a.callbacks_nav.next").height()*0.5+"px"});
    jQuery("#carousel1 .overlap_widget_wrapper .left_image .title").css({"left":w3+"px"});
    jQuery("#carousel1 .overlap_widget_wrapper .right_image .title").css({"right":w3+"px"});
    jQuery(".overlap_widget_wrapper .left_image").css({"width":w1+"px"});
    jQuery(".overlap_widget_wrapper .left_image .placeholder").css({"width":w2+"px"});
    jQuery(".overlap_widget_wrapper .right_image").css({"width":w1+"px"});
    jQuery(".overlap_widget_wrapper .right_image .placeholder").css({"width":w2+"px"});

    var is_open = false;
    var z_index = 0;

    jQuery(".placeholder").mouseenter(function(){
        is_open = true;
        jQuery(this).parent().css({"zIndex":"999"});
        jQuery(this).stop().animate({
            "width":w1+"px"
        }, 550, 'easeOutQuad');
    });

    jQuery(".placeholder").mouseleave(function(){
        is_open = false;
        z_index++;
        jQuery(this).parent().css({"zIndex":z_index});
        jQuery(this).stop().animate({
            "width":w2+"px"
        }, 550, 'easeOutQuad');
    });
}
*/


function validate() {
    var mail=/^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
    if(!mail.test(document.index_newsletter.email_address.value)) {
        alert("Please Enter Your  Mail id Properly");
        document.index_newsletter.email_address.focus();
        return false;
    }
}


jQuery(document).ready(function () {

    jQuery("#back-top").css({"bottom":jQuery("#footer").height()+150+"px"});  


    jQuery('#nav_block_head').click(function() {
        jQuery('.nav_block_dropdown').toggleClass('visible_on');
    });

    jQuery('#menu_block_head').click(function() {
        jQuery('.menu_block_dropdown').toggleClass('visible_on');
    });


    jQuery('#nav_block_head1').click(function() {
        jQuery('.nav_block_dropdown1').toggleClass('visible_on');
    });






    jQuery("#back-top").hide();
    jQuery(window).scroll(function () {
            if (jQuery(this).scrollTop() > 600) {
                jQuery('#back-top').fadeIn();
            } else {
                jQuery('#back-top').fadeOut();
            }
    });
    jQuery('#back-top a').click(function () {
            jQuery('body,html').animate({
                scrollTop: 0
            }, 400);
            return false;
    });

    jQuery('#mycarousel').jcarousel({
        vertical: true,
        scroll: 1,
        auto:2,
        wrap: 'circular',
        animation: 1000,
        easing: 'linear'
    });


    jQuery('#carousel_bestsellers').elastislide({
        easing		: 'easeInOutQuad',
        speed		: 1200
    });

    jQuery('#carousel_brands').elastislide({
        easing		: 'easeInOutQuad',
        speed		: 1200
    });

    jQuery('#carousel_also').elastislide({
        easing		: 'easeInOutQuad',
        speed		: 1200
    });

jQuery('#carousel_hotselling').elastislide({
        easing		: 'easeInOutQuad',
        speed		: 1200
});

jQuery('#carousel_sevensaver').elastislide({
    easing: 'easeInOutQuad',
    speed: 1200
});

jQuery('#carousel_fashion').elastislide({
    easing: 'easeInOutQuad',
    speed: 1200
});
jQuery('#carousel_electronics').elastislide({
    easing: 'easeInOutQuad',
    speed: 1200
});






    jQuery(".shopping_cart div.open").live('click',function() {
        jQuery("#shopping_cart_mini").fadeToggle(300, "linear");
    });
    jQuery("#shopping_cart_mini").mouseleave(function() {
        jQuery("#shopping_cart_mini").fadeOut("fast", "linear");
    });

    jQuery(".product").hover(function() {
        jQuery(this).find(".roll_over_img").fadeToggle("fast", "linear");
        jQuery(this).find(".product-image-wrapper-hover").fadeToggle("fast", "linear");
    });

    jQuery(".product-list-wrapper").hover(function() {
        jQuery(this).find(".roll_over_img").fadeToggle("fast", "linear");
    });


});


//jQuery.noConflict();