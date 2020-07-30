document.addEventListener("drop", function (e) {  //拖离   
    e.preventDefault();
});
document.addEventListener("dragleave", function (e) {  //拖后放   
    e.preventDefault();
});
document.addEventListener("dragenter", function (e) {  //拖进  
    e.preventDefault();
});
document.addEventListener("dragover", function (e) {  //拖来拖去    
    e.preventDefault();
});
function stopDefault(e) {
    if (e && e.preventDefault) {
        e.preventDefault();
    } else {
        window.event.returnValue = false;
    }
}
//注册后台类
(async function () {
    await CefSharp.BindObjectAsync("main");

})();
//变量区
var DoubleClickTime = 550;
var showMin = true, isMove = false, isZoom, isOnResize = -1, isdrop = true, isUpName = true;//isMove：是否移动isZoom:是否原尺寸   showMin 是否现实缩略图
var view = $(".ViewBox"), win = $(window), image = new Image(), urlArray = new Array(), OurlArray = new Array();//view图片显示对象,window对象，图片对象
var downLeft, downTop, viewLeft, viewTop, WH, wh, wtop = 0, wbot = 0, zoom, deg = 0, isout, fast = 200, min_x, max_x, min_y, max_y, urlIndex, zoomindex;//鼠标按下时的左坐标和上坐标,图片的左上坐标,win的长宽比，图片的长宽比,zoom:倍率,deg旋转角度，isout鼠标是否在图片上


//函数区

String.prototype.replaceAll = function (FindText, RepText) {
    return this.replace(new RegExp(FindText, "g"), RepText);
};

//function doName(array) {
//    for (var i = 0; i < array.length; i++) {
//        for (var j = 0; j < length; j++) {

//        }
//    }
//}
//数组排序
function downName(b, a) {
    return a.Name.localeCompare(b.Name);
}
function upName(a, b) {
    return a.Name.localeCompare(b.Name);
}
function downDate(a, b) {
    return Date.parse(b.CreateTime) - Date.parse(a.CreateTime);
}
function upDate(b, a) {
    return Date.parse(b.CreateTime) - Date.parse(a.CreateTime);
}
function downSize(a, b) {
    return b.Size - a.Size;
}
function upSize(b, a) {
    return b.Size - a.Size;
}
/**获取图片连接数组
 * 
 * @param {JSON}url 图片连接数组
 * @param {number}xx 当前连接下标
 */
function getUrlArray(url, xx) {
    url = url.replaceAll("\'", "\"");
    urlArray = JSON.parse(url);
    urlIndex = xx;
    //ChangeImage(JSON.parse(url)[xx]);
}
function setUrlArray(url) {
    OurlArray = url;
    urlArray = url;
}
//添加缩略图
function setMinImg() {
    if (showMin) {
        $(".ShowMinImg").html("");
        for (var i = 0; i < urlArray.length; i++) {
            if (urlArray[i].Base) {
                $(".ShowMinImg").append("<img src=\"bin/Debug/ImageTum/" + urlArray[i].FullName.replaceAll("/", "-").replaceAll(":", "").replaceAll("#", "%23") + "\"/>");
            } else {
                $(".ShowMinImg").append("<img src='" + urlArray[i].FullName + "'/>");
            }

            if (urlArray[i].Name === $(".ShowUrl").text()) {
                urlIndex = i;
            }
        }
        //if (showMin) {
        //    main.changeindexs(urlIndex + "");
        //}
        timerr = setInterval(LoadedImg, 10);
    } else {
        $(".ShowMinImg").hide();
    }
}
//判断缩略图是否价值完毕
function LoadedImg() {
    if ($(".ShowMinImg img").eq(urlArray.length - 1).width() > 0) {
        clearTimeout(timerr);
        $(".ShowMinImg").stop(true, true).animate({ left: $(".ShowMinImg").offset().left + (win.width() / 2 - $(".ShowMinImg img").eq(urlIndex).offset().left) }, 200);
        $(".ShowMinImg img").eq(urlIndex).css({ "border": "solid 1px #d8d8d8", "outline": "solid 1px #000", "margin": "0px 1px" }).height(96);

    } else {
        console.log($(".ShowMinImg img").eq(urlArray.length - 1).width());
    }
}
////修改图片
function ChangeImage(url) {
    url = url.replaceAll("#", "%23");
    //console.log(url);
    imageUrl = url;
    isdrop = true;
    image.src = imageUrl;
    try {
        if (image.complete) {
            wh = image.width / image.height;
            inSize(0);

            $(".ViewBox").attr("src", imageUrl);
            var pix = image.height * image.width;
            if (pix >= 10000000) {
                zoomindex = 0.05;
            } else if (pix >= 1000000 && pix < 10000000) {
                zoomindex = 0.1;
            } else if (pix < 1000000) {
                zoomindex = 0.2;
            }
        } else {

            image.onload = function () {
                wh = image.width / image.height;
                inSize(0);
                $(".ViewBox").attr("src", imageUrl);
                var pix = image.height * image.width;
                if (pix >= 10000000) {
                    zoomindex = 0.05;
                } else if (pix >= 1000000 && pix < 10000000) {
                    zoomindex = 0.1;
                } else if (pix < 1000000) {
                    zoomindex = 0.2;
                }
            };


        }
    } catch (e) {
        alert(e);
    }
    $(".ShowUrl").html(getShow(url.substring(url.lastIndexOf('/') + 1), (win.width - 400) / 7) + "<div></div>");
}
function getShow(content, lengthSize) {
    var res = 0;
    for (var i = 0; i < content.length; i++) {
        //判断是不是有中文
        if (content.charCodeAt(i) >= 0 && content.charCodeAt(i) <= 255) {
            res += 1;
        }
        else {
            res += 2;//中文，相当于占两位字符
        }
        if (res >= lengthSize) {//如果占的位数超过了最大显示的位数
            return content.substring(0, i + 1) + "...";
        }
    }
    return content;//如果占的位数没有超过最大显示的位数
}

