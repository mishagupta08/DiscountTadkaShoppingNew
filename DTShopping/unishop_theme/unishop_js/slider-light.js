$(window).load(function() { /* makes sure the whole site is loaded */
    $("#status").fadeOut(); /* will first fade out the loading animation */
    $("#preloader").delay(350).fadeOut("slow"); /* will fade out the white DIV that covers the website. */
})

jQuery(function() {
    jQuery('#layerslider').layerSlider({
        skinsPath : 'skins/',
        skin : 'light'
    });

})
$(window).resize(function(){
    jQuery('.ls-nav-next').click();
});
