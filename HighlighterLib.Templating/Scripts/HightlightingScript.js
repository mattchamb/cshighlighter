(function() {
    var all = document.querySelectorAll("#formattedCode *");
    for (var i = 0; i < all.length; i++) {
        var element = all[i];
        if (element.data && element.data.hover) {
            var hoverId = element.data.hover;
            element.addEventListener("mouseover", function () {
                var toHighlight = document.querySelectorAll("." + hoverId);
                for(var j = 0; j < toHighlight.length; j++) {
                    toHighlight[j].classList.add("highlighted");
                }
            });

            element.addEventListener("mouseout", function () {
                var toHighlight = document.querySelectorAll("." + hoverId);
                for (var j = 0; j < toHighlight.length; j++) {
                    toHighlight[j].classList.remove("highlighted");
                }
            });
        }
    }
})();