//重置大小
function inSize(fast) {
    isZoom = false;
    WH = win.width() / (win.height() + wtop + wbot);
    var imgW, imgH;
    if (WH > wh) {
        imgW = (win.height() - wtop - wbot) * wh;
        imgH = win.height() - wtop - wbot;
        zoom = imgW / image.width;
        view.stop(true, true).animate({ width: imgW, height: imgH, top: wtop, left: (win.width() - imgW) / 2 }, fast);
        //view.animate({ },1000);
    } else {
        imgW = win.width();
        imgH = win.width() / wh;
        zoom = imgW / image.width;
        view.stop(true, true).animate({ width: imgW, height: imgH, top: (win.height() - imgH) / 2, left: 0 }, fast);
    }
}
//旋转后重置大小
function inSizeX(fast) {
    WHX = image.width / image.height;
    wwh = (win.height() - wtop - wbot) / win.width();
    if (WHX > wwh) {

        if (deg === -90 || deg === -270) {
            imgh = (win.height() - wtop - wbot) / WHX;
            imgw = win.height() - wtop - wbot;
            zoom = imgw / image.width;
            pianyi = (imgh - imgw) / 2;
            $(".ViewBox").stop(true, true).animate({ top: pianyi * -1 + wtop, left: (win.width() - imgh) / 2 + pianyi, height: imgh, width: imgw }, fast);
        } else {
            inSize(fast);
        }

    } else {
        if (deg === -90 || deg === -270) {
            imgh = win.width();
            imgw = win.width() * WHX;
            zoom = imgw / image.width;
            pianyi = (imgh - imgw) / 2;
            $(".ViewBox").stop(true, true).animate({ top: (win.height() - imgw) / 2 - pianyi, left: pianyi, height: imgh, width: imgw }, fast);
        } else {
            inSize(fast);
        }

    }
    isZoom = false;
}
/**鼠标不在图片上滚动时
* @param {boolean}on    是否开始
* @param {number}delta  上滚动>1或下滚动<1
*/
function mouseOutWl(on, delta) {
    if (on) {
        var zoomnow = view.width() / image.width;
        viewW = image.width;
        viewH = image.height;
        bizhi = 0;
        if (deg === -90 || deg === -270) {
            bizhi = (view.height() - view.width()) / 2;
        }

        if (delta > 0) {
            if (zoom < 9.99) {

                zoom = getzoom(zoom, zoomindex, true);
                GetMaxMin(zoom);
                //图像放大后乘以比值减去原鼠标位置得到移动量
                view.stop().animate({ width: viewW * zoom, height: viewH * zoom, top: NumVar(view.offset().top - bizhi - viewH * (zoom - zoomnow) / 2, min_y, max_y), left: NumVar(view.offset().left + bizhi - viewW * (zoom - zoomnow) / 2, min_x, max_x) }, fast);
            }
        } else {
            if (zoom >= zoomindex * 2) {
                zoom = getzoom(zoom, zoomindex, false);
                GetMaxMin(zoom);
                view.stop().animate({ width: viewW * zoom, height: viewH * zoom, top: NumVar(view.offset().top - bizhi + viewH * (zoomnow - zoom) / 2, min_y, max_y), left: NumVar(view.offset().left + bizhi + viewW * (zoomnow - zoom) / 2, min_x, max_x) }, fast);

            }
        }
    }
    $(".zoomview").html(Math.round(zoom * 100) + "%").stop(true, true).animate({ opacity: 1 }, 0).animate({ opacity: 0 }, 500);
    $(".iszoom").html("适应尺寸");
    isZoom = true;
}
/**
 * 获取图片最大和最小移动范围
 * @param {any} zoom 放大倍率
 */
