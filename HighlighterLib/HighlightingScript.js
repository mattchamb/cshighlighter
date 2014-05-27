(function () {

    var hoverAttrName = "data-hover";
    var highlightClassName = "highlighted";

    var mouseOver = function (element) {
        return function () {
            var hoverId = element.getAttribute(hoverAttrName);
            var toHighlight = document.querySelectorAll("." + hoverId);
            for (var j = 0; j < toHighlight.length; j++) {
                toHighlight[j].classList.add(highlightClassName);
            }
        };
    };

    var mouseOut = function(element) {
        return function () {
            var hoverId = element.getAttribute(hoverAttrName);
            var toHighlight = document.querySelectorAll("." + hoverId);
            for (var j = 0; j < toHighlight.length; j++) {
                toHighlight[j].classList.remove(highlightClassName);
            }
        };
    };

    var all = document.querySelectorAll("#formattedCode *");
    for (var i = 0; i < all.length; i++) {
        var element = all[i];
        if (element.hasAttribute(hoverAttrName)) {
            element.addEventListener("mouseover", mouseOver(element));

            element.addEventListener("mouseout", mouseOut(element));
        }
    }
})();