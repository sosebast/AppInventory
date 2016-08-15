/*!
 * Common.js
 * Version: 1.0.0
 * Common resuable functions
 * Copyright 2016 Soju Sebastian
 */

var barChart = function (ctx, labelName, labels, values, filters, displayXAxis, title, type) {

    if (displayXAxis == undefined)
        displayXAxis = true;

    var displayTitle = true;
    if (title == undefined) {
        title = "";
        displayTitle = false;
    }

    if (type == undefined)
        type = 'bar';



    var colors = dynamicColors(values.length)

    var data = {
        labels: labels,
        datasets: [
            {
                label: labelName,
                backgroundColor: colors.backColors,
                hoverBackgroundColor: colors.highColors,
                borderColor: colors.borders,
                hoverBorderColor: colors.borders,
                borderWidth: 1,
                data: values
            }
        ]
    };

    var displayYAxis = true;
    var scalePosition = 'bottom';
    if (type != 'bar')
    {
        displayYAxis = displayXAxis;
        displayXAxis = true;
        scalePosition = 'top';
    }

    var barChart = new Chart(ctx, {
        type: type,
        data: data,
        options: {
            scales: {
                xAxes: [{
                    stacked: true,
                    display: displayXAxis,
                    position: scalePosition
                }],
                yAxes: [{
                    stacked: true,
                    display: displayYAxis
                }]
            },
            title: {
                display: displayTitle,
                text: title
            },
            legend: {
                display: false
            }
        }
    });

    if (filters != null) {
        ctx.click(
           function (evt) {
               var activePoints = barChart.getElementAtEvent(evt);
               if (activePoints.length > 0) {
                   var index = activePoints[0]["_index"];
                   location.href = filters[index];
               }
           });
    }

    return barChart;
}



var piChart = function (ctx, labelName, labels, values, filters) {
    var colors = dynamicColors(values.length)

    var data = {
        labels: labels,
        datasets: [
            {
                label: labelName,
                backgroundColor: colors.backColors,
                hoverBackgroundColor: colors.highColors,
                borderColor: colors.borders,
                hoverBorderColor: colors.borders,
                borderWidth: 1,
                data: values
            }
        ]
    };

    var pieChart = new Chart(ctx, {
        type: "pie",
        data: data
    });


    if (filters != null) {
        ctx.click(
           function (evt) {
               var activePoints = pieChart.getElementAtEvent(evt);
               if (activePoints.length > 0) {
                   var index = activePoints[0]["_index"];
                   location.href = filters[index];
               }
           });
    }

    return pieChart;
}

var dynamicColors = function (count) {
    var backColors = [];
    var highColors = [];
    var borders = [];

    for (var i = 0; i < count; i++) {
        var r = Math.floor(Math.random() * 255);
        var g = Math.floor(Math.random() * 255);
        var b = Math.floor(Math.random() * 255);
        var backColor = "rgba(" + r + "," + g + "," + b + ", 0.4)";
        var highColor = "rgba(" + r + "," + g + "," + b + ", 0.8)";
        var border = "rgba(" + r + "," + g + "," + b + ", 1)";

        backColors.push(backColor);
        highColors.push(highColor);
        borders.push(border);
    }

    return { backColors: backColors, highColors: highColors, borders: borders };
}



var percentColors = [
    { pct: 0.0, color: { r: 0xff, g: 0x00, b: 0 } },
    { pct: 0.5, color: { r: 0xff, g: 0xff, b: 0 } },
    { pct: 1.0, color: { r: 0x00, g: 0xff, b: 0 } }];

var getColorForPercentage = function (pct) {
    for (var i = 1; i < percentColors.length - 1; i++) {
        if (pct < percentColors[i].pct) {
            break;
        }
    }
    var lower = percentColors[i - 1];
    var upper = percentColors[i];
    var range = upper.pct - lower.pct;
    var rangePct = (pct - lower.pct) / range;
    var pctLower = 1 - rangePct;
    var pctUpper = rangePct;
    var color = {
        r: Math.floor(lower.color.r * pctLower + upper.color.r * pctUpper),
        g: Math.floor(lower.color.g * pctLower + upper.color.g * pctUpper),
        b: Math.floor(lower.color.b * pctLower + upper.color.b * pctUpper)
    };
    return 'rgb(' + [color.r, color.g, color.b].join(',') + ')';
    // or output as hex if preferred
}