function GetMaxMin(zoom) {
    x_v = win.width() / 2;
    y_v = win.height() / 2;
    zoomW = image.width * zoom;
    zoomH = image.height * zoom;
    if (x_v > zoomW) {
        x_v = zoomW;
    }
    if (y_v > zoomH) {
        y_v = zoomH;
    }
    max_x = $(window).width() - x_v;
    max_y = $(window).height() - y_v;
    min_x = -(zoomW - x_v);
    min_y = -(zoomH - y_v);
    if (deg === -90 || deg === -270) {
        if (x_v > zoomH) {
            x_v = zoomH;
        }
        if (y_v > zoomW) {
            y_v = zoomW;
        }
        max_x = $(window).width() - x_v;
        max_y = $(window).height() - y_v;
        min_x = -(zoomH - x_v);
        min_y = -(zoomW - y_v);
    }
}
/**
 * 取值区间
 * @param {any}a 被限制
 * @param {any}b 最小值
 * @param {any}c 最大值
 * @returns {any} 返回限制后的值
 */
function NumVar(a, b, c) {

    if (a < b) {
        return b;
    } else if (a > c) {
        return c;
    } else {
        return a;
    }
}
/**
* 鼠标在图片上滚动时
* @param {any}event event
* @param {any}delta delta
*/

function mousewh(event, delta) {
    isout = false;
    viewW = image.width;
    viewH = image.height;
    mW = event.offsetX / view.width();    //图片宽度与鼠标位置的比值
    mH = event.offsetY / view.height();   ////图片高度与鼠标位置的比值
    isMove = false;
    if (delta > 0) {
        if (zoom < 9.99) {
            zoom = getzoom(zoom, zoomindex, true);
            dangqianbi = (image.height - image.width) / 2 * zoom - (view.height() - view.width()) / 2;
            bizhi = (view.height() - view.width()) / 2;
            GetMaxMin(zoom);
            //图像放大后乘以比值减去原鼠标位置得到移动量 (图像放大后乘以比值得出同像素的位置再减去当前鼠标位置就能得出偏移量)
            if (deg === -90 || deg === -270) {
                mW = event.offsetX / view.height();    //图片高度与鼠标位置X的比值
                mH = event.offsetY / view.width();   ////图片宽度与鼠标位置Y的比值
                view.stop().animate({ height: viewH * zoom, width: viewW * zoom, top: NumVar(view.offset().top - bizhi - dangqianbi - (viewW * zoom * mH - event.offsetY), min_y, max_y), left: NumVar(view.offset().left + bizhi - dangqianbi * -1 - (viewH * zoom * mW - event.offsetX), min_x, max_x) + "px" }, 200);
            } else {
                view.stop().animate({ height: viewH * zoom, width: viewW * zoom, top: NumVar(view.offset().top - (viewH * zoom * mH - event.offsetY), min_y, max_y), left: NumVar(view.offset().left - (viewW * zoom * mW - event.offsetX), min_x, max_x) + "px" }, fast);
            }

        }

    } else {
        if (zoom >= zoomindex * 2) {
            zoom = getzoom(zoom, zoomindex, false);
            dangqianbi = (image.height - image.width) / 2 * zoom - (view.height() - view.width()) / 2;
            bizhi = (view.height() - view.width()) / 2;
            GetMaxMin(zoom);
            if (deg === -90 || deg === -270) {
                mW = event.offsetX / view.height();    //图片高度与鼠标位置X的比值
                mH = event.offsetY / view.width();   ////图片宽度与鼠标位置Y的比值
                view.stop().animate({ height: viewH * zoom, width: viewW * zoom, top: NumVar(view.offset().top - bizhi + dangqianbi * -1 + (event.offsetY - viewW * zoom * mH), min_y, max_y), left: NumVar(view.offset().left + bizhi + dangqianbi + (event.offsetX - viewH * zoom * mW), min_x, max_x) + "px" }, 200);
            } else {
                view.stop().animate({ width: viewW * zoom, height: viewH * zoom, top: NumVar(view.offset().top + (event.offsetY - viewH * zoom * mH), min_y, max_y), left: NumVar(view.offset().left + (event.offsetX - viewW * zoom * mW), min_x, max_x) }, fast);
            }
        }

    }
}
//添加链接图片
function AddImg(url) {
    var TestImg = new Image();
    TestImg.src = url;
    TestImg.onload = function () {
        FindObj(urlArray, url, function (v) {
            //console.log(v);
            if (v < 0) {
                //ChangeImage(url);
                //FindObj(urlArray, url);
                urlArray.splice(urlIndex + 1, 0, { Name: url, FullName: url, CreateTime: "", Size: 0, Base: false });
                $(".ShowMinImg img").eq(urlIndex).after("<img src='" + urlArray[urlIndex + 1].FullName + "'/>");
                ChangeIndex(urlIndex + 1);
            } else {
                //urlIndex = v;
                ChangeIndex(v);
            }
        });

    };
}
/**
 * 在数组中添加值
 * @param {any} findData
 * @param {any} findStr
 * @param {any} xfun
 */
