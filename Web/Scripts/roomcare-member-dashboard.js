(function ($) {
    var $toggle = $("#sidebarToggle");
    var $sidebar = $("#" + $toggle.attr("aria-controls"));

    function showPanel(name) {
        var $panel = $("#panel-" + name);
        if (!$panel.length) {
            return;
        }

        $(".rc-workspace-panel").removeClass("is-active is-entering");
        $panel.addClass("is-active");
        if (!window.matchMedia || !window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            $panel.addClass("is-entering");
        }
        $(".rc-sidebar-nav button").removeClass("is-active");
        $('.rc-sidebar-nav button[data-panel-target="' + name + '"]').addClass("is-active");
        $sidebar.removeClass("is-open");
        $("#sidebarBackdrop").removeClass("is-visible");
        $toggle.attr("aria-expanded", "false");
        window.scrollTo(0, 0);
    }

    function showLoadingBar() {
        $("#pageLoadingBar").addClass("is-visible").attr("aria-hidden", "false");
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

    $("a[href]").on("click", function (event) {
        var link = this;
        var href = link.getAttribute("href");
        if (event.isDefaultPrevented() ||
            event.which > 1 ||
            event.ctrlKey ||
            event.metaKey ||
            event.shiftKey ||
            event.altKey ||
            (link.target && link.target !== "_self") ||
            link.hasAttribute("download") ||
            !href ||
            href.charAt(0) === "#" ||
            link.protocol !== window.location.protocol ||
            link.host !== window.location.host) {
            return;
        }

        showLoadingBar();
    });

    $(".rc-workspace-panel").on("animationend webkitAnimationEnd", function () {
        $(this).removeClass("is-entering");
    });

    $(".rc-member-update-form, .rc-inline-form").on("submit", function (event) {
        var $form = $(this);
        if ($form.hasClass("is-loading")) {
            event.preventDefault();
            return;
        }

        var submitButton = event.originalEvent && event.originalEvent.submitter;
        if (!submitButton || !$.contains(this, submitButton)) {
            submitButton = this.querySelector("button[type='submit'], button:not([type]), input[type='submit']");
        }

        $form.addClass("is-loading").attr("aria-busy", "true");
        showLoadingBar();
        $("main").attr("aria-busy", "true");

        if (submitButton) {
            var $button = $(submitButton);
            $button.attr("data-original-label", $button.is("input") ? $button.val() : $button.text());
            $button.prop("disabled", true).attr("aria-busy", "true");
            if ($button.is("input")) {
                $button.val("Đang xử lý...");
            } else {
                $button.text("Đang xử lý...");
            }
        }
    });

    $(window).on("pageshow", function () {
        $(".rc-member-update-form, .rc-inline-form")
            .removeClass("is-loading")
            .removeAttr("aria-busy");
        $(".rc-member-update-form :submit, .rc-inline-form :submit").each(function () {
            var $button = $(this);
            var label = $button.attr("data-original-label");
            if (label !== undefined) {
                if ($button.is("input")) {
                    $button.val(label);
                } else {
                    $button.text(label);
                }
                $button.removeAttr("data-original-label");
                $button.prop("disabled", false);
            }
            $button.removeAttr("aria-busy");
        });
        $("#pageLoadingBar").removeClass("is-visible").attr("aria-hidden", "true");
        $("main").removeAttr("aria-busy");
    });
})(jQuery);
