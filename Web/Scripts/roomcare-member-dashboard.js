(function ($) {
    var $toggle = $("#sidebarToggle");
    var $sidebar = $("#" + $toggle.attr("aria-controls"));

    function showPanel(name) {
        var $panel = $("#panel-" + name);
        if (!$panel.length) {
            return;
        }

        $(".rc-workspace-panel").removeClass("is-active");
        $panel.addClass("is-active");
        $(".rc-sidebar-nav button").removeClass("is-active");
        $('.rc-sidebar-nav button[data-panel-target="' + name + '"]').addClass("is-active");
        $sidebar.removeClass("is-open");
        $("#sidebarBackdrop").removeClass("is-visible");
        $toggle.attr("aria-expanded", "false");
        window.scrollTo(0, 0);
    }

    $("[data-panel-target]").on("click", function () {
        showPanel($(this).data("panel-target"));
    });

    $("#sidebarToggle, #sidebarBackdrop").on("click", function () {
        var isOpen = !$sidebar.hasClass("is-open");
        $sidebar.toggleClass("is-open", isOpen);
        $("#sidebarBackdrop").toggleClass("is-visible", isOpen);
        $toggle.attr("aria-expanded", isOpen ? "true" : "false");
    });
})(jQuery);