function FindObj(findData, findStr, xfun) {
    for (var i = 0; i < findData.length; i++) {
        if (findData[i].FullName === findStr) {
            xfun(i);
            return;
        }
    }
    xfun(-1);
}
/**下一张图片 */
function ImgDown() {
    if (urlArray !== null) {
        if (urlIndex < urlArray.length - 1) {
            if (showMin) {
                try {
                    ChangeIndex(urlIndex - (-1));
                    //main.changeindexs(urlIndex - (-1) + "");
                } catch (e) {
                    console.log(e);
                    urlIndex++;
                    ChangeImage(urlArray[urlIndex].FullName);
                }

            } else {
                urlIndex++;
                ChangeImage(urlArray[urlIndex].FullName);

            }


        } else {

            if (showMin) {
                try {
                    ChangeIndex(0);
                    //main.changeindexs(0 + "");
                } catch (e) {
                    urlIndex = 0;
                    ChangeImage(urlArray[urlIndex].FullName);
                }

            } else {
                urlIndex = 0;
                ChangeImage(urlArray[urlIndex].FullName);

            }
        }
    }
    deg = 0;
    view.css("transform-origin", "center");
    view.css("transform", "rotate(0deg)");
}
/**上一张图片 */
function ImgUp() {
    if (urlArray !== null) {
        if (urlIndex > 0) {
            if (showMin) {
                try {
                    ChangeIndex(urlIndex - 1);
                    //main.changeindexs(urlArray - 1 + "");
                } catch (e) {
                    urlIndex--;
                    ChangeImage(urlArray[urlIndex].FullName);
                }

            } else {
                urlIndex--;
                ChangeImage(urlArray[urlIndex].FullName);
            }

            //ChangeImage(urlArray[urlIndex].FullName);
        } else {
            if (showMin) {
                try {
                    ChangeIndex(urlArray.length - 1);
                    //main.changeindexs(urlArray.length - 1 + "");
                } catch (e) {
                    urlIndex = urlArray.length - 1;
                    ChangeImage(urlArray[urlIndex].FullName);
                }

            } else {
                urlIndex = urlArray.length - 1;
                ChangeImage(urlArray[urlIndex].FullName);
            }


        }
        //console.log(urlArray[urlIndex]);
    }
    deg = 0;
    view.css("transform-origin", "center");
    view.css("transform", "rotate(0deg)");
}
/**获取倍率 十位小数以上
 * 
 * @param {number}zoom  当前倍率
 * @param {boolean}x   false减true加
 * @param {kao}kao  靠拢量
 */

