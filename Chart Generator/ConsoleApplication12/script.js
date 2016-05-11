// Polyfill
if (!String.prototype.startsWith) {
    String.prototype.startsWith = function (searchString, position) {
        position = position || 0;
        return this.substr(position, searchString.length) === searchString;
    };
}

// Code
var system = require("system");
var webpage = require("webpage");
var page = webpage.create();
page.onConsoleMessage = function (msg, line, source) { console.log("" + msg); };

var render = function (width, height, outputPath, onSuccess) {
    console.log("Rendering to " + outputPath);
    var svg = page.evaluate(function () {
        var element = document.getElementById("chart_wrapper");
        if (element && element.classList.contains("drawn")) {
            return element.innerHTML;
        }

        return null;
    });

    if (!svg) {
        window.setTimeout(function () { render(width, height, outputPath, onSuccess); }, 10);
        return;
    }

    page.viewportSize = { width: width, height: height };
    page.clipRect = {
        top: 0,
        left: 0,
        width: width,
        height: height
    };

    window.setTimeout(function () {
        page.render(outputPath);
        onSuccess(svg);
    }, 10);
};

function readInput() {
    var input = system.stdin.readLine();
    if (input) {
        //console.log("command: " + input);

        if (input === "phantom.exit()") {
            phantom.exit();
            return;
        }

        if (input.startsWith("renderChart ")) { // renderChart <width> <height> <ouput_path>
            var split = input.split(" ");
            var indexof = input.indexOf(' ');
            indexof = input.indexOf(' ', indexof + 1);
            indexof = input.indexOf(' ', indexof + 1);
            var outputPath = input.substring(indexof + 1);
            render(parseInt(split[1], 10), parseInt(split[2], 10), outputPath, function (content) {
                console.log("MC0001: " + content);
                readInput();
            });
            return;
        } else {
            page.evaluateJavaScript(input);
            readInput();
            return;
        }
    }

    readInput();
};

page.open("about:blank", function (status) {
    //console.log("Page loaded: " + status);
    page.includeJs("https://www.gstatic.com/charts/loader.js", function () {
        //console.log("Script included");

        page.evaluate(function () {
            google.charts.load('current', { packages: ['corechart'] });
        });

        readInput();
    });
});