function getzoom(zoom, kao, x) {

    var over = kao.toString().length - kao.toString().indexOf(".") - 2;
    var fzoom = Math.floor(zoom * Math.pow(10, over)) / Math.pow(10, over);
    if (x) {
        while (fzoom > -1) {
            fzoom = fzoom + kao;
            if (fzoom - zoom > kao / 2) {
                return fzoom;
            }
        }

    } else {
        fzoom = fzoom + kao * 10;
        while (fzoom > -1) {
            fzoom = fzoom - kao;
            if (fzoom - zoom < -kao / 2) {
                return fzoom;
            }
        }
    }
    return zoom;
}
function ChangeIndex(index) {

    $(".ShowMinImg img").eq(index).css({ "border": "solid 1px #d8d8d8", "outline": "solid 1px #000", "margin": "0px 1px" }).height(96);
    $(".ShowMinImg img").eq(urlIndex).css({ "border": "none", "outline": "none", "margin": "0" }).height(100);
    urlIndex = parseInt(index);
    ChangeImage(urlArray[urlIndex].FullName);
    $(".ShowMinImg").stop(true, true).animate({ left: $(".ShowMinImg").offset().left + (win.width() / 2 - $(".ShowMinImg img").eq(index).offset().left) }, 200);
}

//事件区
var whatSort = -1;//排序类型
$(".ChangeSrot").mousemove(function () {
    $(".ChangeSrot").css("background", "#d8d8d8");
    $(".rightcLose").css("background", "#fff");
    $(".SrotList").show();
    whatSort = 6;
});
$(".rightcLose").mousemove(function () {
    $(".ChangeSrot").css("background", "#fff");
    $(".rightcLose").css("background", "#d8d8d8");
    $(".UpOrDown").hide();
    $(".SrotList").hide();
    whatSort = 7;
});
$(".SortName").mousemove(function () {
    $(".SortName").css("background", "#d8d8d8");
    $(".SortTime").css("background", "#fff");
    $(".SortSize").css("background", "#fff");
    $(".UpOrDown").show().css("top", $(".SrotList").offset().top);
    whatSort = 0;
});
$(".SortTime").mousemove(function () {
    $(".SortName").css("background", "#fff");
    $(".SortTime").css("background", "#d8d8d8");
    $(".SortSize").css("background", "#fff");
    $(".UpOrDown").show().css("top", $(".SrotList").offset().top + 30);
    whatSort = 1;
});
$(".SortSize").mousemove(function () {
    $(".SortName").css("background", "#fff");
    $(".SortTime").css("background", "#fff");
    $(".SortSize").css("background", "#d8d8d8");
    $(".UpOrDown").show().css("top", $(".SrotList").offset().top + 60);
    whatSort = 2;
});

$(".SortUp").click(function () {
    switch (whatSort) {
        case 0:
            //urlArray.sort(upName);
            if (isUpName) {
                urlArray = OurlArray;
            } else {
                urlArray = OurlArray;
                urlArray.reverse();
                isUpName = true;
            }
            main.changeSortType(1);

            break;
        case 1:
            urlArray.sort(upDate);
            main.changeSortType(3);
            break;
        case 2:
            urlArray.sort(upSize);
            main.changeSortType(5);
            break;
    }
    setMinImg();
    $(".rightMain").hide();
    $(".SrotList").hide();
    $(".UpOrDown").hide();
});
$(".SortDown").click(function () {
    switch (whatSort) {
        case 0:
            //urlArray.sort(downName);

            if (isUpName) {
                urlArray = OurlArray;
                urlArray.reverse();
                isUpName = false;

            } else {
                urlArray = OurlArray;
            }
            main.changeSortType(2);
            break;
        case 1:
            urlArray.sort(downDate);
            main.changeSortType(4);
            break;
        case 2:
            urlArray.sort(downSize);
            main.changeSortType(6);
            break;
    }

    setMinImg();
    $(".rightMain").hide();
    $(".SrotList").hide();
    $(".UpOrDown").hide();
});
$(".ShowMinImg").on("click", "img", function (e) {
    $(".ShowMinImg img").eq(urlIndex).css("border", "none").height(100);
    $(this).css({ "border": "solid 1px #d8d8d8", "outline": "solid 1px #000" }).height(96);
    urlIndex = $(this).index();
    ChangeImage(urlArray[urlIndex].FullName);
    $(".ShowMinImg").stop(true).animate({ left: $(".ShowMinImg").offset().left + (win.width() / 2 - $(this).offset().left) }, 200);
}).on("mouseenter", "img", function () {
    $(this).css({ "border": "solid 1px #d8d8d8", "outline": "solid 1px #000", "margin": "0px 1px" }).height(96);
}).on("mouseout", "img", function () {
    if ($(this).index() !== urlIndex) {
        $(this).css({ "border": "none", "outline": "none", "margin": "0" }).height(100);
    }
});
//$(".ShowAllImg").on("mouseover", "img", function (e) {
//    $(this).stop().animate({ height: 120, top: -10 });
//}).on("mouseleave", "img", function (e) {
//    $(this).stop().animate({ height: 100, top:0 });
//}).on("click", "div", function (e) {

//    urlIndex = $(this).attr("id");
//    $("#" + urlIndex).css("border", "solid 2px #d8d8d8").height(96);
//    ChangeImage(urlArray[urlIndex].FullName);
//    $(".ShowAllImg").hide();
//    ChangeIndex(urlIndex);
//});
$(".ViewBox").bind("mousewheel", mousewh).mouseout(function () {
    isout = true;
});
$(".ImgDown").click(function () {
    ImgDown();
});
$(".ImgUp").click(function () {
    ImgUp();
});
$(".Turnleft").click(function () {
    if (deg === -270) {
        deg = 0;
    } else {
        deg -= 90;
    }
    view.css("transform-origin", "center");
    view.css("transform", "rotate(" + deg + "deg)");
    inSizeX(0);
    //view.css("margin", view.width() + "px 0px 0px 0px");
});
$(".TurnRight").click(function () {
    if (deg === 0) {
        deg = -270;
    } else {
        deg += 90;
    }

    view.css("transform-origin", "center");
    view.css("transform", "rotate(" + deg + "deg)");
    inSizeX(0);

    //view.css("margin", view.width() + "px 0px 0px 0px");
});
var donthide = true;
$(".rightMain,.SrotList,.UpOrDown").mousemove(function () {
    donthide = false;
}).mouseleave(function () {
    donthide = true;
});
/**鼠标不在图片上滚动时
* @param {boolean}on    是否开始
* @param {number}delta  上滚动>1或下滚动<1
*/
win.bind("mousewheel", function (event, delta) {
    mouseOutWl(isout, delta);
});
//图片被鼠标按下时记录下当前位置
view.mousedown(function (e) {

    if (e.pageY > 30 && e.pageY < win.height() - 50) {
        isMove = true;
        downLeft = e.pageX;
        downTop = e.pageY;
        viewLeft = view.offset().left;
        viewTop = view.offset().top;
        GetMaxMin(zoom);
    } else {
        isMove = false;
    }
});
var xONbotton = false;
var date = new Date;
var clickTime = 0;
//鼠标移动
win.mousedown(function (e) {


    if (!isMove && isOnResize < 0 || isOnResize > 8) {
        if (new this.Date() - date < this.DoubleClickTime) {
            //在规定时间内点击
            clickTime += 1;
            if (clickTime === 2) {
                //在规定时间内点击2次触发双击
                main.changeWindow(2);
            }
        } else {
            clickTime = 1;
            date = new this.Date();
        }
        //isOnResize = 9;
        //移动窗体
        main.sendMove(1);
        isMove = false;
        isOnResize = -1;
        //按下鼠标右键
        if (e.which === 3) {
            $(".rightMain").show();
            $(".rightMain").css({ left: e.pageX, top: e.pageY });
            $(".SrotList").css({ left: e.pageX + 200, top: e.pageY });
            $(".UpOrDown").css({ left: e.pageX + 400, top: e.pageY });
            $(".UpOrDown").css("background", "#fff");
            $(".rightcLose").css("background", "#fff");
            $(".SortName").css("background", "#fff");
            $(".SortTime").css("background", "#fff");
            $(".SortSize").css("background", "#fff");
        } else if (e.which === 1) {
            //按下鼠标左键
            if (donthide) {
                $(".rightMain").hide();
                $(".SrotList").hide();
                $(".UpOrDown").hide();
            }
        }
    }
    windowW = $(window).width();
    windowH = $(window).height();
}).mousemove(function (e) {
    //开始移动图片
    if (isMove && isOnResize === -1) {
        view.offset({ left: NumVar(viewLeft - downLeft + e.pageX, min_x, max_x), top: NumVar(viewTop - downTop + e.pageY, min_y, max_y) });
        isZoom = true;
    }
    //显示或隐藏底部
    if (e.pageY > $(this.window).height() - 150 && !isMove) {
        if (!xONbotton) {
            $(".bottom").slideDown(100);
            $(".ShowMinImg").stop().animate({ opacity: 1 }, 300);
            xONbotton = true;
        }

        //$(".ShowMinImg").slideDown(100);
    } else {
        //$(".ShowMinImg").slideUp(100);
        if (xONbotton) {
            $(".bottom").slideUp(100);
            $(".ShowMinImg").stop().animate({ opacity: 0 }, 300);
            xONbotton = false;
        }

    }
    //显示或隐藏顶部
    if (e.pageY <= 50 && !isMove) {
        $(".top").slideDown(100);
    } else {
        $(".top").slideUp(100);
    }

    //拖动边框（已过时）
    switch (isOnResize) {
        case 1:
            main.showborder([e.pageX - isOnResizeX + windowW, windowH]);
            break;
        case 2:
            main.showborder([windowW, e.pageY - isOnResizeY + windowH]);
            break;
        case 3:
            main.hideborder([100, 0, isOnResizeX * -1, isOnResizeY * -1]);
            console.log([100, 0, isOnResizeX * -1, isOnResizeY * -1]);
            break;
        case 4:
            main.hideborder([200, 0, isOnResizeX * -1, isOnResizeY * -1]);
            break;
        case 5:
            main.showborder([e.pageX - isOnResizeX + windowW, e.pageY - isOnResizeY + windowH]);
            break;
        case 6:
            main.hideborder([300, 0, isOnResizeX * -1, isOnResizeY * -1]);
            break;
        case 7:
            main.hideborder([400, 0, isOnResizeX * -1, isOnResizeY * -1]);
            break;
        case 8:
            main.hideborder([500, 0, isOnResizeX * -1, isOnResizeY * -1]);
            break;
        case 9:
            //main.hideborder([600, 0, 0, 0]);
            break;
    }
}).mouseup(function (e) {
    this.console.log(isMove);
    isMove = false;
    isOnResize = -1;
    if (e.which === 3) {
        $(".rightMain").show();
        $(".rightMain").css({ left: e.pageX, top: e.pageY });
        $(".SrotList").css({ left: e.pageX + 200, top: e.pageY });
        $(".UpOrDown").css({ left: e.pageX + 400, top: e.pageY });
        $(".UpOrDown").css("background", "#fff");
        $(".rightcLose").css("background", "#fff");
        $(".SortName").css("background", "#fff");
        $(".SortTime").css("background", "#fff");
        $(".SortSize").css("background", "#fff");
    } else if (e.which === 1) {
        if (donthide) {
            $(".rightMain").hide();
            $(".SrotList").hide();
            $(".UpOrDown").hide();
        }
    }
}).resize(() => { windowResize(); this.console.log("adasd"); }).on("drop", function (e) {

    if (e.originalEvent.dataTransfer.getData("Url") !== "") {
        if (main.isimg(e.originalEvent.dataTransfer.getData("Url"))) {
            this.AddImg(e.originalEvent.dataTransfer.getData("Url"));
        }
    } else {
        main.drap();
    }

});
//窗体大小改变
function windowResize() {
    if (isZoom) {
        view.offset({ left: Math.min(view.offset().left, win.width() - win.width() / 2), top: Math.min(view.offset().top, win.height() - win.height() / 2) });
    } else {
        if (deg === -90 || deg === -270) {
            inSizeX(0);
        } else {
            inSize(0);
        }
    }
    //$("html").css('--num', parseInt(win.width() / 150));
}

$(".iszoom").click(function () {
    changeZoom();
});
//$(".Close").click(function () {
//    main.close();
//});
//切换模式
function changeZoom() {
    if (isZoom) {
        $(".iszoom").html("实际尺寸");
        if (deg === -90 || deg === -270) {
            inSizeX(200);
        } else {
            inSize(200);
        }
    } else {
        view.animate({ width: image.width, height: image.height, top: (win.height() - image.height) / 2, left: (win.width() - image.width) / 2 }, 200);
        isZoom = true;
        zoom = 1;
        $(".iszoom").html("适应尺寸");
    }
}
$(".isMany").click(function () {
    if (main.changeMany()) {
        $(".isMany").html("点击禁止多窗口（重启生效）");
    } else {
        $(".isMany").html("点击启动多窗口（重启生效）");
    }
});
//$(".top").click(function (e) {
//    console.log(window.outerWidth + " " + screen.availWidth);
//    if (window.outerWidth !== screen.availWidth && window.outerHeight !== screen.availHeight && e.pageX < window.outerWidth - 120) {
//        NanUI.hostWindow.maximize();
//    }
//});
$(".top").dblclick(function (e) {
    //最大化
    main.changeWindow(2);
    //if (window.outerWidth === screen.availWidth && window.outerHeight === screen.availHeight && e.pageX < window.outerWidth - 120) {
    
    //}
});
$(".Close").click(function () {
    //关闭
    main.changeWindow(3);
});
$(".rightcLose").click(function () {
    //关闭
    main.changeWindow(3);
});
$(".Max").click(function () {
    //最大化
    main.changeWindow(2);
});
$(".Min").click(function () {
    //最小化
    main.changeWindow(1);
});
$(".xs").click(function () {
    //开始幻灯片
    timer = setInterval(ImgDown, 5000);
});
$(".yc").click(function () {
    //停止幻灯片
    clearInterval(timer);
});
$("[id=dMove]").mousedown(function () {
    //指定为不可移动图片的对象
    isOnResize = 0;
});
$(".resizeR").mousedown(function (e) {
    //拖动右边框
    main.sendMove(2);
    //isOnResize = 1;
    //isOnResizeX = e.pageX;
    //isOnResizeY = $(window).height();
    //console.log(isOnResizeY);
});
$(".resizeB").mousedown(function (e) {
    //拖动下边框
    main.sendMove(3);
});
$(".resizeRT").mousedown(function (e) {
    //拖动右上角
    main.sendMove(4);
});
$(".resizeRB").mousedown(function (e) {
    //拖动右下角
    main.sendMove(5);
});
$(".resizeLB").mousedown(function (e) {
    //拖动左下角
    main.sendMove(6);
});
$(".resizeLT").mousedown(function (e) {
    //拖动左上角
    main.sendMove(7);
});
$(".resizeL").mousedown(function (e) {
    //拖动左边框
    main.sendMove(8);
});
$(".resizeT").mousedown(function (e) {
    //拖动上边框
    main.sendMove(9);
});
document.onkeydown = function (event) {
    var e = event || window.event || arguments.callee.caller.arguments[0];
    if (e) {
        switch (e.keyCode) {
            case 32:// 按 空格
                changeZoom();
                break;
            case 37:// 按  左
                //要做的事情
                if (!e.ctrlKey && !e.shiftKey) {
                    ImgUp();
                } else if (e.ctrlKey && !e.shiftKey) {
                    MoveView(MoveViewType.left, 10);
                } else if (e.shiftKey && e.ctrlKey) {
                    MoveView(MoveViewType.left, 100);
                }
                break;
            case 38://上
                if (!e.ctrlKey && !e.shiftKey) {
                    mouseOutWl(true, 1);
                } else if (e.ctrlKey && !e.shiftKey) {
                    MoveView(MoveViewType.top, 10);
                } else if (e.shiftKey && e.ctrlKey) {
                    MoveView(MoveViewType.top, 100);
                }
                break;
            case 39://右
                if (!e.ctrlKey && !e.shiftKey) {
                    ImgDown();
                } else if (e.ctrlKey && !e.shiftKey) {
                    MoveView(MoveViewType.right, 10);
                } else if (e.shiftKey && e.ctrlKey) {
                    MoveView(MoveViewType.right, 100);
                }
                break;
            case 40://下
                if (!e.ctrlKey && !e.shiftKey) {
                    mouseOutWl(true, -1);
                } else if (e.ctrlKey && !e.shiftKey) {
                    MoveView(MoveViewType.bottom, 10);
                } else if (e.shiftKey && e.ctrlKey) {
                    MoveView(MoveViewType.bottom, 100);
                }
                break;
            case 116://F5
                window.location.reload();
                break;
            case 123://f12
                break;
        }
    }
};
/**
 * 
 * @param {any} type 移动方向
 * @param {any} val  移动距离
 */
function MoveView(type, val) {
    //获取最大和最小移动距离
    GetMaxMin(zoom);
    switch (type) {
        case MoveViewType.left:
            view.offset({ left: NumVar(view.offset().left - val, min_x, max_x), top: view.offset().top });
            break;
        case MoveViewType.top:
            view.offset({ left: view.offset().left, top: NumVar(view.offset().top - val, min_y, max_y) });
            break;
        case MoveViewType.right:
            view.offset({ left: NumVar(view.offset().left + val, min_x, max_x), top: view.offset().top });
            break;
        case MoveViewType.bottom:
            view.offset({ left: view.offset().left, top: NumVar(view.offset().top + val, min_y, max_y) });
            break;
    }
}
//移动图片的枚举参数
var MoveViewType = { left: 1, top: 2, right: 3, bottom: 4 };
$(function (e) {
    //初始化
    $(".bottom").slideUp(0);
    $(".ShowMinImg").css({ opacity: 0 });
    xONbotton = false;
    $(".top").slideUp(0);
});
